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
    gradient: "from-blue-500 to-indigo-600",
    glow: "shadow-blue-200",
    accent: "text-blue-600 bg-blue-50 border-blue-100",
    title: "Dạy thật, không sửa hộ",
    description:
      "Thay vì đưa bản sửa sẵn để copy, NomiWrite chỉ ra nhóm lỗi cố hữu, biến thành bài tập buộc bạn phải hiểu mới qua được.",
    highlight: "Bắt buộc xử lý lỗi — không thụ động",
  },
  {
    icon: BarChart3,
    gradient: "from-emerald-500 to-teal-600",
    glow: "shadow-emerald-200",
    accent: "text-emerald-700 bg-emerald-50 border-emerald-100",
    title: "Tiến bộ đo được theo thời gian",
    description:
      "Hệ thống so sánh từng bài viết với lịch sử. Nhóm lỗi nào đang giảm, từ vựng nào đã thành thạo — hiển thị rõ trên dashboard.",
    highlight: "Không còn cảm giác 'học mãi không tiến'",
  },
  {
    icon: Layers,
    gradient: "from-violet-500 to-purple-600",
    glow: "shadow-violet-200",
    accent: "text-violet-700 bg-violet-50 border-violet-100",
    title: "Đa loại văn bản trong một nơi",
    description:
      "IELTS Task 2, email công việc, luận học bổng, học thuật — mỗi loại chấm theo tiêu chí riêng, không dùng chung một thước đo.",
    highlight: "Từ luyện thi đến viết thực tế công việc",
  },
  {
    icon: UserCheck,
    gradient: "from-orange-500 to-red-500",
    glow: "shadow-orange-200",
    accent: "text-orange-700 bg-orange-50 border-orange-100",
    title: "Cá nhân hóa theo lỗi người Việt",
    description:
      "20 nhóm lỗi ngữ pháp phổ biến nhất của người Việt. Từ vựng và quiz gắn với chủ đề bài viết thật của bạn.",
    highlight: "Không phải bài tập chung chung cho mọi người",
  },
];

export default function Features() {
  return (
    <section id="features" className="py-20 sm:py-28 bg-white">
      <div className="max-w-6xl mx-auto px-4 sm:px-6">
        {/* Header */}
        <div className="text-center mb-16">
          <span className="inline-block px-3 py-1 text-xs font-bold text-violet-600 bg-violet-50 rounded-full uppercase tracking-widest mb-4">
            Tính năng
          </span>
          <h2 className="text-4xl sm:text-5xl font-extrabold text-slate-900 leading-tight">
            Khác biệt then chốt
          </h2>
          <p className="mt-5 text-lg text-slate-500 max-w-xl mx-auto">
            Bốn điểm làm NomiWrite khác với mọi công cụ bạn đã dùng.
          </p>
        </div>

        {/* Feature grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {features.map((feature) => {
            const Icon = feature.icon;
            return (
              <div
                key={feature.title}
                className="group rounded-2xl border border-slate-100 bg-white p-8 card-hover overflow-hidden relative"
              >
                {/* Subtle top gradient accent */}
                <div className={`absolute top-0 left-0 right-0 h-1 bg-gradient-to-r ${feature.gradient}`} />

                <div className={`w-12 h-12 rounded-2xl bg-gradient-to-br ${feature.gradient} flex items-center justify-center mb-5 shadow-lg ${feature.glow}`}>
                  <Icon className="w-6 h-6 text-white" />
                </div>
                <h3 className="text-xl font-extrabold text-slate-900 mb-3">
                  {feature.title}
                </h3>
                <p className="text-sm text-slate-500 leading-relaxed mb-5">
                  {feature.description}
                </p>
                <div className={`inline-flex items-center gap-2 px-3 py-1.5 border rounded-full ${feature.accent}`}>
                  <Check className="w-3.5 h-3.5 shrink-0" />
                  <span className="text-xs font-semibold">
                    {feature.highlight}
                  </span>
                </div>
              </div>
            );
          })}
        </div>

        {/* Grammar categories teaser */}
        <div className="mt-10 rounded-2xl bg-gradient-to-br from-slate-900 to-slate-800 p-8 overflow-hidden relative">
          <div className="absolute top-0 right-0 w-64 h-64 bg-blue-500/10 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2" />
          <div className="relative">
            <div className="flex flex-col sm:flex-row items-start sm:items-center gap-3 mb-5">
              <div className="w-10 h-10 rounded-xl bg-blue-500/20 border border-blue-400/30 flex items-center justify-center shrink-0">
                <UserCheck className="w-5 h-5 text-blue-400" />
              </div>
              <div>
                <h4 className="font-extrabold text-white text-lg">
                  20 nhóm lỗi ngữ pháp phổ biến nhất của người Việt
                </h4>
                <p className="text-sm text-slate-400 mt-0.5">
                  Danh mục cố định — AI luôn gán lỗi đúng nhóm, không tự đặt tên mới. Thống kê nhất quán theo thời gian.
                </p>
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
                      ? "bg-blue-500/20 border-blue-400/40 text-blue-300"
                      : "bg-white/10 border-white/10 text-slate-300 hover:bg-white/15 transition-colors"
                  }`}
                >
                  {cat}
                </span>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
