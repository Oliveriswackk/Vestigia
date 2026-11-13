import 'dart:async';
import 'dart:convert';

import 'package:flutter/foundation.dart';

import 'backend_config.dart';
import 'room_api.dart';
import 'room_channel.dart';
import 'room_models.dart';

enum RoomPhase { idle, joining, connecting, connected, reconnecting, closed }

enum CloseAction { reconnect, endSession }

/// Qué hacer según el código con el que el servidor cerró el WebSocket
/// (`docs/tiempo-real.md` del backend).
CloseAction closeActionFor(int? code) => switch (code) {
  1000 || 4000 || 4001 || 4009 || 1009 => CloseAction.endSession,
  _ => CloseAction.reconnect,
};

String _closeMessage(int? code) => switch (code) {
  1000 => 'La sala se cerró.',
  4001 => 'El token ya no sirve. Vuelve a elegir el pueblo.',
  4009 => 'Otra copia de la app tomó la sala.',
  _ => 'La conexión se cerró con el código $code.',
};

Duration defaultBackoff(int attempt) =>
    Duration(seconds: attempt < 5 ? 1 << attempt : 30);

class RoomLogEntry {
  const RoomLogEntry(this.time, this.text);

  final DateTime time;
  final String text;
}

/// Sesión de la app con una sala del juego: unirse por REST, abrir el
/// WebSocket, autenticarse, reconectar y mantener el último estado.
class RoomConnection extends ChangeNotifier {
  RoomConnection({
    required this.config,
    required this.api,
    RoomChannelOpener? opener,
    Duration Function(int attempt)? backoff,
  }) : _opener = opener ?? IoRoomChannel.open,
       _backoff = backoff ?? defaultBackoff;

  factory RoomConnection.standard() {
    const config = BackendConfig();
    return RoomConnection(config: config, api: HttpRoomApi(config));
  }

  static const maxLogEntries = 40;

  final BackendConfig config;
  final RoomApi api;
  final RoomChannelOpener _opener;
  final Duration Function(int attempt) _backoff;

  RoomPhase _phase = RoomPhase.idle;
  String? _code;
  String? _token;
  String? _error;
  RoomSnapshot? _snapshot;
  bool _gameConnected = false;
  final List<RoomLogEntry> _log = [];

  RoomChannel? _channel;
  StreamSubscription<String>? _subscription;
  Timer? _retryTimer;
  int _attempts = 0;
  int _session = 0;
  int _socketGeneration = 0;
  int _stateRequest = 0;
  bool _disposed = false;

  RoomPhase get phase => _phase;
  String? get code => _code;
  String? get error => _error;
  RoomSnapshot? get snapshot => _snapshot;
  bool get gameConnected => _gameConnected;

  /// Más reciente primero.
  List<RoomLogEntry> get log => List.unmodifiable(_log);

  bool get inRoom =>
      _phase == RoomPhase.connecting ||
      _phase == RoomPhase.connected ||
      _phase == RoomPhase.reconnecting;

  Future<void> join(String raw) async {
    if (_phase != RoomPhase.idle && _phase != RoomPhase.closed) return;
    final code = normalizeRoomCode(raw);
    if (code.isEmpty) {
      _error = 'Escribe el nombre del pueblo que ves en la PC.';
      _notify();
      return;
    }
    final session = ++_session;
    _error = null;
    _code = code;
    _snapshot = null;
    _gameConnected = false;
    _log.clear();
    _phase = RoomPhase.joining;
    _notify();

    final String token;
    try {
      token = await api.join(code);
    } on RoomApiException catch (e) {
      if (session != _session || _disposed) return;
      _failJoin(switch (e.status) {
        404 =>
          'No encontramos una partida con ese código. Revisa el nombre que aparece en la pantalla de la PC.',
        409 => 'Esa partida ya tiene un teléfono conectado.',
        _ => e.message,
      });
      return;
    } on NoConnectionException {
      if (session != _session || _disposed) return;
      _failJoin('No hay conexión. Revisa tu internet e inténtalo de nuevo.');
      return;
    }
    if (session != _session || _disposed) return;
    _token = token;
    _attempts = 0;
    _addLog('Unida a la sala $code');
    _phase = RoomPhase.connecting;
    _notify();
    unawaited(_open());
  }

  /// Olvida la sala en esta app. El servidor no se entera: la partida sigue
  /// enlazada a este teléfono hasta que el juego la cierre o expire.
  void leave() {
    if (_phase == RoomPhase.idle) return;
    _session++;
    _retryTimer?.cancel();
    _release();
    _token = null;
    _code = null;
    _snapshot = null;
    _gameConnected = false;
    _error = null;
    _log.clear();
    _phase = RoomPhase.idle;
    _notify();
  }

