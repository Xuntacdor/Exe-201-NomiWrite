"use client";

import { useLocale } from "@/lib/i18n/locale";
import { ArrowRight, TrendingUp, Layers, BrainCircuit } from "lucide-react";

const stats = [
  { value: "20+", label: "Nhóm lỗi ngữ pháp", Icon: BrainCircuit, color: "text-accent-ink" },
  { value: "6",   label: "Bước vòng lặp học tập", Icon: TrendingUp, color: "text-rose-ink" },
  { value: "8+",  label: "Loại văn bản hỗ trợ", Icon: Layers, color: "text-success-ink" },
];

export default function CTA() {
  const { t: translateUi } = useLocale();
  return (
    <section className="relative py-24 sm:py-36 overflow-hidden bg-surface-muted">
      {/* Background layers */}
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute inset-0 bg-surface-muted" />
        {/* Grid pattern */}

      </div>

      <div className="relative max-w-5xl mx-auto px-4 sm:px-6 text-center">
        {/* Eyebrow */}
        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-surface border border-line text-muted text-sm font-semibold mb-10">
          <div className="w-2 h-2 rounded-full bg-success" />
          {translateUi("Sẵn sàng bắt đầu")}</div>

        {/* Headline */}
        <h2 className="text-4xl sm:text-5xl md:text-6xl font-bold text-ink leading-tight tracking-tight max-w-3xl mx-auto">
          {translateUi("Bắt đầu luyện viết")}{" "}
          <span className="text-accent-ink">
            {translateUi("đúng cách")}</span>{" "}
          {translateUi("ngay hôm nay")}</h2>

        <p className="mt-6 text-lg sm:text-xl text-muted max-w-xl mx-auto leading-relaxed">
          {translateUi("Nộp bài đầu tiên miễn phí. Xem AI phân tích lỗi và sinh quiz cá nhân hóa ngay lập tức.")}</p>

        {/* CTA buttons */}
        <div className="mt-10 flex flex-col sm:flex-row items-center justify-center gap-3">
          <a
            href="/register"
            className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-bold text-ink bg-accent rounded-full hover:bg-accent-hover transition-all shadow-sm "
          >
            {translateUi("Dùng miễn phí ngay")}<ArrowRight className="w-4 h-4" />
          </a>
          <a
            href="/login"
            className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-semibold text-muted bg-surface rounded-full hover:bg-surface transition-all border border-line"
          >
            {translateUi("Đăng nhập")}</a>
        </div>

        <p className="mt-5 text-sm text-muted">
          {translateUi("Miễn phí · Không cần thẻ tín dụng · Bắt đầu trong 30 giây")}</p>

        {/* Stats */}
        <div className="mt-16 grid grid-cols-3 gap-4 max-w-lg mx-auto">
          {stats.map(({ value, label, Icon, color }) => (
            <div key={label} className="flex flex-col items-center gap-2 p-4 rounded-xl bg-surface border border-line">
              <Icon className={`w-5 h-5 ${color}`} />
              <p className="text-2xl font-bold text-ink">{value}</p>
              <p className="text-xs text-muted leading-tight text-center">{translateUi(label)}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
