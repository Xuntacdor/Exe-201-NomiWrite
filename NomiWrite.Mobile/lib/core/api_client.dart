import 'dart:convert';
import 'package:http/http.dart' as http;
import 'app_config.dart';
import 'models.dart';

class ApiException implements Exception {
  ApiException(this.message, this.statusCode);
  final String message;
  final int statusCode;
  @override
  String toString() => message;
}

class ApiClient {
  ApiClient({http.Client? client}) : _client = client ?? http.Client();
  final http.Client _client;
  AuthSession? session;

  Future<dynamic> request(
    String path, {
    String method = 'GET',
    Object? body,
    bool auth = false,
    Map<String, String>? query,
  }) async {
    final uri = Uri.parse(
      '${AppConfig.apiBaseUrl}$path',
    ).replace(queryParameters: query);
    final headers = <String, String>{
      'Accept': 'application/json',
      'Content-Type': 'application/json',
    };
    if (auth && session?.accessToken.isNotEmpty == true)
      headers['Authorization'] = 'Bearer ${session!.accessToken}';
    http.Response response;
    final encoded = body == null ? null : jsonEncode(body);
    switch (method) {
      case 'POST':
        response = await _client.post(uri, headers: headers, body: encoded);
        break;
      case 'PUT':
        response = await _client.put(uri, headers: headers, body: encoded);
        break;
      case 'PATCH':
        response = await _client.patch(uri, headers: headers, body: encoded);
        break;
      case 'DELETE':
        response = await _client.delete(uri, headers: headers, body: encoded);
        break;
      default:
        response = await _client.get(uri, headers: headers);
    }
    dynamic data;
    if (response.body.isNotEmpty) {
      try {
        data = jsonDecode(response.body);
      } catch (_) {
        data = response.body;
      }
    }
    if (response.statusCode < 200 || response.statusCode >= 300) {
      var message = 'Không thể kết nối máy chủ (${response.statusCode}).';
      if (data is Map) {
        message = asText(data['message'], message);
        if (data['errors'] is List)
          message = (data['errors'] as List).join(' ');
      } else if (data is String && data.isNotEmpty) {
        message = data;
      }
      throw ApiException(message, response.statusCode);
    }
    return data;
  }

  dynamic unwrap(dynamic data) =>
      data is Map && data.containsKey('data') ? data['data'] : data;

  Future<AuthSession> login(String email, String password) async =>
      AuthSession.fromJson(
        Map<String, dynamic>.from(
          await request(
            '/api/auth/login',
            method: 'POST',
            body: {'email': email, 'password': password},
          ),
        ),
      );
  Future<AuthSession> register(
    String name,
    String email,
    String password,
  ) async => AuthSession.fromJson(
    Map<String, dynamic>.from(
      await request(
        '/api/auth/register',
        method: 'POST',
        body: {'fullName': name, 'email': email, 'password': password},
      ),
    ),
  );
  Future<AuthSession> refresh(String token) async => AuthSession.fromJson(
    Map<String, dynamic>.from(
      await request(
        '/api/auth/refresh',
        method: 'POST',
        body: {'refreshToken': token},
      ),
    ),
  );
  Future<void> logout() async =>
      request('/api/auth/logout', method: 'POST', auth: true);

  Future<Json> getProfile() async =>
      Map<String, dynamic>.from(await request('/api/users/me', auth: true));
  Future<Json> getProgress() async => Map<String, dynamic>.from(
    await request('/api/users/me/progress', auth: true),
  );
  Future<Json> updateProfile(Json data) async => Map<String, dynamic>.from(
    await request('/api/users/me', method: 'PUT', body: data, auth: true),
  );
  Future<List<WritingType>> writingTypes() async =>
      (await request('/api/writing/types') as List)
          .map((e) => WritingType.fromJson(Map<String, dynamic>.from(e)))
          .toList();
  Future<List<WritingPrompt>> prompts({
    String? typeId,
    String? difficulty,
  }) async =>
      (await request(
                '/api/writing/prompts',
                query: {
                  if (typeId != null) 'typeId': typeId,
                  if (difficulty != null) 'difficulty': difficulty,
                },
              )
              as List)
          .map((e) => WritingPrompt.fromJson(Map<String, dynamic>.from(e)))
          .toList();
  Future<WritingPrompt> prompt(String id) async => WritingPrompt.fromJson(
    Map<String, dynamic>.from(await request('/api/writing/prompts/$id')),
  );
  Future<Submission> submitEssay(
    String promptId,
    String content,
    bool timed,
  ) async {
    final created = Map<String, dynamic>.from(
      await request(
        '/api/writing/submissions',
        method: 'POST',
        body: {'writingPromptId': promptId, 'isTimed': timed},
        auth: true,
      ),
    );
    final id = asText(created['id']);
    await request(
      '/api/writing/submissions/$id',
      method: 'PUT',
      body: {'content': content},
      auth: true,
    );
    return Submission.fromJson(
      Map<String, dynamic>.from(
        await request(
          '/api/writing/submissions/$id/submit',
          method: 'POST',
          auth: true,
        ),
      ),
    );
  }

