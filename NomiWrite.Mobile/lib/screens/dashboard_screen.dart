import 'package:flutter/material.dart';
import '../core/app_state.dart';
import '../core/models.dart';
import '../ui/theme.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({
    super.key,
    required this.state,
    required this.openTab,
  });
  final AppState state;
  final ValueChanged<int> openTab;
  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  late Future<List<dynamic>> data;
  @override
  void initState() {
    super.initState();
    data = _load();
  }

  Future<List<dynamic>> _load() => Future.wait([
    widget.state.api.getProgress(),
    widget.state.api.submissions(),
  ]);
  void _refresh() => setState(() => data = _load());

  @override
  Widget build(BuildContext context) => SafeArea(
    child: FutureBuilder<List<dynamic>>(
      future: data,
      builder: (context, snapshot) {
        final name = asText(
          widget.state.profile?['displayName'],
          widget.state.session?.fullName ?? 'Bạn',
        );
        return RefreshIndicator(
          onRefresh: () async {
            _refresh();
            await data;
          },
          child: CustomScrollView(
            slivers: [
              SliverAppBar(
                floating: true,
                title: const BrandMark(),
                actions: [
                  IconButton(
                    onPressed: _refresh,
                    icon: const Icon(Icons.refresh_rounded),
                  ),
                ],
              ),
              SliverPadding(
                padding: const EdgeInsets.fromLTRB(20, 12, 20, 28),
                sliver: SliverList(
                  delegate: SliverChildListDelegate([
                    Text(
                      'Xin chào, ${name.split(' ').last} 👋',
                      style: Theme.of(context).textTheme.headlineSmall
                          ?.copyWith(fontWeight: FontWeight.w900),
                    ),
                    const SizedBox(height: 6),
                    Text(
                      'Mỗi bài viết là một bước gần hơn tới mục tiêu.',
                      style: TextStyle(color: Colors.grey.shade600),
                    ),
                    const SizedBox(height: 20),
                    if (snapshot.connectionState == ConnectionState.waiting)
                      const Padding(
                        padding: EdgeInsets.all(48),
                        child: Center(child: CircularProgressIndicator()),
                      )
                    else if (snapshot.hasError)
                      ErrorView(error: snapshot.error!, retry: _refresh)
                    else
                      ..._content(
                        snapshot.data![0] as Json,
                        snapshot.data![1] as List<Submission>,
                      ),
                  ]),
                ),
              ),
            ],
          ),
        );
      },
    ),
  );

  List<Widget> _content(Json progress, List<Submission> submissions) {
    final history = asJsonList(progress['bandHistory']);
    final latestBand = history.isEmpty ? null : asDouble(history.last['band']);
    return [
      Row(
        children: [
          Expanded(
            child: _Metric(
              label: 'Bài đã viết',
              value: '${asInt(progress['totalSubmissions'])}',
              icon: Icons.description_outlined,
              color: NomiTheme.primary,
            ),
          ),
          const SizedBox(width: 8),
          Expanded(
            child: _Metric(
              label: 'Band mới nhất',
              value: latestBand?.toStringAsFixed(1) ?? '—',
              icon: Icons.auto_graph_rounded,
              color: NomiTheme.gold,
            ),
          ),
          const SizedBox(width: 8),
          Expanded(
            child: _Metric(
              label: 'Chuỗi ngày',
              value: '${asInt(progress['currentStreak'])}',
              icon: Icons.local_fire_department_outlined,
              color: Colors.deepOrange,
            ),
          ),
        ],
      ),
      const SizedBox(height: 20),
      Card(
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Sẵn sàng luyện viết?',
                style: TextStyle(fontWeight: FontWeight.w800, fontSize: 19),
              ),
              const SizedBox(height: 7),
              Text(
                'Chọn đề, viết bài và nhận phân tích chi tiết từ AI.',
                style: TextStyle(color: Colors.grey.shade600),
              ),
              const SizedBox(height: 18),
              FilledButton.icon(
                onPressed: () => widget.openTab(1),
                icon: const Icon(Icons.edit_rounded),
                label: const Text('Viết bài mới'),
              ),
            ],
          ),
        ),
      ),
      const SizedBox(height: 22),
      Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          const Text(
            'Bài viết gần đây',
            style: TextStyle(fontWeight: FontWeight.w800, fontSize: 19),
          ),
          TextButton(
            onPressed: () => widget.openTab(2),
            child: const Text('Xem tất cả'),
          ),
        ],
      ),
      if (submissions.isEmpty)
        const SizedBox(
          height: 180,
          child: EmptyState(
            icon: Icons.article_outlined,
            title: 'Chưa có bài viết',
            message: 'Bài đầu tiên của bạn sẽ xuất hiện ở đây.',
          ),
        )
      else
        ...submissions
            .take(3)
            .map(
              (s) => Card(
                child: ListTile(
                  contentPadding: const EdgeInsets.symmetric(
                    horizontal: 16,
                    vertical: 8,
                  ),
                  leading: CircleAvatar(
                    backgroundColor: NomiTheme.primary.withValues(alpha: .1),
                    child: const Icon(
                      Icons.article_outlined,
                      color: NomiTheme.primary,
                    ),
                  ),
                  title: Text(
                    s.title,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontWeight: FontWeight.w700),
                  ),
                  subtitle: Text('${s.wordCount} từ · ${s.status}'),
                  trailing: s.overallScore == null
                      ? const Icon(Icons.chevron_right)
                      : Text(
                          s.overallScore!.toStringAsFixed(1),
                          style: const TextStyle(
                            fontWeight: FontWeight.w900,
                            fontSize: 18,
                            color: NomiTheme.primary,
                          ),
                        ),
                ),
              ),
            ),
    ];
  }
}

class _Metric extends StatelessWidget {
  const _Metric({
    required this.label,
    required this.value,
    required this.icon,
    required this.color,
  });
  final String label, value;
  final IconData icon;
  final Color color;
  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 5),
      child: Column(
        children: [
          Icon(icon, color: color),
          const SizedBox(height: 8),
          Text(
            value,
            style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 21),
          ),
          const SizedBox(height: 3),
          Text(
            label,
            textAlign: TextAlign.center,
            style: TextStyle(color: Colors.grey.shade600, fontSize: 11),
          ),
        ],
      ),
    ),
  );
}
