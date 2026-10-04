import 'package:flutter/material.dart';
import '../core/app_state.dart';
import '../core/models.dart';
import '../ui/theme.dart';

class WritingScreen extends StatefulWidget {
  const WritingScreen({super.key, required this.state});
  final AppState state;
  @override
  State<WritingScreen> createState() => _WritingScreenState();
}

class _WritingScreenState extends State<WritingScreen> {
  late Future<List<dynamic>> data;
  String? selectedType;
  @override
  void initState() {
    super.initState();
    data = _load();
  }

  Future<List<dynamic>> _load() => Future.wait([
    widget.state.api.writingTypes(),
    widget.state.api.prompts(typeId: selectedType),
  ]);
  void _reload() => setState(() => data = _load());

  @override
  Widget build(BuildContext context) => SafeArea(
    child: Column(
      children: [
        AppBar(
          title: const Text(
            'Luyện viết',
            style: TextStyle(fontWeight: FontWeight.w900),
          ),
        ),
        Expanded(
          child: FutureBuilder<List<dynamic>>(
            future: data,
            builder: (context, snap) {
              if (snap.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }
              if (snap.hasError) {
                return ErrorView(error: snap.error!, retry: _reload);
              }
              final types = snap.data![0] as List<WritingType>,
                  prompts = snap.data![1] as List<WritingPrompt>;
              return RefreshIndicator(
                onRefresh: () async {
                  _reload();
                  await data;
                },
                child: ListView(
                  padding: const EdgeInsets.fromLTRB(20, 4, 20, 28),
                  children: [
                    const Text(
                      'Chọn dạng bài',
                      style: TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 18,
                      ),
                    ),
                    const SizedBox(height: 10),
                    SizedBox(
                      height: 42,
                      child: ListView(
                        scrollDirection: Axis.horizontal,
                        children: [
                          ChoiceChip(
                            label: const Text('Tất cả'),
                            selected: selectedType == null,
                            onSelected: (_) {
                              selectedType = null;
                              _reload();
                            },
                          ),
                          const SizedBox(width: 8),
                          ...types.expand(
                            (t) => [
                              ChoiceChip(
                                label: Text(t.name),
                                selected: selectedType == t.id,
                                onSelected: (_) {
                                  selectedType = t.id;
                                  _reload();
                                },
                              ),
                              const SizedBox(width: 8),
                            ],
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 22),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text(
                          'Đề luyện tập',
                          style: TextStyle(
                            fontWeight: FontWeight.w800,
                            fontSize: 18,
                          ),
                        ),
                        Text(
                          '${prompts.length} đề',
                          style: TextStyle(color: Colors.grey.shade600),
                        ),
                      ],
                    ),
                    const SizedBox(height: 10),
                    if (prompts.isEmpty)
                      const SizedBox(
                        height: 300,
                        child: EmptyState(
                          icon: Icons.search_off,
                          title: 'Không có đề phù hợp',
                          message: 'Hãy chọn một dạng bài khác.',
                        ),
                      )
                    else
                      ...prompts.map(
                        (p) => Card(
                          child: InkWell(
                            borderRadius: BorderRadius.circular(20),
                            onTap: () => Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (_) => PromptScreen(
                                  state: widget.state,
                                  prompt: p,
                                ),
                              ),
                            ),
                            child: Padding(
                              padding: const EdgeInsets.all(18),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    children: [
                                      Container(
                                        padding: const EdgeInsets.symmetric(
                                          horizontal: 10,
                                          vertical: 5,
                                        ),
                                        decoration: BoxDecoration(
                                          color: NomiTheme.primary.withValues(
                                            alpha: .1,
                                          ),
                                          borderRadius: BorderRadius.circular(
                                            20,
                                          ),
                                        ),
                                        child: Text(
                                          p.difficulty,
                                          style: const TextStyle(
                                            color: NomiTheme.primary,
                                            fontWeight: FontWeight.w700,
                                            fontSize: 12,
                                          ),
                                        ),
                                      ),
                                      const Spacer(),
                                      if (p.minWords != null)
                                        Text(
                                          '${p.minWords}+ từ',
                                          style: TextStyle(
                                            color: Colors.grey.shade600,
                                            fontSize: 12,
                                          ),
                                        ),
                                    ],
                                  ),
                                  const SizedBox(height: 12),
                                  Text(
                                    p.title,
                                    style: const TextStyle(
                                      fontWeight: FontWeight.w800,
                                      fontSize: 17,
                                    ),
                                  ),
                                  if (p.typeName.isNotEmpty) ...[
                                    const SizedBox(height: 7),
                                    Text(
                                      p.typeName,
                                      style: TextStyle(
                                        color: Colors.grey.shade600,
                                      ),
                                    ),
                                  ],
                                  const SizedBox(height: 10),
                                  Text(
                                    p.instructions,
                                    maxLines: 3,
                                    overflow: TextOverflow.ellipsis,
                                    style: const TextStyle(height: 1.45),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ),
                      ),
                  ],
                ),
              );
            },
          ),
        ),
      ],
    ),
  );
}

