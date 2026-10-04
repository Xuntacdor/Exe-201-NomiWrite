import 'package:flutter/material.dart';
import '../core/app_state.dart';
import '../ui/theme.dart';

class AuthScreen extends StatefulWidget {
  const AuthScreen({super.key, required this.state});
  final AppState state;
  @override
  State<AuthScreen> createState() => _AuthScreenState();
}

class _AuthScreenState extends State<AuthScreen> {
  final _formKey = GlobalKey<FormState>();
  final _name = TextEditingController(),
      _email = TextEditingController(),
      _password = TextEditingController();
  bool register = false, hidden = true;
  @override
  void dispose() {
    _name.dispose();
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    try {
      if (register) {
        await widget.state.register(
          _name.text.trim(),
          _email.text.trim(),
          _password.text,
        );
      } else {
        await widget.state.login(_email.text.trim(), _password.text);
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(widget.state.error ?? 'Đăng nhập thất bại.')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    body: SafeArea(
      child: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 440),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  const Align(
                    alignment: Alignment.centerLeft,
                    child: BrandMark(),
                  ),
                  const SizedBox(height: 42),
                  Text(
                    register ? 'Bắt đầu hành trình viết' : 'Chào mừng trở lại',
                    style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                      fontWeight: FontWeight.w900,
                      color: NomiTheme.ink,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    register
                        ? 'Tạo tài khoản để luyện viết cùng phản hồi AI.'
                        : 'Đăng nhập để tiếp tục tiến độ của bạn.',
                    style: TextStyle(color: Colors.grey.shade600, fontSize: 16),
                  ),
                  const SizedBox(height: 28),
                  if (register) ...[
                    TextFormField(
                      controller: _name,
                      textInputAction: TextInputAction.next,
                      decoration: const InputDecoration(
                        labelText: 'Họ và tên',
                        prefixIcon: Icon(Icons.person_outline),
                      ),
                      validator: (v) => v == null || v.trim().length < 2
                          ? 'Vui lòng nhập họ tên.'
                          : null,
                    ),
                    const SizedBox(height: 14),
                  ],
                  TextFormField(
                    controller: _email,
                    keyboardType: TextInputType.emailAddress,
                    textInputAction: TextInputAction.next,
                    decoration: const InputDecoration(
                      labelText: 'Email',
                      prefixIcon: Icon(Icons.mail_outline),
                    ),
                    validator: (v) => v != null && v.contains('@')
                        ? null
                        : 'Email không hợp lệ.',
                  ),
                  const SizedBox(height: 14),
                  TextFormField(
                    controller: _password,
                    obscureText: hidden,
                    onFieldSubmitted: (_) => _submit(),
                    decoration: InputDecoration(
                      labelText: 'Mật khẩu',
                      prefixIcon: const Icon(Icons.lock_outline),
                      suffixIcon: IconButton(
                        onPressed: () => setState(() => hidden = !hidden),
                        icon: Icon(
                          hidden
                              ? Icons.visibility_outlined
                              : Icons.visibility_off_outlined,
                        ),
                      ),
                    ),
                    validator: (v) => (v?.length ?? 0) < 6
                        ? 'Mật khẩu cần ít nhất 6 ký tự.'
                        : null,
                  ),
                  if (!register)
                    Align(
                      alignment: Alignment.centerRight,
                      child: TextButton(
                        onPressed: () => Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => AccountRecoveryScreen(
                              state: widget.state,
                              initialEmail: _email.text.trim(),
                            ),
                          ),
                        ),
                        child: const Text('Quên mật khẩu hoặc xác minh email?'),
                      ),
                    ),
                  const SizedBox(height: 22),
                  AnimatedBuilder(
                    animation: widget.state,
                    builder: (_, _) => FilledButton(
                      onPressed: widget.state.busy ? null : _submit,
                      child: widget.state.busy
                          ? const SizedBox(
                              width: 22,
                              height: 22,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                color: Colors.white,
                              ),
                            )
                          : Text(register ? 'Tạo tài khoản' : 'Đăng nhập'),
                    ),
                  ),
                  const SizedBox(height: 12),
                  TextButton(
                    onPressed: () => setState(() => register = !register),
                    child: Text(
                      register
                          ? 'Đã có tài khoản? Đăng nhập'
                          : 'Chưa có tài khoản? Đăng ký',
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    ),
  );
}

