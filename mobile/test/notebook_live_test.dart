import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:radio_coordenadas/app/notebook_style.dart';
import 'package:radio_coordenadas/core/haptics.dart';
import 'package:radio_coordenadas/core/room/room_models.dart';
import 'package:radio_coordenadas/notebook/notebook_screen.dart';

import 'support/fakes.dart';
import 'support/room_fakes.dart';

RoomSnapshot _snapshot(Map<String, String> discoveries) => RoomSnapshot(
  scene: 'pueblo',
  gameConnected: true,
  puzzles: const {'radio': 'resuelto'},
  discoveries: discoveries,
);

String _discovery(String key) =>
    '{"type":"descubrimiento.nuevo","payload":{"clave":"$key","origen":"juego"}}';

/// Una libreta ya bastante llena, para que lo nuevo quede fuera de la vista.
const _full = {
  'pescador_dialogo_completo': '',
  'pieza_1': '',
  'pieza_2': '',
  'llave_faro': '',
};

Future<(RoomHarness, FakeHaptics)> _open(WidgetTester tester) async {
  usePhoneScreen(tester);
  final harness = RoomHarness();
  final haptics = FakeHaptics();
  harness.api.snapshot = _snapshot(_full);
  await harness.connection.join('TAPALPA');
  harness.channel.receive(authOk());
  await tester.pumpWidget(
    MaterialApp(
      navigatorObservers: [notebookRouteObserver],
      home: NotebookScreen(
        room: harness.connection,
        puzzles: const [],
        haptics: haptics,
      ),
    ),
  );
  await tester.pumpAndSettle();
  return (harness, haptics);
}

Future<void> _arrive(
  WidgetTester tester,
  RoomHarness harness,
  String key,
  String value,
) async {
  harness.api.snapshot = _snapshot({..._full, key: value});
  harness.channel.receive(_discovery(key));
  await tester.pumpAndSettle();
}

void main() {
  testWidgets('al abrir la libreta no vibra; lo que llega después sí', (
    tester,
  ) async {
    final (harness, haptics) = await _open(tester);
    expect(haptics.cues, isEmpty);

    await _arrive(tester, harness, 'coordenada_a', '137');
    expect(haptics.cues, [HapticCue.notebookMilestone]);
  });

  testWidgets('lo nuevo se desplaza hasta quedar a la vista', (tester) async {
    final (harness, _) = await _open(tester);
    await _arrive(tester, harness, 'coordenada_a', '137');

    final rect = tester.getRect(
      find.byKey(const ValueKey('coordinate-coordenada_a')),
    );
    expect(rect.top, greaterThanOrEqualTo(0));
    expect(rect.bottom, lessThanOrEqualTo(800));
  });

  testWidgets('al hacer scroll las notas no se vuelven a animar', (
    tester,
  ) async {
    await _open(tester);
    State first() => tester.state(find.byType(AppearOnce).first);
    final before = first();

    await tester.drag(find.byType(Scrollable), const Offset(0, -2000));
    await tester.pumpAndSettle();
    await tester.drag(find.byType(Scrollable), const Offset(0, 2000));
    await tester.pumpAndSettle();

    expect(identical(first(), before), isTrue);
    expect(tester.hasRunningAnimations, isFalse);
  });

  testWidgets('con un puzzle abierto, lo nuevo espera a que se cierre', (
    tester,
  ) async {
    final (harness, haptics) = await _open(tester);
    final navigator = tester.state<NavigatorState>(find.byType(Navigator));
    navigator.push(
      MaterialPageRoute<void>(builder: (_) => const Scaffold(body: Text('P'))),
    );
    await tester.pumpAndSettle();

    await _arrive(tester, harness, 'coordenada_a', '137');
    expect(haptics.cues, isEmpty);

    navigator.pop();
    await tester.pumpAndSettle();
    expect(haptics.cues, [HapticCue.notebookMilestone]);
  });
}
