import 'package:flutter/material.dart';

import '../app/notebook_style.dart';
import '../app/puzzle_route.dart';
import '../core/haptics.dart';
import '../core/room/room_connection.dart';
import '../menu/puzzle_entry.dart';
import '../puzzles/photo/photo_config.dart';
import '../puzzles/photo/torn_photo.dart';
import '../puzzles/photo/widgets/photo_piece_view.dart';
import 'cover_screen.dart';
import 'notebook_entries.dart';

/// Raíz de la app: la tapa mientras no hay partida y la libreta en cuanto
/// el teléfono se une a la sala.
class NotebookHome extends StatelessWidget {
  const NotebookHome({super.key, required this.room, required this.puzzles});

  final RoomConnection room;
  final List<PuzzleEntry> puzzles;

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: room,
      builder: (context, _) => AnimatedSwitcher(
        duration: const Duration(milliseconds: 500),
        child: room.inRoom
            ? NotebookScreen(
                key: const ValueKey('notebook'),
                room: room,
                puzzles: puzzles,
              )
            : CoverScreen(
                key: const ValueKey('cover'),
                room: room,
                puzzles: puzzles,
              ),
      ),
    );
  }
}

/// Avisa a la libreta cuando vuelve a quedar a la vista (al cerrarse un
/// puzzle encima de ella).
final notebookRouteObserver = RouteObserver<ModalRoute<void>>();

/// La libreta: anotaciones, coordenadas, fotos pegadas y fichas de los
/// puzzles, en el orden en que la historia los va revelando.
///
/// Lo que llega nuevo se pega con una animación, vibra según su importancia
/// y la página se desplaza hasta mostrarlo. Si llega mientras hay un puzzle
/// abierto encima, todo eso espera a que la libreta vuelva a verse.
class NotebookScreen extends StatefulWidget {
  const NotebookScreen({
    super.key,
    required this.room,
    required this.puzzles,
    this.haptics,
  });

  final RoomConnection room;
  final List<PuzzleEntry> puzzles;
  final Haptics? haptics;

  static const closeKey = Key('notebook-close');

  @override
  State<NotebookScreen> createState() => _NotebookScreenState();
}

class _NotebookScreenState extends State<NotebookScreen> with RouteAware {
  late final Haptics _haptics = widget.haptics ?? PlatformHaptics();

  /// Firma de cada elemento ya visto (id y, en las fichas, su estado).
  final _signatures = <String, String>{};

  /// Elementos que estaban al abrir la libreta: entran escalonados.
  final _initial = <String>{};

  /// Elementos nuevos que esperan a que la libreta vuelva a la vista.
  final _hidden = <String>{};

  /// Retraso de entrada de cada elemento nuevo (varios a la vez se escalonan).
  final _delays = <String, Duration>{};

  final _itemKeys = <String, GlobalKey>{};

  HapticCue? _pendingCue;
  String? _pendingScroll;
  ModalRoute<void>? _route;

  @override
  void initState() {
    super.initState();
    for (final item in buildNotebook(widget.room.snapshot)) {
      _signatures[item.id] = _signature(item);
      _initial.add(item.id);
    }
    widget.room.addListener(_onRoomChanged);
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    final route = ModalRoute.of(context);
    if (route != _route) {
      notebookRouteObserver.unsubscribe(this);
      _route = route;
      if (route != null) notebookRouteObserver.subscribe(this, route);
    }
  }

  @override
  void dispose() {
    widget.room.removeListener(_onRoomChanged);
    notebookRouteObserver.unsubscribe(this);
    super.dispose();
  }

  static String _signature(NotebookItem item) =>
      item is PuzzleItem ? '${item.id}:${item.state}' : item.id;

  /// Cuánto pesa lo que acaba de pasar; null si no merece vibrar.
  static HapticCue? _cueFor(NotebookItem item, {required bool isNew}) {
    return switch (item) {
      CoordinateItem() when isNew => HapticCue.notebookMilestone,
      PuzzleItem(state: 'resuelto') => HapticCue.notebookMilestone,
      PuzzleItem() when isNew => HapticCue.notebookFind,
      PhotoPieceItem() when isNew => HapticCue.notebookFind,
      NoteItem(id: 'llave_faro') when isNew => HapticCue.notebookFind,
      NoteItem() when isNew => HapticCue.notebookNote,
      _ => null,
    };
  }

