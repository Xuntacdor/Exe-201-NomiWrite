import { ArrowRight, TrendingUp, Layers, BrainCircuit } from "lucide-react";

const stats = [
  { value: "20+", label: "Nhóm lỗi ngữ pháp", Icon: BrainCircuit, color: "text-blue-400" },
  { value: "6",   label: "Bước vòng lặp học tập", Icon: TrendingUp, color: "text-violet-400" },
  { value: "8+",  label: "Loại văn bản hỗ trợ", Icon: Layers, color: "text-emerald-400" },
];

export default function CTA() {
  return (
    <section className="relative py-24 sm:py-36 overflow-hidden bg-slate-950">
      {/* Background layers */}
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_80%_60%_at_50%_-10%,rgba(37,99,235,0.25),transparent)]" />
        <div className="absolute -top-20 left-1/4 w-72 h-72 bg-blue-600/15 rounded-full blur-3xl" />
        <div className="absolute bottom-0 right-1/4 w-72 h-72 bg-violet-600/15 rounded-full blur-3xl" />
        {/* Grid pattern */}
        <div className="absolute inset-0 opacity-[0.03]" style={{
          backgroundImage: "linear-gradient(rgba(255,255,255,0.5) 1px,transparent 1px),linear-gradient(90deg,rgba(255,255,255,0.5) 1px,transparent 1px)",
          backgroundSize: "40px 40px"
        }} />
      </div>

      <div className="relative max-w-5xl mx-auto px-4 sm:px-6 text-center">
        {/* Eyebrow */}
        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/5 border border-white/10 text-slate-300 text-sm font-semibold mb-10 backdrop-blur">
          <div className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse-slow" />
          Sẵn sàng bắt đầu
        </div>

        {/* Headline */}
        <h2 className="text-4xl sm:text-5xl md:text-6xl font-extrabold text-white leading-tight tracking-tight max-w-3xl mx-auto">
          Bắt đầu luyện viết{" "}
          <span className="bg-gradient-to-r from-blue-400 to-violet-400 bg-clip-text text-transparent">
            đúng cách
          </span>{" "}
          ngay hôm nay
        </h2>

        <p className="mt-6 text-lg sm:text-xl text-slate-400 max-w-xl mx-auto leading-relaxed">
          Nộp bài đầu tiên miễn phí. Xem AI phân tích lỗi và sinh quiz cá nhân hóa ngay lập tức.
        </p>

        {/* CTA buttons */}
        <div className="mt-10 flex flex-col sm:flex-row items-center justify-center gap-3">
          <a
            href="/register"
            className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-bold text-white bg-blue-600 rounded-full hover:bg-blue-500 transition-all shadow-2xl shadow-blue-900/50 hover:shadow-blue-700/60 hover:-translate-y-1"
          >
            Dùng miễn phí ngay
            <ArrowRight className="w-4 h-4" />
          </a>
          <a
            href="/login"
            className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-semibold text-slate-300 bg-white/5 backdrop-blur rounded-full hover:bg-white/10 transition-all border border-white/10"
          >
            Đăng nhập
          </a>
        </div>

        <p className="mt-5 text-sm text-slate-600">
          Miễn phí · Không cần thẻ tín dụng · Bắt đầu trong 30 giây
        </p>

        {/* Stats */}
        <div className="mt-16 grid grid-cols-3 gap-4 max-w-lg mx-auto">
          {stats.map(({ value, label, Icon, color }) => (
            <div key={label} className="flex flex-col items-center gap-2 p-4 rounded-2xl bg-white/5 border border-white/8">
              <Icon className={`w-5 h-5 ${color}`} />
              <p className="text-2xl font-extrabold text-white">{value}</p>
              <p className="text-xs text-slate-500 leading-tight text-center">{label}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
