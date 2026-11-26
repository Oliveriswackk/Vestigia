import 'package:flutter/material.dart';

/// Abre un puzzle como si la ficha de cartón se levantara de la libreta:
/// llega inclinada en perspectiva, gira hasta quedar de frente y asienta
/// con un pequeño rebote. Al cerrarse hace el camino inverso.
Route<void> puzzleRoute(WidgetBuilder builder) => PageRouteBuilder<void>(
  transitionDuration: const Duration(milliseconds: 750),
  reverseTransitionDuration: const Duration(milliseconds: 550),
  pageBuilder: (context, animation, secondary) => builder(context),
  transitionsBuilder: (context, animation, secondary, child) {
    final motion = CurvedAnimation(
      parent: animation,
      curve: Curves.easeOutBack,
      reverseCurve: Curves.easeInCubic,
    );
    final fade = CurvedAnimation(
      parent: animation,
      curve: const Interval(0, 0.55, curve: Curves.easeOut),
    );
    return FadeTransition(
      opacity: fade,
      child: AnimatedBuilder(
        animation: motion,
        child: child,
        builder: (context, child) {
          final t = motion.value;
          final rest = 1 - t;
          final transform = Matrix4.identity()
            ..setEntry(3, 2, 0.0012)
            ..translateByDouble(0, rest * 90, 0, 1)
            ..rotateX(rest * 0.55)
            ..rotateZ(rest * -0.07)
            ..scaleByDouble(0.82 + 0.18 * t, 0.82 + 0.18 * t, 1, 1);
          return Transform(
            alignment: Alignment.center,
            transform: transform,
            child: child,
          );
        },
      ),
    );
  },
);
