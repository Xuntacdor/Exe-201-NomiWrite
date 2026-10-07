import 'package:flutter/material.dart';

import '../../core/app_state.dart';
import '../../core/models.dart';
import '../../ui/theme.dart';

class AdminShell extends StatefulWidget {
  const AdminShell({super.key, required this.state});

  final AppState state;

  @override
  State<AdminShell> createState() => _AdminShellState();
}

class _AdminShellState extends State<AdminShell> {
  int index = 0;

  late final pages = <Widget>[
    _AdminOverview(state: widget.state),
    _AdminUsers(state: widget.state),
    _AdminPrompts(state: widget.state),
    _AdminAiConfig(state: widget.state),
  ];

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(
      title: const Row(
        children: [
          BrandMark(compact: true),
          SizedBox(width: 10),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('NomiWrite', style: TextStyle(fontWeight: FontWeight.w800)),
              Text(
                'ADMIN MOBILE',
                style: TextStyle(
                  color: NomiTheme.primary,
                  fontSize: 10,
                  fontWeight: FontWeight.w800,
                  letterSpacing: 1.2,
                ),
              ),
            ],
          ),
        ],
      ),
      actions: [
        IconButton(
          tooltip: 'Đăng xuất',
          onPressed: widget.state.signOut,
          icon: const Icon(Icons.logout_rounded),
        ),
      ],
    ),
    body: IndexedStack(index: index, children: pages),
    bottomNavigationBar: NavigationBar(
      selectedIndex: index,
      onDestinationSelected: (value) => setState(() => index = value),
      destinations: const [
        NavigationDestination(
          icon: Icon(Icons.dashboard_outlined),
          selectedIcon: Icon(Icons.dashboard_rounded),
          label: 'Tổng quan',
        ),
        NavigationDestination(
          icon: Icon(Icons.people_outline),
          selectedIcon: Icon(Icons.people_rounded),
          label: 'Người dùng',
        ),
        NavigationDestination(
          icon: Icon(Icons.article_outlined),
          selectedIcon: Icon(Icons.article_rounded),
          label: 'Đề bài',
        ),
        NavigationDestination(
          icon: Icon(Icons.smart_toy_outlined),
          selectedIcon: Icon(Icons.smart_toy_rounded),
          label: 'AI',
        ),
      ],
    ),
  );
}

class _AdminOverview extends StatefulWidget {
  const _AdminOverview({required this.state});

  final AppState state;

  @override
  State<_AdminOverview> createState() => _AdminOverviewState();
}

class _AdminOverviewState extends State<_AdminOverview> {
  late Future<Json> data = widget.state.api.adminOverview();

  Future<void> refresh() async {
    final next = widget.state.api.adminOverview();
    setState(() => data = next);
    await next;
  }

