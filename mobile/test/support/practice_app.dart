import 'package:flutter/material.dart';
import 'package:radio_coordenadas/app/vestigia_theme.dart';
import 'package:radio_coordenadas/core/room/room_connection.dart';
import 'package:radio_coordenadas/menu/main_menu_screen.dart';
import 'package:radio_coordenadas/menu/puzzle_catalog.dart';
import 'package:radio_coordenadas/menu/puzzle_entry.dart';

/// Menú de práctica con todos los puzzles. En la app se abre manteniendo
/// presionado el título de la tapa; los tests de cada puzzle entran por
/// aquí para no depender de una partida.
Widget practiceApp({List<PuzzleEntry>? puzzles, RoomConnection? room}) =>
    MaterialApp(
      debugShowCheckedModeBanner: false,
      theme: buildVestigiaTheme(),
      home: MainMenuScreen(
        puzzles:
            puzzles ??
            defaultPuzzleCatalog(room: room ?? RoomConnection.standard()),
      ),
    );
