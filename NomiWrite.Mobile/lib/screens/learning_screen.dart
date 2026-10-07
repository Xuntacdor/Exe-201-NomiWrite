import 'package:flutter/material.dart';
import '../core/app_state.dart';
import '../core/models.dart';
import '../ui/theme.dart';

class LearningScreen extends StatelessWidget {
  const LearningScreen({super.key, required this.state});
  final AppState state;
  @override
  Widget build(BuildContext context) => DefaultTabController(
    length: 3,
    child: SafeArea(
      child: Column(
        children: [
          AppBar(
            title: const Text(
              'Trung tâm học tập',
              style: TextStyle(fontWeight: FontWeight.w900),
            ),
            bottom: const TabBar(
              tabs: [
                Tab(text: 'Từ vựng'),
                Tab(text: 'Quiz'),
                Tab(text: 'Lộ trình'),
              ],
            ),
          ),
          Expanded(
            child: TabBarView(
              children: [
                _VocabularyTab(state: state),
                _QuizTab(state: state),
                _GuideTab(state: state),
              ],
            ),
          ),
        ],
      ),
    ),
  );
}

class _VocabularyTab extends StatefulWidget {
  const _VocabularyTab({required this.state});
  final AppState state;
  @override
  State<_VocabularyTab> createState() => _VocabularyTabState();
}

class _VocabularyTabState extends State<_VocabularyTab> {
  late Future<List<VocabularyItem>> data;
  @override
  void initState() {
    super.initState();
    data = widget.state.api.vocabulary();
  }

  void reload() => setState(() => data = widget.state.api.vocabulary());
  @override
  Widget build(BuildContext context) => FutureBuilder<List<VocabularyItem>>(
    future: data,
    builder: (_, snap) {
      if (snap.connectionState == ConnectionState.waiting) {
        return const Center(child: CircularProgressIndicator());
      }
      if (snap.hasError) return ErrorView(error: snap.error!, retry: reload);
      if (snap.data!.isEmpty) {
        return const EmptyState(
          icon: Icons.menu_book_outlined,
          title: 'Chưa có từ gợi ý',
          message: 'Từ vựng hay sẽ được rút ra sau khi AI chấm bài.',
        );
      }
      return RefreshIndicator(
        onRefresh: () async {
          reload();
          await data;
        },
        child: ListView.builder(
          padding: const EdgeInsets.all(16),
          itemCount: snap.data!.length,
          itemBuilder: (_, i) {
            final v = snap.data![i];
            return Card(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 12, 8, 12),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          if (v.topic.isNotEmpty)
                            Text(
                              v.topic.toUpperCase(),
                              style: const TextStyle(
                                fontSize: 10,
                                fontWeight: FontWeight.w800,
                                color: NomiTheme.primary,
                              ),
                            ),
                          const SizedBox(height: 5),
                          Text(
                            '${v.original}  →  ${v.suggested}',
                            style: const TextStyle(
                              fontWeight: FontWeight.w800,
                              fontSize: 16,
                            ),
                          ),
                          if (v.example.isNotEmpty) ...[
                            const SizedBox(height: 7),
                            Text(
                              v.example,
                              style: TextStyle(
                                color: Colors.grey.shade700,
                                fontStyle: FontStyle.italic,
                              ),
                            ),
                          ],
                        ],
                      ),
                    ),
                    Checkbox(
                      value: v.mastered,
                      onChanged: (value) async {
                        try {
                          await widget.state.api.setMastered(
                            v.id,
                            value ?? false,
                          );
                          reload();
                        } catch (e) {
                          if (context.mounted) {
                            ScaffoldMessenger.of(
                              context,
                            ).showSnackBar(SnackBar(content: Text('$e')));
                          }
                        }
                      },
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      );
    },
  );
}

class _QuizTab extends StatefulWidget {
  const _QuizTab({required this.state});
  final AppState state;
  @override
  State<_QuizTab> createState() => _QuizTabState();
}

class _QuizTabState extends State<_QuizTab> {
  late Future<List<QuizSummary>> data;
  @override
  void initState() {
    super.initState();
    data = widget.state.api.quizzes();
  }