  @override
  Widget build(BuildContext context) => FutureBuilder<Json>(
    future: data,
    builder: (context, snapshot) => RefreshIndicator(
      onRefresh: refresh,
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 28),
        children: [
          const _PageHeading(
            title: 'Tổng quan hệ thống',
            subtitle: 'Theo dõi người dùng, bài viết và doanh thu.',
          ),
          const SizedBox(height: 18),
          if (snapshot.connectionState == ConnectionState.waiting)
            const SizedBox(
              height: 300,
              child: Center(child: CircularProgressIndicator()),
            )
          else if (snapshot.hasError)
            SizedBox(
              height: 320,
              child: ErrorView(error: snapshot.error!, retry: refresh),
            )
          else
            ..._content(snapshot.data!),
        ],
      ),
    ),
  );

  List<Widget> _content(Json value) {
    final metrics = [
      (
        'Người dùng',
        '${asInt(value['totalUsers'])}',
        Icons.people_rounded,
        NomiTheme.primary,
      ),
      (
        'Đang hoạt động',
        '${asInt(value['activeUsers'])}',
        Icons.verified_user_rounded,
        NomiTheme.mint,
      ),
      (
        'Bài đã nộp',
        '${asInt(value['totalSubmissions'])}',
        Icons.description_rounded,
        Colors.deepOrange,
      ),
      (
        'Đang chờ chấm',
        '${asInt(value['pendingSubmissions'])}',
        Icons.pending_actions_rounded,
        NomiTheme.gold,
      ),
      (
        'VIP đang hoạt động',
        '${asInt(value['activeVipMembers'])}',
        Icons.workspace_premium_rounded,
        Colors.purple,
      ),
      (
        'Giao dịch',
        '${asInt(value['completedTransactions'])}',
        Icons.payments_rounded,
        Colors.blue,
      ),
    ];
    final warnings = value['warnings'] is List
        ? value['warnings'] as List
        : const [];
    return [
      GridView.builder(
        shrinkWrap: true,
        physics: const NeverScrollableScrollPhysics(),
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2,
          crossAxisSpacing: 10,
          mainAxisSpacing: 10,
          childAspectRatio: 1.42,
        ),
        itemCount: metrics.length,
        itemBuilder: (_, i) {
          final item = metrics[i];
          return _MetricCard(
            label: item.$1,
            value: item.$2,
            icon: item.$3,
            color: item.$4,
          );
        },
      ),
      const SizedBox(height: 12),
      Card(
        child: ListTile(
          leading: const CircleAvatar(
            child: Icon(Icons.account_balance_wallet_outlined),
          ),
          title: const Text(
            'Tổng doanh thu',
            style: TextStyle(fontWeight: FontWeight.w700),
          ),
          subtitle: const Text('Doanh thu từ các giao dịch hoàn tất'),
          trailing: Text(
            '${asDouble(value['totalRevenue'])?.toStringAsFixed(0) ?? '0'} đ',
            style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 17),
          ),
        ),
      ),
      if (warnings.isNotEmpty) ...[
        const SizedBox(height: 12),
        Card(
          color: Colors.amber.shade50,
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Cảnh báo dịch vụ',
                  style: TextStyle(fontWeight: FontWeight.w800),
                ),
                const SizedBox(height: 8),
                ...warnings.map((item) => Text('• $item')),
              ],
            ),
          ),
        ),
      ],
    ];
  }
}

class _AdminUsers extends StatefulWidget {
  const _AdminUsers({required this.state});

  final AppState state;

  @override
  State<_AdminUsers> createState() => _AdminUsersState();
}

class _AdminUsersState extends State<_AdminUsers> {
  final search = TextEditingController();
  int page = 1;
  late Future<Json> data = load();

  Future<Json> load() => widget.state.api.adminUsers(
    search: search.text,
    page: page,
    pageSize: 15,
  );

  void reload({bool firstPage = false}) {
    if (firstPage) page = 1;
    setState(() => data = load());
  }

  Future<void> updateUser(Json user, {int? role, int? status}) async {
    try {
      if (role != null) {
        await widget.state.api.updateAdminUserRole(asText(user['id']), role);
      }
      if (status != null) {
        await widget.state.api.updateAdminUserStatus(
          asText(user['id']),
          status,
        );
      }
      reload();
    } catch (error) {
      if (mounted) _message(context, '$error', error: true);
    }
  }

