import 'dart:async';

import 'package:flutter/material.dart';

import '../core/room/room_connection.dart';
import '../menu/puzzle_catalog.dart';
import '../menu/puzzle_entry.dart';
import '../notebook/notebook_screen.dart';
import 'puzzle_route.dart';
import 'vestigia_theme.dart';

class VestigiaApp extends StatefulWidget {
  const VestigiaApp({super.key, this.puzzles, this.room});

  /// Permite sustituir el catálogo (por ejemplo, en pruebas).
  final List<PuzzleEntry>? puzzles;

  /// Permite sustituir la conexión con la sala (por ejemplo, en pruebas).
  final RoomConnection? room;

  @override
  State<VestigiaApp> createState() => _VestigiaAppState();
}

class _VestigiaAppState extends State<VestigiaApp> with WidgetsBindingObserver {
  late final RoomConnection _room = widget.room ?? RoomConnection.standard();
  late final List<PuzzleEntry> _puzzles =
      widget.puzzles ?? defaultPuzzleCatalog(room: _room);
  final _navigatorKey = GlobalKey<NavigatorState>();
  StreamSubscription<String>? _openedSubscription;
  String? _autoOpened;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _openedSubscription = _room.puzzleOpened.listen(_openPuzzle);
  }

  /// El juego dejó un puzzle disponible: abre su pantalla. Si ya se abrió
  /// por este camino y sigue en pantalla, no la apila otra vez.
  void _openPuzzle(String id) {
    if (_autoOpened == id) return;
    final navigator = _navigatorKey.currentState;
    if (navigator == null) return;
    PuzzleEntry? entry;
    for (final candidate in _puzzles) {
      if (candidate.id == id) entry = candidate;
    }
    if (entry == null) return;
    _autoOpened = id;
    unawaited(
      navigator.push<void>(puzzleRoute(entry.builder)).whenComplete(() {
        if (_autoOpened == id) _autoOpened = null;
      }),
    );
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) _room.refresh();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _openedSubscription?.cancel();
    if (widget.room == null) _room.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      navigatorKey: _navigatorKey,
      navigatorObservers: [notebookRouteObserver],
      title: 'Vestigia',
      debugShowCheckedModeBanner: false,
      theme: buildVestigiaTheme(),
      home: NotebookHome(room: _room, puzzles: _puzzles),
    );
  }
}
