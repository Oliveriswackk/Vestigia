import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:radio_coordenadas/app/vestigia_app.dart';
import 'package:radio_coordenadas/menu/puzzle_entry.dart';

import 'support/room_fakes.dart';

PuzzleEntry _entry(String id) => PuzzleEntry(
  id: id,
  title: 'Puzzle $id',
  description: 'desc',
  icon: Icons.radio,
  builder: (_) => Scaffold(body: Center(child: Text('PANTALLA-$id'))),
);

const _radioAvailable =
    '{"type":"descubrimiento.nuevo","payload":{"clave":"abrir_radio","origen":"juego"}}';

void main() {
  testWidgets('el juego deja la radio disponible y la app abre su pantalla', (
    tester,
  ) async {
    final harness = RoomHarness();
    await tester.pumpWidget(
      VestigiaApp(puzzles: [_entry('radio')], room: harness.connection),
    );
    await harness.connection.join('TAPALPA');
    await tester.pump();
    harness.channel.receive(authOk());
    await tester.pump();

    expect(find.text('PANTALLA-radio'), findsNothing);

    harness.channel.receive(_radioAvailable);
    await tester.pumpAndSettle();

    expect(find.text('PANTALLA-radio'), findsOneWidget);

    // Un segundo aviso no apila otra pantalla encima.
    harness.channel.receive(_radioAvailable);
    await tester.pumpAndSettle();
    Navigator.of(tester.element(find.text('PANTALLA-radio'))).pop();
    await tester.pumpAndSettle();
    expect(find.text('PANTALLA-radio'), findsNothing);
  });

  testWidgets('un puzzle que la app no conoce se ignora', (tester) async {
    final harness = RoomHarness();
    await tester.pumpWidget(
      VestigiaApp(puzzles: [_entry('photo')], room: harness.connection),
    );
    await harness.connection.join('TAPALPA');
    await tester.pump();
    harness.channel.receive(authOk());
    await tester.pump();

    harness.channel.receive(_radioAvailable);
    await tester.pumpAndSettle();

    expect(find.textContaining('PANTALLA-'), findsNothing);
  });
}