  @override
  void dispose() {
    search.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => Column(
    children: [
      Padding(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 10),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const _PageHeading(
              title: 'Quản lý người dùng',
              subtitle: 'Tìm kiếm, đổi vai trò và trạng thái tài khoản.',
            ),
            const SizedBox(height: 14),
            TextField(
              controller: search,
              textInputAction: TextInputAction.search,
              onSubmitted: (_) => reload(firstPage: true),
              decoration: InputDecoration(
                hintText: 'Tìm theo tên hoặc email',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: IconButton(
                  onPressed: () => reload(firstPage: true),
                  icon: const Icon(Icons.arrow_forward_rounded),
                ),
              ),
            ),
          ],
        ),
      ),
      Expanded(
        child: FutureBuilder<Json>(
          future: data,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snapshot.hasError) {
              return ErrorView(error: snapshot.error!, retry: reload);
            }
            final value = snapshot.data!;
            final users = asJsonList(value['items']);
            if (users.isEmpty) {
              return const EmptyState(
                icon: Icons.person_search_rounded,
                title: 'Không có người dùng',
                message: 'Thử thay đổi nội dung tìm kiếm.',
              );
            }
            final totalPages = asInt(value['totalPages'], 1);
            return RefreshIndicator(
              onRefresh: () async => reload(),
              child: ListView.builder(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.fromLTRB(12, 2, 12, 20),
                itemCount: users.length + 1,
                itemBuilder: (context, index) {
                  if (index == users.length) {
                    return _Pagination(
                      page: page,
                      totalPages: totalPages,
                      onPrevious: page > 1
                          ? () {
                              page--;
                              reload();
                            }
                          : null,
                      onNext: page < totalPages
                          ? () {
                              page++;
                              reload();
                            }
                          : null,
                    );
                  }
                  final user = users[index];
                  final role = _enumNumber(user['role'], const {
                    'student': 0,
                    'admin': 1,
                    'moderator': 2,
                  });
                  final status = _enumNumber(user['accountStatus'], const {
                    'active': 0,
                    'deactivated': 1,
                    'banned': 2,
                  });
                  return Card(
                    child: ListTile(
                      isThreeLine: true,
                      leading: CircleAvatar(
                        backgroundColor: NomiTheme.primary.withValues(
                          alpha: .12,
                        ),
                        child: Text(
                          _initial(asText(user['fullName'])),
                          style: const TextStyle(
                            fontWeight: FontWeight.w800,
                            color: NomiTheme.primary,
                          ),
                        ),
                      ),
                      title: Text(
                        asText(user['fullName'], 'Chưa đặt tên'),
                        style: const TextStyle(fontWeight: FontWeight.w800),
                      ),
                      subtitle: Text(
                        '${asText(user['email'])}\n${_roleLabel(role)} • ${_statusLabel(status)}',
                      ),
                      trailing: PopupMenuButton<String>(
                        onSelected: (action) {
                          final parts = action.split(':');
                          updateUser(
                            user,
                            role: parts[0] == 'role'
                                ? int.parse(parts[1])
                                : null,
                            status: parts[0] == 'status'
                                ? int.parse(parts[1])
                                : null,
                          );
                        },
                        itemBuilder: (_) => const [
                          PopupMenuItem(
                            value: 'role:0',
                            child: Text('Vai trò: Học viên'),
                          ),
                          PopupMenuItem(
                            value: 'role:2',
                            child: Text('Vai trò: Điều hành viên'),
                          ),
                          PopupMenuItem(
                            value: 'role:1',
                            child: Text('Vai trò: Quản trị viên'),
                          ),
                          PopupMenuDivider(),
                          PopupMenuItem(
                            value: 'status:0',
                            child: Text('Kích hoạt tài khoản'),
                          ),
                          PopupMenuItem(
                            value: 'status:1',
                            child: Text('Vô hiệu hóa tài khoản'),
                          ),
                          PopupMenuItem(
                            value: 'status:2',
                            child: Text('Khóa tài khoản'),
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
            );
          },
        ),
      ),
    ],
  );
}

class _AdminPrompts extends StatefulWidget {
  const _AdminPrompts({required this.state});

  final AppState state;

  @override
  State<_AdminPrompts> createState() => _AdminPromptsState();
}

class _AdminPromptsState extends State<_AdminPrompts> {
  int page = 1;
  late Future<Json> data = load();

  Future<Json> load() =>
      widget.state.api.adminPrompts(page: page, pageSize: 15);
  void reload() => setState(() => data = load());

  Future<void> openEditor([Json? prompt]) async {
    final changed = await Navigator.push<bool>(
      context,
      MaterialPageRoute(
        builder: (_) =>
            AdminPromptEditorScreen(state: widget.state, prompt: prompt),
      ),
    );
    if (changed == true) reload();
  }

  Future<void> deletePrompt(Json prompt) async {
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Xóa đề bài?'),
        content: Text(
          'Đề “${asText(prompt['title'])}” sẽ bị xóa khỏi hệ thống.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Đóng'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Xóa'),
          ),
        ],
      ),
    );
    if (accepted != true) return;
    try {
      await widget.state.api.deleteAdminPrompt(asText(prompt['id']));
      reload();
    } catch (error) {
      if (mounted) _message(context, '$error', error: true);
    }
  }

