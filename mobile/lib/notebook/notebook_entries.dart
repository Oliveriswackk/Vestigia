import '../core/room/room_models.dart';

/// Lo que aparece escrito o pegado en la libreta. Se deriva por completo
/// del estado de la sala: cada descubrimiento del juego deja una anotación
/// y cada puzzle disponible deja una ficha de cartón.
sealed class NotebookItem {
  const NotebookItem(this.id);

  /// Estable entre actualizaciones, para animar solo lo nuevo.
  final String id;
}

/// Una nota a mano.
class NoteItem extends NotebookItem {
  const NoteItem(super.id, this.text);

  final String text;
}

/// Una coordenada, escrita grande y encerrada en un círculo.
class CoordinateItem extends NotebookItem {
  const CoordinateItem(
    super.id, {
    required this.label,
    required this.value,
    this.note,
  });

  final String label;
  final String value;
  final String? note;
}

/// Un pedazo de la fotografía pegado con cinta (`piece` empieza en 0).
class PhotoPieceItem extends NotebookItem {
  const PhotoPieceItem(super.id, {required this.piece, required this.caption});

  final int piece;
  final String caption;
}

/// Ficha de un puzzle que ya se puede jugar en el teléfono.
class PuzzleItem extends NotebookItem {
  const PuzzleItem(
    super.id, {
    required this.puzzle,
    required this.title,
    required this.hint,
    required this.state,
  });

  final String puzzle;
  final String title;
  final String hint;

  /// `disponible`, `en_curso` o `resuelto`.
  final String state;
}

/// Títulos de capítulo según la escena del juego.
const sceneTitles = {
  'introduccion': 'Prólogo',
  'pueblo': 'El pueblo',
  'faro': 'El faro',
  'observatorio': 'El observatorio',
  'desenlace': 'Desenlace',
};

/// Fichas de los puzzles. `after` son los descubrimientos que los
/// desbloquean: la ficha se pega justo después del último de ellos.
const puzzleCards = {
  'radio': (
    title: 'La radio del pescador',
    hint: 'Sintonizar la señal entre la estática.',
    after: ['pescador_dialogo_completo'],
  ),
  'photo': (
    title: 'La fotografía rasgada',
    hint: 'Unir los tres pedazos.',
    after: ['pieza_1', 'pieza_2', 'pieza_3'],
  ),
  'lookout': (
    title: 'El mirador del faro',
    hint: 'Limpiar el lente y buscar en el horizonte.',
    after: ['lookout_enabled', 'observatorio_visible'],
  ),
  'observatory': (
    title: 'El telescopio',
    hint: 'Apuntar con las coordenadas anotadas.',
    after: ['observatory_enabled'],
  ),
};

const _pieceCaptions = [
  'Encontrada en el primer piso del faro.',
  'Estaba en el segundo piso, bajo la escalera.',
  'La última, arriba, junto a la lente.',
];

/// Convierte un descubrimiento en su anotación. Los que no tienen texto
/// (marcas internas del juego) devuelven `null` y no se muestran.
NotebookItem? itemForDiscovery(String key, String value) {
  switch (key) {
    case 'llave_faro':
      return const NoteItem(
        'llave_faro',
        'En el primer piso del faro encontré una llave vieja, '
            'con óxido en los dientes. Abre la reja de la escalera.',
      );
    case 'pescador_dialogo_completo':
      return const NoteItem(
        'pescador_dialogo_completo',
        'El pescador dice que su radio primero cortaba la música y ahora '
            'solo hace ruido. Entre la estática algo se repite… '
            'Me dejó revisarla.',
      );
    case 'coordenada_a':
      return CoordinateItem(
        key,
        label: 'Ascensión recta',
        value: '${value.isEmpty ? '???' : value}°',
        note: 'La voz de la radio repetía estos números.',
      );
    case 'pieza_1' || 'pieza_2' || 'pieza_3':
      final index = int.parse(key.substring(6)) - 1;
      return PhotoPieceItem(key, piece: index, caption: _pieceCaptions[index]);
    case 'coordenada_b':
      return CoordinateItem(
        key,
        label: 'Declinación',
        value: '${value.isEmpty ? '???' : value}°',
        note: 'Escrito con lápiz detrás de la foto.',
      );
    case 'lookout_enabled':
      return const NoteItem(
        'lookout_enabled',
        'Arriba del faro hay un mirador. El lente está cubierto de sal.',
      );
    case 'observatorio_visible':
      return const NoteItem(
        'observatorio_visible',
        'Desde el mirador se ve el observatorio en la colina. '
            'Algo brilló entre las nubes, justo encima.',
      );
    case 'observatory_enabled':
      return const NoteItem(
        'observatory_enabled',
        'La cúpula del observatorio está abierta. '
            'Con las dos coordenadas puedo apuntar el telescopio.',
      );
    case 'magnetar_localizado':
      return const NoteItem(
        'magnetar_localizado',
        'Lo encontré. No es una estrella: es un magnetar.',
      );
  }
  return null;
}

/// Arma la página completa a partir del estado de la sala.
List<NotebookItem> buildNotebook(RoomSnapshot? snapshot) {
  if (snapshot == null) return const [];

  final items = <NotebookItem>[];
  // Dónde termina cada descubrimiento en la página (punto de inserción
  // justo después de su anotación, aunque no tenga texto).
  final after = <String, int>{};
  snapshot.discoveries.forEach((key, value) {
    final item = itemForDiscovery(key, value);
    if (item != null) items.add(item);
    after[key] = items.length;
  });

  // Fichas de puzzle: se insertan de atrás hacia adelante para no mover
  // las posiciones calculadas de las demás.
  final cards = <(int, PuzzleItem)>[];
  puzzleCards.forEach((puzzle, card) {
    final state = snapshot.puzzles[puzzle];
    if (state == null || state == 'bloqueado') return;
    var at = -1;
    for (final key in card.after) {
      final p = after[key];
      if (p != null && p > at) at = p;
    }
    if (at < 0) at = items.length;
    cards.add((
      at,
      PuzzleItem(
        'puzzle_$puzzle',
        puzzle: puzzle,
        title: card.title,
        hint: card.hint,
        state: state,
      ),
    ));
  });
  cards.sort((a, b) => b.$1.compareTo(a.$1));
  for (final (at, card) in cards) {
    items.insert(at.clamp(0, items.length), card);
  }
  return items;
}