  /// Vuelve a pedir el estado. Se llama al volver la app al primer plano.
  Future<void> refresh() async {
    final code = _code;
    final token = _token;
    if (code == null || token == null || !inRoom) return;
    final session = _session;
    final request = ++_stateRequest;
    try {
      final snapshot = await api.state(code, token);
      if (_disposed || session != _session || request != _stateRequest) return;
      _snapshot = snapshot;
      _gameConnected = snapshot.gameConnected;
      _notify();
    } on RoomApiException catch (e) {
      if (_disposed || session != _session) return;
      if (e.status == 401) {
        _end('La partida terminó. Vuelve a elegir el pueblo.');
      } else {
        _addLog('No se pudo pedir el estado: ${e.message}');
        _notify();
      }
    } on NoConnectionException {
      if (_disposed || session != _session) return;
      _addLog('Sin red al pedir el estado');
      _notify();
    }
  }

  Future<void> _open() async {
    _release();
    final generation = _socketGeneration;
    final RoomChannel channel;
    try {
      channel = await _opener(config.webSocketUri);
    } catch (_) {
      if (_isStale(generation)) return;
      _addLog('No se pudo abrir el WebSocket');
      _scheduleRetry();
      return;
    }
    if (_isStale(generation)) {
      unawaited(channel.close().catchError((Object _) {}));
      return;
    }
    _channel = channel;
    channel.send(
      jsonEncode({
        'type': 'auth',
        'payload': {'token': _token},
      }),
    );
    _subscription = channel.messages.listen(
      (text) => _onText(generation, text),
      onError: (Object _) => _onClosed(generation, channel),
      onDone: () => _onClosed(generation, channel),
      cancelOnError: true,
    );
  }

  bool _isStale(int generation) => _disposed || generation != _socketGeneration;

  void _release() {
    _socketGeneration++;
    _subscription?.cancel();
    _subscription = null;
    final channel = _channel;
    _channel = null;
    if (channel != null) {
      unawaited(channel.close().catchError((Object _) {}));
    }
  }

  void _scheduleRetry() {
    _phase = RoomPhase.reconnecting;
    final delay = _backoff(_attempts++);
    _addLog('Reintento en ${delay.inSeconds} s');
    _notify();
    _retryTimer?.cancel();
    _retryTimer = Timer(delay, () => unawaited(_open()));
  }

  void _onClosed(int generation, RoomChannel channel) {
    if (_isStale(generation) || !identical(_channel, channel)) return;
    final code = channel.closeCode;
    _channel = null;
    _subscription = null;
    _addLog('Conexión cerrada (${code ?? 'sin código'})');
    switch (closeActionFor(code)) {
      case CloseAction.endSession:
        _end(_closeMessage(code));
      case CloseAction.reconnect:
        _scheduleRetry();
    }
  }

  void _onText(int generation, String text) {
    if (_isStale(generation)) return;
    final message = RoomMessage.tryParse(text);
    if (message == null) {
      _addLog('Mensaje ilegible');
      _notify();
      return;
    }
    final payload = message.payload;
    switch (message.type) {
      case 'auth.ok':
        _attempts = 0;
        _phase = RoomPhase.connected;
        _addLog('Autenticada como ${payload['rol']} en ${payload['sala']}');
        _notify();
        unawaited(refresh());
      case 'dispositivo.conectado':
        if (payload['rol'] == 'juego') _gameConnected = true;
        _addLog('Conectado: ${payload['rol']}');
        _notify();
      case 'dispositivo.desconectado':
        if (payload['rol'] == 'juego') _gameConnected = false;
        _addLog('Desconectado: ${payload['rol']}');
        _notify();
      case 'puzzle.actualizado':
        _addLog('Puzzle ${payload['puzzle']}: ${payload['estado']}');
        _notify();
        unawaited(refresh());
      case 'descubrimiento.nuevo':
        _addLog(
          'Descubrimiento ${payload['clave']} (${payload['origen']})',
        );
        _notify();
        unawaited(refresh());
      case 'escena.cambiada':
        _addLog('Escena: ${payload['escena']}');
        _notify();
        unawaited(refresh());
      case 'sala.cerrada':
        _addLog('Sala cerrada (${payload['estado']})');
        _end('La sala se cerró (${payload['estado']}).');
      case 'error':
        _addLog('Error ${payload['codigo']}: ${payload['message']}');
        _notify();
      default:
        _addLog('Mensaje ${message.type}');
        _notify();
    }
  }

  void _failJoin(String message) {
    _code = null;
    _error = message;
    _phase = RoomPhase.idle;
    _notify();
  }

  void _end(String message) {
    _session++;
    _retryTimer?.cancel();
    _release();
    _token = null;
    _error = message;
    _phase = RoomPhase.closed;
    _notify();
  }

  void _addLog(String text) {
    _log.insert(0, RoomLogEntry(DateTime.now(), text));
    if (_log.length > maxLogEntries) _log.removeLast();
  }

  void _notify() {
    if (!_disposed) notifyListeners();
  }

  @override
  void dispose() {
    _disposed = true;
    _retryTimer?.cancel();
    _release();
    super.dispose();
  }
}