  @override
  Widget build(BuildContext context) => Column(
    children: [
      Padding(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 12),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Expanded(
              child: _PageHeading(
                title: 'Kho đề bài',
                subtitle: 'Tạo và quản lý đề luyện viết.',
              ),
            ),
            IconButton.filled(
              onPressed: openEditor,
              tooltip: 'Tạo đề',
              icon: const Icon(Icons.add_rounded),
            ),
          ],
        ),
      ),
      Expanded(
        child: FutureBuilder<Json>(
          future: data,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snapshot.hasError) {
              return ErrorView(error: snapshot.error!, retry: reload);
            }
            final value = snapshot.data!;
            final prompts = asJsonList(value['items']);
            if (prompts.isEmpty) {
              return const EmptyState(
                icon: Icons.article_outlined,
                title: 'Chưa có đề bài',
                message: 'Danh sách đề bài đang trống.',
              );
            }
            final totalPages = asInt(value['totalPages'], 1);
            return RefreshIndicator(
              onRefresh: () async => reload(),
              child: ListView.builder(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.fromLTRB(12, 0, 12, 20),
                itemCount: prompts.length + 1,
                itemBuilder: (context, index) {
                  if (index == prompts.length) {
                    return _Pagination(
                      page: page,
                      totalPages: totalPages,
                      onPrevious: page > 1
                          ? () {
                              page--;
                              reload();
                            }
                          : null,
                      onNext: page < totalPages
                          ? () {
                              page++;
                              reload();
                            }
                          : null,
                    );
                  }
                  final prompt = prompts[index];
                  final difficulty = _enumNumber(prompt['difficulty'], const {
                    'beginner': 0,
                    'intermediate': 1,
                    'advanced': 2,
                  });
                  final active = prompt['isActive'] == true;
                  return Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Icon(
                                Icons.description_outlined,
                                color: NomiTheme.primary,
                              ),
                              const SizedBox(width: 10),
                              Expanded(
                                child: Text(
                                  asText(prompt['title']),
                                  style: const TextStyle(
                                    fontWeight: FontWeight.w800,
                                    fontSize: 16,
                                  ),
                                ),
                              ),
                              PopupMenuButton<String>(
                                onSelected: (value) => value == 'edit'
                                    ? openEditor(prompt)
                                    : deletePrompt(prompt),
                                itemBuilder: (_) => const [
                                  PopupMenuItem(
                                    value: 'edit',
                                    child: Text('Chỉnh sửa'),
                                  ),
                                  PopupMenuItem(
                                    value: 'delete',
                                    child: Text('Xóa'),
                                  ),
                                ],
                              ),
                            ],
                          ),
                          const SizedBox(height: 12),
                          Wrap(
                            spacing: 8,
                            runSpacing: 8,
                            children: [
                              Chip(
                                label: Text(
                                  asText(prompt['writingTypeName'], 'Writing'),
                                ),
                              ),
                              Chip(label: Text(_difficultyLabel(difficulty))),
                              Chip(
                                avatar: Icon(
                                  active ? Icons.check_circle : Icons.drafts,
                                  size: 17,
                                ),
                                label: Text(
                                  active ? 'Đã xuất bản' : 'Bản nháp',
                                ),
                              ),
                              if (prompt['isVipOnly'] == true)
                                const Chip(label: Text('VIP')),
                            ],
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
            );
          },
        ),
      ),
    ],
  );
}

class AdminPromptEditorScreen extends StatefulWidget {
  const AdminPromptEditorScreen({super.key, required this.state, this.prompt});

