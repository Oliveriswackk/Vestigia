import 'dart:math';

import 'package:flutter/material.dart';

import '../photo_assembly.dart';

/// Objetos sueltos de escritorio que estorban sobre la mesa. Se arrastran
/// como los pedazos, pero nunca encajan ni cuentan para resolver.
enum ClutterKind {
  crumpledPaper,
  tornScrap,
  pencil,
  matchbox,
  clip,
  foldedNote,
}

class ClutterItem {
  ClutterItem({
    required this.id,
    required this.kind,
    required this.size,
    required this.center,
    required this.angle,
    required this.variant,
  });

  final int id;
  final ClutterKind kind;

  /// Tamaño en anchos de foto (antes de rotar).
  final Size size;

  /// Centro sobre la superficie, en anchos de foto.
  Offset center;
  final double angle;
  final int variant;

  Offset _local(Offset point) {
    final d = point - center;
    final c = cos(-angle), s = sin(-angle);
    return Offset(d.dx * c - d.dy * s, d.dx * s + d.dy * c);
  }

  /// ¿Cae [point] dentro del objeto (rectángulo rotado inflado [margin])?
  /// Los objetos delgados tienen una zona de toque mínima.
  bool contains(Offset point, {double margin = 0}) {
    final l = _local(point);
    return l.dx.abs() <= size.width / 2 + margin &&
        l.dy.abs() <= max(size.height, 0.06) / 2 + margin;
  }
}

/// Reparte los estorbos: dos por pedazo tapándolo en parte (sin cubrir el
/// punto interior de ningún pedazo) y el resto por la mesa. Determinista
/// respecto a [random] y a las posiciones de [assembly].
List<ClutterItem> scatterClutter(PhotoAssembly assembly, Random random) {
  final surface = assembly.surface;
  final photo = assembly.photo;
  final keepOut = [
    for (final p in photo.pieces) assembly.originOf(p.id) + p.interiorPoint,
  ];

  const specs = [
    (ClutterKind.crumpledPaper, Size(0.20, 0.18)),
    (ClutterKind.pencil, Size(0.42, 0.035)),
    (ClutterKind.tornScrap, Size(0.24, 0.16)),
    (ClutterKind.foldedNote, Size(0.16, 0.19)),
    (ClutterKind.pencil, Size(0.38, 0.035)),
    (ClutterKind.matchbox, Size(0.18, 0.105)),
    (ClutterKind.crumpledPaper, Size(0.15, 0.14)),
    (ClutterKind.clip, Size(0.05, 0.13)),
    (ClutterKind.pencil, Size(0.34, 0.035)),
  ];
  final order = List.generate(specs.length, (i) => i)..shuffle(random);

  final items = <ClutterItem>[];
  final pieces = photo.pieces;
  for (var i = 0; i < specs.length; i++) {
    final spec = specs[order[i]];
    final item = ClutterItem(
      id: items.length,
      kind: spec.$1,
      size: spec.$2,
      center: Offset.zero,
      angle: (random.nextDouble() - 0.5) * pi * 1.6,
      variant: random.nextInt(1 << 20),
    );

    bool ok(Offset c) {
      item.center = c;
      if (c.dx < item.size.width / 2 ||
          c.dy < item.size.height / 2 ||
          c.dx > surface.width - item.size.width / 2 ||
          c.dy > surface.height - item.size.height / 2) {
        return false;
      }
      return keepOut.every((k) => !item.contains(k, margin: 0.04));
    }

    Offset? placed;
    // Los primeros seis se acomodan sobre el contorno de un pedazo.
    if (i < pieces.length * 2) {
      final piece = pieces[i % pieces.length];
      final origin = assembly.originOf(piece.id);
      for (var t = 0; t < 80 && placed == null; t++) {
        final v = piece.outline[random.nextInt(piece.outline.length)];
        final c =
            origin +
            v +
            Offset(
              (random.nextDouble() - 0.5) * item.size.width * 0.8,
              (random.nextDouble() - 0.5) * item.size.width * 0.8,
            );
        if (ok(c)) placed = c;
      }
    }
    for (var t = 0; t < 200 && placed == null; t++) {
      final c = Offset(
        random.nextDouble() * surface.width,
        random.nextDouble() * surface.height,
      );
      if (ok(c)) placed = c;
    }
    item.center = placed ?? item.center;
    items.add(item);
  }
  return items;
}

