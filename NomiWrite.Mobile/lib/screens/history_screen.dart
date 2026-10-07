import 'package:flutter/material.dart';
import '../core/app_state.dart';
import '../core/models.dart';
import '../ui/theme.dart';
import 'writing_screen.dart';

class HistoryScreen extends StatefulWidget {
  const HistoryScreen({super.key, required this.state});
  final AppState state;
  @override
  State<HistoryScreen> createState() => _HistoryScreenState();
}

class _HistoryScreenState extends State<HistoryScreen> {
  late Future<List<Submission>> data;
  @override
  void initState() {
    super.initState();
    data = widget.state.api.submissions();
  }

  void reload() => setState(() => data = widget.state.api.submissions());
  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: Column(
        children: [
          AppBar(
            title: const Text(
              'Lịch sử bài viết',
              style: TextStyle(fontWeight: FontWeight.w900),
            ),
            actions: [
              IconButton(onPressed: reload, icon: const Icon(Icons.refresh)),
            ],
          ),
          Expanded(
            child: FutureBuilder<List<Submission>>(
              future: data,
              builder: (context, snap) {
                if (snap.connectionState == ConnectionState.waiting) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (snap.hasError) {
                  return ErrorView(error: snap.error!, retry: reload);
                }
                final items = snap.data!;
                if (items.isEmpty) {
                  return const EmptyState(
                    icon: Icons.history_edu,
                    title: 'Chưa có lịch sử',
                    message: 'Những bài bạn đã nộp sẽ được lưu tại đây.',
                  );
                }
                return RefreshIndicator(
                  onRefresh: () async {
                    reload();
                    await data;
                  },
                  child: ListView.separated(
                    padding: const EdgeInsets.all(20),
                    itemCount: items.length,
                    separatorBuilder: (_, _) => const SizedBox(height: 8),
                    itemBuilder: (_, i) {
                      final s = items[i];
                      final ready = s.status.toLowerCase() == 'graded';
                      return Card(
                        child: ListTile(
                          contentPadding: const EdgeInsets.all(14),
                          onTap: ready
                              ? () => Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (_) => SubmissionResultScreen(
                                      state: widget.state,
                                      submission: s,
                                    ),
                                  ),
                                )
                              : null,
                          leading: CircleAvatar(
                            backgroundColor:
                                (ready ? NomiTheme.mint : NomiTheme.gold)
                                    .withValues(alpha: .15),
                            child: Icon(
                              ready
                                  ? Icons.check_rounded
                                  : Icons.hourglass_top_rounded,
                              color: ready
                                  ? NomiTheme.primary
                                  : Colors.orange.shade800,
                            ),
                          ),
                          title: Text(
                            s.title,
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                            style: const TextStyle(fontWeight: FontWeight.w800),
                          ),
                          subtitle: Padding(
                            padding: const EdgeInsets.only(top: 6),
                            child: Text(
                              '${s.wordCount} từ · ${_status(s.status)}',
                            ),
                          ),
                          trailing: const Icon(Icons.chevron_right),
                        ),
                      );
                    },
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  String _status(String value) => switch (value.toLowerCase()) {
    'graded' => 'Đã chấm',
    'grading' || 'submitted' => 'Đang chấm',
    'failed' => 'Chấm lỗi',
    'draft' => 'Bản nháp',
    _ => value,
  };
}
