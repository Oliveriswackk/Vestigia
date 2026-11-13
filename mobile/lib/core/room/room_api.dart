import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'backend_config.dart';
import 'room_models.dart';

/// El servidor respondió con un error (`status` HTTP).
class RoomApiException implements Exception {
  const RoomApiException(this.status, this.message, {this.code});

  final int status;
  final String message;
  final String? code;

  @override
  String toString() => 'RoomApiException($status, $message)';
}

/// No hubo respuesta: sin red, servidor apagado o tiempo agotado.
class NoConnectionException implements Exception {
  const NoConnectionException();
}

/// Peticiones REST de la app a la sala.
abstract interface class RoomApi {
  /// `POST /salas/{codigo}/unirse`. Devuelve el token, que el servidor
  /// entrega una sola vez.
  Future<String> join(String code);

  /// `GET /salas/{codigo}/estado`.
  Future<RoomSnapshot> state(String code, String token);
}

class HttpRoomApi implements RoomApi {
  HttpRoomApi(this.config);

  static const _timeout = Duration(seconds: 10);

  final BackendConfig config;
  HttpClient? _httpClient;

  HttpClient get _client =>
      _httpClient ??= HttpClient()..connectionTimeout = _timeout;

  @override
  Future<String> join(String code) async {
    final body = await _send('POST', '/api/v1/salas/${Uri.encodeComponent(code)}/unirse');
    final token = body['token'];
    if (token is! String || token.isEmpty) {
      throw const RoomApiException(200, 'La respuesta no trae token.');
    }
    return token;
  }

  @override
  Future<RoomSnapshot> state(String code, String token) async {
    final body = await _send(
      'GET',
      '/api/v1/salas/${Uri.encodeComponent(code)}/estado',
      token: token,
    );
    return RoomSnapshot.fromJson(body);
  }

  Future<Map<String, dynamic>> _send(
    String method,
    String path, {
    String? token,
  }) async {
    final int status;
    final String text;
    try {
      final request = await _client
          .openUrl(method, config.rest(path))
          .timeout(_timeout);
      request.headers.set(HttpHeaders.acceptHeader, 'application/json');
      if (token != null) {
        request.headers.set(HttpHeaders.authorizationHeader, 'Bearer $token');
      }
      final response = await request.close().timeout(_timeout);
      status = response.statusCode;
      text = await response.transform(utf8.decoder).join().timeout(_timeout);
    } on SocketException {
      throw const NoConnectionException();
    } on TimeoutException {
      throw const NoConnectionException();
    } on HttpException {
      throw const NoConnectionException();
    } on HandshakeException {
      throw const NoConnectionException();
    }

    Map<String, dynamic>? json;
    try {
      final decoded = jsonDecode(text);
      if (decoded is Map<String, dynamic>) json = decoded;
    } on FormatException {
      json = null;
    }

    if (status >= 400) {
      final message = json?['message'];
      final code = json?['codigo'];
      throw RoomApiException(
        status,
        message is String ? message : 'Error $status del servidor.',
        code: code is String ? code : null,
      );
    }
    if (json == null) {
      throw RoomApiException(status, 'Respuesta inesperada del servidor.');
    }
    return json;
  }
}
