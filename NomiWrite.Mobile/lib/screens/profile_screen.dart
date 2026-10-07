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
    widget.state.api.getAccount(),
    widget.state.api.subscription(),
    widget.state.api.plans(),
  ]);
  void reload() => setState(() => data = _load());
  bool actionBusy = false;

  Future<void> cancelSubscription() async {
    final confirmed = await _confirm(
      title: 'Hủy gói hiện tại?',
      message:
          'Quyền lợi vẫn tuân theo trạng thái mà hệ thống trả về sau khi hủy.',
      action: 'Hủy gói',
    );
    if (!confirmed) return;
    setState(() => actionBusy = true);
    try {
      await widget.state.api.cancelSubscription();
      reload();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Đã cập nhật trạng thái gói.')),
        );
      }
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$error')));
      }
    } finally {
      if (mounted) setState(() => actionBusy = false);
    }
  }

  Future<void> deactivateAccount() async {
    final confirmed = await _confirm(
      title: 'Vô hiệu hóa tài khoản?',
      message: 'Bạn sẽ bị đăng xuất và cần liên hệ hỗ trợ để kích hoạt lại.',
      action: 'Vô hiệu hóa',
    );
    if (!confirmed) return;
    setState(() => actionBusy = true);
    try {
      await widget.state.api.deactivateAccount();
      await widget.state.signOut();
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$error')));
      }
    } finally {
      if (mounted) setState(() => actionBusy = false);
    }
  }

  Future<bool> _confirm({
    required String title,
    required String message,
    required String action,
  }) async =>
      await showDialog<bool>(
        context: context,
        builder: (context) => AlertDialog(
          title: Text(title),
          content: Text(message),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Đóng'),
            ),
            FilledButton(
              onPressed: () => Navigator.pop(context, true),
              child: Text(action),
            ),
          ],
        ),
      ) ??
      false;
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
                if (snap.connectionState == ConnectionState.waiting) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (snap.hasError) {
                  return ErrorView(error: snap.error!, retry: reload);
                }
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
                      const SizedBox(height: 12),
                      Card(
                        child: Column(
                          children: [
                            ListTile(
                              leading: const Icon(Icons.receipt_long_outlined),
                              title: const Text('Lịch sử thanh toán'),
                              trailing: const Icon(Icons.chevron_right),
                              onTap: () => Navigator.push(
                                context,
                                MaterialPageRoute(
                                  builder: (_) =>
                                      PaymentHistoryScreen(state: widget.state),
                                ),
                              ),
                            ),
                            if (subscription != null) ...[
                              const Divider(height: 1),
                              ListTile(
                                leading: const Icon(Icons.cancel_outlined),
                                title: const Text('Hủy gói đăng ký'),
                                onTap: actionBusy ? null : cancelSubscription,
                              ),
                            ],
                            const Divider(height: 1),
                            ListTile(
                              leading: const Icon(
                                Icons.person_off_outlined,
                                color: Colors.redAccent,
                              ),
                              title: const Text(
                                'Vô hiệu hóa tài khoản',
                                style: TextStyle(color: Colors.redAccent),
                              ),
                              onTap: actionBusy ? null : deactivateAccount,
                            ),
                          ],
                        ),
                      ),
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
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$e')));
      }
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
  final promo = TextEditingController();
  int? discountPercent;
  bool validatingPromo = false;

  @override
  void dispose() {
    promo.dispose();
    super.dispose();
  }

  Future<void> validatePromo() async {
    if (promo.text.trim().isEmpty) return;
    setState(() => validatingPromo = true);
    try {
      final value = await widget.state.api.validatePromoCode(promo.text.trim());
      final valid = value['valid'] == true;
      setState(() {
        discountPercent = valid ? asInt(value['discountPercent']) : null;
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              valid
                  ? 'Mã hợp lệ: giảm $discountPercent%.'
                  : 'Mã khuyến mãi không hợp lệ.',
            ),
          ),
        );
      }
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$error')));
      }
    } finally {
      if (mounted) setState(() => validatingPromo = false);
    }
  }

  Future<void> pay(Json plan) async {
    setState(() => loadingId = asText(plan['id']));
    try {
      final result = await widget.state.api.checkout(
        asText(plan['id']),
        asDouble(plan['price']) ?? 0,
        provider,
        promoCode: promo.text.trim().isEmpty ? null : promo.text.trim(),
      );
      final url = asText(result['paymentUrl']);
      if (url.isEmpty ||
          !await launchUrl(
            Uri.parse(url),
            mode: LaunchMode.externalApplication,
          )) {
        throw Exception('Không thể mở cổng thanh toán.');
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$e')));
      }
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
          const SizedBox(height: 14),
          Row(
            children: [
              Expanded(
                child: TextField(
                  controller: promo,
                  textCapitalization: TextCapitalization.characters,
                  decoration: InputDecoration(
                    labelText: 'Mã khuyến mãi',
                    prefixIcon: const Icon(Icons.local_offer_outlined),
                    helperText: discountPercent == null
                        ? null
                        : 'Đang áp dụng giảm $discountPercent%',
                  ),
                ),
              ),
              const SizedBox(width: 8),
              IconButton.filledTonal(
                onPressed: validatingPromo ? null : validatePromo,
                tooltip: 'Kiểm tra mã',
                icon: validatingPromo
                    ? const SizedBox.square(
                        dimension: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.check_rounded),
              ),
            ],
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

class PaymentHistoryScreen extends StatefulWidget {
  const PaymentHistoryScreen({super.key, required this.state});

  final AppState state;

  @override
  State<PaymentHistoryScreen> createState() => _PaymentHistoryScreenState();
}

class _PaymentHistoryScreenState extends State<PaymentHistoryScreen> {
  late Future<List<dynamic>> data = load();

  Future<List<dynamic>> load() => Future.wait([
    widget.state.api.paymentHistory(),
    widget.state.api.refundRequests(),
  ]);

  void reload() => setState(() => data = load());

  Future<void> requestRefund(Json payment) async {
    final reason = TextEditingController();
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Yêu cầu hoàn tiền'),
        content: TextField(
          controller: reason,
          minLines: 2,
          maxLines: 4,
          decoration: const InputDecoration(
            labelText: 'Lý do',
            alignLabelWithHint: true,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Đóng'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Gửi yêu cầu'),
          ),
        ],
      ),
    );
    final message = reason.text.trim();
    reason.dispose();
    if (accepted != true || message.isEmpty) return;
    try {
      await widget.state.api.requestRefund(asText(payment['id']), message);
      reload();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Đã gửi yêu cầu hoàn tiền.')),
        );
      }
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$error')));
      }
    }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(
      title: const Text('Lịch sử thanh toán'),
      actions: [IconButton(onPressed: reload, icon: const Icon(Icons.refresh))],
    ),
    body: FutureBuilder<List<dynamic>>(
      future: data,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }
        if (snapshot.hasError) {
          return ErrorView(error: snapshot.error!, retry: reload);
        }
        final payments = snapshot.data![0] as List<Json>;
        final refunds = snapshot.data![1] as List<Json>;
        final refundedIds = refunds
            .map((item) => asText(item['paymentOrderId']))
            .toSet();
        if (payments.isEmpty) {
          return const EmptyState(
            icon: Icons.receipt_long_outlined,
            title: 'Chưa có giao dịch',
            message: 'Các giao dịch của bạn sẽ xuất hiện tại đây.',
          );
        }
        return RefreshIndicator(
          onRefresh: () async {
            reload();
            await data;
          },
          child: ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: payments.length,
            itemBuilder: (context, index) {
              final payment = payments[index];
              final status = asText(payment['status']);
              final completed = status.toLowerCase() == 'completed';
              final refundSent = refundedIds.contains(asText(payment['id']));
              return Card(
                child: ListTile(
                  isThreeLine: true,
                  leading: CircleAvatar(
                    child: Icon(
                      completed ? Icons.check_rounded : Icons.payments_outlined,
                    ),
                  ),
                  title: Text(
                    '${asDouble(payment['amount'])?.toStringAsFixed(0) ?? '0'} ${asText(payment['currency'], 'VND')}',
                    style: const TextStyle(fontWeight: FontWeight.w800),
                  ),
                  subtitle: Text(
                    '${asText(payment['provider'])} • $status\n${_dateText(asText(payment['createdAt']))}',
                  ),
                  trailing: completed && !refundSent
                      ? IconButton(
                          tooltip: 'Yêu cầu hoàn tiền',
                          onPressed: () => requestRefund(payment),
                          icon: const Icon(Icons.undo_rounded),
                        )
                      : refundSent
                      ? const Icon(Icons.hourglass_top_rounded)
                      : null,
                ),
              );
            },
          ),
        );
      },
    ),
  );
}

String _dateText(String value) {
  final date = DateTime.tryParse(value)?.toLocal();
  if (date == null) return value;
  return '${date.day}/${date.month}/${date.year}';
}
