import 'package:flutter/material.dart';

import '../core/room/room_connection.dart';
import '../menu/main_menu_screen.dart';
import '../menu/puzzle_catalog.dart';
import '../menu/puzzle_entry.dart';
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

class _VestigiaAppState extends State<VestigiaApp>
    with WidgetsBindingObserver {
  late final RoomConnection _room = widget.room ?? RoomConnection.standard();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) _room.refresh();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    if (widget.room == null) _room.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Vestigia',
      debugShowCheckedModeBanner: false,
      theme: buildVestigiaTheme(),
      home: MainMenuScreen(
        puzzles: widget.puzzles ?? defaultPuzzleCatalog(room: _room),
      ),
    );
  }
}