class PromptScreen extends StatelessWidget {
  const PromptScreen({super.key, required this.state, required this.prompt});
  final AppState state;
  final WritingPrompt prompt;
  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Chi tiết đề')),
    body: ListView(
      padding: const EdgeInsets.all(20),
      children: [
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: [
            Chip(label: Text(prompt.typeName)),
            Chip(label: Text(prompt.difficulty)),
            if (prompt.minWords != null)
              Chip(
                avatar: const Icon(Icons.short_text, size: 18),
                label: Text('Tối thiểu ${prompt.minWords} từ'),
              ),
          ],
        ),
        const SizedBox(height: 18),
        Text(
          prompt.title,
          style: Theme.of(
            context,
          ).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900),
        ),
        const SizedBox(height: 16),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(18),
            child: Text(
              prompt.instructions,
              style: const TextStyle(fontSize: 16, height: 1.6),
            ),
          ),
        ),
        const SizedBox(height: 22),
        FilledButton.icon(
          onPressed: () => Navigator.push(
            context,
            MaterialPageRoute(
              builder: (_) => EssayEditorScreen(state: state, prompt: prompt),
            ),
          ),
          icon: const Icon(Icons.edit_rounded),
          label: const Text('Bắt đầu viết'),
        ),
        const SizedBox(height: 10),
        OutlinedButton.icon(
          onPressed: () async {
            try {
              final answer = await state.api.sampleAnswer(prompt.id);
              if (!context.mounted) return;
              await showModalBottomSheet<void>(
                context: context,
                isScrollControlled: true,
                builder: (context) => SafeArea(
                  child: DraggableScrollableSheet(
                    expand: false,
                    initialChildSize: .65,
                    maxChildSize: .9,
                    builder: (context, controller) => ListView(
                      controller: controller,
                      padding: const EdgeInsets.all(20),
                      children: [
                        const Text(
                          'Bài viết mẫu',
                          style: TextStyle(
                            fontSize: 22,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                        const SizedBox(height: 16),
                        Text(
                          answer ?? 'Đề bài này chưa có bài mẫu.',
                          style: const TextStyle(fontSize: 16, height: 1.6),
                        ),
                      ],
                    ),
                  ),
                ),
              );
            } catch (error) {
              if (context.mounted) {
                ScaffoldMessenger.of(
                  context,
                ).showSnackBar(SnackBar(content: Text('$error')));
              }
            }
          },
          icon: const Icon(Icons.menu_book_outlined),
          label: const Text('Xem bài mẫu'),
        ),
      ],
    ),
  );
}

class EssayEditorScreen extends StatefulWidget {
  const EssayEditorScreen({
    super.key,
    required this.state,
    required this.prompt,
  });
  final AppState state;
  final WritingPrompt prompt;
  @override
  State<EssayEditorScreen> createState() => _EssayEditorScreenState();
}