  final AppState state;
  final Json? prompt;

  @override
  State<AdminPromptEditorScreen> createState() =>
      _AdminPromptEditorScreenState();
}

class _AdminPromptEditorScreenState extends State<AdminPromptEditorScreen> {
  final formKey = GlobalKey<FormState>();
  final title = TextEditingController();
  final instructions = TextEditingController();
  final timeLimit = TextEditingController();
  final minWords = TextEditingController();
  final maxWords = TextEditingController();
  final imageUrl = TextEditingController();
  final sampleAnswer = TextEditingController();
  List<WritingType> types = const [];
  String? typeId;
  int difficulty = 1;
  bool vipOnly = false;
  bool loading = true;
  bool saving = false;
  Object? error;

  bool get editing => widget.prompt != null;

  @override
  void initState() {
    super.initState();
    load();
  }

  Future<void> load() async {
    if (mounted) {
      setState(() {
        loading = true;
        error = null;
      });
    }
    try {
      types = await widget.state.api.writingTypes();
      Json? value;
      if (editing) {
        value = await widget.state.api.adminPrompt(
          asText(widget.prompt!['id']),
        );
      }
      value ??= widget.prompt;
      if (value != null) {
        title.text = asText(value['title']);
        instructions.text = asText(value['instructions']);
        timeLimit.text = value['timeLimitMinutes'] == null
            ? ''
            : '${value['timeLimitMinutes']}';
        minWords.text = value['minWords'] == null ? '' : '${value['minWords']}';
        maxWords.text = value['maxWords'] == null ? '' : '${value['maxWords']}';
        imageUrl.text = asText(value['imageUrl']);
        sampleAnswer.text = asText(value['sampleAnswer']);
        typeId = asText(value['writingTypeId']);
        difficulty = _enumNumber(value['difficulty'], const {
          'beginner': 0,
          'intermediate': 1,
          'advanced': 2,
        });
        vipOnly = value['isVipOnly'] == true;
      }
      typeId ??= types.isEmpty ? null : types.first.id;
    } catch (value) {
      error = value;
    } finally {
      if (mounted) setState(() => loading = false);
    }
  }

  Future<void> save() async {
    if (!formKey.currentState!.validate() || typeId == null) return;
    setState(() => saving = true);
    final value = <String, dynamic>{
      'writingTypeId': typeId,
      'title': title.text.trim(),
      'instructions': instructions.text.trim(),
      'difficulty': difficulty,
      'timeLimitMinutes': _optionalInt(timeLimit.text),
      'minWords': _optionalInt(minWords.text),
      'maxWords': _optionalInt(maxWords.text),
      'imageUrl': _optionalText(imageUrl.text),
      'sampleAnswer': _optionalText(sampleAnswer.text),
      'isVipOnly': vipOnly,
    };
    try {
      if (editing) {
        await widget.state.api.updateAdminPrompt(
          asText(widget.prompt!['id']),
          value,
        );
      } else {
        await widget.state.api.createAdminPrompt(value);
      }
      if (mounted) Navigator.pop(context, true);
    } catch (error) {
      if (mounted) _message(context, '$error', error: true);
    } finally {
      if (mounted) setState(() => saving = false);
    }
  }

