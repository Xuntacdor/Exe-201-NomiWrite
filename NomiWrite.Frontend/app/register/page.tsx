import Link from "next/link";
import { PenLine, Mail, Lock, User, ArrowRight, Globe2, Check } from "lucide-react";

const levels = ["A2 — Sơ cấp", "B1 — Trung cấp", "B2 — Khá", "C1 — Cao cấp"];
const targets = ["IELTS Writing", "VSTEP Writing", "Email công việc", "Viết học thuật", "Viết tự do"];

export default function RegisterPage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-blue-950 to-slate-900 flex items-center justify-center p-4 py-10">
      {/* Background orbs */}
      <div className="fixed inset-0 pointer-events-none overflow-hidden">
        <div className="absolute top-1/4 -left-32 w-96 h-96 bg-blue-600/10 rounded-full blur-3xl" />
        <div className="absolute bottom-1/4 -right-32 w-96 h-96 bg-violet-600/10 rounded-full blur-3xl" />
      </div>

      <div className="relative w-full max-w-md">
        {/* Logo */}
        <div className="text-center mb-8">
          <Link href="/" className="inline-flex items-center gap-2.5">
            <div className="w-10 h-10 rounded-xl bg-blue-600 flex items-center justify-center shadow-lg shadow-blue-900/40">
              <PenLine className="w-5 h-5 text-white" strokeWidth={2.5} />
            </div>
            <span className="text-xl font-extrabold text-white">NomiWrite</span>
          </Link>
          <p className="mt-3 text-sm text-slate-400">
            Tạo tài khoản và bắt đầu luyện viết đúng cách
          </p>
        </div>

        {/* Card */}
        <div className="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl p-8 shadow-2xl">
          {/* Google */}
          <button className="w-full flex items-center justify-center gap-3 py-3 rounded-xl bg-white text-slate-700 text-sm font-semibold hover:bg-slate-50 transition-colors shadow-sm mb-6">
            <Globe2 className="w-4 h-4 text-blue-500" />
            Đăng ký với Google
          </button>

          <div className="flex items-center gap-3 mb-6">
            <div className="flex-1 h-px bg-white/10" />
            <span className="text-xs text-slate-500 font-medium">hoặc điền thông tin</span>
            <div className="flex-1 h-px bg-white/10" />
          </div>

          <form className="space-y-4">
            {/* Name */}
            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">Họ và tên</label>
              <div className="relative">
                <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
                <input
                  type="text"
                  placeholder="Nguyễn Văn A"
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-white/8 border border-white/10 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-blue-500 focus:bg-white/10 transition-all"
                />
              </div>
            </div>

            {/* Email */}
            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">Email</label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
                <input
                  type="email"
                  placeholder="ten@email.com"
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-white/8 border border-white/10 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-blue-500 focus:bg-white/10 transition-all"
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">Mật khẩu</label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
                <input
                  type="password"
                  placeholder="Ít nhất 8 ký tự"
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-white/8 border border-white/10 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-blue-500 focus:bg-white/10 transition-all"
                />
              </div>
            </div>

            {/* Current level */}
            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-2">Trình độ hiện tại</label>
              <div className="grid grid-cols-2 gap-2">
                {levels.map((level, i) => (
                  <label
                    key={level}
                    className={`flex items-center gap-2 p-2.5 rounded-xl border cursor-pointer transition-all ${
                      i === 1
                        ? "border-blue-500 bg-blue-500/15 text-blue-300"
                        : "border-white/10 bg-white/5 text-slate-400 hover:border-white/20"
                    }`}
                  >
                    <div className={`w-3.5 h-3.5 rounded-full border-2 flex items-center justify-center shrink-0 ${
                      i === 1 ? "border-blue-500 bg-blue-500" : "border-slate-600"
                    }`}>
                      {i === 1 && <Check className="w-2 h-2 text-white" />}
                    </div>
                    <span className="text-xs font-medium">{level}</span>
                  </label>
                ))}
              </div>
            </div>

            {/* Target */}
            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-2">Mục tiêu luyện viết</label>
              <div className="flex flex-wrap gap-2">
                {targets.map((t, i) => (
                  <span
                    key={t}
                    className={`px-3 py-1.5 text-xs font-semibold rounded-full border cursor-pointer transition-all ${
                      i === 0
                        ? "border-blue-500 bg-blue-500/15 text-blue-300"
                        : "border-white/10 text-slate-400 hover:border-white/20"
                    }`}
                  >
                    {t}
                  </span>
                ))}
              </div>
            </div>

            {/* Terms */}
            <label className="flex items-start gap-3 cursor-pointer">
              <div className="w-4 h-4 rounded border-2 border-blue-500 bg-blue-500/20 flex items-center justify-center shrink-0 mt-0.5">
                <Check className="w-2.5 h-2.5 text-blue-300" />
              </div>
              <span className="text-xs text-slate-400 leading-relaxed">
                Tôi đồng ý với{" "}
                <a href="#" className="text-blue-400 hover:underline">Điều khoản sử dụng</a>
                {" "}và{" "}
                <a href="#" className="text-blue-400 hover:underline">Chính sách bảo mật</a>
              </span>
            </label>

            <a
              href="/dashboard"
              className="flex items-center justify-center gap-2 w-full py-3.5 rounded-xl bg-blue-600 hover:bg-blue-500 text-white text-sm font-bold transition-all shadow-lg shadow-blue-900/40 hover:-translate-y-0.5 mt-2"
            >
              Tạo tài khoản
              <ArrowRight className="w-4 h-4" />
            </a>
          </form>
        </div>

        <p className="text-center mt-6 text-sm text-slate-500">
          Đã có tài khoản?{" "}
          <a href="/login" className="text-blue-400 hover:text-blue-300 font-semibold transition-colors">
            Đăng nhập
          </a>
        </p>
      </div>
    </div>
  );
}