class _EssayEditorScreenState extends State<EssayEditorScreen> {
  final controller = TextEditingController();
  bool timed = false, sending = false;
  int get words => controller.text.trim().isEmpty
      ? 0
      : controller.text.trim().split(RegExp(r'\s+')).length;
  @override
  void dispose() {
    controller.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (words < (widget.prompt.minWords ?? 1)) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Bài viết cần ít nhất ${widget.prompt.minWords ?? 1} từ.',
          ),
        ),
      );
      return;
    }
    final confirmed =
        await showDialog<bool>(
          context: context,
          builder: (_) => AlertDialog(
            title: const Text('Nộp bài viết?'),
            content: const Text(
              'Sau khi nộp, hệ thống sẽ gửi bài tới AI để chấm điểm.',
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context, false),
                child: const Text('Xem lại'),
              ),
              FilledButton(
                onPressed: () => Navigator.pop(context, true),
                child: const Text('Nộp bài'),
              ),
            ],
          ),
        ) ??
        false;
    if (!confirmed) return;
    setState(() => sending = true);
    try {
      final submission = await widget.state.api.submitEssay(
        widget.prompt.id,
        controller.text.trim(),
        timed,
      );
      if (!mounted) return;
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(
          builder: (_) => SubmissionResultScreen(
            state: widget.state,
            submission: submission,
          ),
        ),
      );
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

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(
      title: const Text('Soạn bài'),
      actions: [
        Row(
          children: [
            const Text('Hẹn giờ', style: TextStyle(fontSize: 12)),
            Switch(value: timed, onChanged: (v) => setState(() => timed = v)),
          ],
        ),
      ],
    ),
    body: SafeArea(
      child: Column(
        children: [
          Container(
            width: double.infinity,
            color: Colors.white,
            padding: const EdgeInsets.fromLTRB(18, 12, 18, 14),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  widget.prompt.title,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 5),
                Text(
                  widget.prompt.instructions,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(color: Colors.grey.shade600, fontSize: 12),
                ),
              ],
            ),
          ),
          Expanded(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: TextField(
                controller: controller,
                expands: true,
                maxLines: null,
                minLines: null,
                textAlignVertical: TextAlignVertical.top,
                onChanged: (_) => setState(() {}),
                decoration: const InputDecoration(
                  hintText: 'Viết bài của bạn tại đây…',
                  alignLabelWithHint: true,
                ),
              ),
            ),
          ),
          Container(
            color: Colors.white,
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 12),
            child: Row(
              children: [
                Text(
                  '$words từ',
                  style: TextStyle(
                    fontWeight: FontWeight.w700,
                    color: words >= (widget.prompt.minWords ?? 0)
                        ? NomiTheme.primary
                        : Colors.grey,
                  ),
                ),
                const Spacer(),
                SizedBox(
                  width: 150,
                  child: FilledButton(
                    onPressed: sending ? null : _submit,
                    child: sending
                        ? const SizedBox.square(
                            dimension: 20,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: Colors.white,
                            ),
                          )
                        : const Text('Nộp bài'),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    ),
  );
}

class SubmissionResultScreen extends StatefulWidget {
  const SubmissionResultScreen({
    super.key,
    required this.state,
    required this.submission,
  });
  final AppState state;
  final Submission submission;
  @override
  State<SubmissionResultScreen> createState() => _SubmissionResultScreenState();
}

class _SubmissionResultScreenState extends State<SubmissionResultScreen> {
  late Future<Json> feedback;
  bool actionBusy = false;
  @override
  void initState() {
    super.initState();
    feedback = widget.state.api.feedback(widget.submission.id);
  }

  Future<void> retryGrading() async {
    setState(() => actionBusy = true);
    try {
      await widget.state.api.retryGrading(widget.submission.id);
      setState(
        () => feedback = widget.state.api.feedback(widget.submission.id),
      );
    } catch (error) {
      if (mounted) _showMessage('$error');
    } finally {
      if (mounted) setState(() => actionBusy = false);
    }
  }

  Future<void> requestTutorReview() async {
    setState(() => actionBusy = true);
    try {
      await widget.state.api.requestTutorReview(widget.submission.id);
      if (mounted) _showMessage('Đã gửi yêu cầu giảng viên đánh giá.');
    } catch (error) {
      if (mounted) _showMessage('$error');
    } finally {
      if (mounted) setState(() => actionBusy = false);
    }
  }

