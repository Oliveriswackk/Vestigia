import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../app/puzzle_top_bar.dart';
import '../app/vestigia_theme.dart';
import '../core/room/room_connection.dart';
import '../core/room/room_models.dart';

/// Pantalla provisional para probar la conexión entre el juego y la app:
/// se escribe el código de sala (el pueblo elegido en la PC), se une y se
/// ven el estado de la conexión y los mensajes que llegan.
class RoomScreen extends StatefulWidget {
  const RoomScreen({super.key, required this.connection});

  final RoomConnection connection;

  static const codeFieldKey = Key('room-code');
  static const joinKey = Key('room-join');
  static const leaveKey = Key('room-leave');
  static const statusKey = Key('room-status');
  static const errorKey = Key('room-error');
  static const logKey = Key('room-log');

  @override
  State<RoomScreen> createState() => _RoomScreenState();
}

class _RoomScreenState extends State<RoomScreen> {
  final _controller = TextEditingController();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _join() {
    FocusScope.of(context).unfocus();
    widget.connection.join(_controller.text);
  }

  @override
  Widget build(BuildContext context) {
    final connection = widget.connection;
    return Scaffold(
      body: SafeArea(
        child: Column(
          children: [
            PuzzleTopBar(
              title: 'Sala',
              onBack: () => Navigator.of(context).pop(),
              onRestart: connection.leave,
              restartKey: RoomScreen.leaveKey,
              restartTooltip: 'Salir de la sala',
              restartIcon: Icons.logout,
            ),
            Expanded(
              child: ListenableBuilder(
                listenable: connection,
                builder: (context, _) {
                  final joined = connection.inRoom;
                  return ListView(
                    padding: const EdgeInsets.fromLTRB(20, 16, 20, 24),
                    children: [
                      if (joined)
                        _RoomPanel(connection: connection)
                      else
                        _JoinPanel(
                          connection: connection,
                          controller: _controller,
                          onJoin: _join,
                        ),
                      const SizedBox(height: 16),
                      Text(
                        'Servidor: ${connection.config.baseUrl}',
                        textAlign: TextAlign.center,
                        style: const TextStyle(
                          fontSize: 12,
                          color: VestigiaColors.creamMuted,
                        ),
                      ),
                    ],
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _Panel extends StatelessWidget {
  const _Panel({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: VestigiaColors.wood,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: VestigiaColors.brassDark, width: 1.5),
      ),
      child: child,
    );
  }
}

class _Label extends StatelessWidget {
  const _Label(this.text);

  final String text;

  @override
  Widget build(BuildContext context) {
    return Text(
      text,
      style: const TextStyle(
        fontSize: 13,
        letterSpacing: 3,
        color: VestigiaColors.brass,
      ),
    );
  }
}

class _UpperCaseNoSpaces extends TextInputFormatter {
  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    final text = normalizeRoomCode(newValue.text);
    if (text == newValue.text) return newValue;
    final cursor = newValue.selection.baseOffset
        .clamp(0, newValue.text.length)
        .toInt();
    return TextEditingValue(
      text: text,
      selection: TextSelection.collapsed(
        offset: normalizeRoomCode(newValue.text.substring(0, cursor)).length,
      ),
    );
  }
}

class _JoinPanel extends StatelessWidget {
  const _JoinPanel({
    required this.connection,
    required this.controller,
    required this.onJoin,
  });

  final RoomConnection connection;
  final TextEditingController controller;
  final VoidCallback onJoin;

  @override
  Widget build(BuildContext context) {
    final joining = connection.phase == RoomPhase.joining;
    final error = connection.error;
    const codeStyle = TextStyle(
      fontSize: 30,
      letterSpacing: 6,
      fontWeight: FontWeight.w600,
      color: VestigiaColors.cream,
    );
    OutlineInputBorder border(Color color) => OutlineInputBorder(
      borderRadius: BorderRadius.circular(10),
      borderSide: BorderSide(color: color, width: 1.5),
    );

    return _Panel(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const _Label('CÓDIGO DE SALA'),
          const SizedBox(height: 8),
          const Text(
            'Escribe el pueblo que elegiste en el juego de la PC.',
            style: TextStyle(fontSize: 14, color: VestigiaColors.creamMuted),
          ),
          const SizedBox(height: 20),
          TextField(
            key: RoomScreen.codeFieldKey,
            controller: controller,
            enabled: !joining,
            autocorrect: false,
            enableSuggestions: false,
            textAlign: TextAlign.center,
            textCapitalization: TextCapitalization.characters,
            textInputAction: TextInputAction.go,
            inputFormatters: [_UpperCaseNoSpaces()],
            onSubmitted: (_) => onJoin(),
            cursorColor: VestigiaColors.brass,
            style: codeStyle,
            decoration: InputDecoration(
              filled: true,
              fillColor: VestigiaColors.glass,
              hintText: 'TAPALPA',
              hintStyle: codeStyle.copyWith(
                color: VestigiaColors.creamMuted.withValues(alpha: 0.35),
              ),
              contentPadding: const EdgeInsets.symmetric(vertical: 18),
              enabledBorder: border(VestigiaColors.brassDark),
              focusedBorder: border(VestigiaColors.brass),
              disabledBorder: border(VestigiaColors.brassDark),
            ),
          ),
          if (error != null) ...[
            const SizedBox(height: 14),
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Icon(
                  Icons.error_outline,
                  size: 20,
                  color: VestigiaColors.needle,
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    error,
                    key: RoomScreen.errorKey,
                    style: const TextStyle(
                      fontSize: 14,
                      color: VestigiaColors.needle,
                    ),
                  ),
                ),
              ],
            ),
          ],
          const SizedBox(height: 20),
          FilledButton(
            key: RoomScreen.joinKey,
            onPressed: joining ? null : onJoin,
            child: joining
                ? const SizedBox(
                    width: 22,
                    height: 22,
                    child: CircularProgressIndicator(
                      strokeWidth: 2.5,
                      color: VestigiaColors.background,
                    ),
                  )
                : const Text('Unirme'),
          ),
        ],
      ),
    );
  }
}

class _RoomPanel extends StatelessWidget {
  const _RoomPanel({required this.connection});

  final RoomConnection connection;

  @override
  Widget build(BuildContext context) {
    final (statusText, statusColor) = switch (connection.phase) {
      RoomPhase.connected => ('Conectada', VestigiaColors.signal),
      RoomPhase.reconnecting => ('Reconectando…', VestigiaColors.glow),
      _ => ('Conectando…', VestigiaColors.glow),
    };
    final snapshot = connection.snapshot;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        _Panel(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const _Label('SALA'),
              const SizedBox(height: 8),
              Text(
                connection.code ?? '',
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 34,
                  letterSpacing: 6,
                  fontWeight: FontWeight.w600,
                  color: VestigiaColors.cream,
                ),
              ),
              const SizedBox(height: 14),
              _StatusRow(
                label: 'App',
                value: statusText,
                color: statusColor,
                valueKey: RoomScreen.statusKey,
              ),
              _StatusRow(
                label: 'PC (juego)',
                value: connection.gameConnected ? 'Conectada' : 'Sin conexión',
                color: connection.gameConnected
                    ? VestigiaColors.signal
                    : VestigiaColors.creamMuted,
              ),
              _StatusRow(
                label: 'Escena',
                value: snapshot?.scene ?? '—',
                color: VestigiaColors.cream,
              ),
            ],
          ),
        ),
        const SizedBox(height: 14),
        _Panel(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const _Label('PUZZLES'),
              const SizedBox(height: 8),
              if (snapshot == null || snapshot.puzzles.isEmpty)
                const Text(
                  '—',
                  style: TextStyle(color: VestigiaColors.creamMuted),
                )
              else
                for (final entry in snapshot.puzzles.entries)
                  _StatusRow(
                    label: entry.key,
                    value: entry.value,
                    color: switch (entry.value) {
                      'resuelto' => VestigiaColors.signal,
                      'disponible' || 'en_curso' => VestigiaColors.glow,
                      _ => VestigiaColors.creamMuted,
                    },
                  ),
              const SizedBox(height: 14),
              const _Label('DESCUBRIMIENTOS'),
              const SizedBox(height: 8),
              if (snapshot == null || snapshot.discoveries.isEmpty)
                const Text(
                  '—',
                  style: TextStyle(color: VestigiaColors.creamMuted),
                )
              else
                for (final entry in snapshot.discoveries.entries)
                  _StatusRow(
                    label: entry.key,
                    value: entry.value.isEmpty ? '✓' : entry.value,
                    color: VestigiaColors.cream,
                  ),
            ],
          ),
        ),
        const SizedBox(height: 14),
        _Panel(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const _Label('REGISTRO'),
              const SizedBox(height: 10),
              Container(
                key: RoomScreen.logKey,
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: VestigiaColors.glass,
                  borderRadius: BorderRadius.circular(8),
                ),
                child: connection.log.isEmpty
                    ? const Text(
                        'Sin mensajes todavía.',
                        style: TextStyle(color: VestigiaColors.creamMuted),
                      )
                    : Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          for (final entry in connection.log)
                            Padding(
                              padding: const EdgeInsets.symmetric(vertical: 2),
                              child: Text(
                                '${_clock(entry.time)}  ${entry.text}',
                                style: const TextStyle(
                                  fontSize: 13,
                                  fontFamily: 'monospace',
                                  color: VestigiaColors.cream,
                                ),
                              ),
                            ),
                        ],
                      ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _StatusRow extends StatelessWidget {
  const _StatusRow({
    required this.label,
    required this.value,
    required this.color,
    this.valueKey,
  });

  final String label;
  final String value;
  final Color color;
  final Key? valueKey;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          Container(
            width: 10,
            height: 10,
            decoration: BoxDecoration(shape: BoxShape.circle, color: color),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              label,
              style: const TextStyle(
                fontSize: 15,
                color: VestigiaColors.creamMuted,
              ),
            ),
          ),
          Text(
            value,
            key: valueKey,
            style: TextStyle(fontSize: 15, color: color),
          ),
        ],
      ),
    );
  }
}

String _clock(DateTime time) {
  String two(int n) => n.toString().padLeft(2, '0');
  return '${two(time.hour)}:${two(time.minute)}:${two(time.second)}';
}
