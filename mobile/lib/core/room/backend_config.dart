/// Dirección del backend. Se cambia al compilar con
/// `--dart-define=VESTIGIA_URL=http://10.0.2.2:8080` (emulador, backend local).
class BackendConfig {
  const BackendConfig({this.baseUrl = defaultBaseUrl});

  static const defaultBaseUrl = String.fromEnvironment(
    'VESTIGIA_URL',
    defaultValue: 'https://vestigia-backend-production-745d.up.railway.app',
  );

  final String baseUrl;

  String get _trimmed =>
      baseUrl.endsWith('/') ? baseUrl.substring(0, baseUrl.length - 1) : baseUrl;

  Uri rest(String path) => Uri.parse('$_trimmed$path');

  Uri get webSocketUri {
    final base = Uri.parse(_trimmed);
    return base.replace(
      scheme: base.scheme == 'https' ? 'wss' : 'ws',
      path: '${base.path}/ws',
    );
  }
}
