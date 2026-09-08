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
  { Icon: GraduationCap, bg: "bg-blue-500" },
  { Icon: Briefcase,    bg: "bg-violet-500" },
  { Icon: PenLine,      bg: "bg-emerald-500" },
  { Icon: Trophy,       bg: "bg-amber-500" },
];

export default function Hero() {
  return (
    <section className="hero-gradient min-h-screen flex items-center pt-16 overflow-hidden relative">
      {/* Background orbs */}
      <div className="absolute inset-0 pointer-events-none overflow-hidden">
        <div className="absolute -top-32 -left-32 w-96 h-96 rounded-full bg-blue-300/20 blur-3xl animate-pulse-slow" />
        <div className="absolute top-1/3 -right-40 w-80 h-80 rounded-full bg-violet-300/20 blur-3xl animate-pulse-slow" style={{ animationDelay: "1.5s" }} />
        <div className="absolute bottom-0 left-1/3 w-64 h-64 rounded-full bg-indigo-200/30 blur-3xl" />
      </div>

      <div className="relative max-w-6xl mx-auto px-4 sm:px-6 py-20 sm:py-28">
        <div className="flex flex-col items-center text-center">

          {/* Eyebrow badge */}
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/80 backdrop-blur border border-blue-200 text-blue-700 text-sm font-semibold mb-8 shadow-sm shadow-blue-100">
            <Sparkles className="w-4 h-4 text-blue-500" />
            <span>AI chấm bài · Học từ chính bài viết của bạn</span>
          </div>

          {/* Headline */}
          <h1 className="text-5xl sm:text-6xl md:text-7xl font-extrabold text-slate-900 leading-[1.08] tracking-tight max-w-4xl">
            Luyện viết tiếng Anh{" "}
            <span className="gradient-text">dạy thật,</span>
            <br />
            <span className="text-slate-800">không chỉ sửa hộ</span>
          </h1>

          {/* Subheadline */}
          <p className="mt-7 text-xl sm:text-2xl text-slate-500 max-w-2xl leading-relaxed">
            Viết bài → AI phân loại lỗi cố hữu → Sinh quiz cá nhân hóa → Đo tiến bộ thật.
          </p>
          <p className="mt-3 text-base font-semibold text-slate-700">
            Bài viết là điểm xuất phát, không phải đích đến.
          </p>

          {/* CTA buttons */}
          <div className="mt-10 flex flex-col sm:flex-row items-center gap-3">
            <a
              href="/register"
              className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-bold text-white bg-blue-600 rounded-full hover:bg-blue-700 transition-all shadow-xl shadow-blue-300/40 hover:shadow-blue-400/50 hover:-translate-y-1"
            >
              Bắt đầu miễn phí
              <ArrowRight className="w-4 h-4" />
            </a>
            <a
              href="#how-it-works"
              className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-8 py-4 text-base font-semibold text-slate-700 bg-white/90 backdrop-blur rounded-full hover:bg-white transition-all border border-slate-200 shadow-md hover:-translate-y-0.5"
            >
              Xem cách hoạt động
            </a>
          </div>

          {/* Social proof */}
          <div className="mt-8 flex flex-wrap justify-center items-center gap-3 text-sm text-slate-500">
            <div className="flex -space-x-2.5">
              {avatarIcons.map(({ Icon, bg }, i) => (
                <div
                  key={i}
                  className={`w-8 h-8 rounded-full ${bg} border-2 border-white flex items-center justify-center shadow-sm`}
                >
                  <Icon className="w-3.5 h-3.5 text-white" />
                </div>
              ))}
            </div>
            <div className="flex items-center gap-0.5">
              {[...Array(5)].map((_, i) => (
                <Star key={i} className="w-4 h-4 fill-amber-400 text-amber-400" />
              ))}
            </div>
            <span className="text-slate-600 font-medium">Dành cho người luyện IELTS &amp; viết chuyên nghiệp</span>
          </div>

          {/* Writing type badges */}
          <div className="mt-10 flex flex-wrap justify-center gap-2">
            {badges.map((badge) => (
              <span
                key={badge}
                className="px-4 py-1.5 text-xs font-semibold text-slate-600 bg-white/90 backdrop-blur border border-slate-200 rounded-full shadow-sm hover:border-blue-300 hover:text-blue-700 transition-colors"
              >
                {badge}
              </span>
            ))}
          </div>

          {/* Product UI preview */}
          <div className="mt-16 w-full max-w-4xl animate-float">
            <div className="bg-white rounded-3xl shadow-2xl shadow-blue-200/60 border border-slate-200 overflow-hidden glow-blue">
              {/* Window chrome */}
              <div className="flex items-center gap-2 px-5 py-3.5 bg-slate-50/80 border-b border-slate-100">
                <div className="w-3 h-3 rounded-full bg-red-400" />
                <div className="w-3 h-3 rounded-full bg-amber-400" />
                <div className="w-3 h-3 rounded-full bg-emerald-400" />
                <div className="ml-4 flex-1 h-5 bg-slate-200 rounded-md max-w-sm" />
                <div className="flex items-center gap-1.5 ml-auto">
                  <div className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse-slow" />
                  <span className="text-xs text-slate-400 font-medium">AI đang chấm…</span>
                </div>
              </div>

              {/* Content — 3 panels */}
              <div className="grid grid-cols-1 md:grid-cols-3 divide-y md:divide-y-0 md:divide-x divide-slate-100">
                {/* Panel 1: Essay editor */}
                <div className="p-5 space-y-3">
                  <div className="flex items-center justify-between mb-1">
                    <div className="flex items-center gap-2">
                      <PenLine className="w-3.5 h-3.5 text-slate-400" />
                      <span className="text-xs font-semibold text-slate-500 uppercase tracking-wide">Bài viết</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className="px-2 py-0.5 text-xs bg-blue-50 text-blue-700 rounded-full font-semibold">IELTS Task 2</span>
                      <span className="text-xs text-slate-400">287 từ</span>
                    </div>
                  </div>
                  <div className="space-y-2">
                    <div className="h-2.5 bg-slate-100 rounded-full w-full" />
                    <div className="h-2.5 bg-slate-100 rounded-full w-11/12" />
                    <div className="relative h-6 flex items-center">
                      <div className="h-2.5 bg-red-100 rounded-full w-4/5 border border-red-200" />
                      <span className="absolute left-2 text-[10px] text-red-500 font-semibold">article error</span>
                    </div>
                    <div className="h-2.5 bg-slate-100 rounded-full w-full" />
                    <div className="h-2.5 bg-slate-100 rounded-full w-3/4" />
                    <div className="relative h-6 flex items-center">
                      <div className="h-2.5 bg-yellow-100 rounded-full w-full border border-yellow-200" />
                      <span className="absolute left-16 text-[10px] text-yellow-600 font-semibold">verb tense</span>
                    </div>
                    <div className="h-2.5 bg-slate-100 rounded-full w-5/6" />
                    <div className="h-2.5 bg-slate-100 rounded-full w-2/3" />
                  </div>
                </div>

                {/* Panel 2: AI feedback */}
                <div className="p-5 space-y-3.5 bg-slate-50/40">
                  <div className="flex items-center gap-1.5 mb-1">
                    <Sparkles className="w-3.5 h-3.5 text-violet-500" />
                    <span className="text-xs font-semibold text-slate-500 uppercase tracking-wide">Kết quả AI</span>
                  </div>

                  {/* Score card */}
                  <div className="flex items-center gap-3 p-3 rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 shadow-sm shadow-blue-300/30">
                    <div className="w-11 h-11 rounded-lg bg-white/20 flex items-center justify-center text-white font-black text-lg">
                      6.5
                    </div>
                    <div>
                      <p className="text-sm font-bold text-white">Band 6.5</p>
                      <p className="text-xs text-blue-100">Mức dự kiến IELTS Writing</p>
                    </div>
                  </div>

                  <div>
                    <p className="text-xs font-semibold text-slate-500 mb-2">Lỗi thường gặp của bạn</p>
                    {[
                      { label: "Mạo từ (a/an/the)", count: 5, w: "w-5/6", color: "bg-red-500" },
                      { label: "Chia thì động từ",  count: 3, w: "w-3/5", color: "bg-orange-400" },
                      { label: "Giới từ",            count: 2, w: "w-2/5", color: "bg-yellow-400" },
                    ].map((err) => (
                      <div key={err.label} className="mb-2">
                        <div className="flex justify-between mb-1">
                          <span className="text-xs text-slate-600">{err.label}</span>
                          <span className="text-xs font-semibold text-slate-500">{err.count} lỗi</span>
                        </div>
                        <div className="h-1.5 bg-slate-100 rounded-full overflow-hidden">
                          <div className={`h-full ${err.color} ${err.w} rounded-full`} />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Panel 3: Quiz */}
                <div className="p-5 space-y-3 bg-violet-50/30">
                  <div className="flex items-center gap-1.5 mb-1">
                    <GraduationCap className="w-3.5 h-3.5 text-violet-500" />
                    <span className="text-xs font-semibold text-slate-500 uppercase tracking-wide">Quiz sinh từ lỗi</span>
                  </div>

                  <div className="p-3 rounded-xl bg-white border border-violet-100 shadow-sm">
                    <p className="text-xs font-semibold text-slate-700 mb-2">Câu 1/8 — Mạo từ</p>
                    <p className="text-xs text-slate-600 mb-3 leading-relaxed">
                      &quot;__ environment is a shared responsibility.&quot;
                    </p>
                    <div className="space-y-1.5">
                      {["A", "An", "The", "—"].map((opt, i) => (
                        <div
                          key={opt}
                          className={`flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-medium ${
                            i === 2
                              ? "bg-emerald-50 border border-emerald-300 text-emerald-700"
                              : "bg-slate-50 border border-slate-100 text-slate-600"
                          }`}
                        >
                          <span className={`w-4 h-4 rounded-full flex items-center justify-center text-[10px] font-bold ${
                            i === 2 ? "bg-emerald-500 text-white" : "bg-slate-200 text-slate-500"
                          }`}>{opt}</span>
                          <span>{opt === "—" ? "Không mạo từ" : opt}</span>
                        </div>
                      ))}
                    </div>
                  </div>

                  <div className="flex items-center justify-between px-1">
                    <span className="text-xs text-violet-600 font-semibold">7/8 đúng hôm nay</span>
                    <div className="flex gap-0.5">
                      {[...Array(8)].map((_, i) => (
                        <div key={i} className={`w-2 h-2 rounded-full ${i < 7 ? "bg-emerald-400" : "bg-slate-200"}`} />
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