/// Dibuja un estorbo con su sombra. El widget mide el tamaño del objeto;
/// la sombra y los trazos pueden salirse un poco.
class ClutterView extends StatelessWidget {
  const ClutterView({super.key, required this.item, required this.scale});

  final ClutterItem item;
  final double scale;

  @override
  Widget build(BuildContext context) {
    return Transform.rotate(
      angle: item.angle,
      child: SizedBox(
        width: item.size.width * scale,
        height: item.size.height * scale,
        child: CustomPaint(painter: ClutterPainter(item.kind, item.variant)),
      ),
    );
  }
}

class ClutterPainter extends CustomPainter {
  ClutterPainter(this.kind, this.variant);

  final ClutterKind kind;
  final int variant;

  @override
  void paint(Canvas canvas, Size size) {
    switch (kind) {
      case ClutterKind.crumpledPaper:
        _crumpled(canvas, size);
      case ClutterKind.tornScrap:
        _scrap(canvas, size);
      case ClutterKind.pencil:
        _pencil(canvas, size);
      case ClutterKind.matchbox:
        _matchbox(canvas, size);
      case ClutterKind.clip:
        _clip(canvas, size);
      case ClutterKind.foldedNote:
        _note(canvas, size);
    }
  }

  void _shadow(Canvas canvas, Path path, {double blur = 5, Offset? shift}) {
    canvas.drawPath(
      path.shift(shift ?? const Offset(2.5, 4)),
      Paint()
        ..color = Colors.black.withValues(alpha: 0.38)
        ..maskFilter = MaskFilter.blur(BlurStyle.normal, blur),
    );
  }

  Path _blob(Size s, Random r, int n, double jitter) {
    final pts = <Offset>[
      for (var i = 0; i < n; i++)
        Offset(
          s.width / 2 +
              cos(2 * pi * i / n) * s.width / 2 * (1 - jitter * r.nextDouble()),
          s.height / 2 +
              sin(2 * pi * i / n) *
                  s.height /
                  2 *
                  (1 - jitter * r.nextDouble()),
        ),
    ];
    return Path()..addPolygon(pts, true);
  }

  void _crumpled(Canvas canvas, Size s) {
    final r = Random(variant);
    final path = _blob(s, r, 11, 0.3);
    _shadow(canvas, path);
    canvas.drawPath(
      path,
      Paint()
        ..shader = const RadialGradient(
          center: Alignment(-0.4, -0.5),
          colors: [Color(0xFFF1EBDD), Color(0xFFCFC5AD)],
        ).createShader(Offset.zero & s),
    );
    canvas.save();
    canvas.clipPath(path);
    for (var i = 0; i < 9; i++) {
      final a = Offset(r.nextDouble() * s.width, r.nextDouble() * s.height);
      final b = Offset(r.nextDouble() * s.width, r.nextDouble() * s.height);
      canvas.drawLine(
        a,
        b,
        Paint()
          ..strokeWidth = 1.1
          ..color = (i.isEven ? Colors.black : Colors.white).withValues(
            alpha: i.isEven ? 0.16 : 0.45,
          ),
      );
    }
    canvas.restore();
    canvas.drawPath(
      path,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 0.8
        ..color = const Color(0x55584B34),
    );
  }