  @override
  void dispose() {
    for (final controller in [
      title,
      instructions,
      timeLimit,
      minWords,
      maxWords,
      imageUrl,
      sampleAnswer,
    ]) {
      controller.dispose();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: Text(editing ? 'Chỉnh sửa đề' : 'Tạo đề mới')),
    body: loading
        ? const Center(child: CircularProgressIndicator())
        : error != null
        ? ErrorView(error: error!, retry: load)
        : Form(
            key: formKey,
            child: ListView(
              padding: const EdgeInsets.all(16),
              children: [
                DropdownButtonFormField<String>(
                  initialValue: typeId,
                  decoration: const InputDecoration(labelText: 'Dạng bài'),
                  items: types
                      .map(
                        (type) => DropdownMenuItem(
                          value: type.id,
                          child: Text(type.name),
                        ),
                      )
                      .toList(),
                  onChanged: (value) => setState(() => typeId = value),
                  validator: (value) => value == null ? 'Chọn dạng bài.' : null,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: title,
                  decoration: const InputDecoration(labelText: 'Tiêu đề'),
                  validator: _required,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: instructions,
                  minLines: 4,
                  maxLines: 8,
                  decoration: const InputDecoration(
                    labelText: 'Yêu cầu đề bài',
                    alignLabelWithHint: true,
                  ),
                  validator: _required,
                ),
                const SizedBox(height: 12),
                SegmentedButton<int>(
                  segments: const [
                    ButtonSegment(value: 0, label: Text('Cơ bản')),
                    ButtonSegment(value: 1, label: Text('Trung cấp')),
                    ButtonSegment(value: 2, label: Text('Nâng cao')),
                  ],
                  selected: {difficulty},
                  onSelectionChanged: (value) =>
                      setState(() => difficulty = value.first),
                ),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(
                      child: TextFormField(
                        controller: minWords,
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          labelText: 'Từ tối thiểu',
                        ),
                        validator: _optionalPositiveInt,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: TextFormField(
                        controller: maxWords,
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          labelText: 'Từ tối đa',
                        ),
                        validator: _optionalPositiveInt,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: timeLimit,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: 'Thời gian (phút)',
                  ),
                  validator: _optionalPositiveInt,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: imageUrl,
                  keyboardType: TextInputType.url,
                  decoration: const InputDecoration(labelText: 'Image URL'),
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: sampleAnswer,
                  minLines: 4,
                  maxLines: 10,
                  decoration: const InputDecoration(
                    labelText: 'Bài mẫu',
                    alignLabelWithHint: true,
                  ),
                ),
                SwitchListTile(
                  contentPadding: EdgeInsets.zero,
                  value: vipOnly,
                  onChanged: (value) => setState(() => vipOnly = value),
                  title: const Text('Chỉ dành cho VIP'),
                ),
                const SizedBox(height: 12),
                FilledButton.icon(
                  onPressed: saving ? null : save,
                  icon: saving
                      ? const SizedBox.square(
                          dimension: 18,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                      : const Icon(Icons.save_rounded),
                  label: Text(saving ? 'Đang lưu...' : 'Lưu đề bài'),
                ),
              ],
            ),
          ),
  );
}

class _AdminAiConfig extends StatefulWidget {
  const _AdminAiConfig({required this.state});

  final AppState state;

  @override
  State<_AdminAiConfig> createState() => _AdminAiConfigState();
}

class _AdminAiConfigState extends State<_AdminAiConfig> {
  final formKey = GlobalKey<FormState>();
  final model = TextEditingController();
  final fallback = TextEditingController();
  final apiKey = TextEditingController();
  final temperature = TextEditingController();
  final maxTokens = TextEditingController();
  final systemPrompt = TextEditingController();
  Json? config;
  Object? error;
  bool loading = true;
  bool saving = false;
  bool clearApiKey = false;
  bool showApiKey = false;

  @override
  void initState() {
    super.initState();
    load();
  }

  Future<void> load() async {
    setState(() {
      loading = true;
      error = null;
    });
    try {
      final value = await widget.state.api.adminAiConfig();
      config = value;
      model.text = asText(value['modelName']);
      fallback.text = asText(value['fallbackModelName']);
      temperature.text = value['temperature'] == null
          ? ''
          : '${value['temperature']}';
      maxTokens.text = value['maxOutputTokens'] == null
          ? ''
          : '${value['maxOutputTokens']}';
      systemPrompt.text = asText(value['systemPromptTemplate']);
      apiKey.clear();
      clearApiKey = false;
    } catch (value) {
      error = value;
    } finally {
      if (mounted) setState(() => loading = false);
    }
  }

