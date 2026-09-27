"use client";

import { useLocale } from "@/lib/i18n/locale";
import {
  Target,
  BarChart3,
  Layers,
  UserCheck,
  Check,
} from "lucide-react";

const features = [
  {
    icon: Target,
    gradient: "bg-accent ",
    glow: "",
    accent: "text-accent-ink bg-accent border-line",
    title: "Dạy thật, không sửa hộ",
    description:
      "Thay vì đưa bản sửa sẵn để copy, NomiWrite chỉ ra nhóm lỗi cố hữu, biến thành bài tập buộc bạn phải hiểu mới qua được.",
    highlight: "Bắt buộc xử lý lỗi — không thụ động",
  },
  {
    icon: BarChart3,
    gradient: "bg-success ",
    glow: "",
    accent: "text-success-ink bg-success border-line",
    title: "Tiến bộ đo được theo thời gian",
    description:
      "Hệ thống so sánh từng bài viết với lịch sử. Nhóm lỗi nào đang giảm, từ vựng nào đã thành thạo — hiển thị rõ trên dashboard.",
    highlight: "Không còn cảm giác 'học mãi không tiến'",
  },
  {
    icon: Layers,
    gradient: "bg-rose ",
    glow: "",
    accent: "text-rose-ink bg-rose border-line",
    title: "Đa loại văn bản trong một nơi",
    description:
      "IELTS Task 2, email công việc, luận học bổng, học thuật — mỗi loại chấm theo tiêu chí riêng, không dùng chung một thước đo.",
    highlight: "Từ luyện thi đến viết thực tế công việc",
  },
  {
    icon: UserCheck,
    gradient: "bg-accent ",
    glow: "",
    accent: "text-accent-ink bg-accent border-line",
    title: "Cá nhân hóa theo lỗi người Việt",
    description:
      "20 nhóm lỗi ngữ pháp phổ biến nhất của người Việt. Từ vựng và quiz gắn với chủ đề bài viết thật của bạn.",
    highlight: "Không phải bài tập chung chung cho mọi người",
  },
];

export default function Features() {
  const { t: translateUi } = useLocale();
  return (
    <section id="features" className="py-20 sm:py-28 bg-surface">
      <div className="max-w-6xl mx-auto px-4 sm:px-6">
        {/* Header */}
        <div className="text-center mb-16">
          <span className="inline-block px-3 py-1 text-xs font-bold text-rose-ink bg-rose rounded-full uppercase tracking-widest mb-4">
            {translateUi("Tính năng")}</span>
          <h2 className="text-4xl sm:text-5xl font-bold text-ink leading-tight">
            {translateUi("Khác biệt then chốt")}</h2>
          <p className="mt-5 text-lg text-muted max-w-xl mx-auto">
            {translateUi("Bốn điểm làm NomiWrite khác với mọi công cụ bạn đã dùng.")}</p>
        </div>

        {/* Feature grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {features.map((feature) => {
            const Icon = feature.icon;
            return (
              <div
                key={feature.title}
                className="group rounded-xl border border-line bg-surface p-8 card-hover overflow-hidden relative"
              >
                {/* Subtle top gradient accent */}
                <div className={`absolute top-0 left-0 right-0 h-1  ${feature.gradient}`} />

                <div className={`w-12 h-12 rounded-xl  ${feature.gradient} flex items-center justify-center mb-5 shadow-sm ${feature.glow}`}>
                  <Icon className="w-6 h-6 text-ink" />
                </div>
                <h3 className="text-xl font-bold text-ink mb-3">
                  {translateUi(feature.title)}
                </h3>
                <p className="text-sm text-muted leading-relaxed mb-5">
                  {translateUi(feature.description)}
                </p>
                <div className={`inline-flex items-center gap-2 px-3 py-1.5 border rounded-full ${feature.accent}`}>
                  <Check className="w-3.5 h-3.5 shrink-0" />
                  <span className="text-xs font-semibold">
                    {translateUi(feature.highlight)}
                  </span>
                </div>
              </div>
            );
          })}
        </div>

        {/* Grammar categories teaser */}
        <div className="mt-10 rounded-xl bg-surface-muted p-8 overflow-hidden relative">
          <div className="relative">
            <div className="flex flex-col sm:flex-row items-start sm:items-center gap-3 mb-5">
              <div className="w-10 h-10 rounded-xl bg-accent border border-focus flex items-center justify-center shrink-0">
                <UserCheck className="w-5 h-5 text-accent-ink" />
              </div>
              <div>
                <h4 className="font-bold text-ink text-lg">
                  {translateUi("20 nhóm lỗi ngữ pháp phổ biến nhất của người Việt")}</h4>
                <p className="text-sm text-muted mt-0.5">
                  {translateUi("Danh mục cố định — AI luôn gán lỗi đúng nhóm, không tự đặt tên mới. Thống kê nhất quán theo thời gian.")}</p>
              </div>
            </div>
            <div className="flex flex-wrap gap-2">
              {[
                "Mạo từ", "Chia thì", "Giới từ", "Số ít / nhiều",
                "Câu điều kiện", "Bị động", "Liên từ", "Collocation",
                "Word form", "Trật tự từ", "+ 10 nhóm khác",
              ].map((cat, i) => (
                <span
                  key={cat}
                  className={`px-3 py-1.5 text-xs font-semibold rounded-full border ${
                    i === 10
                      ? "bg-accent border-focus text-accent-ink"
                      : "bg-surface border-line text-muted hover:bg-surface transition-colors"
                  }`}
                >
                  {translateUi(cat)}
                </span>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