  Future<void> flagResult(String resultId) async {
    final controller = TextEditingController();
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Báo cáo phản hồi AI'),
        content: TextField(
          controller: controller,
          minLines: 2,
          maxLines: 4,
          decoration: const InputDecoration(labelText: 'Lý do'),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Đóng'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Gửi'),
          ),
        ],
      ),
    );
    final reason = controller.text.trim();
    controller.dispose();
    if (accepted != true || reason.isEmpty) return;
    setState(() => actionBusy = true);
    try {
      await widget.state.api.flagFeedback(resultId, reason);
      if (mounted) _showMessage('Đã gửi báo cáo phản hồi.');
    } catch (error) {
      if (mounted) _showMessage('$error');
    } finally {
      if (mounted) setState(() => actionBusy = false);
    }
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Kết quả chấm bài')),
    body: FutureBuilder<Json>(
      future: feedback,
      builder: (context, snap) {
        if (snap.connectionState == ConnectionState.waiting) {
          return const Center(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                CircularProgressIndicator(),
                SizedBox(height: 18),
                Text('AI đang phân tích bài viết…'),
              ],
            ),
          );
        }
        if (snap.hasError) {
          return ErrorView(
            error: snap.error!,
            retry: () => setState(
              () => feedback = widget.state.api.feedback(widget.submission.id),
            ),
          );
        }
        final f = snap.data!,
            scores = asJsonList(f['criterionScores']),
            errors = asJsonList(f['grammarErrors']),
            vocabulary = asJsonList(f['vocabularySuggestions']),
            rewrites = asJsonList(f['restructuringSuggestions']);
        return ListView(
          padding: const EdgeInsets.all(20),
          children: [
            Center(
              child: CircleAvatar(
                radius: 52,
                backgroundColor: NomiTheme.primary,
                child: Text(
                  (asDouble(f['overallBand']) ?? 0).toStringAsFixed(1),
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w900,
                    fontSize: 34,
                  ),
                ),
              ),
            ),
            const SizedBox(height: 24),
            Text(
              asText(f['overallFeedback'], 'Đã hoàn thành chấm bài.'),
              style: const TextStyle(fontSize: 16, height: 1.55),
            ),
            const SizedBox(height: 20),
            const Text(
              'Điểm thành phần',
              style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
            ),
            ...scores.map(
              (s) => ListTile(
                contentPadding: EdgeInsets.zero,
                title: Text(asText(s['criterionName'])),
                subtitle: Text(asText(s['comment'])),
                trailing: Text(
                  (asDouble(s['score']) ?? 0).toStringAsFixed(1),
                  style: const TextStyle(
                    fontWeight: FontWeight.w900,
                    color: NomiTheme.primary,
                  ),
                ),
              ),
            ),
            if (errors.isNotEmpty) ...[
              const SizedBox(height: 12),
              const Text(
                'Lỗi cần lưu ý',
                style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
              ),
              ...errors.map(
                (e) => Card(
                  child: ListTile(
                    title: Text(asText(e['originalText'])),
                    subtitle: Text(
                      '${asText(e['suggestion'])}\n${asText(e['explanation'])}',
                    ),
                  ),
                ),
              ),
            ],
            if (vocabulary.isNotEmpty) ...[
              const SizedBox(height: 16),
              const Text(
                'Gợi ý từ vựng',
                style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
              ),
              ...vocabulary.map(
                (item) => Card(
                  child: ListTile(
                    title: Text(asText(item['originalWord'])),
                    subtitle: Text(
                      (item['suggestedAlternatives'] as List?)
                              ?.map(asText)
                              .join(', ') ??
                          '',
                    ),
                  ),
                ),
              ),
            ],
            if (rewrites.isNotEmpty) ...[
              const SizedBox(height: 16),
              const Text(
                'Gợi ý viết lại',
                style: TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
              ),
              ...rewrites.map(
                (item) => Card(
                  child: Padding(
                    padding: const EdgeInsets.all(14),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(asText(item['originalSentence'])),
                        const SizedBox(height: 8),
                        Text(
                          asText(item['suggestedRewrite']),
                          style: const TextStyle(
                            fontWeight: FontWeight.w700,
                            color: NomiTheme.primary,
                          ),
                        ),
                        if (asText(item['reason']).isNotEmpty) ...[
                          const SizedBox(height: 6),
                          Text(asText(item['reason'])),
                        ],
                      ],
                    ),
                  ),
                ),
              ),
            ],
            const SizedBox(height: 20),
            OutlinedButton.icon(
              onPressed: actionBusy ? null : requestTutorReview,
              icon: const Icon(Icons.rate_review_outlined),
              label: const Text('Yêu cầu giảng viên đánh giá'),
            ),
            const SizedBox(height: 8),
            OutlinedButton.icon(
              onPressed: actionBusy ? null : () => flagResult(asText(f['id'])),
              icon: const Icon(Icons.flag_outlined),
              label: const Text('Báo cáo phản hồi AI'),
            ),
            const SizedBox(height: 8),
            TextButton.icon(
              onPressed: actionBusy ? null : retryGrading,
              icon: const Icon(Icons.refresh_rounded),
              label: const Text('Chấm lại bài viết'),
            ),
          ],
        );
      },
    ),
  );
}