  Future<void> save() async {
    if (!formKey.currentState!.validate()) return;
    setState(() => saving = true);
    try {
      final value = <String, dynamic>{
        'providerName': 'Gemini',
        'modelName': model.text.trim(),
        'fallbackModelName': fallback.text.trim().isEmpty
            ? null
            : fallback.text.trim(),
        'clearApiKey': clearApiKey,
        'temperature': temperature.text.trim().isEmpty
            ? null
            : double.parse(temperature.text.trim()),
        'maxOutputTokens': maxTokens.text.trim().isEmpty
            ? null
            : int.parse(maxTokens.text.trim()),
        'systemPromptTemplate': systemPrompt.text.trim().isEmpty
            ? null
            : systemPrompt.text.trim(),
        if (apiKey.text.trim().isNotEmpty) 'apiKey': apiKey.text.trim(),
      };
      config = await widget.state.api.updateAdminAiConfig(value);
      apiKey.clear();
      clearApiKey = false;
      if (mounted) _message(context, 'Đã kích hoạt cấu hình AI mới.');
    } catch (value) {
      if (mounted) _message(context, '$value', error: true);
    } finally {
      if (mounted) setState(() => saving = false);
    }
  }

  @override
  void dispose() {
    for (final controller in [
      model,
      fallback,
      apiKey,
      temperature,
      maxTokens,
      systemPrompt,
    ]) {
      controller.dispose();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (loading) return const Center(child: CircularProgressIndicator());
    if (error != null) return ErrorView(error: error!, retry: load);
    return Form(
      key: formKey,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 28),
        children: [
          const _PageHeading(
            title: 'Cấu hình AI',
            subtitle:
                'Thay đổi model Gemini mà không cần triển khai lại backend.',
          ),
          const SizedBox(height: 16),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    'Provider: ${asText(config?['providerName'], 'Gemini')}',
                    style: const TextStyle(fontWeight: FontWeight.w800),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    config?['hasStoredApiKey'] == true
                        ? 'Khóa đang lưu: ${asText(config?['apiKeyHint'], 'Đã cấu hình')}'
                        : 'Đang dùng khóa từ environment',
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 12),
          TextFormField(
            controller: model,
            decoration: const InputDecoration(labelText: 'Primary model'),
            validator: (value) =>
                value?.trim().isEmpty == true ? 'Vui lòng nhập model.' : null,
          ),
          const SizedBox(height: 12),
          TextFormField(
            controller: fallback,
            decoration: const InputDecoration(labelText: 'Fallback model'),
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: TextFormField(
                  controller: temperature,
                  keyboardType: const TextInputType.numberWithOptions(
                    decimal: true,
                  ),
                  decoration: const InputDecoration(labelText: 'Temperature'),
                  validator: (value) {
                    if (value == null || value.trim().isEmpty) return null;
                    final number = double.tryParse(value);
                    return number == null || number < 0 || number > 2
                        ? 'Từ 0 đến 2'
                        : null;
                  },
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: TextFormField(
                  controller: maxTokens,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(labelText: 'Max tokens'),
                  validator: (value) {
                    if (value == null || value.trim().isEmpty) return null;
                    return (int.tryParse(value) ?? 0) < 1
                        ? 'Phải lớn hơn 0'
                        : null;
                  },
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          TextFormField(
            controller: apiKey,
            obscureText: !showApiKey,
            enabled: !clearApiKey,
            decoration: InputDecoration(
              labelText: config?['hasStoredApiKey'] == true
                  ? 'Thay API key'
                  : 'API key mới',
              helperText: 'Để trống để giữ nguyên cấu hình hiện tại.',
              suffixIcon: IconButton(
                onPressed: () => setState(() => showApiKey = !showApiKey),
                icon: Icon(
                  showApiKey ? Icons.visibility_off : Icons.visibility,
                ),
              ),
            ),
          ),
          if (config?['hasStoredApiKey'] == true)
            CheckboxListTile(
              contentPadding: EdgeInsets.zero,
              value: clearApiKey,
              onChanged: (value) => setState(() {
                clearApiKey = value == true;
                if (clearApiKey) apiKey.clear();
              }),
              title: const Text('Xóa khóa đang lưu'),
              subtitle: const Text(
                'Backend sẽ quay lại dùng khóa từ environment.',
              ),
            ),
          const SizedBox(height: 12),
          TextFormField(
            controller: systemPrompt,
            minLines: 6,
            maxLines: 14,
            decoration: const InputDecoration(
              labelText: 'System prompt override',
              alignLabelWithHint: true,
            ),
          ),
          const SizedBox(height: 18),
          FilledButton.icon(
            onPressed: saving ? null : save,
            icon: saving
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: Colors.white,
                    ),
                  )
                : const Icon(Icons.save_rounded),
            label: Text(saving ? 'Đang kích hoạt...' : 'Lưu và kích hoạt'),
          ),
        ],
      ),
    );
  }
}

