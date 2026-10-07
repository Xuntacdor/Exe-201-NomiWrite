import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:nomiwrite_mobile/core/api_client.dart';
import 'package:nomiwrite_mobile/core/models.dart';

void main() {
  group('ApiClient', () {
    test('refreshes an expired session once and retries the request', () async {
      var overviewCalls = 0;
      AuthSession? persisted;
      final client = ApiClient(
        client: MockClient((request) async {
          if (request.url.path == '/api/auth/refresh') {
            expect(jsonDecode(request.body)['refreshToken'], 'refresh-old');
            return http.Response(
              jsonEncode({
                'accessToken': 'access-new',
                'refreshToken': 'refresh-new',
                'expiresAt': '2027-01-01T00:00:00Z',
                'userId': 'user-1',
                'email': 'admin@nomiwrite.test',
                'fullName': 'Admin',
                'role': 'Admin',
              }),
              200,
            );
          }
          if (request.url.path == '/api/admin/analytics/overview') {
            overviewCalls++;
            if (overviewCalls == 1) {
              expect(request.headers['authorization'], 'Bearer access-old');
              return http.Response('{}', 401);
            }
            expect(request.headers['authorization'], 'Bearer access-new');
            return http.Response(jsonEncode({'totalUsers': 12}), 200);
          }
          return http.Response('not found', 404);
        }),
      );
      client.session = const AuthSession(
        accessToken: 'access-old',
        refreshToken: 'refresh-old',
        expiresAt: '2026-01-01T00:00:00Z',
        userId: 'user-1',
        email: 'admin@nomiwrite.test',
        fullName: 'Admin',
        role: 'Admin',
      );
      client.onSessionRefreshed = (session) async => persisted = session;

      final overview = await client.adminOverview();

      expect(overview['totalUsers'], 12);
      expect(overviewCalls, 2);
      expect(client.session?.accessToken, 'access-new');
      expect(persisted?.refreshToken, 'refresh-new');
    });

    test(
      'uses the gateway contracts for promo and admin prompt updates',
      () async {
        final requests = <http.Request>[];
        final client = ApiClient(
          client: MockClient((request) async {
            requests.add(request);
            if (request.method == 'GET') {
              return http.Response(
                jsonEncode({'valid': true, 'discountPercent': 20}),
                200,
              );
            }
            return http.Response(jsonEncode({'id': 'prompt-1'}), 200);
          }),
        );
        client.session = const AuthSession(
          accessToken: 'token',
          refreshToken: 'refresh',
          expiresAt: '2027-01-01T00:00:00Z',
          userId: 'user-1',
          email: 'admin@nomiwrite.test',
          fullName: 'Admin',
          role: 'Admin',
        );

        final promo = await client.validatePromoCode('SAVE 20');
        await client.updateAdminPrompt('prompt-1', {
          'writingTypeId': 'type-1',
          'title': 'Updated prompt',
          'instructions': 'Write an essay.',
          'difficulty': 1,
          'isVipOnly': false,
        });

        expect(promo['discountPercent'], 20);
        expect(
          requests.first.url.toString(),
          contains('/api/subscriptions/promo-codes/SAVE%2020/validate'),
        );
        expect(requests.last.method, 'PUT');
        expect(requests.last.url.path, '/api/admin/prompts/prompt-1');
        expect(requests.last.headers['authorization'], 'Bearer token');
        expect(jsonDecode(requests.last.body)['title'], 'Updated prompt');
      },
    );
  });
}
