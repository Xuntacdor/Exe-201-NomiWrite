import 'dart:io';

class AppConfig {
  const AppConfig._();

  static const _definedBaseUrl = String.fromEnvironment('API_BASE_URL');

  static String get apiBaseUrl {
    if (_definedBaseUrl.isNotEmpty) return _definedBaseUrl;
    if (Platform.isAndroid) return 'http://10.0.2.2:5097';
    return 'http://localhost:5097';
  }
}