  bool get _visible => _route?.isCurrent ?? true;

  void _onRoomChanged() {
    if (!mounted) return;
    setState(() {});
    HapticCue? cue;
    String? target;
    var fresh = 0;
    for (final item in buildNotebook(widget.room.snapshot)) {
      final signature = _signature(item);
      final previous = _signatures[item.id];
      if (previous == signature) continue;
      _signatures[item.id] = signature;
      final isNew = previous == null;
      if (isNew) {
        _delays[item.id] = Duration(milliseconds: 160 * fresh++);
        if (!_visible) _hidden.add(item.id);
      }
      final itemCue = _cueFor(item, isNew: isNew);
      if (itemCue == null) continue;
      if (cue == null || itemCue.index > cue.index) cue = itemCue;
      target = item.id;
    }
    if (cue == null) return;
    if (_visible) {
      _announce(cue, target);
    } else {
      final pending = _pendingCue;
      if (pending == null || cue.index > pending.index) _pendingCue = cue;
      _pendingScroll = target ?? _pendingScroll;
    }
  }

  /// La libreta vuelve a la vista: entra lo que llegó mientras tanto.
  @override
  void didPopNext() {
    final cue = _pendingCue;
    final target = _pendingScroll;
    _pendingCue = null;
    _pendingScroll = null;
    if (_hidden.isNotEmpty) setState(_hidden.clear);
    if (cue != null) {
      // Deja terminar la transición de cierre del puzzle.
      Future<void>.delayed(const Duration(milliseconds: 350), () {
        if (mounted) _announce(cue, target);
      });
    }
  }