  void _scrap(Canvas canvas, Size s) {
    final r = Random(variant);
    final pts = <Offset>[];
    const n = 7;
    for (var i = 0; i <= n; i++) {
      pts.add(Offset(s.width * i / n, r.nextDouble() * s.height * 0.12));
    }
    for (var i = 0; i <= 4; i++) {
      pts.add(
        Offset(
          s.width - r.nextDouble() * s.width * 0.05,
          s.height * (0.15 + 0.7 * i / 4),
        ),
      );
    }
    for (var i = n; i >= 0; i--) {
      pts.add(
        Offset(s.width * i / n, s.height - r.nextDouble() * s.height * 0.14),
      );
    }
    final path = Path()..addPolygon(pts, true);
    _shadow(canvas, path, blur: 3.5, shift: const Offset(1.5, 2.5));
    canvas.drawPath(path, Paint()..color = const Color(0xFFEDE6D3));
    canvas.save();
    canvas.clipPath(path);
    final line = Paint()
      ..strokeWidth = 0.8
      ..color = const Color(0x553C6FA8);
    for (var y = s.height * 0.28; y < s.height; y += s.height * 0.2) {
      canvas.drawLine(Offset(0, y), Offset(s.width, y), line);
    }
    canvas.drawLine(
      Offset(s.width * 0.15, 0),
      Offset(s.width * 0.15, s.height),
      Paint()
        ..strokeWidth = 0.8
        ..color = const Color(0x55B04040),
    );
    canvas.restore();
  }

