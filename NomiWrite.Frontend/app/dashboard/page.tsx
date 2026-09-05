"use client";

import { useState } from "react";
import AppShell from "../components/AppShell";
import {
  PenLine,
  TrendingUp,
  BrainCircuit,
  BookOpen,
  Bell,
  ArrowRight,
  Flame,
  AlertCircle,
  ChevronRight,
  Sparkles,
  GraduationCap,
  X,
} from "lucide-react";

const stats = [
  { label: "Bài đã nộp",     value: "12",  sub: "+3 tháng này",              color: "text-blue-600",    bg: "bg-blue-50",    Icon: PenLine    },
  { label: "Band trung bình", value: "6.2", sub: "↑ 0.5 so với tháng trước", color: "text-emerald-600", bg: "bg-emerald-50", Icon: TrendingUp  },
  { label: "Quiz đã làm",    value: "38",  sub: "87% chính xác",             color: "text-violet-600",  bg: "bg-violet-50",  Icon: BrainCircuit },
  { label: "Từ đã học",      value: "94",  sub: "12 từ thành thạo",           color: "text-orange-600",  bg: "bg-orange-50",  Icon: BookOpen   },
];

const errorProfile = [
  { label: "Mạo từ (a/an/the)", count: 28, pct: 85, color: "bg-red-500"    },
  { label: "Chia thì động từ",  count: 19, pct: 58, color: "bg-orange-400" },
  { label: "Giới từ",           count: 14, pct: 42, color: "bg-yellow-400" },
  { label: "Collocation",       count: 11, pct: 33, color: "bg-violet-400" },
  { label: "Trật tự từ",        count:  7, pct: 21, color: "bg-blue-400"   },
];

const recentSubmissions = [
  { topic: "Technology & Society", type: "IELTS Task 2", band: 6.5, errors: 8,  date: "Hôm nay"       },
  { topic: "Environment & Climate", type: "IELTS Task 2", band: 6.0, errors: 12, date: "2 ngày trước"  },
  { topic: "Job Application",      type: "Email",        band: 7.0, errors: 4,  date: "5 ngày trước"  },
  { topic: "Education System",     type: "IELTS Task 2", band: 5.5, errors: 15, date: "1 tuần trước"  },
];

const bandHistory = [5.0, 5.5, 5.5, 6.0, 6.0, 6.5, 6.0, 6.5, 6.5, 7.0, 6.5, 6.5];

const onboardingSteps = [
  { icon: PenLine,      color: "bg-blue-600",   title: "Chọn loại bài & viết",  desc: "6 loại văn bản, AI hướng dẫn cấu trúc ngay trong trình soạn thảo" },
  { icon: Sparkles,     color: "bg-violet-600", title: "AI chấm & phân tích",   desc: "Nhận band score, danh sách lỗi phân loại chi tiết theo nhóm" },
  { icon: BrainCircuit, color: "bg-emerald-600", title: "Quiz từ lỗi của bạn",   desc: "Quiz tự động sinh từ đúng nhóm lỗi — không phải câu hỏi chung chung" },
];

