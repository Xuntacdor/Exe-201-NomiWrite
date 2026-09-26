import {
  PenLine,
  Bot,
  BookOpen,
  Bookmark,
  BrainCircuit,
  TrendingUp,
  RefreshCw,
  ArrowRight,
} from "lucide-react";

const steps = [
  {
    number: "01",
    icon: PenLine,
    title: "Viết bài",
    description: "Chọn loại văn bản và chủ đề. Viết trong trình soạn thảo đếm từ tự động.",
    gradient: "bg-accent ",
    glow: "",
    border: "border-line",
    num: "text-accent-ink",
  },
  {
    number: "02",
    icon: Bot,
    title: "AI chấm điểm",
    description: "AI phân tích theo tiêu chí riêng từng loại văn bản, trả về điểm và lỗi có cấu trúc.",
    gradient: "bg-rose ",
    glow: "",
    border: "border-line",
    num: "text-rose-ink",
  },
  {
    number: "03",
    icon: BookOpen,
    title: "Phân loại lỗi & từ vựng",
    description: "Lỗi gán vào nhóm cố định (mạo từ, chia thì…). Từ vựng nghèo được gợi ý nâng cấp.",
    gradient: "bg-success ",
    glow: "",
    border: "border-line",
    num: "text-success-ink",
  },
  {
    number: "04",
    icon: Bookmark,
    title: "Lưu hồ sơ cá nhân",
    description: "Toàn bộ lỗi và từ vựng lưu vào hồ sơ riêng. Hệ thống biết bạn hay sai nhóm nào nhất.",
    gradient: "bg-accent ",
    glow: "",
    border: "border-line",
    num: "text-accent-ink",
  },
  {
    number: "05",
    icon: BrainCircuit,
    title: "Sinh quiz ôn tập",
    description: "Quiz tạo từ đúng lỗi và từ vựng của bạn. Nhắm 2–3 nhóm yếu nhất, không học tràn lan.",
    gradient: "bg-rose ",
    glow: "",
    border: "border-line",
    num: "text-rose-ink",
  },
  {
    number: "06",
    icon: TrendingUp,
    title: "Đo tiến bộ thật",
    description: "Bài viết sau so sánh với lịch sử — lỗi cũ giảm chưa? Từ đã ôn có dùng đúng không?",
    gradient: "bg-success ",
    glow: "",
    border: "border-line",
    num: "text-success-ink",
  },
];

export default function HowItWorks() {
  return (
    <section id="how-it-works" className="py-20 sm:py-28 bg-canvas">
      <div className="max-w-6xl mx-auto px-4 sm:px-6">
        {/* Header */}
        <div className="text-center mb-16">
          <span className="inline-block px-3 py-1 text-xs font-bold text-accent-ink bg-accent rounded-full uppercase tracking-widest mb-4">
            Cách hoạt động
          </span>
          <h2 className="text-4xl sm:text-5xl font-bold text-ink leading-tight">
            Vòng lặp học tập khép kín
          </h2>
          <p className="mt-5 text-lg text-muted max-w-xl mx-auto">
            6 bước biến mỗi bài viết thành một buổi học cá nhân hóa —
            từ chấm điểm đến đo tiến bộ thật.
          </p>
        </div>

        {/* Steps grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
          {steps.map((step, idx) => {
            const Icon = step.icon;
            const isLast = idx === steps.length - 1;
            return (
              <div key={step.number} className="relative">
                <div className={`bg-surface rounded-xl border ${step.border} p-6 card-hover h-full flex flex-col shadow-sm`}>
                  {/* Step number watermark */}
                  <span className={`absolute top-4 right-5 text-5xl font-black ${step.num} select-none`}>
                    {step.number}
                  </span>

                  {/* Icon */}
                  <div className={`w-12 h-12 rounded-xl  ${step.gradient} flex items-center justify-center mb-4 shadow-sm ${step.glow}`}>
                    <Icon className="w-5 h-5 text-ink" />
                  </div>

                  <h3 className="text-base font-bold text-ink mb-2">
                    {step.title}
                  </h3>
                  <p className="text-sm text-muted leading-relaxed flex-1">
                    {step.description}
                  </p>

                  {/* Connector arrow on lg screens */}
                  {!isLast && (idx + 1) % 3 !== 0 && (
                    <div className="hidden lg:flex absolute -right-3 top-1/2 -translate-y-1/2 w-6 h-6 bg-surface border border-line rounded-full items-center justify-center z-10 shadow-sm">
                      <ArrowRight className="w-3 h-3 text-muted" />
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>

        {/* Loop indicator */}
        <div className="mt-10 flex justify-center">
          <div className="inline-flex items-center gap-3 px-6 py-3 bg-surface border border-line rounded-full text-sm font-semibold text-accent-ink shadow-sm">
            <RefreshCw className="w-4 h-4 text-accent-ink animate-spin-slow" />
            <span>Bài viết tiếp theo lại bắt đầu vòng lặp — bạn tiến bộ hơn mỗi lần</span>
          </div>
        </div>
      </div>
    </section>
  );
}
