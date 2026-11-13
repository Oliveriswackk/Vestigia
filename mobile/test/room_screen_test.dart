import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:radio_coordenadas/app/vestigia_app.dart';
import 'package:radio_coordenadas/core/room/room_api.dart';
import 'package:radio_coordenadas/room/room_screen.dart';

import 'support/fakes.dart';
import 'support/room_fakes.dart';

Future<RoomHarness> openRoom(WidgetTester tester) async {
  usePhoneScreen(tester);
  final harness = RoomHarness();
  addTearDown(harness.connection.dispose);
  await tester.pumpWidget(VestigiaApp(room: harness.connection));
  await tester.tap(find.text('Sala'));
  await tester.pumpAndSettle();
  expect(find.byKey(RoomScreen.codeFieldKey), findsOneWidget);
  return harness;
}

void main() {
  testWidgets('el código se escribe en mayúsculas y sin espacios', (
    tester,
  ) async {
    await openRoom(tester);

    await tester.enterText(find.byKey(RoomScreen.codeFieldKey), ' tap alpa ');
    await tester.pump();

    final field = tester.widget<TextField>(find.byKey(RoomScreen.codeFieldKey));
    expect(field.controller!.text, 'TAPALPA');
  });

  testWidgets('unirse muestra la sala, la conexión y los mensajes', (
    tester,
  ) async {
    final harness = await openRoom(tester);

    await tester.enterText(find.byKey(RoomScreen.codeFieldKey), 'tapalpa');
    await tester.tap(find.byKey(RoomScreen.joinKey));
    await tester.pump();
    await tester.pump();

    expect(harness.api.joined, ['TAPALPA']);
    expect(find.text('TAPALPA'), findsOneWidget);
    expect(find.text('Conectando…'), findsOneWidget);

    harness.channel.receive(authOk());
    await tester.pumpAndSettle();

    expect(
      tester.widget<Text>(find.byKey(RoomScreen.statusKey)).data,
      'Conectada',
    );
    expect(find.text('Conectada'), findsNWidgets(2)); // app y PC (juego)
    expect(find.text('pueblo'), findsOneWidget);
    expect(find.text('radio'), findsOneWidget);
    expect(find.byKey(RoomScreen.logKey), findsOneWidget);
    expect(find.textContaining('Autenticada como app en TAPALPA'), findsOneWidget);
  });

  testWidgets('un código inexistente muestra el error y deja reintentar', (
    tester,
  ) async {
    final harness = await openRoom(tester);
    harness.api.joinError = const RoomApiException(404, 'no hay');

    await tester.enterText(find.byKey(RoomScreen.codeFieldKey), 'TULUM');
    await tester.tap(find.byKey(RoomScreen.joinKey));
    await tester.pumpAndSettle();

    expect(find.byKey(RoomScreen.errorKey), findsOneWidget);
    expect(find.textContaining('No encontramos una partida'), findsOneWidget);
    expect(find.byKey(RoomScreen.joinKey), findsOneWidget);
  });

  testWidgets('salir de la sala vuelve al formulario y cierra el canal', (
    tester,
  ) async {
    final harness = await openRoom(tester);
    await tester.enterText(find.byKey(RoomScreen.codeFieldKey), 'TAPALPA');
    await tester.tap(find.byKey(RoomScreen.joinKey));
    await tester.pump();
    await tester.pump();

    await tester.tap(find.byKey(RoomScreen.leaveKey));
    await tester.pumpAndSettle();

    expect(harness.channel.closed, isTrue);
    expect(find.byKey(RoomScreen.joinKey), findsOneWidget);
  });
}
