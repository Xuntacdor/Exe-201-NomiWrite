import 'package:flutter/material.dart';
import 'core/api_client.dart';
import 'core/app_state.dart';
import 'screens/auth_screen.dart';
import 'screens/admin/admin_shell.dart';
import 'screens/home_shell.dart';
import 'ui/theme.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const NomiWriteApp());
}

class NomiWriteApp extends StatefulWidget {
  const NomiWriteApp({super.key});
  @override
  State<NomiWriteApp> createState() => _NomiWriteAppState();
}

class _NomiWriteAppState extends State<NomiWriteApp> {
  late final AppState state;
  @override
  void initState() {
    super.initState();
    state = AppState(ApiClient());
    state.restore();
  }

  @override
  void dispose() {
    state.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => MaterialApp(
    title: 'NomiWrite',
    debugShowCheckedModeBanner: false,
    theme: NomiTheme.light,
    home: AnimatedBuilder(
      animation: state,
      builder: (_, _) {
        if (!state.ready) {
          return const Scaffold(
            body: Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  BrandMark(),
                  SizedBox(height: 24),
                  CircularProgressIndicator(),
                ],
              ),
            ),
          );
        }
        if (!state.signedIn) return AuthScreen(state: state);
        return state.isAdmin
            ? AdminShell(state: state)
            : HomeShell(state: state);
      },
    ),
  );
}
