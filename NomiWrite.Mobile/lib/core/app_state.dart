import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'api_client.dart';
import 'models.dart';

class AppState extends ChangeNotifier {
  AppState(this.api);
  final ApiClient api;
  final _storage = const FlutterSecureStorage(
    aOptions: AndroidOptions(encryptedSharedPreferences: true),
  );
  static const _sessionKey = 'nomiwrite_session';

  AuthSession? session;
  Json? profile;
  bool ready = false;
  bool busy = false;
  String? error;
  bool get signedIn => session != null;

  Future<void> restore() async {
    try {
      final raw = await _storage.read(key: _sessionKey);
      if (raw != null) {
        session = AuthSession.fromJson(
          Map<String, dynamic>.from(jsonDecode(raw)),
        );
        api.session = session;
        try {
          await loadProfile();
        } on ApiException catch (e) {
          if (e.statusCode == 401 && session!.refreshToken.isNotEmpty) {
            await _acceptSession(await api.refresh(session!.refreshToken));
            await loadProfile();
          }
        }
      }
    } catch (_) {
      await _storage.delete(key: _sessionKey);
      session = null;
    } finally {
      ready = true;
      notifyListeners();
    }
  }

  Future<void> login(String email, String password) =>
      _run(() async => _acceptSession(await api.login(email, password)));
  Future<void> register(String name, String email, String password) => _run(
    () async => _acceptSession(await api.register(name, email, password)),
  );

  Future<void> _acceptSession(AuthSession value) async {
    session = value;
    api.session = value;
    await _storage.write(key: _sessionKey, value: jsonEncode(value.toJson()));
    await loadProfile();
  }

  Future<void> loadProfile() async {
    profile = await api.getProfile();
    notifyListeners();
  }

  Future<void> signOut() async {
    try {
      await api.logout();
    } catch (_) {}
    session = null;
    profile = null;
    api.session = null;
    await _storage.delete(key: _sessionKey);
    notifyListeners();
  }

  Future<void> _run(Future<void> Function() action) async {
    busy = true;
    error = null;
    notifyListeners();
    try {
      await action();
    } catch (e) {
      error = e.toString();
      rethrow;
    } finally {
      busy = false;
      notifyListeners();
    }
  }
}
