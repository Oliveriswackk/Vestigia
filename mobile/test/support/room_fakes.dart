import 'dart:async';

import 'package:radio_coordenadas/core/room/backend_config.dart';
import 'package:radio_coordenadas/core/room/room_api.dart';
import 'package:radio_coordenadas/core/room/room_channel.dart';
import 'package:radio_coordenadas/core/room/room_connection.dart';
import 'package:radio_coordenadas/core/room/room_models.dart';

const sampleSnapshot = RoomSnapshot(
  scene: 'pueblo',
  gameConnected: true,
  puzzles: {'radio': 'bloqueado', 'photo': 'bloqueado'},
  discoveries: {},
);

class FakeRoomApi implements RoomApi {
  String token = 'token-app';
  Object? joinError;
  Object? stateError;
  RoomSnapshot snapshot = sampleSnapshot;
  Completer<String>? joinGate;
  final joined = <String>[];
  int stateCalls = 0;

  @override
  Future<String> join(String code) async {
    joined.add(code);
    final gate = joinGate;
    if (gate != null) return gate.future;
    final error = joinError;
    if (error != null) throw error;
    return token;
  }

  @override
  Future<RoomSnapshot> state(String code, String token) async {
    stateCalls++;
    final error = stateError;
    if (error != null) throw error;
    return snapshot;
  }
}

class FakeRoomChannel implements RoomChannel {
  final _controller = StreamController<String>();
  final sent = <String>[];
  bool closed = false;

  @override
  int? closeCode;

  @override
  Stream<String> get messages => _controller.stream;

  @override
  void send(String text) => sent.add(text);

  @override
  Future<void> close() async {
    closed = true;
    _controller.close();
  }

  void receive(String text) => _controller.add(text);

  void serverClose(int? code) {
    closeCode = code;
    _controller.close();
  }
}

class RoomHarness {
  RoomHarness() {
    connection = RoomConnection(
      config: const BackendConfig(baseUrl: 'https://example.test'),
      api: api,
      opener: (uri) async {
        opened.add(uri);
        final channel = FakeRoomChannel();
        channels.add(channel);
        return channel;
      },
      backoff: (_) => Duration.zero,
    );
  }

  final api = FakeRoomApi();
  final opened = <Uri>[];
  final channels = <FakeRoomChannel>[];
  late final RoomConnection connection;

  FakeRoomChannel get channel => channels.last;
}

String authOk([String rol = 'app', String sala = 'TAPALPA']) =>
    '{"type":"auth.ok","payload":{"rol":"$rol","sala":"$sala"}}';
