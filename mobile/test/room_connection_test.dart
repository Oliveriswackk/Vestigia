import 'dart:async';
import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:radio_coordenadas/core/room/room_api.dart';
import 'package:radio_coordenadas/core/room/room_connection.dart';
import 'package:radio_coordenadas/core/room/room_models.dart';

import 'support/room_fakes.dart';

Future<RoomHarness> joined() async {
  final harness = RoomHarness();
  await harness.connection.join('TAPALPA');
  await pumpEventQueue();
  return harness;
}

void main() {
  test('normaliza el código: sin espacios y en mayúsculas', () {
    expect(normalizeRoomCode(' tap alpa\n'), 'TAPALPA');
  });

  test('unirse abre el WebSocket y manda auth con el token', () async {
    final harness = await joined();

    expect(harness.api.joined, ['TAPALPA']);
    expect(harness.opened, [Uri.parse('wss://example.test/ws')]);
    expect(jsonDecode(harness.channel.sent.single), {
      'type': 'auth',
      'payload': {'token': 'token-app'},
    });
    expect(harness.connection.phase, RoomPhase.connecting);
  });

  test('auth.ok conecta y pide el estado', () async {
    final harness = await joined();
    harness.channel.receive(authOk());
    await pumpEventQueue();

    expect(harness.connection.phase, RoomPhase.connected);
    expect(harness.api.stateCalls, 1);
    expect(harness.connection.snapshot?.scene, 'pueblo');
    expect(harness.connection.gameConnected, isTrue);
  });

  test('un código vacío no llama al servidor', () async {
    final harness = RoomHarness();
    await harness.connection.join('   ');

    expect(harness.api.joined, isEmpty);
    expect(harness.connection.phase, RoomPhase.idle);
    expect(harness.connection.error, isNotNull);
  });

  test('404, 409 y falta de red dan un mensaje claro', () async {
    final harness = RoomHarness();

    harness.api.joinError = const RoomApiException(404, 'no hay');
    await harness.connection.join('TULUM');
    expect(harness.connection.phase, RoomPhase.idle);
    expect(harness.connection.error, contains('No encontramos una partida'));

    harness.api.joinError = const RoomApiException(409, 'ocupada');
    await harness.connection.join('TULUM');
    expect(harness.connection.error, contains('ya tiene un teléfono'));

    harness.api.joinError = const NoConnectionException();
    await harness.connection.join('TULUM');
    expect(harness.connection.error, contains('No hay conexión'));
    expect(harness.opened, isEmpty);
  });

  test('presencia del juego y avisos que piden el estado de nuevo', () async {
    final harness = await joined();
    harness.channel.receive(authOk());
    await pumpEventQueue();

    harness.channel.receive(
      '{"type":"dispositivo.desconectado","payload":{"rol":"juego"}}',
    );
    await pumpEventQueue();
    expect(harness.connection.gameConnected, isFalse);

    harness.channel.receive(
      '{"type":"dispositivo.conectado","payload":{"rol":"juego"}}',
    );
    await pumpEventQueue();
    expect(harness.connection.gameConnected, isTrue);

    final before = harness.api.stateCalls;
    harness.channel.receive(
      '{"type":"puzzle.actualizado","payload":{"puzzle":"radio","estado":"disponible","datos":{}}}',
    );
    await pumpEventQueue();
    expect(harness.api.stateCalls, before + 1);
    expect(harness.connection.log.first.text, 'Puzzle radio: disponible');
  });

  test('sala.cerrada termina la sesión sin reconectar', () async {
    final harness = await joined();
    harness.channel.receive(authOk());
    harness.channel.receive(
      '{"type":"sala.cerrada","payload":{"estado":"finalizada"}}',
    );
    await pumpEventQueue();

    expect(harness.connection.phase, RoomPhase.closed);
    expect(harness.connection.error, contains('finalizada'));
    expect(harness.channel.closed, isTrue);
    expect(harness.opened, hasLength(1));
  });

  test('4001 termina la sesión; no se reconecta', () async {
    final harness = await joined();
    harness.channel.serverClose(4001);
    await pumpEventQueue();

    expect(harness.connection.phase, RoomPhase.closed);
    expect(harness.opened, hasLength(1));
  });

  test('una caída sin código reconecta y vuelve a mandar auth', () async {
    final harness = await joined();
    harness.channel.receive(authOk());
    await pumpEventQueue();

    harness.channel.serverClose(null);
    await pumpEventQueue();

    expect(harness.opened, hasLength(2));
    expect(jsonDecode(harness.channel.sent.single)['type'], 'auth');
    harness.channel.receive(authOk());
    await pumpEventQueue();
    expect(harness.connection.phase, RoomPhase.connected);
  });

  test('un 401 al pedir el estado cierra la sesión', () async {
    final harness = await joined();
    harness.api.stateError = const RoomApiException(401, 'sin sala');
    harness.channel.receive(authOk());
    await pumpEventQueue();

    expect(harness.connection.phase, RoomPhase.closed);
    expect(harness.channel.closed, isTrue);
  });

  test('salir cierra el canal y descarta un unirse pendiente', () async {
    final harness = await joined();
    harness.connection.leave();
    expect(harness.connection.phase, RoomPhase.idle);
    expect(harness.channel.closed, isTrue);

    final pending = RoomHarness();
    final gate = Completer<String>();
    pending.api.joinGate = gate;
    final future = pending.connection.join('TAPALPA');
    expect(pending.connection.phase, RoomPhase.joining);
    pending.connection.leave();
    gate.complete('tarde');
    await future;
    await pumpEventQueue();

    expect(pending.connection.phase, RoomPhase.idle);
    expect(pending.opened, isEmpty);
  });

  test('qué códigos de cierre reconectan', () {
    for (final code in [1000, 4000, 4001, 4009, 1009]) {
      expect(closeActionFor(code), CloseAction.endSession, reason: '$code');
    }
    for (final code in [null, 4008, 4500, 1006]) {
      expect(closeActionFor(code), CloseAction.reconnect, reason: '$code');
    }
  });

  test('el mensaje ilegible o sin tipo no rompe nada', () {
    expect(RoomMessage.tryParse('no es json'), isNull);
    expect(RoomMessage.tryParse('{"payload":{}}'), isNull);
    expect(RoomMessage.tryParse('{"type":"x"}')?.payload, isEmpty);
  });
}
