import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import '../core/app_state.dart';
import '../core/models.dart';
import '../ui/theme.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key, required this.state});
  final AppState state;
  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  late Future<List<dynamic>> data;
  @override
  void initState() {
    super.initState();
    data = _load();
  }

  Future<List<dynamic>> _load() => Future.wait([
    widget.state.api.getProfile(),
    widget.state.api.subscription(),
    widget.state.api.plans(),
  ]);
  void reload() => setState(() => data = _load());
  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: Column(
        children: [
          AppBar(
            title: const Text(
              'Tài khoản',
              style: TextStyle(fontWeight: FontWeight.w900),
            ),
          ),
          Expanded(
            child: FutureBuilder<List<dynamic>>(
              future: data,
              builder: (_, snap) {
                if (snap.connectionState == ConnectionState.waiting)
                  return const Center(child: CircularProgressIndicator());
                if (snap.hasError)
                  return ErrorView(error: snap.error!, retry: reload);
                final profile = snap.data![0] as Json;
                final subscription = snap.data![1] as Json?;
                final plans = snap.data![2] as List<Json>;
                final active = profile['hasActiveSubscription'] == true;
                return RefreshIndicator(
                  onRefresh: () async {
                    reload();
                    await data;
                  },
                  child: ListView(
                    padding: const EdgeInsets.all(20),
                    children: [
                      Center(
                        child: CircleAvatar(
                          radius: 43,
                          backgroundColor: NomiTheme.primary,
                          child: Text(
                            _initials(
                              asText(
                                profile['displayName'],
                                widget.state.session?.fullName ?? 'N W',
                              ),
                            ),
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 25,
                              fontWeight: FontWeight.w900,
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 12),
                      Center(
                        child: Text(
                          asText(
                            profile['displayName'],
                            widget.state.session?.fullName ?? '',
                          ),
                          style: const TextStyle(
                            fontWeight: FontWeight.w900,
                            fontSize: 22,
                          ),
                        ),
                      ),
                      Center(
                        child: Text(
                          widget.state.session?.email ?? '',
                          style: TextStyle(color: Colors.grey.shade600),
                        ),
                      ),
                      const SizedBox(height: 20),
                      Card(
                        child: ListTile(
                          leading: Icon(
                            active
                                ? Icons.workspace_premium
                                : Icons.stars_outlined,
                            color: active ? NomiTheme.gold : NomiTheme.primary,
                          ),
                          title: Text(
                            active
                                ? asText(
                                    profile['subscriptionPlanName'],
                                    'Premium',
                                  )
                                : 'Gói miễn phí',
                            style: const TextStyle(fontWeight: FontWeight.w800),
                          ),
                          subtitle: Text(
                            active
                                ? 'Còn hiệu lực đến ${_date(asText(profile['subscriptionEndDate']))}'
                                : 'Nâng cấp để mở khóa toàn bộ tính năng',
                          ),
                          trailing: active
                              ? const Icon(
                                  Icons.verified,
                                  color: NomiTheme.mint,
                                )
                              : const Icon(Icons.chevron_right),
                          onTap: active
                              ? null
                              : () => Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (_) => PlansScreen(
                                      state: widget.state,
                                      plans: plans,
                                    ),
                                  ),
                                ),
                        ),
                      ),
                      const SizedBox(height: 12),
                      Card(
                        child: Column(
                          children: [
                            ListTile(
                              leading: const Icon(Icons.person_outline),
                              title: const Text('Chỉnh sửa hồ sơ'),
                              trailing: const Icon(Icons.chevron_right),
                              onTap: () async {
                                await Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (_) => EditProfileScreen(
                                      state: widget.state,
                                      profile: profile,
                                    ),
                                  ),
                                );
                                reload();
                              },
                            ),
                            const Divider(height: 1),
                            ListTile(
                              leading: const Icon(Icons.flag_outlined),
                              title: const Text('Mục tiêu'),
                              subtitle: Text(
                                '${asText(profile['targetExam'], 'Chưa chọn')} · Band ${asText(profile['targetBand'], '—')}',
                              ),
                            ),
                            const Divider(height: 1),
                            ListTile(
                              leading: const Icon(Icons.translate),
                              title: const Text('Trình độ tiếng Anh'),
                              subtitle: Text(
                                asText(
                                  profile['englishLevel'],
                                  'Chưa cập nhật',
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      if (subscription != null) ...[
                        const SizedBox(height: 12),
                        Card(
                          child: ListTile(
                            leading: const Icon(Icons.event_available),
                            title: Text(
                              asText(subscription['planName'], 'Gói hiện tại'),
                            ),
                            subtitle: Text(
                              '${asText(subscription['status'])} · còn ${asText(subscription['daysRemaining'])} ngày',
                            ),
                          ),
                        ),
                      ],
                      const SizedBox(height: 18),
                      OutlinedButton.icon(
                        onPressed: () => widget.state.signOut(),
                        icon: const Icon(Icons.logout),
                        label: const Text('Đăng xuất'),
                        style: OutlinedButton.styleFrom(
                          foregroundColor: Colors.redAccent,
                          minimumSize: const Size.fromHeight(50),
                        ),
                      ),
                      const SizedBox(height: 20),
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

  String _initials(String name) {
    final p = name.trim().split(RegExp(r'\s+'));
    return p.take(2).map((e) => e.isEmpty ? '' : e[0]).join().toUpperCase();
  }

  String _date(String value) {
    final d = DateTime.tryParse(value);
    return d == null ? value : '${d.day}/${d.month}/${d.year}';
  }
}

class EditProfileScreen extends StatefulWidget {
  const EditProfileScreen({
    super.key,
    required this.state,
    required this.profile,
  });
  final AppState state;
  final Json profile;
  @override
  State<EditProfileScreen> createState() => _EditProfileScreenState();
}

class _EditProfileScreenState extends State<EditProfileScreen> {
  late final TextEditingController name;
  String? level, exam;
  double? band;
  bool saving = false;
  @override
  void initState() {
    super.initState();
    name = TextEditingController(text: asText(widget.profile['displayName']));
    level = widget.profile['englishLevel'] as String?;
    exam = widget.profile['targetExam'] as String?;
    band = asDouble(widget.profile['targetBand']);
  }

  @override
  void dispose() {
    name.dispose();
    super.dispose();
  }

  Future<void> save() async {
    setState(() => saving = true);
    try {
      await widget.state.api.updateProfile({
        'displayName': name.text.trim(),
        'englishLevel': level,
        'targetExam': exam,
        'targetBand': band,
      });
      await widget.state.loadProfile();
      if (mounted) Navigator.pop(context);
    } catch (e) {
      if (mounted)
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$e')));
    } finally {
      if (mounted) setState(() => saving = false);
    }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Chỉnh sửa hồ sơ')),
    body: ListView(
      padding: const EdgeInsets.all(20),
      children: [
        TextField(
          controller: name,
          decoration: const InputDecoration(
            labelText: 'Tên hiển thị',
            prefixIcon: Icon(Icons.person_outline),
          ),
        ),
        const SizedBox(height: 14),
        DropdownButtonFormField<String>(
          initialValue: level,
          decoration: const InputDecoration(labelText: 'Trình độ hiện tại'),
          items: [
            'Beginner',
            'Elementary',
            'Intermediate',
            'Upper-Intermediate',
            'Advanced',
          ].map((v) => DropdownMenuItem(value: v, child: Text(v))).toList(),
          onChanged: (v) => setState(() => level = v),
        ),
        const SizedBox(height: 14),
        DropdownButtonFormField<String>(
          initialValue: exam,
          decoration: const InputDecoration(labelText: 'Mục tiêu'),
          items: [
            'IELTS',
            'TOEFL',
            'Academic Writing',
            'Professional Writing',
          ].map((v) => DropdownMenuItem(value: v, child: Text(v))).toList(),
          onChanged: (v) => setState(() => exam = v),
        ),
        const SizedBox(height: 20),
        Text(
          'Band mục tiêu: ${band?.toStringAsFixed(1) ?? 'Chưa chọn'}',
          style: const TextStyle(fontWeight: FontWeight.w700),
        ),
        Slider(
          value: band ?? 6.5,
          min: 4,
          max: 9,
          divisions: 10,
          label: (band ?? 6.5).toStringAsFixed(1),
          onChanged: (v) => setState(() => band = v),
        ),
        const SizedBox(height: 20),
        FilledButton(
          onPressed: saving ? null : save,
          child: saving
              ? const CircularProgressIndicator(color: Colors.white)
              : const Text('Lưu thay đổi'),
        ),
      ],
    ),
  );
}

class PlansScreen extends StatefulWidget {
  const PlansScreen({super.key, required this.state, required this.plans});
  final AppState state;
  final List<Json> plans;
  @override
  State<PlansScreen> createState() => _PlansScreenState();
}

class _PlansScreenState extends State<PlansScreen> {
  String provider = 'VNPay';
  String? loadingId;
  Future<void> pay(Json plan) async {
    setState(() => loadingId = asText(plan['id']));
    try {
      final result = await widget.state.api.checkout(
        asText(plan['id']),
        asDouble(plan['price']) ?? 0,
        provider,
      );
      final url = asText(result['paymentUrl']);
      if (url.isEmpty ||
          !await launchUrl(
            Uri.parse(url),
            mode: LaunchMode.externalApplication,
          ))
        throw Exception('Không thể mở cổng thanh toán.');
    } catch (e) {
      if (mounted)
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$e')));
    } finally {
      if (mounted) setState(() => loadingId = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Nâng cấp Premium')),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          const Text(
            'Chọn gói phù hợp',
            style: TextStyle(fontSize: 24, fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 6),
          Text(
            'Mở khóa đề VIP, AI chấm bài và lộ trình cá nhân hóa.',
            style: TextStyle(color: Colors.grey.shade600),
          ),
          const SizedBox(height: 18),
          SegmentedButton<String>(
            segments: const [
              ButtonSegment(value: 'VNPay', label: Text('VNPay')),
              ButtonSegment(value: 'Momo', label: Text('MoMo')),
              ButtonSegment(value: 'VietQR', label: Text('VietQR')),
            ],
            selected: {provider},
            onSelectionChanged: (v) => setState(() => provider = v.first),
          ),
          const SizedBox(height: 16),
          if (widget.plans.isEmpty)
            const EmptyState(
              icon: Icons.credit_card_off,
              title: 'Chưa có gói',
              message: 'Hiện chưa có gói thuê bao đang mở bán.',
            )
          else
            ...widget.plans.map(
              (p) => Card(
                child: Padding(
                  padding: const EdgeInsets.all(18),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        asText(p['name']),
                        style: const TextStyle(
                          fontWeight: FontWeight.w900,
                          fontSize: 20,
                        ),
                      ),
                      const SizedBox(height: 5),
                      Text(asText(p['description'])),
                      const SizedBox(height: 14),
                      Text(
                        '${_money(asDouble(p['price']) ?? 0)} ${asText(p['currency'], 'VND')} / ${asText(p['billingCycle'])}',
                        style: const TextStyle(
                          fontWeight: FontWeight.w900,
                          fontSize: 18,
                          color: NomiTheme.primary,
                        ),
                      ),
                      const SizedBox(height: 14),
                      FilledButton(
                        onPressed: loadingId == asText(p['id'])
                            ? null
                            : () => pay(p),
                        child: loadingId == asText(p['id'])
                            ? const CircularProgressIndicator(
                                color: Colors.white,
                              )
                            : const Text('Chọn gói này'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  String _money(double value) {
    final raw = value.toStringAsFixed(0);
    return raw.replaceAllMapped(RegExp(r'\B(?=(\d{3})+(?!\d))'), (_) => '.');
  }
}
