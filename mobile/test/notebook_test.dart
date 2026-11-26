import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:radio_coordenadas/app/vestigia_app.dart';
import 'package:radio_coordenadas/core/room/room_api.dart';
import 'package:radio_coordenadas/core/room/room_models.dart';
import 'package:radio_coordenadas/menu/puzzle_entry.dart';
import 'package:radio_coordenadas/notebook/cover_screen.dart';
import 'package:radio_coordenadas/notebook/notebook_entries.dart';
import 'package:radio_coordenadas/notebook/notebook_screen.dart';

import 'support/fakes.dart';
import 'support/room_fakes.dart';

RoomSnapshot snapshot({
  String scene = 'pueblo',
  Map<String, String> puzzles = const {},
  Map<String, String> discoveries = const {},
}) => RoomSnapshot(
  scene: scene,
  gameConnected: true,
  puzzles: puzzles,
  discoveries: discoveries,
);

PuzzleEntry _entry(String id) => PuzzleEntry(
  id: id,
  title: id,
  description: 'desc',
  icon: Icons.radio,
  builder: (_) => Scaffold(body: Center(child: Text('PANTALLA-$id'))),
);

void main() {
  group('buildNotebook', () {
    test('sin estado no hay nada', () {
      expect(buildNotebook(null), isEmpty);
    });

    test(
      'los puzzles bloqueados no aparecen y las marcas internas se ocultan',
      () {
        final items = buildNotebook(
          snapshot(
            puzzles: {'radio': 'bloqueado', 'photo': 'bloqueado'},
            discoveries: {'marca_interna': ''},
          ),
        );
        expect(items, isEmpty);
      },
    );

    test('la ficha de la radio va justo después de la nota del pescador', () {
      final items = buildNotebook(
        snapshot(
          puzzles: {'radio': 'resuelto'},
          discoveries: {'pescador_dialogo_completo': '', 'coordenada_a': '137'},
        ),
      );
      expect(items.map((i) => i.id), [
        'pescador_dialogo_completo',
        'puzzle_radio',
        'coordenada_a',
      ]);
      final coordinate = items.last as CoordinateItem;
      expect(coordinate.value, '137°');
      expect((items[1] as PuzzleItem).state, 'resuelto');
    });

    test('las fotos aparecen una a una y la ficha tras la última', () {
      final items = buildNotebook(
        snapshot(
          puzzles: {'photo': 'disponible'},
          discoveries: {'pieza_2': '', 'pieza_1': '', 'pieza_3': ''},
        ),
      );
      expect(items.map((i) => i.id), [
        'pieza_2',
        'pieza_1',
        'pieza_3',
        'puzzle_photo',
      ]);
      expect((items.first as PhotoPieceItem).piece, 1);
    });

    test('la llave del faro deja nota y abrir_* no se escribe', () {
      final items = buildNotebook(
        snapshot(discoveries: {'llave_faro': '', 'abrir_photo': ''}),
      );
      expect(items.map((i) => i.id), ['llave_faro']);
      expect((items.single as NoteItem).text, contains('llave'));
    });

    test('un puzzle disponible sin su descubrimiento va al final', () {
      final items = buildNotebook(
        snapshot(
          puzzles: {'observatory': 'disponible'},
          discoveries: {'coordenada_a': '137'},
        ),
      );
      expect(items.map((i) => i.id), ['coordenada_a', 'puzzle_observatory']);
    });
  });

  group('libreta', () {
    testWidgets('la tapa pide el pueblo y al unirse se abre la libreta', (
      tester,
    ) async {
      usePhoneScreen(tester);
      final harness = RoomHarness();
      harness.api.snapshot = snapshot(
        puzzles: {'radio': 'disponible'},
        discoveries: {'pescador_dialogo_completo': ''},
      );
      await tester.pumpWidget(
        VestigiaApp(puzzles: [_entry('radio')], room: harness.connection),
      );

      await tester.enterText(find.byKey(CoverScreen.codeFieldKey), 'aculco');
      expect(find.text('ACULCO'), findsOneWidget);
      await tester.tap(find.byKey(CoverScreen.openKey));
      await tester.pump();
      harness.channel.receive(authOk());
      await tester.pumpAndSettle();

      expect(find.byType(NotebookScreen), findsOneWidget);
      expect(find.byKey(const Key('notebook-town')), findsOneWidget);
      expect(find.text('ACULCO'), findsOneWidget);
      expect(
        find.textContaining('El pueblo', findRichText: true),
        findsOneWidget,
      );
      expect(find.textContaining('El pescador dice'), findsOneWidget);
      expect(find.text('PENDIENTE'), findsOneWidget);

      final card = find.byKey(const ValueKey('puzzle-card-radio'));
      await tester.ensureVisible(card);
      await tester.pumpAndSettle();
      await tester.tap(card);
      await tester.pumpAndSettle();
      expect(find.text('PANTALLA-radio'), findsOneWidget);
    });

    testWidgets('tocar cualquier parte de la etiqueta enfoca el campo', (
      tester,
    ) async {
      usePhoneScreen(tester);
      final harness = RoomHarness();
      await tester.pumpWidget(
        VestigiaApp(puzzles: [_entry('radio')], room: harness.connection),
      );
      // Toca sobre el rótulo «PUEBLO», fuera del renglón.
      await tester.tap(find.text('PUEBLO'));
      await tester.pump();
      final field = tester.widget<EditableText>(find.byType(EditableText));
      expect(field.focusNode.hasFocus, isTrue);
    });

    testWidgets('un error al unirse se escribe en la tapa', (tester) async {
      usePhoneScreen(tester);
      final harness = RoomHarness();
      harness.api.joinError = const NoConnectionException();
      await tester.pumpWidget(
        VestigiaApp(puzzles: [_entry('radio')], room: harness.connection),
      );

      await tester.enterText(find.byKey(CoverScreen.codeFieldKey), 'ACULCO');
      await tester.tap(find.byKey(CoverScreen.openKey));
      await tester.pumpAndSettle();

      expect(find.byType(NotebookScreen), findsNothing);
      expect(find.byKey(const Key('cover-error')), findsOneWidget);
    });

    testWidgets('cerrar la libreta vuelve a la tapa', (tester) async {
      usePhoneScreen(tester);
      final harness = RoomHarness();
      await tester.pumpWidget(
        VestigiaApp(puzzles: [_entry('radio')], room: harness.connection),
      );
      await harness.connection.join('ACULCO');
      await tester.pump();
      harness.channel.receive(authOk());
      await tester.pumpAndSettle();

      await tester.scrollUntilVisible(find.byKey(NotebookScreen.closeKey), 200);
      await tester.tap(find.byKey(NotebookScreen.closeKey));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Cerrar'));
      await tester.pumpAndSettle();

      expect(find.byType(CoverScreen), findsOneWidget);
    });
  });
}