class _PageHeading extends StatelessWidget {
  const _PageHeading({required this.title, required this.subtitle});
  final String title;
  final String subtitle;

  @override
  Widget build(BuildContext context) => Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      Text(
        title,
        style: Theme.of(
          context,
        ).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900),
      ),
      const SizedBox(height: 4),
      Text(subtitle, style: TextStyle(color: Colors.grey.shade600)),
    ],
  );
}

class _MetricCard extends StatelessWidget {
  const _MetricCard({
    required this.label,
    required this.value,
    required this.icon,
    required this.color,
  });
  final String label;
  final String value;
  final IconData icon;
  final Color color;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(14),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Icon(icon, color: color),
          Text(
            value,
            style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 24),
          ),
          Text(
            label,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: TextStyle(color: Colors.grey.shade600, fontSize: 12),
          ),
        ],
      ),
    ),
  );
}

class _Pagination extends StatelessWidget {
  const _Pagination({
    required this.page,
    required this.totalPages,
    this.onPrevious,
    this.onNext,
  });
  final int page;
  final int totalPages;
  final VoidCallback? onPrevious;
  final VoidCallback? onNext;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 12),
    child: Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        OutlinedButton(onPressed: onPrevious, child: const Text('Trước')),
        Text(
          '$page / $totalPages',
          style: const TextStyle(fontWeight: FontWeight.w700),
        ),
        OutlinedButton(onPressed: onNext, child: const Text('Sau')),
      ],
    ),
  );
}

int _enumNumber(dynamic value, Map<String, int> names) {
  if (value is num) return value.toInt();
  final text = asText(value).toLowerCase();
  return int.tryParse(text) ?? names[text] ?? 0;
}

String _roleLabel(int role) => switch (role) {
  1 => 'Quản trị viên',
  2 => 'Điều hành viên',
  _ => 'Học viên',
};

String _statusLabel(int status) => switch (status) {
  1 => 'Đã vô hiệu hóa',
  2 => 'Đã khóa',
  _ => 'Đang hoạt động',
};

String _difficultyLabel(int difficulty) => switch (difficulty) {
  0 => 'Cơ bản',
  2 => 'Nâng cao',
  _ => 'Trung cấp',
};

String _initial(String value) {
  final trimmed = value.trim();
  return trimmed.isEmpty ? '?' : trimmed.characters.first.toUpperCase();
}

String? _required(String? value) =>
    value == null || value.trim().isEmpty ? 'Trường này là bắt buộc.' : null;

String? _optionalPositiveInt(String? value) {
  if (value == null || value.trim().isEmpty) return null;
  return (int.tryParse(value.trim()) ?? 0) > 0 ? null : 'Nhập số lớn hơn 0.';
}

int? _optionalInt(String value) =>
    value.trim().isEmpty ? null : int.tryParse(value.trim());

String? _optionalText(String value) =>
    value.trim().isEmpty ? null : value.trim();

void _message(BuildContext context, String text, {bool error = false}) {
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(
      content: Text(text),
      backgroundColor: error ? Colors.red.shade700 : NomiTheme.primary,
    ),
  );
}
