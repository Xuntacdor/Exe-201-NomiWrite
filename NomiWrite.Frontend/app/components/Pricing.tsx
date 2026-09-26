import { Check, Zap, Lock } from "lucide-react";

const freeFeatures = [
  "5 bài viết chấm / tháng",
  "Phân loại lỗi ngữ pháp cơ bản",
  "Gợi ý nâng cấp từ vựng",
  "Quiz sinh tự động từ lỗi",
  "Lưu lịch sử 30 ngày",
  "Hồ sơ điểm yếu ngữ pháp",
];

const freeLocked = [
  "Chấm điểm chi tiết theo tiêu chí",
  "Phân tích sâu tiến bộ",
  "Xuất báo cáo",
];

const proFeatures = [
  "Không giới hạn số bài viết",
  "Chấm điểm chi tiết theo từng tiêu chí",
  "Phân tích sâu tiến bộ theo thời gian",
  "Quiz ưu tiên nhóm lỗi yếu nhất",
  "Sổ từ vựng cá nhân đầy đủ",
  "Lưu lịch sử không giới hạn",
  "Xuất báo cáo tiến bộ PDF",
  "Hỗ trợ ưu tiên qua email",
];

export default function Pricing() {
  return (
    <section id="pricing" className="py-20 sm:py-28 bg-canvas">
      <div className="max-w-5xl mx-auto px-4 sm:px-6">
        {/* Header */}
        <div className="text-center mb-16">
          <span className="inline-block px-3 py-1 text-xs font-bold text-accent-ink bg-accent rounded-full uppercase tracking-widest mb-4">
            Bảng giá
          </span>
          <h2 className="text-4xl sm:text-5xl font-bold text-ink leading-tight">
            Đơn giản và minh bạch
          </h2>
          <p className="mt-5 text-lg text-muted max-w-xl mx-auto">
            Bắt đầu miễn phí. Nâng cấp khi bạn thấy giá trị và muốn học không giới hạn.
          </p>
        </div>

        {/* Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 max-w-3xl mx-auto">

          {/* Free card */}
          <div className="rounded-xl border-2 border-line bg-surface p-8 flex flex-col">
            <div className="mb-7">
              <p className="text-xs font-bold text-muted uppercase tracking-widest mb-3">
                Miễn phí
              </p>
              <div className="flex items-end gap-1 mb-3">
                <span className="text-5xl font-bold text-ink">0đ</span>
                <span className="text-sm text-muted mb-1.5">mãi mãi</span>
              </div>
              <p className="text-sm text-muted leading-relaxed">
                Đủ để trải nghiệm vòng lặp học tập và thấy giá trị thật.
              </p>
            </div>

            <a
              href="/register"
              className="block w-full py-3.5 text-sm font-bold text-center text-accent-ink bg-surface border-2 border-focus rounded-full hover:bg-accent-hover transition-colors mb-7"
            >
              Bắt đầu ngay
            </a>

            <div className="space-y-3">
              {freeFeatures.map((f) => (
                <div key={f} className="flex items-start gap-3">
                  <div className="w-5 h-5 rounded-full bg-success flex items-center justify-center shrink-0 mt-0.5">
                    <Check className="w-3 h-3 text-success-ink" />
                  </div>
                  <span className="text-sm text-ink">{f}</span>
                </div>
              ))}
              {freeLocked.map((f) => (
                <div key={f} className="flex items-start gap-3 opacity-35">
                  <div className="w-5 h-5 rounded-full bg-surface-muted flex items-center justify-center shrink-0 mt-0.5">
                    <Lock className="w-2.5 h-2.5 text-muted" />
                  </div>
                  <span className="text-sm text-muted">{f}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Premium card */}
          <div className="relative rounded-xl overflow-hidden flex flex-col">
            {/* Gradient background */}
            <div className="absolute inset-0 bg-accent" />
            <div className="absolute inset-0 bg-surface-muted" />

            {/* Badge */}
            <div className="relative flex justify-center pt-4">
              <div className="inline-flex items-center gap-1.5 px-4 py-1 bg-warning text-warning-ink text-xs font-bold rounded-full shadow-sm">
                <Zap className="w-3 h-3 fill-warning-ink" />
                Phổ biến nhất
              </div>
            </div>

            <div className="relative p-8 flex flex-col flex-1">
              <div className="mb-7">
                <p className="text-xs font-bold text-accent-ink uppercase tracking-widest mb-3">
                  Premium
                </p>
                <div className="flex items-end gap-1 mb-3">
                  <span className="text-5xl font-bold text-ink">199K</span>
                  <span className="text-sm text-accent-ink mb-1.5">/ tháng</span>
                </div>
                <p className="text-sm text-accent-ink leading-relaxed">
                  Cho người học nghiêm túc muốn thấy tiến bộ rõ ràng trước ngày thi.
                </p>
              </div>

              <a
                href="/upgrade"
                className="block w-full py-3.5 text-sm font-bold text-center text-accent-ink bg-surface rounded-full hover:bg-accent-hover transition-colors mb-7 shadow-sm"
              >
                Dùng Premium
              </a>

              <div className="space-y-3">
                {proFeatures.map((f) => (
                  <div key={f} className="flex items-start gap-3">
                    <div className="w-5 h-5 rounded-full bg-surface flex items-center justify-center shrink-0 mt-0.5">
                      <Check className="w-3 h-3 text-ink" />
                    </div>
                    <span className="text-sm text-accent-ink">{f}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* Trust note */}
        <p className="mt-8 text-center text-sm text-muted">
          Không cần thẻ tín dụng để dùng gói miễn phí · Hủy bất cứ lúc nào · Thanh toán an toàn
        </p>
      </div>
    </section>
  );
}
