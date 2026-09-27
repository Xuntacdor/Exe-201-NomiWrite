"use client";

import { useLocale } from "@/lib/i18n/locale";
import {
  ArrowRight,
  Sparkles,
  Star,
  GraduationCap,
  Briefcase,
  PenLine,
  Trophy,
} from "lucide-react";

const badges = [
  "IELTS Writing Task 2",
  "Email công việc",
  "Luận học thuật",
  "Luận học bổng",
  "VSTEP Writing",
];

const avatarIcons = [
  { Icon: GraduationCap, bg: "bg-accent" },
  { Icon: Briefcase,    bg: "bg-rose" },
  { Icon: PenLine,      bg: "bg-success" },
  { Icon: Trophy,       bg: "bg-warning" },
];

export default function Hero() {
  const { t: translateUi } = useLocale();
  return (
    <section className="hero-gradient min-h-screen flex items-center pt-16 overflow-hidden relative">
      {/* Background orbs */}
      <div className="absolute inset-0 pointer-events-none overflow-hidden">
      </div>

      <div className="relative max-w-6xl mx-auto px-4 sm:px-6 py-20 sm:py-28">
        <div className="flex flex-col items-center text-center">

          {/* Eyebrow badge */}
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-surface border border-line text-accent-ink text-sm font-semibold mb-8 shadow-sm">
            <Sparkles className="w-4 h-4 text-accent-ink" />
            <span>{translateUi("AI chấm bài · Học từ chính bài viết của bạn")}</span>
          </div>

          {/* Headline */}
          <h1 className="text-5xl sm:text-6xl md:text-7xl font-bold text-ink leading-[1.08] tracking-tight max-w-4xl">
            {translateUi("Luyện viết tiếng Anh")}{" "}
            <span className="gradient-text">{translateUi("dạy thật,")}</span>
            <br />
            <span className="text-ink">{translateUi("không chỉ sửa hộ")}</span>
          </h1>

          {/* Subheadline */}
          <p className="mt-7 text-xl sm:text-2xl text-muted max-w-2xl leading-relaxed">
            {translateUi("Viết bài → AI phân loại lỗi cố hữu → Sinh quiz cá nhân hóa → Đo tiến bộ thật.")}</p>
          <p className="mt-3 text-base font-semibold text-ink">
            {translateUi("Bài viết là điểm xuất phát, không phải đích đến.")}</p>

          {/* CTA buttons */}
          <div className="mt-10 flex flex-col sm:flex-row items-center gap-3">
            <a
              href="/register"
              className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-bold text-ink bg-accent rounded-full hover:bg-accent-hover transition-all shadow-sm "
            >
              {translateUi("Bắt đầu miễn phí")}<ArrowRight className="w-4 h-4" />
            </a>
            <a
              href="#how-it-works"
              className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-semibold text-ink bg-surface rounded-full hover:bg-surface transition-all border border-line shadow-sm "
            >
              {translateUi("Xem cách hoạt động")}</a>
          </div>

          {/* Social proof */}
          <div className="mt-8 flex flex-wrap justify-center items-center gap-3 text-sm text-muted">
            <div className="flex -space-x-2.5">
              {avatarIcons.map(({ Icon, bg }, i) => (
                <div
                  key={i}
                  className={`w-8 h-8 rounded-full ${bg} border-2 border-line flex items-center justify-center shadow-sm`}
                >
                  <Icon className="w-3.5 h-3.5 text-ink" />
                </div>
              ))}
            </div>
            <div className="flex items-center gap-0.5">
              {[...Array(5)].map((_, i) => (
                <Star key={i} className="w-4 h-4 fill-warning-ink text-warning-ink" />
              ))}
            </div>
            <span className="text-muted font-medium">{translateUi("Dành cho người luyện IELTS & viết chuyên nghiệp")}</span>
          </div>

          {/* Writing type badges */}
          <div className="mt-10 flex flex-wrap justify-center gap-2">
            {badges.map((badge) => (
              <span
                key={badge}
                className="px-4 py-1.5 text-xs font-semibold text-muted bg-surface border border-line rounded-full shadow-sm hover:border-focus hover:text-accent-ink transition-colors"
              >
                {translateUi(badge)}
              </span>
            ))}
          </div>

          {/* Mock UI preview */}
          <div className="mt-16 w-full max-w-4xl">
            <div className="bg-surface rounded-xl shadow-sm border border-line overflow-hidden">
              {/* Window chrome */}
              <div className="flex items-center gap-2 px-5 py-3.5 bg-canvas border-b border-line">
                <div className="w-3 h-3 rounded-full bg-danger" />
                <div className="w-3 h-3 rounded-full bg-warning" />
                <div className="w-3 h-3 rounded-full bg-success" />
                <div className="ml-4 flex-1 h-5 bg-surface-muted rounded-md max-w-sm" />
                <div className="flex items-center gap-1.5 ml-auto">
                  <div className="w-2 h-2 rounded-full bg-success" />
                  <span className="text-xs text-muted font-medium">{translateUi("AI đang chấm…")}</span>
                </div>
              </div>

              {/* Content — 3 panels */}
              <div className="grid grid-cols-1 md:grid-cols-3 divide-y md:divide-y-0 md:divide-x divide-line">
                {/* Panel 1: Essay editor */}
                <div className="p-5 space-y-3">
                  <div className="flex items-center justify-between mb-1">
                    <div className="flex items-center gap-2">
                      <PenLine className="w-3.5 h-3.5 text-muted" />
                      <span className="text-xs font-semibold text-muted uppercase tracking-wide">{translateUi("Bài viết")}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs bg-accent text-accent-ink rounded-full font-semibold">{translateUi("IELTS Task 2")}</span>
                      <span className="text-xs text-muted">{translateUi("287 từ")}</span>
                    </div>
                  </div>
                  <div className="space-y-2">
                    <div className="h-2.5 bg-surface-muted rounded-full w-full" />
                    <div className="h-2.5 bg-surface-muted rounded-full w-11/12" />
                    <div className="relative h-6 flex items-center">
                      <div className="h-2.5 bg-danger rounded-full w-4/5 border border-line" />
                      <span className="absolute left-2 text-[10px] text-danger-ink font-semibold">{translateUi("article error")}</span>
                    </div>
                    <div className="h-2.5 bg-surface-muted rounded-full w-full" />
                    <div className="h-2.5 bg-surface-muted rounded-full w-3/4" />
                    <div className="relative h-6 flex items-center">
                      <div className="h-2.5 bg-warning rounded-full w-full border border-line" />
                      <span className="absolute left-16 text-[10px] text-warning-ink font-semibold">{translateUi("verb tense")}</span>
                    </div>
                    <div className="h-2.5 bg-surface-muted rounded-full w-5/6" />
                    <div className="h-2.5 bg-surface-muted rounded-full w-2/3" />
                  </div>
                </div>

                {/* Panel 2: AI feedback */}
                <div className="p-5 space-y-3.5 bg-canvas">
                  <div className="flex items-center gap-1.5 mb-1">
                    <Sparkles className="w-3.5 h-3.5 text-rose-ink" />
                    <span className="text-xs font-semibold text-muted uppercase tracking-wide">{translateUi("Kết quả AI")}</span>
                  </div>

                  {/* Score card */}
                  <div className="flex items-center gap-3 p-3 rounded-xl bg-accent shadow-sm">
                    <div className="w-11 h-11 rounded-lg bg-surface flex items-center justify-center text-ink font-black text-lg">
                      6.5
                    </div>
                    <div>
                      <p className="text-sm font-bold text-ink">{translateUi("Band 6.5")}</p>
                      <p className="text-xs text-accent-ink">{translateUi("Mức dự kiến IELTS Writing")}</p>
                    </div>
                  </div>

                  <div>
                    <p className="text-xs font-semibold text-muted mb-2">{translateUi("Lỗi thường gặp của bạn")}</p>
                    {[
                      { label: "Mạo từ (a/an/the)", count: 5, w: "w-5/6", color: "bg-danger" },
                      { label: "Chia thì động từ",  count: 3, w: "w-3/5", color: "bg-accent" },
                      { label: "Giới từ",            count: 2, w: "w-2/5", color: "bg-warning" },
                    ].map((err) => (
                      <div key={err.label} className="mb-2">
                        <div className="flex justify-between mb-1">
                          <span className="text-xs text-muted">{translateUi(err.label)}</span>
                          <span className="text-xs font-semibold text-muted">{translateUi(err.count)} {translateUi(" lỗi")}</span>
                        </div>
                        <div className="h-1.5 bg-surface-muted rounded-full overflow-hidden">
                          <div className={`h-full ${err.color} ${err.w} rounded-full`} />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Panel 3: Quiz */}
                <div className="p-5 space-y-3 bg-rose">
                  <div className="flex items-center gap-1.5 mb-1">
                    <GraduationCap className="w-3.5 h-3.5 text-rose-ink" />
                    <span className="text-xs font-semibold text-muted uppercase tracking-wide">{translateUi("Quiz sinh từ lỗi")}</span>
                  </div>

                  <div className="p-3 rounded-xl bg-surface border border-line shadow-sm">
                    <p className="text-xs font-semibold text-ink mb-2">{translateUi("Câu 1/8 — Mạo từ")}</p>
                    <p className="text-xs text-muted mb-3 leading-relaxed">
                      {translateUi("\"__ environment is a shared responsibility.\"")}</p>
                    <div className="space-y-1.5">
                      {["A", "An", "The", "—"].map((opt, i) => (
                        <div
                          key={opt}
                          className={`flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-medium ${
                            i === 2
                              ? "bg-success border border-focus text-success-ink"
                              : "bg-canvas border border-line text-muted"
                          }`}
                        >
                          <span className={`w-4 h-4 rounded-full flex items-center justify-center text-[10px] font-bold ${
                            i === 2 ? "bg-success text-ink" : "bg-surface-muted text-muted"
                          }`}>{translateUi(opt)}</span>
                          <span>{translateUi(opt === "—" ? "Không mạo từ" : opt)}</span>
                        </div>
                      ))}
                    </div>
                  </div>

                  <div className="flex items-center justify-between px-1">
                    <span className="text-xs text-rose-ink font-semibold">{translateUi("7/8 đúng hôm nay")}</span>
                    <div className="flex gap-0.5">
                      {[...Array(8)].map((_, i) => (
                        <div key={i} className={`w-2 h-2 rounded-full ${i < 7 ? "bg-success" : "bg-surface-muted"}`} />
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