  void reload() => setState(() => data = widget.state.api.quizzes());
  Future<void> generate() async {
    try {
      final q = await widget.state.api.generateQuiz();
      if (mounted) {
        await Navigator.push(
          context,
          MaterialPageRoute(
            builder: (_) => QuizPlayScreen(state: widget.state, quiz: q),
          ),
        );
        reload();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$e')));
      }
    }
  }

  @override
  Widget build(BuildContext context) => FutureBuilder<List<QuizSummary>>(
    future: data,
    builder: (_, snap) {
      if (snap.connectionState == ConnectionState.waiting) {
        return const Center(child: CircularProgressIndicator());
      }
      if (snap.hasError) return ErrorView(error: snap.error!, retry: reload);
      return ListView(
        padding: const EdgeInsets.all(16),
        children: [
          FilledButton.icon(
            onPressed: generate,
            icon: const Icon(Icons.auto_awesome),
            label: const Text('Tạo quiz từ lỗi của tôi'),
          ),
          const SizedBox(height: 14),
          if (snap.data!.isEmpty)
            const SizedBox(
              height: 250,
              child: EmptyState(
                icon: Icons.quiz_outlined,
                title: 'Chưa có quiz',
                message: 'Tạo một bộ câu hỏi cá nhân hóa từ bài viết của bạn.',
              ),
            )
          else
            ...snap.data!.map(
              (q) => Card(
                child: ListTile(
                  contentPadding: const EdgeInsets.all(14),
                  leading: const CircleAvatar(child: Icon(Icons.quiz_outlined)),
                  title: Text(
                    q.category,
                    style: const TextStyle(fontWeight: FontWeight.w800),
                  ),
                  subtitle: Text(
                    '${q.questionCount} câu${q.latestScore == null ? '' : ' · Gần nhất ${q.latestScore}/${q.latestTotal}'}',
                  ),
                  trailing: const Icon(Icons.play_arrow_rounded),
                  onTap: () async {
                    final value = await widget.state.api.quiz(q.id);
                    if (context.mounted) {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (_) =>
                              QuizPlayScreen(state: widget.state, quiz: value),
                        ),
                      );
                    }
                  },
                ),
              ),
            ),
        ],
      );
    },
  );
}

class QuizPlayScreen extends StatefulWidget {
  const QuizPlayScreen({super.key, required this.state, required this.quiz});
  final AppState state;
  final Json quiz;
  @override
  State<QuizPlayScreen> createState() => _QuizPlayScreenState();
}

class _QuizPlayScreenState extends State<QuizPlayScreen> {
  final answers = <String, String>{};
  bool sending = false;
  @override
  Widget build(BuildContext context) {
    final questions = asJsonList(widget.quiz['questions']);
    return Scaffold(
      appBar: AppBar(title: const Text('Làm quiz')),
      body: ListView(
        padding: const EdgeInsets.all(18),
        children: [
          for (var i = 0; i < questions.length; i++) _question(i, questions[i]),
          const SizedBox(height: 12),
          FilledButton(
            onPressed: sending ? null : () => submit(questions),
            child: sending
                ? const CircularProgressIndicator(color: Colors.white)
                : const Text('Nộp đáp án'),
          ),
        ],
      ),
    );
  }

  Widget _question(int i, Json q) {
    final id = asText(q['id']);
    final options = (q['options'] as List?)?.map(asText).toList() ?? [];
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Câu ${i + 1}',
              style: const TextStyle(
                color: NomiTheme.primary,
                fontWeight: FontWeight.w800,
              ),
            ),
            const SizedBox(height: 7),
            Text(
              asText(q['question'] ?? q['sentence']),
              style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16),
            ),
            const SizedBox(height: 10),
            if (options.isNotEmpty)
              RadioGroup<String>(
                groupValue: answers[id],
                onChanged: (value) => setState(() {
                  if (value != null) answers[id] = value;
                }),
                child: Column(
                  children: options
                      .map(
                        (option) => RadioListTile<String>(
                          contentPadding: EdgeInsets.zero,
                          value: option,
                          title: Text(option),
                        ),
                      )
                      .toList(),
                ),
              )
            else
              TextField(
                decoration: const InputDecoration(hintText: 'Nhập câu trả lời'),
                onChanged: (v) => answers[id] = v,
              ),
          ],
        ),
      ),
    );
  }

  Future<void> submit(List<Json> questions) async {
    if (answers.length < questions.length) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Hãy trả lời tất cả câu hỏi.')),
      );
      return;
    }
    setState(() => sending = true);
    try {
      final result = await widget.state.api.submitQuiz(
        asText(widget.quiz['id']),
        answers,
      );
      if (mounted) {
        showDialog(
          context: context,
          builder: (_) => AlertDialog(
            title: const Text('Hoàn thành!'),
            content: Text(
              'Điểm của bạn: ${asText(result['score'])}/${asText(result['totalQuestions'], '${questions.length}')}',
            ),
            actions: [
              FilledButton(
                onPressed: () {
                  Navigator.pop(context);
                  Navigator.pop(context);
                },
                child: const Text('Xong'),
              ),
            ],
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$e')));
      }
    } finally {
      if (mounted) setState(() => sending = false);
    }
  }
}

