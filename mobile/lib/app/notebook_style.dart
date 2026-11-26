import 'dart:math';

import 'package:flutter/material.dart';

/// Materiales de la libreta de campo: papel, cartón, tapa, tinta y cinta.
/// Las texturas son CC0 (ver `assets/CREDITS.md`); si una no carga, cada
/// superficie conserva un color de respaldo parecido.
abstract final class NotebookAssets {
  static const paper = 'assets/textures/paper.jpg';
  static const cardboard = 'assets/textures/cardboard.jpg';
  static const cover = 'assets/textures/cover.jpg';
}

abstract final class NotebookFonts {
  /// Letra a mano para lo que anota la protagonista.
  static const hand = 'Caveat';

  /// Máquina de escribir para rótulos, sellos y fichas.
  static const typewriter = 'SpecialElite';
}

abstract final class NotebookColors {
  static const paper = Color(0xFFEFE4C8);
  static const ink = Color(0xFF26303F);
  static const inkFaded = Color(0xFF5B6372);
  static const pencil = Color(0xFF6E6A62);
  static const rule = Color(0x405C7FA8);
  static const margin = Color(0x80C0504D);
  static const redInk = Color(0xFFA8382C);
  static const blueInk = Color(0xFF2F4A7A);
  static const greenInk = Color(0xFF3E6B3A);
  static const cardboard = Color(0xFFB08A5A);
  static const cardboardInk = Color(0xFF3B2A18);
  static const cover = Color(0xFF2A2119);
  static const coverInk = Color(0xFFE6D6AE);
  static const tape = Color(0xC8E8DCB5);
}

DecorationImage _texture(String asset, {double opacity = 1}) => DecorationImage(
  image: AssetImage(asset),
  repeat: ImageRepeat.repeat,
  scale: 2,
  opacity: opacity,
);

/// Hoja de la libreta: papel, renglones y la línea roja del margen.
class PaperBackground extends StatelessWidget {
  const PaperBackground({
    super.key,
    required this.child,
    this.ruled = true,
    this.marginX = 44,
    this.lineGap = 30,
  });

  final Widget child;
  final bool ruled;
  final double marginX;
  final double lineGap;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: NotebookColors.paper,
        image: _texture(NotebookAssets.paper),
      ),
      child: CustomPaint(
        painter: ruled
            ? _RulesPainter(marginX: marginX, lineGap: lineGap)
            : null,
        child: child,
      ),
    );
  }
}

class _RulesPainter extends CustomPainter {
  const _RulesPainter({required this.marginX, required this.lineGap});

  final double marginX;
  final double lineGap;

  @override
  void paint(Canvas canvas, Size size) {
    final rule = Paint()
      ..color = NotebookColors.rule
      ..strokeWidth = 1;
    for (var y = lineGap * 3; y < size.height; y += lineGap) {
      canvas.drawLine(Offset(0, y), Offset(size.width, y), rule);
    }
    final margin = Paint()
      ..color = NotebookColors.margin
      ..strokeWidth = 1.4;
    canvas.drawLine(Offset(marginX, 0), Offset(marginX, size.height), margin);
  }

  @override
  bool shouldRepaint(_RulesPainter old) =>
      old.marginX != marginX || old.lineGap != lineGap;
}

/// Fondo de cartón para las pantallas de los puzzles.
class CardboardBackground extends StatelessWidget {
  const CardboardBackground({
    super.key,
    required this.child,
    this.shade = 0.35,
  });

  final Widget child;

  /// Oscurece el cartón para que los instrumentos resalten (0 = sin velo).
  final double shade;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: NotebookColors.cardboard,
        image: _texture(NotebookAssets.cardboard),
      ),
      child: DecoratedBox(
        decoration: BoxDecoration(
          gradient: RadialGradient(
            radius: 1.1,
            colors: [
              Colors.black.withValues(alpha: shade * 0.4),
              Colors.black.withValues(alpha: shade),
            ],
          ),
        ),
        child: child,
      ),
    );
  }
}

/// Tapa de la libreta (portada donde se escribe el pueblo).
class CoverBackground extends StatelessWidget {
  const CoverBackground({super.key, required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: NotebookColors.cover,
        image: _texture(NotebookAssets.cover),
      ),
      child: DecoratedBox(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.centerLeft,
            end: Alignment.centerRight,
            stops: [0, 0.06, 0.08, 1],
            colors: [
              Color(0x99000000),
              Color(0x33000000),
              Color(0x00000000),
              Color(0x22000000),
            ],
          ),
        ),
        child: child,
      ),
    );
  }
}

/// Ficha de cartón recortado, con sombra y esquinas gastadas.
class CardboardCard extends StatelessWidget {
  const CardboardCard({
    super.key,
    required this.child,
    this.onTap,
    this.angle = 0,
    this.padding = const EdgeInsets.fromLTRB(16, 14, 14, 14),
  });

  final Widget child;
  final VoidCallback? onTap;
  final double angle;
  final EdgeInsetsGeometry padding;

  @override
  Widget build(BuildContext context) {
    return Transform.rotate(
      angle: angle,
      child: DecoratedBox(
        decoration: BoxDecoration(
          color: NotebookColors.cardboard,
          image: _texture(NotebookAssets.cardboard),
          borderRadius: BorderRadius.circular(3),
          border: Border.all(color: const Color(0x553B2A18)),
          boxShadow: const [
            BoxShadow(
              color: Color(0x55000000),
              blurRadius: 6,
              offset: Offset(1, 3),
            ),
          ],
        ),
        child: Material(
          type: MaterialType.transparency,
          child: InkWell(
            onTap: onTap,
            splashColor: const Color(0x223B2A18),
            child: Padding(padding: padding, child: child),
          ),
        ),
      ),
    );
  }
}

