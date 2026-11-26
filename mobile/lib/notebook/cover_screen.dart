import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../app/notebook_style.dart';
import '../core/room/room_connection.dart';
import '../menu/main_menu_screen.dart';
import '../menu/puzzle_entry.dart';

/// Tapa de la libreta: se escribe el nombre del pueblo que muestra la PC
/// para unirse a la partida.
class CoverScreen extends StatefulWidget {
  const CoverScreen({super.key, required this.room, required this.puzzles});

  final RoomConnection room;

  /// Para el modo práctica (mantener presionado el título).
  final List<PuzzleEntry> puzzles;

  static const codeFieldKey = Key('cover-code');
  static const openKey = Key('cover-open');
  static const titleKey = Key('cover-title');
  static const labelKey = Key('cover-label');

  @override
  State<CoverScreen> createState() => _CoverScreenState();
}

class _CoverScreenState extends State<CoverScreen> {
  final _controller = TextEditingController();
  final _focus = FocusNode();

  @override
  void dispose() {
    _controller.dispose();
    _focus.dispose();
    super.dispose();
  }

  /// Tocar cualquier parte de la etiqueta abre el teclado, no solo el
  /// renglón. Si el campo ya tenía el foco (teclado cerrado con Atrás),
  /// vuelve a mostrarlo.
  void _focusField() {
    if (widget.room.phase == RoomPhase.joining) return;
    if (_focus.hasFocus) {
      SystemChannels.textInput.invokeMethod<void>('TextInput.show');
    } else {
      _focus.requestFocus();
    }
  }

  void _open() {
    FocusScope.of(context).unfocus();
    widget.room.join(_controller.text);
  }

  void _practice() {
    Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => MainMenuScreen(puzzles: widget.puzzles),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final room = widget.room;
    final joining = room.phase == RoomPhase.joining;
    final error = room.error;

    return Scaffold(
      backgroundColor: NotebookColors.cover,
      resizeToAvoidBottomInset: true,
      body: CoverBackground(
        child: SafeArea(
          child: LayoutBuilder(
            builder: (context, constraints) => SingleChildScrollView(
              padding: const EdgeInsets.fromLTRB(40, 24, 28, 24),
              child: ConstrainedBox(
                constraints: BoxConstraints(
                  minHeight: constraints.maxHeight - 48,
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const SizedBox(height: 40),
                    GestureDetector(
                      key: CoverScreen.titleKey,
                      onLongPress: _practice,
                      child: Column(
                        children: [
                          Text(
                            'VESTIGIA',
                            style: typewriterStyle(
                              size: 38,
                              color: NotebookColors.coverInk,
                              letterSpacing: 10,
                            ),
                          ),
                          const SizedBox(height: 6),
                          Container(
                            width: 120,
                            height: 1.5,
                            color: NotebookColors.coverInk.withValues(
                              alpha: 0.5,
                            ),
                          ),
                          const SizedBox(height: 10),
                          Text(
                            'Libreta de campo',
                            style: handStyle(
                              size: 22,
                              color: NotebookColors.coverInk.withValues(
                                alpha: 0.8,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 48),
                    _Label(
                      joining: joining,
                      controller: _controller,
                      focusNode: _focus,
                      onTapLabel: _focusField,
                      onSubmit: _open,
                    ),
                    const SizedBox(height: 16),
                    SizedBox(
                      height: 52,
                      child: error == null
                          ? null
                          : Text(
                              error,
                              key: const Key('cover-error'),
                              textAlign: TextAlign.center,
                              style: handStyle(
                                size: 17,
                                color: const Color(0xFFF0B8A8),
                              ),
                            ),
                    ),
                    const SizedBox(height: 8),
                    _OpenButton(joining: joining, onPressed: _open),
                    const SizedBox(height: 40),
                    Text(
                      'Escribe el pueblo que aparece en la pantalla de la PC.',
                      textAlign: TextAlign.center,
                      style: handStyle(
                        size: 16,
                        color: NotebookColors.coverInk.withValues(alpha: 0.85),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

/// Etiqueta de papel pegada en la tapa, con el renglón para escribir.
class _Label extends StatelessWidget {
  const _Label({
    required this.joining,
    required this.controller,
    required this.focusNode,
    required this.onTapLabel,
    required this.onSubmit,
  });

  final FocusNode focusNode;
  final VoidCallback onTapLabel;

  final bool joining;
  final TextEditingController controller;
  final VoidCallback onSubmit;

  @override
  Widget build(BuildContext context) {
    return Stack(
      clipBehavior: Clip.none,
      alignment: Alignment.topCenter,
      children: [
        GestureDetector(
          key: CoverScreen.labelKey,
          behavior: HitTestBehavior.opaque,
          onTap: onTapLabel,
          child: Transform.rotate(
            angle: -0.015,
            child: Container(
              decoration: const BoxDecoration(
                boxShadow: [
                  BoxShadow(
                    color: Color(0x88000000),
                    blurRadius: 10,
                    offset: Offset(2, 5),
                  ),
                ],
              ),
              child: PaperBackground(
                ruled: false,
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(22, 26, 22, 18),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'PUEBLO',
                        style: typewriterStyle(size: 12, letterSpacing: 3),
                      ),
                      TextField(
                        key: CoverScreen.codeFieldKey,
                        controller: controller,
                        focusNode: focusNode,
                        enabled: !joining,
                        autocorrect: false,
                        enableSuggestions: false,
                        textCapitalization: TextCapitalization.characters,
                        textInputAction: TextInputAction.go,
                        onSubmitted: (_) => onSubmit(),
                        inputFormatters: [
                          FilteringTextInputFormatter.deny(RegExp(r'\s')),
                          _UpperCase(),
                        ],
                        cursorColor: NotebookColors.ink,
                        style: handStyle(size: 30, weight: FontWeight.w700),
                        decoration: InputDecoration(
                          isDense: true,
                          hintText: 'escribe aquí',
                          hintStyle: handStyle(
                            size: 30,
                            color: NotebookColors.ink.withValues(alpha: 0.55),
                          ),
                          filled: false,
                          enabledBorder: const UnderlineInputBorder(
                            borderSide: BorderSide(
                              color: NotebookColors.inkFaded,
                            ),
                          ),
                          focusedBorder: const UnderlineInputBorder(
                            borderSide: BorderSide(
                              color: NotebookColors.ink,
                              width: 1.6,
                            ),
                          ),
                          disabledBorder: const UnderlineInputBorder(
                            borderSide: BorderSide(
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
        ),
        const Positioned(
          top: -12,
          child: TapeStrip(width: 92, angle: 0.04, seed: 3),
        ),
      ],
    );
  }
}

class _OpenButton extends StatelessWidget {
  const _OpenButton({required this.joining, required this.onPressed});

  final bool joining;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: double.infinity,
      child: CardboardCard(
        key: CoverScreen.openKey,
        onTap: joining ? null : onPressed,
        padding: const EdgeInsets.symmetric(vertical: 14),
        child: Center(
          child: joining
              ? Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: NotebookColors.cardboardInk,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Text(
                      'BUSCANDO LA PARTIDA…',
                      style: typewriterStyle(
                        size: 15,
                        color: NotebookColors.cardboardInk,
                      ),
                    ),
                  ],
                )
              : Text(
                  'ABRIR LA LIBRETA',
                  style: typewriterStyle(
                    size: 16,
                    color: NotebookColors.cardboardInk,
                    letterSpacing: 2,
                  ),
                ),
        ),
      ),
    );
  }
}

class _UpperCase extends TextInputFormatter {
  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) => newValue.copyWith(text: newValue.text.toUpperCase());
}
