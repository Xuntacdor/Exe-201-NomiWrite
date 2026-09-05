import AppShell from "../components/AppShell";
import {
  User,
  Mail,
  Target,
  TrendingUp,
  Award,
  Edit2,
  Zap,
  BookOpen,
  PenLine,
  BrainCircuit,
  CheckCircle2,
} from "lucide-react";

const errorBreakdown = [
  { label: "Mạo từ",         pct: 85, color: "bg-red-500" },
  { label: "Chia thì",       pct: 58, color: "bg-orange-400" },
  { label: "Giới từ",        pct: 42, color: "bg-yellow-400" },
  { label: "Collocation",    pct: 33, color: "bg-violet-400" },
  { label: "Trật tự từ",    pct: 21, color: "bg-blue-400" },
];

const achievements = [
  { icon: PenLine,      label: "Bài viết đầu tiên",    done: true  },
  { icon: BrainCircuit, label: "Hoàn thành 10 quiz",   done: true  },
  { icon: TrendingUp,   label: "Band 7.0 lần đầu",     done: true  },
  { icon: BookOpen,     label: "Thành thạo 50 từ",     done: false },
  { icon: Award,        label: "Streak 30 ngày",        done: false },
  { icon: Zap,          label: "100 bài nộp",           done: false },
];

export default function ProfilePage() {
  return (
    <AppShell activePath="/profile">
      {/* Top bar */}
      <div className="sticky top-0 z-10 bg-white/90 backdrop-blur border-b border-slate-100 px-6 h-14 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <User className="w-4 h-4 text-slate-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Hồ sơ học tập</h1>
        </div>
        <button className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 bg-slate-100 rounded-lg hover:bg-slate-200 transition-colors">
          <Edit2 className="w-3.5 h-3.5" />
          Chỉnh sửa
        </button>
      </div>

      <div className="p-6 w-full space-y-5">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">

          {/* Left: Profile card */}
          <div className="lg:col-span-1 space-y-4">
            {/* Avatar + info */}
            <div className="bg-white rounded-2xl border border-slate-100 shadow-sm p-6 text-center">
              <div className="w-20 h-20 rounded-2xl bg-gradient-to-br from-blue-500 to-violet-600 flex items-center justify-center text-white text-3xl font-extrabold mx-auto mb-4 shadow-lg shadow-blue-200">
                M
              </div>
              <h2 className="text-base font-extrabold text-slate-900">Minh Quang</h2>
              <div className="flex items-center justify-center gap-1.5 mt-1 text-xs text-slate-500">
                <Mail className="w-3 h-3" />
                <span>minhquang@email.com</span>
              </div>

              <div className="mt-4 flex items-center justify-center gap-2">
                <span className="px-3 py-1 text-xs font-bold bg-blue-50 text-blue-700 border border-blue-200 rounded-full">
                  B1 — Trung cấp
                </span>
                <span className="px-3 py-1 text-xs font-bold bg-violet-50 text-violet-700 border border-violet-200 rounded-full">
                  Gói miễn phí
                </span>
              </div>

              <div className="mt-5 pt-5 border-t border-slate-100">
                <div className="flex items-center gap-2 text-xs text-slate-500 mb-2">
                  <Target className="w-3.5 h-3.5 text-blue-500" />
                  <span className="font-semibold text-slate-700">Mục tiêu:</span>
                  IELTS Writing Band 7.0
                </div>
                <div className="flex items-center gap-2 text-xs text-slate-500">
                  <TrendingUp className="w-3.5 h-3.5 text-emerald-500" />
                  <span className="font-semibold text-slate-700">Tham gia:</span>
                  3 tháng trước
                </div>
              </div>
            </div>

            {/* Plan card */}
            <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
              <p className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-3">Gói hiện tại</p>
              <p className="text-base font-extrabold text-slate-900 mb-1">Miễn phí</p>
              <div className="flex items-center justify-between text-xs text-slate-500 mb-2">
                <span>Bài đã viết tháng này</span>
                <span className="font-bold text-slate-700">3/5</span>
              </div>
              <div className="h-1.5 bg-slate-100 rounded-full overflow-hidden mb-4">
                <div className="h-full bg-blue-500 rounded-full" style={{ width: "60%" }} />
              </div>
              <a href="/upgrade" className="block w-full py-2.5 text-xs font-bold text-center text-white bg-gradient-to-r from-blue-600 to-violet-600 rounded-xl hover:opacity-90 transition-opacity">
                Nâng cấp Premium →
              </a>
            </div>
          </div>

          {/* Right: Stats + weaknesses + achievements */}
          <div className="lg:col-span-2 space-y-4">
            {/* Stats */}
            <div className="grid grid-cols-3 gap-3">
              {[
                { label: "Tổng bài viết", value: "12",  color: "text-slate-900" },
                { label: "Band trung bình", value: "6.2", color: "text-blue-600" },
                { label: "Streak hiện tại", value: "7 ngày", color: "text-orange-500" },
              ].map(({ label, value, color }) => (
                <div key={label} className="bg-white rounded-2xl border border-slate-100 shadow-sm p-4 text-center">
                  <p className={`text-xl font-extrabold ${color}`}>{value}</p>
                  <p className="text-xs text-slate-500 mt-0.5">{label}</p>
                </div>
              ))}
            </div>

            {/* Weakness profile */}
            <div className="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
              <h3 className="text-sm font-extrabold text-slate-900 mb-4">Nhóm lỗi thường gặp</h3>
              <div className="space-y-3">
                {errorBreakdown.map(({ label, pct, color }) => (
                  <div key={label}>
                    <div className="flex justify-between mb-1.5">
                      <span className="text-xs font-semibold text-slate-700">{label}</span>
                      <span className="text-xs text-slate-400">{pct}% tần suất</span>
                    </div>
                    <div className="h-2 bg-slate-100 rounded-full overflow-hidden">
                      <div className={`h-full ${color} rounded-full transition-all`} style={{ width: `${pct}%` }} />
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Achievements */}
            <div className="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
              <h3 className="text-sm font-extrabold text-slate-900 mb-4">Thành tích</h3>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                {achievements.map(({ icon: Icon, label, done }) => (
                  <div
                    key={label}
                    className={`flex items-center gap-3 p-3 rounded-xl border transition-all ${
                      done
                        ? "border-emerald-200 bg-emerald-50"
                        : "border-slate-100 bg-slate-50 opacity-50"
                    }`}
                  >
                    <div className={`w-8 h-8 rounded-lg flex items-center justify-center shrink-0 ${
                      done ? "bg-emerald-500" : "bg-slate-200"
                    }`}>
                      {done
                        ? <CheckCircle2 className="w-4 h-4 text-white" />
                        : <Icon className="w-4 h-4 text-slate-400" />}
                    </div>
                    <span className={`text-xs font-semibold leading-tight ${done ? "text-emerald-800" : "text-slate-500"}`}>
                      {label}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </AppShell>
  );
}
