// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter_test/flutter_test.dart';
import 'package:flutter/material.dart';
import 'package:nomiwrite_mobile/ui/theme.dart';

void main() {
  testWidgets('renders NomiWrite brand', (WidgetTester tester) async {
    await tester.pumpWidget(
      MaterialApp(
        theme: NomiTheme.light,
        home: const Scaffold(body: BrandMark()),
      ),
    );
    expect(find.text('NomiWrite'), findsOneWidget);
    expect(find.byIcon(Icons.edit_rounded), findsOneWidget);
  });
}