class AccountRecoveryScreen extends StatefulWidget {
  const AccountRecoveryScreen({
    super.key,
    required this.state,
    this.initialEmail = '',
  });

  final AppState state;
  final String initialEmail;

  @override
  State<AccountRecoveryScreen> createState() => _AccountRecoveryScreenState();
}

class _AccountRecoveryScreenState extends State<AccountRecoveryScreen> {
  late final TextEditingController email;
  final token = TextEditingController();
  final password = TextEditingController();
  bool busy = false;
  bool hidePassword = true;

  @override
  void initState() {
    super.initState();
    email = TextEditingController(text: widget.initialEmail);
  }

  @override
  void dispose() {
    email.dispose();
    token.dispose();
    password.dispose();
    super.dispose();
  }

  Future<void> run(Future<String> Function() action) async {
    if (busy) return;
    setState(() => busy = true);
    try {
      final message = await action();
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(message)));
      }
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('$error')));
      }
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  @override
  Widget build(BuildContext context) => DefaultTabController(
    length: 3,
    child: Scaffold(
      appBar: AppBar(
        title: const Text('Khôi phục tài khoản'),
        bottom: const TabBar(
          isScrollable: true,
          tabs: [
            Tab(text: 'Quên mật khẩu'),
            Tab(text: 'Đặt lại mật khẩu'),
            Tab(text: 'Xác minh email'),
          ],
        ),
      ),
      body: TabBarView(
        children: [
          _pane(
            children: [
              const Text(
                'Nhập email để nhận mã đặt lại mật khẩu.',
                style: TextStyle(fontSize: 16),
              ),
              const SizedBox(height: 18),
              TextField(
                controller: email,
                onChanged: (_) => setState(() {}),
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(
                  labelText: 'Email',
                  prefixIcon: Icon(Icons.mail_outline),
                ),
              ),
              const SizedBox(height: 18),
              FilledButton(
                onPressed: busy || !email.text.contains('@')
                    ? null
                    : () => run(
                        () =>
                            widget.state.api.forgotPassword(email.text.trim()),
                      ),
                child: Text(busy ? 'Đang gửi...' : 'Gửi yêu cầu'),
              ),
            ],
          ),
          _pane(
            children: [
              const Text(
                'Dán token nhận được qua email và nhập mật khẩu mới.',
                style: TextStyle(fontSize: 16),
              ),
              const SizedBox(height: 18),
              TextField(
                controller: token,
                onChanged: (_) => setState(() {}),
                decoration: const InputDecoration(
                  labelText: 'Reset token',
                  prefixIcon: Icon(Icons.key_outlined),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: password,
                onChanged: (_) => setState(() {}),
                obscureText: hidePassword,
                decoration: InputDecoration(
                  labelText: 'Mật khẩu mới',
                  prefixIcon: const Icon(Icons.lock_outline),
                  suffixIcon: IconButton(
                    onPressed: () =>
                        setState(() => hidePassword = !hidePassword),
                    icon: Icon(
                      hidePassword ? Icons.visibility : Icons.visibility_off,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 18),
              FilledButton(
                onPressed:
                    busy ||
                        token.text.trim().isEmpty ||
                        password.text.length < 6
                    ? null
                    : () => run(
                        () => widget.state.api.resetPassword(
                          token.text.trim(),
                          password.text,
                        ),
                      ),
                child: Text(busy ? 'Đang xử lý...' : 'Đặt lại mật khẩu'),
              ),
            ],
          ),
          _pane(
            children: [
              const Text(
                'Dán token xác minh được gửi tới email của bạn.',
                style: TextStyle(fontSize: 16),
              ),
              const SizedBox(height: 18),
              TextField(
                controller: token,
                onChanged: (_) => setState(() {}),
                decoration: const InputDecoration(
                  labelText: 'Verification token',
                  prefixIcon: Icon(Icons.verified_outlined),
                ),
              ),
              const SizedBox(height: 18),
              FilledButton(
                onPressed: busy || token.text.trim().isEmpty
                    ? null
                    : () => run(
                        () => widget.state.api.verifyEmail(token.text.trim()),
                      ),
                child: Text(busy ? 'Đang xác minh...' : 'Xác minh email'),
              ),
            ],
          ),
        ],
      ),
    ),
  );

  Widget _pane({required List<Widget> children}) =>
      ListView(padding: const EdgeInsets.all(20), children: children);
}
