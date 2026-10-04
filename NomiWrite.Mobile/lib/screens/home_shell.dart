import 'package:flutter/material.dart';
import '../core/app_state.dart';
import 'dashboard_screen.dart';
import 'history_screen.dart';
import 'learning_screen.dart';
import 'profile_screen.dart';
import 'writing_screen.dart';

class HomeShell extends StatefulWidget {
  const HomeShell({super.key, required this.state});
  final AppState state;
  @override
  State<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends State<HomeShell> {
  int index = 0;
  late final List<Widget> pages = [
    DashboardScreen(
      state: widget.state,
      openTab: (v) => setState(() => index = v),
    ),
    WritingScreen(state: widget.state),
    HistoryScreen(state: widget.state),
    LearningScreen(state: widget.state),
    ProfileScreen(state: widget.state),
  ];
  @override
  Widget build(BuildContext context) => Scaffold(
    body: IndexedStack(index: index, children: pages),
    bottomNavigationBar: NavigationBar(
      selectedIndex: index,
      onDestinationSelected: (v) => setState(() => index = v),
      destinations: const [
        NavigationDestination(
          icon: Icon(Icons.home_outlined),
          selectedIcon: Icon(Icons.home_rounded),
          label: 'Trang chủ',
        ),
        NavigationDestination(
          icon: Icon(Icons.edit_note_outlined),
          selectedIcon: Icon(Icons.edit_note_rounded),
          label: 'Luyện viết',
        ),
        NavigationDestination(
          icon: Icon(Icons.history_outlined),
          selectedIcon: Icon(Icons.history_rounded),
          label: 'Lịch sử',
        ),
        NavigationDestination(
          icon: Icon(Icons.school_outlined),
          selectedIcon: Icon(Icons.school_rounded),
          label: 'Học tập',
        ),
        NavigationDestination(
          icon: Icon(Icons.person_outline),
          selectedIcon: Icon(Icons.person),
          label: 'Cá nhân',
        ),
      ],
    ),
  );
}