  void _announce(HapticCue cue, String? target) {
    _haptics.play(cue);
    if (target == null) return;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final context = _itemKeys[target]?.currentContext;
      if (context == null || !context.mounted) return;
      Scrollable.ensureVisible(
        context,
        alignment: 0.4,
        duration: const Duration(milliseconds: 700),
        curve: Curves.easeInOutCubic,
      );
    });
  }

  void _openPuzzle(BuildContext context, String id) {
    for (final entry in widget.puzzles) {
      if (entry.id == id) {
        Navigator.of(context).push(puzzleRoute(entry.builder));
        return;
      }
    }
  }

  Future<void> _confirmClose(BuildContext context) async {
    final close = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: NotebookColors.paper,
        title: Text('¿Cerrar la libreta?', style: handStyle(size: 24)),
        content: Text(
          'Dejarás de recibir lo que pase en el juego. '
          'Podrás volver a abrirla con el nombre del pueblo.',
          style: handStyle(size: 17, color: NotebookColors.inkFaded),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(
              'Seguir',
              style: typewriterStyle(color: NotebookColors.ink),
            ),
          ),
          TextButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: Text(
              'Cerrar',
              style: typewriterStyle(color: NotebookColors.redInk),
            ),
          ),
        ],
      ),
    );
    if (close == true) widget.room.leave();
  }

  Duration _delayFor(String id, int index) => _initial.contains(id)
      ? Duration(milliseconds: 90 * index.clamp(0, 8))
      : _delays[id] ?? Duration.zero;

  @override
  Widget build(BuildContext context) {
    final room = widget.room;
    final snapshot = room.snapshot;
    final items = buildNotebook(snapshot);
    final chapter = sceneTitles[snapshot?.scene];

    return Scaffold(
      backgroundColor: NotebookColors.paper,
      body: PaperBackground(
        // El papel ocupa toda la pantalla aunque haya poco escrito.
        child: SizedBox.expand(
          child: SafeArea(
            // Una columna, no una lista perezosa: las notas no se destruyen al
            // salir de la pantalla, así que no se vuelven a animar al regresar.
            child: SingleChildScrollView(
              padding: const EdgeInsets.fromLTRB(58, 18, 20, 48),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _Header(room: room),
                  const SizedBox(height: 18),
                  if (chapter != null) ...[
                    _Chapter(title: chapter),
                    const SizedBox(height: 14),
                  ],
                  if (items.isEmpty)
                    _EmptyPage(waiting: snapshot == null)
                  else
                    for (final (index, item) in items.indexed)
                      Padding(
                        key: ValueKey(item.id),
                        padding: const EdgeInsets.only(bottom: 22),
                        child: KeyedSubtree(
                          key: _itemKeys.putIfAbsent(item.id, GlobalKey.new),
                          child: AppearOnce(
                            delay: _delayFor(item.id, index),
                            show: !_hidden.contains(item.id),
                            child: _ItemView(
                              item: item,
                              index: index,
                              onOpenPuzzle: (id) => _openPuzzle(context, id),
                            ),
                          ),
                        ),
                      ),
                  const SizedBox(height: 28),
                  Align(
                    alignment: Alignment.centerRight,
                    child: TextButton(
                      key: NotebookScreen.closeKey,
                      onPressed: () => _confirmClose(context),
                      child: Text(
                        'cerrar libreta',
                        style: typewriterStyle(
                          size: 12,
                          color: NotebookColors.pencil,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _Header extends StatelessWidget {
  const _Header({required this.room});

  final RoomConnection room;

  @override
  Widget build(BuildContext context) {
    final (text, color) = switch (room.phase) {
      RoomPhase.connected when room.gameConnected => (
        'PC conectada',
        NotebookColors.greenInk,
      ),
      RoomPhase.connected => ('La PC no responde', NotebookColors.redInk),
      RoomPhase.reconnecting => ('Buscando señal…', NotebookColors.redInk),
      _ => ('Conectando…', NotebookColors.pencil),
    };
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Expanded(
              child: Text(
                'LIBRETA DE CAMPO',
                style: typewriterStyle(size: 12, letterSpacing: 3),
              ),
            ),
            // El punto va centrado con el texto del estado.
            Flexible(
              child: Row(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  Container(
                    width: 8,
                    height: 8,
                    decoration: BoxDecoration(
                      color: color,
                      shape: BoxShape.circle,
                    ),
                  ),
                  const SizedBox(width: 6),
                  Flexible(
                    child: Text(
                      text,
                      key: const Key('notebook-status'),
                      overflow: TextOverflow.ellipsis,
                      style: handStyle(size: 15, color: color, height: 1.1),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        const SizedBox(height: 4),
        Text(
          room.code ?? '',
          key: const Key('notebook-town'),
          style: handStyle(size: 34, weight: FontWeight.w700, height: 1.1),
        ),
      ],
    );
  }
}

class _Chapter extends StatelessWidget {
  const _Chapter({required this.title});

  final String title;

  @override
  Widget build(BuildContext context) {
    return Text.rich(
      TextSpan(
        children: [
          TextSpan(
            text: 'Ahora: ',
            style: handStyle(size: 18, color: NotebookColors.pencil),
          ),
          TextSpan(
            text: title,
            style: handStyle(
              size: 22,
              color: NotebookColors.blueInk,
              weight: FontWeight.w700,
            ),
          ),
        ],
      ),
      key: const Key('notebook-chapter'),
    );
  }
}

class _EmptyPage extends StatelessWidget {
  const _EmptyPage({required this.waiting});

  final bool waiting;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 24),
      child: Text(
        waiting
            ? 'Abriendo la libreta…'
            : 'Todavía no hay nada anotado.\n'
                  'Sigue la historia en la PC: lo que encuentres '
                  'quedará escrito aquí.',
        style: handStyle(size: 19, color: NotebookColors.pencil),
      ),
    );
  }
}

class _ItemView extends StatelessWidget {
  const _ItemView({
    required this.item,
    required this.index,
    required this.onOpenPuzzle,
  });

  final NotebookItem item;
  final int index;
  final ValueChanged<String> onOpenPuzzle;

  @override
  Widget build(BuildContext context) {
    return switch (item) {
      NoteItem(:final text) => Text(text, style: handStyle()),
      final CoordinateItem c => _CoordinateView(item: c),
      final PhotoPieceItem p => _TapedPiece(item: p),
      final PuzzleItem p => _PuzzleCardView(
        item: p,
        angle: index.isEven ? -0.012 : 0.015,
        // Un puzzle resuelto ya no se repite.
        onTap: p.state == 'resuelto' ? null : () => onOpenPuzzle(p.puzzle),
      ),
    };
  }
}

class _CoordinateView extends StatelessWidget {
  const _CoordinateView({required this.item});

  final CoordinateItem item;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          item.label.toUpperCase(),
          style: typewriterStyle(size: 12, letterSpacing: 2),
        ),
        const SizedBox(height: 2),
        CustomPaint(
          painter: const _CirclePainter(),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 6),
            child: Text(
              item.value,
              key: ValueKey('coordinate-${item.id}'),
              style: handStyle(
                size: 40,
                weight: FontWeight.w700,
                color: NotebookColors.blueInk,
                height: 1.1,
              ),
            ),
          ),
        ),
        if (item.note case final note?)
          Text(note, style: handStyle(size: 17, color: NotebookColors.pencil)),
      ],
    );
  }
}

/// Círculo hecho a mano, un poco abierto, alrededor de una cifra.
class _CirclePainter extends CustomPainter {
  const _CirclePainter();

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = NotebookColors.redInk.withValues(alpha: 0.75)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2
      ..strokeCap = StrokeCap.round;
    final rect = Rect.fromLTWH(0, 0, size.width, size.height).inflate(2);
    canvas.drawArc(rect, -2.6, 5.9, false, paint);
  }

  @override
  bool shouldRepaint(_CirclePainter old) => false;
}

class _TapedPiece extends StatelessWidget {
  const _TapedPiece({required this.item});

  final PhotoPieceItem item;

  static const _config = PhotoPuzzleConfig();
  static final _photo = TornPhoto.threePieces(aspect: _config.aspect);
  static const _angles = [-0.07, 0.05, -0.03];
  static const _alignments = [
    Alignment.centerLeft,
    Alignment.centerRight,
    Alignment.center,
  ];

  @override
  Widget build(BuildContext context) {
    final piece = _photo.piece(item.piece);
    const scale = 190.0;
    return Column(
      children: [
        Align(
          alignment: _alignments[item.piece % 3],
          child: Transform.rotate(
            angle: _angles[item.piece % 3],
            child: Stack(
              clipBehavior: Clip.none,
              alignment: Alignment.topCenter,
              children: [
                Padding(
                  padding: const EdgeInsets.only(top: 10),
                  child: PhotoPieceView(
                    key: ValueKey('taped-piece-${item.piece}'),
                    piece: piece,
                    scale: scale,
                    aspect: _config.aspect,
                    image: AssetImage(_config.photoAsset),
                  ),
                ),
                TapeStrip(
                  angle: -0.08 + item.piece * 0.07,
                  seed: item.piece + 7,
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 6),
        Align(
          alignment: _alignments[item.piece % 3],
          child: Text(
            item.caption,
            style: handStyle(size: 16, color: NotebookColors.pencil),
          ),
        ),
      ],
    );
  }
}

class _PuzzleCardView extends StatelessWidget {
  const _PuzzleCardView({
    required this.item,
    required this.angle,
    required this.onTap,
  });

  final PuzzleItem item;
  final double angle;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final (stamp, color) = switch (item.state) {
      'resuelto' => ('RESUELTO', NotebookColors.greenInk),
      'en_curso' => ('EN CURSO', NotebookColors.blueInk),
      _ => ('PENDIENTE', NotebookColors.redInk),
    };
    return CardboardCard(
      key: ValueKey('puzzle-card-${item.puzzle}'),
      angle: angle,
      onTap: onTap,
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.title,
                  style: typewriterStyle(
                    size: 16,
                    color: NotebookColors.cardboardInk,
                    letterSpacing: 0.5,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  item.hint,
                  style: handStyle(
                    size: 16,
                    color: NotebookColors.cardboardInk.withValues(alpha: 0.85),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(width: 8),
          // El sello cae y se estampa cada vez que cambia el estado.
          AnimatedSwitcher(
            duration: const Duration(milliseconds: 520),
            switchInCurve: Curves.easeOutBack,
            transitionBuilder: (child, animation) => FadeTransition(
              opacity: animation,
              child: ScaleTransition(
                scale: Tween(begin: 1.9, end: 1.0).animate(animation),
                child: child,
              ),
            ),
            child: InkStamp(key: ValueKey(stamp), text: stamp, color: color),
          ),
        ],
      ),
    );
  }
}
