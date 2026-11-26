import 'package:flutter/widgets.dart';

/// Tiempo que se deja ver el puzzle resuelto antes de cerrarlo solo.
const puzzleAutoCloseDelay = Duration(milliseconds: 2600);

/// En una partida el puzzle no se repite: al resolverse se avisa al juego y
/// la pantalla vuelve sola a la libreta tras [puzzleAutoCloseDelay].
/// Fuera de partida (modo práctica) no hace nada.
void closePuzzleWhenSolved(State state, {required bool inRoom}) {
  if (!inRoom) return;
  Future<void>.delayed(puzzleAutoCloseDelay, () {
    if (state.mounted) Navigator.of(state.context).maybePop();
  });
}