  Future<List<Submission>> submissions() async =>
      (await request('/api/writing/submissions', auth: true) as List)
          .map((e) => Submission.fromJson(Map<String, dynamic>.from(e)))
          .toList();
  Future<Json> feedback(String id) async => Map<String, dynamic>.from(
    unwrap(await request('/api/grading/submissions/$id', auth: true)),
  );
  Future<List<VocabularyItem>> vocabulary() async {
    final value = unwrap(
      await request(
        '/api/vocabulary',
        auth: true,
        query: {'page': '1', 'pageSize': '200'},
      ),
    );
    final list = value is Map ? value['items'] : value;
    return (list as List)
        .map((e) => VocabularyItem.fromJson(Map<String, dynamic>.from(e)))
        .toList();
  }

  Future<VocabularyItem> setMastered(String id, bool value) async =>
      VocabularyItem.fromJson(
        Map<String, dynamic>.from(
          unwrap(
            await request(
              '/api/vocabulary/$id/mastered',
              method: 'PATCH',
              body: {'isMastered': value},
              auth: true,
            ),
          ),
        ),
      );
  Future<List<QuizSummary>> quizzes() async =>
      (unwrap(await request('/api/quizzes', auth: true)) as List)
          .map((e) => QuizSummary.fromJson(Map<String, dynamic>.from(e)))
          .toList();
  Future<Json> quiz(String id) async => Map<String, dynamic>.from(
    unwrap(await request('/api/quizzes/$id', auth: true)),
  );
  Future<Json> generateQuiz({
    String? submissionId,
    List<String>? vocabularyIds,
  }) async => Map<String, dynamic>.from(
    unwrap(
      await request(
        '/api/quizzes/generate',
        method: 'POST',
        body: {
          if (submissionId != null) 'sourceSubmissionId': submissionId,
          if (vocabularyIds != null) 'vocabularyIds': vocabularyIds,
        },
        auth: true,
      ),
    ),
  );
  Future<Json> submitQuiz(String id, Map<String, String> answers) async =>
      Map<String, dynamic>.from(
        unwrap(
          await request(
            '/api/quizzes/attempts',
            method: 'POST',
            body: {'quizId': id, 'answers': answers},
            auth: true,
          ),
        ),
      );
  Future<Json?> studyGuide() async {
    try {
      return Map<String, dynamic>.from(
        unwrap(await request('/api/study-guides', auth: true)),
      );
    } on ApiException catch (e) {
      if (e.statusCode == 404) return null;
      rethrow;
    }
  }

  Future<Json> generateStudyGuide() async => Map<String, dynamic>.from(
    unwrap(
      await request(
        '/api/study-guides/generate',
        method: 'POST',
        body: {'forceRefresh': true},
        auth: true,
      ),
    ),
  );
  Future<List<Json>> plans() async =>
      asJsonList(await request('/api/subscriptions/plans'));
  Future<Json?> subscription() async {
    final value = await request('/api/subscriptions/me', auth: true);
    return value == null ? null : Map<String, dynamic>.from(value);
  }

  Future<Json> checkout(
    String planId,
    num amount,
    String provider, {
    String? promoCode,
  }) async => Map<String, dynamic>.from(
    await request(
      '/api/payment',
      method: 'POST',
      auth: true,
      body: {
        'planId': planId,
        'amount': amount,
        'currency': 'VND',
        'provider': provider,
        if (promoCode?.isNotEmpty == true) 'promoCode': promoCode,
      },
    ),
  );
}
