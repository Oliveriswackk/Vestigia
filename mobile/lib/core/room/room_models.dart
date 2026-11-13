import 'dart:convert';

/// Quita todos los espacios y pasa a mayúsculas: `" tapalpa "` y `"TAPALPA"`
/// son la misma sala.
String normalizeRoomCode(String raw) =>
    raw.replaceAll(RegExp(r'\s+'), '').toUpperCase();

/// Mensaje del WebSocket: `{"type": "...", "payload": {...}}`.
class RoomMessage {
  const RoomMessage(this.type, this.payload);

  final String type;
  final Map<String, dynamic> payload;

  static RoomMessage? tryParse(String text) {
    final Object? json;
    try {
      json = jsonDecode(text);
    } on FormatException {
      return null;
    }
    if (json is! Map<String, dynamic>) return null;
    final type = json['type'];
    if (type is! String) return null;
    final payload = json['payload'];
    return RoomMessage(
      type,
      payload is Map<String, dynamic> ? payload : const {},
    );
  }
}

/// Respuesta de `GET /api/v1/salas/{codigo}/estado`, solo con lo que la
/// pantalla de pruebas muestra.
class RoomSnapshot {
  const RoomSnapshot({
    required this.scene,
    required this.gameConnected,
    required this.puzzles,
    required this.discoveries,
  });

  factory RoomSnapshot.fromJson(Map<String, dynamic> json) {
    final puzzles = <String, String>{};
    final rawPuzzles = json['puzzles'];
    if (rawPuzzles is List) {
      for (final item in rawPuzzles) {
        if (item is Map && item['puzzle'] is String && item['estado'] is String) {
          puzzles[item['puzzle'] as String] = item['estado'] as String;
        }
      }
    }
    final discoveries = <String, String>{};
    final rawDiscoveries = json['descubrimientos'];
    if (rawDiscoveries is List) {
      for (final item in rawDiscoveries) {
        if (item is Map && item['clave'] is String) {
          final data = item['datos'];
          final value = data is Map ? data['valor'] : null;
          discoveries[item['clave'] as String] = value is String ? value : '';
        }
      }
    }
    final devices = json['dispositivos'];
    final scene = json['escena_actual'];
    return RoomSnapshot(
      scene: scene is String ? scene : '—',
      gameConnected: devices is Map && devices['juego'] == true,
      puzzles: puzzles,
      discoveries: discoveries,
    );
  }

  final String scene;
  final bool gameConnected;

  /// Estado de cada puzzle por id: bloqueado, disponible, en_curso o resuelto.
  final Map<String, String> puzzles;

  /// Valor (texto) de cada descubrimiento por clave; vacío si no trae `valor`.
  final Map<String, String> discoveries;
}
