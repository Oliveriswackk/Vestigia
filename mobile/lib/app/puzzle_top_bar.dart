import 'package:flutter/material.dart';

import 'notebook_style.dart';
import 'vestigia_theme.dart';

/// Barra común de los puzzles: volver al menú, título y nueva partida
/// (o la acción equivalente de la pantalla, como salir de la sala).
class PuzzleTopBar extends StatelessWidget {
  const PuzzleTopBar({
    super.key,
    required this.title,
    required this.onBack,
    this.onRestart,
    this.restartKey,
    this.restartTooltip = 'Nueva partida',
    this.restartIcon = Icons.refresh,
  });

  final String title;
  final VoidCallback onBack;

  /// `null` oculta el botón (en una partida no se repite el puzzle).
  final VoidCallback? onRestart;
  final Key? restartKey;
  final String restartTooltip;
  final IconData restartIcon;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 4),
      child: Row(
        children: [
          IconButton(
            icon: const Icon(Icons.arrow_back),
            tooltip: 'Volver al menú',
            onPressed: onBack,
          ),
          Expanded(
            child: Text(
              title,
              textAlign: TextAlign.center,
              style: const TextStyle(
                fontFamily: NotebookFonts.typewriter,
                fontSize: 18,
                letterSpacing: 1,
                color: VestigiaColors.cream,
              ),
            ),
          ),
          if (onRestart == null)
            const SizedBox(width: 48)
          else
            IconButton(
              key: restartKey,
              icon: Icon(restartIcon),
              tooltip: restartTooltip,
              onPressed: onRestart,
            ),
        ],
      ),
    );
  }
}