class _GuideTab extends StatefulWidget {
  const _GuideTab({required this.state});
  final AppState state;
  @override
  State<_GuideTab> createState() => _GuideTabState();
}

class _GuideTabState extends State<_GuideTab> {
  late Future<Json?> data;
  bool generating = false;
  @override
  void initState() {
    super.initState();
    data = widget.state.api.studyGuide();
  }

  Future<void> generate() async {
    setState(() => generating = true);
    try {
      final guide = await widget.state.api.generateStudyGuide();
      setState(() => data = Future.value(guide));
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$e')));
      }
    } finally {
      if (mounted) setState(() => generating = false);
    }
  }

  @override
  Widget build(BuildContext context) => FutureBuilder<Json?>(
    future: data,
    builder: (_, snap) {
      if (snap.connectionState == ConnectionState.waiting) {
        return const Center(child: CircularProgressIndicator());
      }
      if (snap.hasError) {
        return ErrorView(
          error: snap.error!,
          retry: () => setState(() => data = widget.state.api.studyGuide()),
        );
      }
      final g = snap.data;
      if (g == null) {
        return EmptyStateWithAction(
          icon: Icons.route_outlined,
          title: 'Chưa có lộ trình',
          message:
              'AI sẽ phân tích bài viết để tạo kế hoạch học riêng cho bạn.',
          label: generating ? 'Đang tạo…' : 'Tạo lộ trình',
          action: generating ? null : generate,
        );
      }
      final steps = asJsonList(g['nextSteps']);
      return ListView(
        padding: const EdgeInsets.all(18),
        children: [
          Card(
            color: NomiTheme.primary,
            child: Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'BAND ƯỚC TÍNH',
                    style: TextStyle(
                      color: Colors.white70,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  Text(
                    (asDouble(g['estimatedBand']) ?? 0).toStringAsFixed(1),
                    style: const TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.w900,
                      fontSize: 42,
                    ),
                  ),
                  Text(
                    asText(g['summary']),
                    style: const TextStyle(color: Colors.white, height: 1.5),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          const Text(
            'Bước tiếp theo',
            style: TextStyle(fontWeight: FontWeight.w900, fontSize: 19),
          ),
          ...steps.asMap().entries.map(
            (e) => Card(
              child: ListTile(
                leading: CircleAvatar(child: Text('${e.key + 1}')),
                title: Text(
                  asText(e.value['title']),
                  style: const TextStyle(fontWeight: FontWeight.w800),
                ),
                subtitle: Text(asText(e.value['description'])),
              ),
            ),
          ),
          const SizedBox(height: 8),
          OutlinedButton.icon(
            onPressed: generating ? null : generate,
            icon: const Icon(Icons.refresh),
            label: const Text('Phân tích lại'),
          ),
        ],
      );
    },
  );
}

class EmptyStateWithAction extends StatelessWidget {
  const EmptyStateWithAction({
    super.key,
    required this.icon,
    required this.title,
    required this.message,
    required this.label,
    required this.action,
  });
  final IconData icon;
  final String title, message, label;
  final VoidCallback? action;
  @override
  Widget build(BuildContext context) => Center(
    child: Padding(
      padding: const EdgeInsets.all(30),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 58, color: NomiTheme.mint),
          const SizedBox(height: 16),
          Text(
            title,
            style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 21),
          ),
          const SizedBox(height: 8),
          Text(message, textAlign: TextAlign.center),
          const SizedBox(height: 20),
          FilledButton(onPressed: action, child: Text(label)),
        ],
      ),
    ),
  );
}