/// Tira de cinta de papel con los extremos rasgados.
class TapeStrip extends StatelessWidget {
  const TapeStrip({
    super.key,
    this.width = 74,
    this.height = 24,
    this.angle = 0,
    this.seed = 1,
  });

  final double width;
  final double height;
  final double angle;
  final int seed;

  @override
  Widget build(BuildContext context) {
    return Transform.rotate(
      angle: angle,
      child: CustomPaint(
        size: Size(width, height),
        painter: _TapePainter(seed),
      ),
    );
  }
}

class _TapePainter extends CustomPainter {
  const _TapePainter(this.seed);

  final int seed;

  @override
  void paint(Canvas canvas, Size size) {
    final random = Random(seed);
    const teeth = 6;
    final path = Path()..moveTo(0, 0);
    path.lineTo(size.width, 0);
    for (var i = 1; i <= teeth; i++) {
      final y = size.height * i / teeth;
      final x = size.width - (i.isOdd ? 2.5 + random.nextDouble() * 2 : 0);
      path.lineTo(x, y);
    }
    path.lineTo(0, size.height);
    for (var i = teeth - 1; i >= 0; i--) {
      final y = size.height * i / teeth;
      final x = i.isOdd ? 2.5 + random.nextDouble() * 2 : 0.0;
      path.lineTo(x, y);
    }
    path.close();
    canvas.drawShadow(path, const Color(0x66000000), 1.5, false);
    canvas.drawPath(path, Paint()..color = NotebookColors.tape);
    final fiber = Paint()
      ..color = const Color(0x18000000)
      ..strokeWidth = 0.7;
    for (var i = 0; i < 9; i++) {
      final y = random.nextDouble() * size.height;
      canvas.drawLine(Offset(4, y), Offset(size.width - 4, y), fiber);
    }
  }

  @override
  bool shouldRepaint(_TapePainter old) => old.seed != seed;
}

/// Sello de tinta inclinado, como los de una oficina antigua.
class InkStamp extends StatelessWidget {
  const InkStamp({
    super.key,
    required this.text,
    required this.color,
    this.angle = -0.12,
  });

  final String text;
  final Color color;
  final double angle;

  @override
  Widget build(BuildContext context) {
    return Transform.rotate(
      angle: angle,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
        decoration: BoxDecoration(
          border: Border.all(color: color.withValues(alpha: 0.85), width: 2),
          borderRadius: BorderRadius.circular(3),
        ),
        child: Text(
          text,
          style: TextStyle(
            fontFamily: NotebookFonts.typewriter,
            fontSize: 12,
            letterSpacing: 1.5,
            color: color.withValues(alpha: 0.9),
          ),
        ),
      ),
    );
  }
}

/// La primera vez que se construye, el elemento «se pega» en la hoja:
/// cae desde un poco más arriba, ladeado y más grande, y asienta con un
/// leve rebote. Así lo nuevo se nota al llegar.
class AppearOnce extends StatefulWidget {
  const AppearOnce({
    super.key,
    required this.child,
    this.delay = Duration.zero,
    this.show = true,
  });

  final Widget child;

  /// Mientras sea falso queda oculto; al volverse verdadero, entra.
  final bool show;

  /// Espera antes de empezar (para escalonar varias entradas).
  final Duration delay;

  @override
  State<AppearOnce> createState() => _AppearOnceState();
}

class _AppearOnceState extends State<AppearOnce>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller = AnimationController(
    vsync: this,
    duration: const Duration(milliseconds: 820),
  );

  @override
  void initState() {
    super.initState();
    if (widget.show) _play();
  }

  @override
  void didUpdateWidget(AppearOnce old) {
    super.didUpdateWidget(old);
    if (widget.show && !old.show) _play();
  }

  void _play() {
    if (widget.delay == Duration.zero) {
      _controller.forward();
    } else {
      Future<void>.delayed(widget.delay, () {
        if (mounted) _controller.forward();
      });
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final fade = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0, 0.45, curve: Curves.easeOut),
    );
    final settle = CurvedAnimation(
      parent: _controller,
      curve: Curves.easeOutBack,
    );
    return FadeTransition(
      opacity: fade,
      child: AnimatedBuilder(
        animation: settle,
        child: widget.child,
        builder: (context, child) {
          final rest = 1 - settle.value;
          return Transform.translate(
            offset: Offset(0, -26 * rest),
            child: Transform.rotate(
              angle: 0.06 * rest,
              child: Transform.scale(scale: 1 + 0.12 * rest, child: child),
            ),
          );
        },
      ),
    );
  }
}

TextStyle handStyle({
  double size = 20,
  Color color = NotebookColors.ink,
  FontWeight weight = FontWeight.w400,
  double height = 1.35,
}) => TextStyle(
  fontFamily: NotebookFonts.hand,
  // Caveat es compacta: se agranda para leerse como las demás letras.
  fontSize: size * 1.2,
  color: color,
  fontWeight: weight,
  // La fuente es variable; el grosor se fija en su eje `wght`.
  fontVariations: [
    FontVariation('wght', weight.value.clamp(400, 700).toDouble()),
  ],
  height: height,
);

TextStyle typewriterStyle({
  double size = 13,
  Color color = NotebookColors.inkFaded,
  double letterSpacing = 1.2,
}) => TextStyle(
  fontFamily: NotebookFonts.typewriter,
  fontSize: size,
  color: color,
  letterSpacing: letterSpacing,
);