  void _pencil(Canvas canvas, Size s) {
    const palette = [Color(0xFFE2B52B), Color(0xFFB8452F), Color(0xFF3F7A56)];
    final body = palette[variant % palette.length];
    final h = s.height;
    final tip = h * 1.6;
    final end = h;
    final bodyRect = Rect.fromLTWH(tip, 0, s.width - tip - end * 1.6, h);
    final outline = Path()
      ..moveTo(0, h / 2)
      ..lineTo(tip, 0)
      ..lineTo(s.width, 0)
      ..lineTo(s.width, h)
      ..lineTo(tip, h)
      ..close();
    _shadow(canvas, outline, blur: 3, shift: const Offset(2, 3.5));
    canvas.drawRect(bodyRect, Paint()..color = body);
    canvas.drawRect(
      Rect.fromLTWH(bodyRect.left, 0, bodyRect.width, h * 0.34),
      Paint()..color = Colors.white.withValues(alpha: 0.22),
    );
    canvas.drawRect(
      Rect.fromLTWH(bodyRect.left, h * 0.7, bodyRect.width, h * 0.3),
      Paint()..color = Colors.black.withValues(alpha: 0.22),
    );
    // Madera y grafito.
    canvas.drawPath(
      Path()
        ..moveTo(0, h / 2)
        ..lineTo(tip, 0)
        ..lineTo(tip, h)
        ..close(),
      Paint()..color = const Color(0xFFE6C593),
    );
    canvas.drawPath(
      Path()
        ..moveTo(0, h / 2)
        ..lineTo(tip * 0.38, h * 0.3)
        ..lineTo(tip * 0.38, h * 0.7)
        ..close(),
      Paint()..color = const Color(0xFF2B2B2B),
    );
    // Casquillo metálico y goma.
    final ferrule = Rect.fromLTWH(bodyRect.right, 0, end * 0.7, h);
    canvas.drawRect(
      ferrule,
      Paint()
        ..shader = const LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [Color(0xFFE8E8E8), Color(0xFF8E8E8E), Color(0xFFD0D0D0)],
        ).createShader(ferrule),
    );
    canvas.drawRRect(
      RRect.fromRectAndRadius(
        Rect.fromLTWH(ferrule.right, 0, s.width - ferrule.right, h),
        Radius.circular(h * 0.3),
      ),
      Paint()..color = const Color(0xFFD98B93),
    );
  }

  void _matchbox(Canvas canvas, Size s) {
    final rect = RRect.fromRectAndRadius(
      Offset.zero & s,
      Radius.circular(s.height * 0.1),
    );
    _shadow(canvas, Path()..addRRect(rect), blur: 4.5);
    canvas.drawRRect(
      rect,
      Paint()
        ..shader = const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFFB5362B), Color(0xFF8A241C)],
        ).createShader(Offset.zero & s),
    );
    final label = Rect.fromLTWH(
      s.width * 0.14,
      s.height * 0.16,
      s.width * 0.5,
      s.height * 0.68,
    );
    canvas.drawRRect(
      RRect.fromRectAndRadius(label, Radius.circular(s.height * 0.06)),
      Paint()..color = const Color(0xFFEFE3C2),
    );
    canvas.drawCircle(
      label.center,
      s.height * 0.2,
      Paint()..color = const Color(0xFF2F4B7A),
    );
    // Lija para encender.
    canvas.drawRect(
      Rect.fromLTWH(
        s.width * 0.72,
        s.height * 0.08,
        s.width * 0.2,
        s.height * 0.84,
      ),
      Paint()..color = const Color(0xFF4B3B33),
    );
    final r = Random(variant);
    for (var i = 0; i < 18; i++) {
      canvas.drawCircle(
        Offset(
          s.width * (0.73 + 0.18 * r.nextDouble()),
          s.height * (0.1 + 0.8 * r.nextDouble()),
        ),
        0.7,
        Paint()..color = const Color(0x55FFFFFF),
      );
    }
  }

  void _clip(Canvas canvas, Size s) {
    final w = s.width, h = s.height;
    final path = Path()
      ..moveTo(w * 0.28, h * 0.78)
      ..lineTo(w * 0.28, h * 0.2)
      ..arcToPoint(Offset(w * 0.72, h * 0.2), radius: Radius.circular(w * 0.22))
      ..lineTo(w * 0.72, h * 0.82)
      ..arcToPoint(
        Offset(w * 0.04, h * 0.82),
        radius: Radius.circular(w * 0.34),
      )
      ..lineTo(w * 0.04, h * 0.14)
      ..arcToPoint(
        Offset(w * 0.96, h * 0.14),
        radius: Radius.circular(w * 0.46),
      )
      ..lineTo(w * 0.96, h * 0.7);
    canvas.drawPath(
      path.shift(const Offset(2, 3)),
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2.2
        ..color = Colors.black.withValues(alpha: 0.35)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 2),
    );
    canvas.drawPath(
      path,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2.2
        ..strokeCap = StrokeCap.round
        ..color = const Color(0xFF9FA4AA),
    );
    canvas.drawPath(
      path.shift(const Offset(-0.5, -0.6)),
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 0.9
        ..strokeCap = StrokeCap.round
        ..color = const Color(0xDDFFFFFF),
    );
  }

  void _note(Canvas canvas, Size s) {
    final rect = Offset.zero & s;
    _shadow(canvas, Path()..addRect(rect), blur: 4);
    canvas.drawRect(
      rect,
      Paint()
        ..shader = const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFFF2E7B6), Color(0xFFDCCB8A)],
        ).createShader(rect),
    );
    canvas.drawLine(
      Offset(0, s.height / 2),
      Offset(s.width, s.height / 2),
      Paint()
        ..strokeWidth = 1
        ..color = Colors.black.withValues(alpha: 0.22),
    );
    canvas.drawLine(
      Offset(0, s.height / 2 + 1),
      Offset(s.width, s.height / 2 + 1),
      Paint()
        ..strokeWidth = 1
        ..color = Colors.white.withValues(alpha: 0.5),
    );
    canvas.drawLine(
      Offset(s.width / 2, 0),
      Offset(s.width / 2, s.height),
      Paint()
        ..strokeWidth = 1
        ..color = Colors.black.withValues(alpha: 0.14),
    );
    final ink = Paint()
      ..strokeWidth = 0.9
      ..color = const Color(0x773A3A55);
    final r = Random(variant);
    for (var i = 0; i < 4; i++) {
      final y = s.height * (0.12 + 0.1 * i);
      canvas.drawLine(
        Offset(s.width * 0.1, y),
        Offset(s.width * (0.5 + 0.35 * r.nextDouble()), y),
        ink,
      );
    }
  }

  @override
  bool shouldRepaint(ClutterPainter old) =>
      old.kind != kind || old.variant != variant;
}