export default function DashboardPage() {
  const [showOnboarding, setShowOnboarding] = useState(() => {
    if (typeof window === "undefined") return false;
    try {
      return !localStorage.getItem("nomiwrite_onboarded");
    } catch {
      return false;
    }
  });
  const maxBand = 9;

  const dismissOnboarding = () => {
    try { localStorage.setItem("nomiwrite_onboarded", "1"); } catch {}
    setShowOnboarding(false);
  };

  return (
    <AppShell activePath="/dashboard">
      {/* Onboarding modal */}
      {showOnboarding && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          {/* Backdrop */}
          <div
            className="absolute inset-0 bg-slate-900/60 backdrop-blur-sm"
            onClick={dismissOnboarding}
          />

          {/* Modal */}
          <div className="relative bg-white rounded-3xl shadow-2xl max-w-md w-full p-8 animate-fade-in">
            <button
              onClick={dismissOnboarding}
              className="absolute top-4 right-4 w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 hover:bg-slate-200 transition-colors"
            >
              <X className="w-4 h-4" />
            </button>

            {/* Logo */}
            <div className="flex items-center gap-2.5 mb-6">
              <div className="w-10 h-10 rounded-xl bg-blue-600 flex items-center justify-center shadow-lg shadow-blue-200">
                <PenLine className="w-5 h-5 text-white" strokeWidth={2.5} />
              </div>
              <span className="text-lg font-extrabold text-slate-900">NomiWrite</span>
            </div>

            <h2 className="text-2xl font-extrabold text-slate-900 mb-2">
              Chào mừng bạn! 👋
            </h2>
            <p className="text-sm text-slate-500 leading-relaxed mb-7">
              NomiWrite dạy bạn viết tiếng Anh tốt hơn — không chỉ sửa lỗi hộ. Đây là cách vòng lặp học tập hoạt động:
            </p>

            {/* 3-step flow */}
            <div className="space-y-4 mb-7">
              {onboardingSteps.map((step, i) => {
                const Icon = step.icon;
                return (
                  <div key={i} className="flex items-start gap-4">
                    <div className={`w-9 h-9 rounded-xl ${step.color} flex items-center justify-center shrink-0 shadow-sm`}>
                      <Icon className="w-4.5 h-4.5 text-white" style={{ width: 18, height: 18 }} />
                    </div>
                    <div>
                      <p className="text-sm font-bold text-slate-800">{step.title}</p>
                      <p className="text-xs text-slate-500 leading-relaxed mt-0.5">{step.desc}</p>
                    </div>
                    {i < onboardingSteps.length - 1 && (
                      <div className="absolute left-[2.65rem] mt-9 w-px h-4 bg-slate-200" style={{ position: "relative", left: "auto", marginLeft: "-2.5rem", marginTop: "0.25rem" }} />
                    )}
                  </div>
                );
              })}
            </div>

            {/* CTAs */}
            <div className="flex flex-col gap-2.5">
              <a
                href="/write"
                onClick={dismissOnboarding}
                className="flex items-center justify-center gap-2 w-full py-3.5 rounded-2xl bg-blue-600 text-white text-sm font-extrabold hover:bg-blue-700 transition-all shadow-lg shadow-blue-200 hover:-translate-y-0.5"
              >
                <PenLine className="w-4 h-4" />
                Bắt đầu bài viết đầu tiên
              </a>
              <a
                href="/guide"
                onClick={dismissOnboarding}
                className="flex items-center justify-center gap-2 w-full py-3 rounded-2xl bg-slate-100 text-slate-600 text-sm font-semibold hover:bg-slate-200 transition-all"
              >
                <GraduationCap className="w-4 h-4" />
                Xem hướng dẫn viết trước
              </a>
              <button
                onClick={dismissOnboarding}
                className="text-xs text-slate-400 hover:text-slate-600 py-1 transition-colors"
              >
                Khám phá dashboard trước
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Top bar */}
      <div className="sticky top-0 z-10 bg-white/80 backdrop-blur border-b border-slate-100 px-6 h-16 flex items-center justify-between">
        <div>
          <h1 className="text-base font-extrabold text-slate-900">Xin chào, Minh Quang!</h1>
          <p className="text-xs text-slate-500">Hôm nay viết 1 bài nhé — streak 7 ngày đang chờ bạn</p>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-orange-50 border border-orange-200 text-orange-600 text-xs font-bold">
            <Flame className="w-3.5 h-3.5" />
            7 ngày
          </div>
          <button className="relative w-9 h-9 rounded-full bg-slate-100 flex items-center justify-center text-slate-500 hover:bg-slate-200 transition-colors">
            <Bell className="w-4 h-4" />
            <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-blue-500 rounded-full" />
          </button>
          <div className="w-9 h-9 rounded-full bg-gradient-to-br from-blue-500 to-violet-500 flex items-center justify-center text-white text-sm font-bold">
            M
          </div>
        </div>
      </div>

      <div className="p-6 space-y-6 w-full">
        {/* Stats */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          {stats.map(({ label, value, sub, color, bg, Icon }) => (
            <div key={label} className="bg-white rounded-2xl border border-slate-100 p-4 shadow-sm">
              <div className={`w-9 h-9 rounded-xl ${bg} flex items-center justify-center mb-3`}>
                <Icon className={`${color}`} style={{ width: 18, height: 18 }} />
              </div>
              <p className={`text-2xl font-extrabold ${color}`}>{value}</p>
              <p className="text-xs font-semibold text-slate-700 mt-0.5">{label}</p>
              <p className="text-xs text-slate-400 mt-0.5">{sub}</p>
            </div>
          ))}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-5 gap-5">
          {/* Band chart */}
          <div className="lg:col-span-3 bg-white rounded-2xl border border-slate-100 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-5">
              <div>
                <h3 className="text-sm font-extrabold text-slate-900">Điểm Band qua thời gian</h3>
                <p className="text-xs text-slate-400 mt-0.5">12 bài gần nhất</p>
              </div>
              <span className="px-2.5 py-1 text-xs font-bold text-emerald-700 bg-emerald-50 rounded-full">
                ↑ Đang tiến bộ
              </span>
            </div>
            <div className="flex items-end gap-1.5 h-28">
              {bandHistory.map((band, i) => {
                const pct = (band / maxBand) * 100;
                const isLast = i === bandHistory.length - 1;
                return (
                  <div key={i} className="flex-1 flex flex-col items-center gap-1">
                    <span className={`text-[10px] font-bold ${isLast ? "text-blue-600" : "text-slate-400"}`}>
                      {isLast ? band : ""}
                    </span>
                    <div className="w-full flex flex-col justify-end" style={{ height: 80 }}>
                      <div
                        className={`w-full rounded-t-md transition-all ${isLast ? "bg-blue-600" : "bg-blue-200"}`}
                        style={{ height: `${pct}%` }}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
            <div className="flex items-center justify-between mt-2">
              <span className="text-xs text-slate-400">3 tháng trước</span>
              <span className="text-xs text-slate-400">Hôm nay</span>
            </div>
          </div>

          {/* Error profile */}
          <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-100 p-5 shadow-sm">
            <div className="flex items-center justify-between mb-5">
              <div>
                <h3 className="text-sm font-extrabold text-slate-900">Nhóm lỗi hay gặp</h3>
                <p className="text-xs text-slate-400 mt-0.5">Tổng {recentSubmissions.length * 9} lỗi đã phân tích</p>
              </div>
              <AlertCircle className="w-4 h-4 text-slate-300" />
            </div>
            <div className="space-y-3.5">
              {errorProfile.map((err) => (
                <div key={err.label}>
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xs font-medium text-slate-700">{err.label}</span>
                    <span className="text-xs font-bold text-slate-500">{err.count}</span>
                  </div>
                  <div className="h-1.5 bg-slate-100 rounded-full overflow-hidden">
                    <div className={`h-full rounded-full ${err.color}`} style={{ width: `${err.pct}%` }} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Recent submissions + Quick actions */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
          <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
            <div className="flex items-center justify-between px-5 py-4 border-b border-slate-50">
              <h3 className="text-sm font-extrabold text-slate-900">Bài viết gần đây</h3>
              <a href="/history" className="text-xs text-blue-600 font-semibold hover:text-blue-700 flex items-center gap-1">
                Xem tất cả <ChevronRight className="w-3 h-3" />
              </a>
            </div>
            <div className="divide-y divide-slate-50">
              {recentSubmissions.map((sub, i) => (
                <a key={i} href="/result" className="flex items-center justify-between px-5 py-3.5 hover:bg-slate-50/60 transition-colors group">
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-slate-800 truncate">{sub.topic}</p>
                    <div className="flex items-center gap-2 mt-0.5">
                      <span className="text-xs text-slate-400">{sub.type}</span>
                      <span className="text-slate-200">·</span>
                      <span className="text-xs text-slate-400">{sub.date}</span>
                      <span className="text-slate-200">·</span>
                      <span className="text-xs text-red-500 font-medium">{sub.errors} lỗi</span>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 ml-4 shrink-0">
                    <div className={`px-2.5 py-1 rounded-lg text-sm font-extrabold ${
                      sub.band >= 7 ? "bg-emerald-50 text-emerald-700" :
                      sub.band >= 6 ? "bg-blue-50 text-blue-700" : "bg-orange-50 text-orange-700"
                    }`}>
                      {sub.band}
                    </div>
                    <ChevronRight className="w-4 h-4 text-slate-300 group-hover:text-slate-500 transition-colors" />
                  </div>
                </a>
              ))}
            </div>
          </div>

          <div className="space-y-4">
            <a href="/write" className="block rounded-2xl bg-gradient-to-br from-blue-600 to-violet-600 p-5 text-white hover:-translate-y-1 transition-all shadow-lg shadow-blue-200 group">
              <PenLine className="w-6 h-6 mb-3 text-blue-200" />
              <p className="font-extrabold text-base mb-1">Viết bài mới</p>
              <p className="text-sm text-blue-100">Còn 2 bài trong tháng này</p>
              <div className="flex items-center gap-1 mt-3 text-blue-200 text-xs font-semibold group-hover:gap-2 transition-all">
                Bắt đầu ngay <ArrowRight className="w-3.5 h-3.5" />
              </div>
            </a>

            <a href="/quiz" className="block rounded-2xl bg-white border border-violet-200 p-5 hover:-translate-y-1 transition-all shadow-sm hover:shadow-md group">
              <BrainCircuit className="w-6 h-6 mb-3 text-violet-500" />
              <p className="font-extrabold text-sm text-slate-900 mb-0.5">Ôn tập quiz hôm nay</p>
              <p className="text-xs text-slate-500">12 câu quiz từ lỗi bài trước</p>
              <div className="flex items-center gap-1 mt-3 text-violet-600 text-xs font-semibold group-hover:gap-2 transition-all">
                Làm quiz <ArrowRight className="w-3.5 h-3.5" />
              </div>
            </a>

            <a href="/vocabulary" className="block rounded-2xl bg-white border border-emerald-200 p-5 hover:-translate-y-1 transition-all shadow-sm hover:shadow-md group">
              <BookOpen className="w-6 h-6 mb-3 text-emerald-500" />
              <p className="font-extrabold text-sm text-slate-900 mb-0.5">Sổ từ vựng</p>
              <p className="text-xs text-slate-500">82 từ cần ôn · 12 thành thạo</p>
              <div className="flex items-center gap-1 mt-3 text-emerald-600 text-xs font-semibold group-hover:gap-2 transition-all">
                Ôn từ vựng <ArrowRight className="w-3.5 h-3.5" />
              </div>
            </a>
          </div>
        </div>
      </div>
    </AppShell>
  );
}
