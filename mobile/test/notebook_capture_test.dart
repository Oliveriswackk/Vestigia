// Capturas de la libreta para revisión visual. Solo genera imágenes con
// `flutter test test/notebook_capture_test.dart --dart-define=VESTIGIA_SCREENSHOTS=true`
// (quedan en build/verification/); sin esa bandera, solo comprueba que todo
// se construye.
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:radio_coordenadas/app/notebook_style.dart';
import 'package:radio_coordenadas/app/vestigia_theme.dart';
import 'package:radio_coordenadas/core/room/room_models.dart';
import 'package:radio_coordenadas/menu/puzzle_entry.dart';
import 'package:radio_coordenadas/notebook/cover_screen.dart';
import 'package:radio_coordenadas/notebook/notebook_screen.dart';

import 'support/fakes.dart';
import 'support/room_fakes.dart';
import 'support/visual_verification.dart';

Future<void> _precache(WidgetTester tester) async {
  await tester.runAsync(() async {
    final context = tester.element(find.byType(Scaffold).first);
    for (final asset in [
      NotebookAssets.paper,
      NotebookAssets.cardboard,
      NotebookAssets.cover,
      'assets/photos/foto_provisional.jpg',
    ]) {
      await precacheImage(AssetImage(asset), context);
    }
  });
  await tester.pumpAndSettle();
}

const _boundary = Key('capture');

Widget _frame(Widget child) => MaterialApp(
  debugShowCheckedModeBanner: false,
  theme: buildVestigiaTheme(),
  home: RepaintBoundary(key: _boundary, child: child),
);

void main() {
  setUpAll(() async {
    await loadVerificationFonts();
  });

  testWidgets('tapa', (tester) async {
    usePhoneScreen(tester);
    final harness = RoomHarness();
    await tester.pumpWidget(
      _frame(CoverScreen(room: harness.connection, puzzles: const [])),
    );
    await _precache(tester);
    await captureScreen(tester, find.byKey(_boundary), 'libreta_1_tapa');
  });

  for (final (name, scene, puzzles, discoveries) in [
    (
      'libreta_2_pueblo',
      'pueblo',
      {'radio': 'disponible'},
      {'pescador_dialogo_completo': ''},
    ),
    (
      'libreta_3_faro',
      'faro',
      {'radio': 'resuelto', 'photo': 'en_curso'},
      {
        'pescador_dialogo_completo': '',
        'coordenada_a': '137',
        'pieza_1': '',
        'pieza_2': '',
        'pieza_3': '',
      },
    ),
  ]) {
    testWidgets(name, (tester) async {
      usePhoneScreen(tester);
      final harness = RoomHarness();
      harness.api.snapshot = RoomSnapshot(
        scene: scene,
        gameConnected: true,
        puzzles: puzzles,
        discoveries: discoveries,
      );
      await harness.connection.join('ACULCO');
      await tester.pump();
      harness.channel.receive(authOk());
      await tester.pumpWidget(
        _frame(
          NotebookScreen(
            room: harness.connection,
            puzzles: const <PuzzleEntry>[],
          ),
        ),
      );
      await _precache(tester);
      await captureScreen(tester, find.byKey(_boundary), name);
    });
  }
}
