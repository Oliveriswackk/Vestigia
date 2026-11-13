import 'dart:async';
import 'dart:io';

/// Conexión WebSocket con el backend. Interfaz mínima para poder sustituirla
/// en pruebas.
abstract interface class RoomChannel {
  /// Mensajes de texto que llegan; termina cuando la conexión se cierra.
  Stream<String> get messages;

  /// Código con el que se cerró la conexión; `null` si sigue abierta o se
  /// perdió sin código.
  int? get closeCode;

  void send(String text);

  Future<void> close();
}

typedef RoomChannelOpener = Future<RoomChannel> Function(Uri uri);

class IoRoomChannel implements RoomChannel {
  IoRoomChannel._(this._socket)
    : messages = _socket.where((event) => event is String).cast<String>();

  static Future<RoomChannel> open(Uri uri) async {
    final socket = await WebSocket.connect(
      uri.toString(),
    ).timeout(const Duration(seconds: 10));
    return IoRoomChannel._(socket);
  }

  final WebSocket _socket;

  @override
  final Stream<String> messages;

  @override
  int? get closeCode => _socket.closeCode;

  @override
  void send(String text) => _socket.add(text);

  @override
  Future<void> close() => _socket.close();
}
