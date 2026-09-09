import {
  ArrowRight,
  BarChart2,
  BrainCircuit,
  CheckCircle2,
  Edit3,
  Layers,
  Sparkles,
  XCircle,
} from "lucide-react";

const problems = [
  {
    tool: "Grammarly",
    Icon: Edit3,
    problem: "Sửa hộ, nhưng không dạy",
    detail:
      "Bạn nhận một bản sửa đẹp hơn, copy vào bài, rồi quên mất vì sao mình sai. Lần sau gặp đúng cấu trúc đó vẫn vấp lại.",
    pain: "Học thụ động",
    metric: "0 bài tập cá nhân hóa",
    border: "border-rose-200",
    bg: "from-rose-50 to-white",
    iconColor: "text-rose-600 bg-rose-100",
    tagColor: "text-rose-700 bg-rose-50 border-rose-100",
  },
  {
    tool: "App chấm IELTS",
    Icon: BarChart2,
    problem: "Có điểm, nhưng thiếu đường đi",
    detail:
      "Biết mình đang band 6.0 hay 7.0 là chưa đủ. Điều cần hơn là biết nhóm lỗi nào đang kéo điểm xuống và nên luyện gì tiếp.",
    pain: "Phản hồi rời rạc",
    metric: "Không thấy lỗi lặp lại",
    border: "border-orange-200",
    bg: "from-orange-50 to-white",
    iconColor: "text-orange-600 bg-orange-100",
    tagColor: "text-orange-700 bg-orange-50 border-orange-100",
  },
  {
    tool: "Quizlet / Anki",
    Icon: Layers,
    problem: "Ôn từ vựng tách khỏi bài viết",
    detail:
      "Flashcard giúp nhớ từ, nhưng không nối trực tiếp với câu bạn vừa viết sai, chủ đề bạn đang dùng, hay mục tiêu bài tiếp theo.",
    pain: "Thiếu ngữ cảnh",
    metric: "Khó áp dụng vào bài thật",
    border: "border-amber-200",
    bg: "from-amber-50 to-white",
    iconColor: "text-amber-700 bg-amber-100",
    tagColor: "text-amber-800 bg-amber-50 border-amber-100",
  },
];

const loopItems = [
  "Lỗi được gom thành nhóm cố định",
  "Quiz sinh từ đúng điểm yếu",
  "Bài sau đo tiến bộ so với bài trước",
];

export default function Problem() {
  return (
    <section className="relative overflow-hidden bg-slate-950 py-20 sm:py-28">
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute left-1/2 top-0 h-80 w-[44rem] -translate-x-1/2 rounded-full bg-blue-500/20 blur-3xl" />
        <div className="absolute -right-28 bottom-12 h-72 w-72 rounded-full bg-violet-500/20 blur-3xl" />
        <div className="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,0.04)_1px,transparent_1px),linear-gradient(90deg,rgba(255,255,255,0.04)_1px,transparent_1px)] bg-[size:44px_44px] [mask-image:radial-gradient(ellipse_at_top,black,transparent_70%)]" />
      </div>

      <div className="relative max-w-6xl mx-auto px-4 sm:px-6">
        <div className="grid items-end gap-8 lg:grid-cols-[1.05fr_0.95fr] mb-12">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-red-400/20 bg-red-400/10 px-3 py-1 text-xs font-bold uppercase tracking-widest text-red-200">
              <XCircle className="h-3.5 w-3.5" />
              Vấn đề
            </span>
            <h2 className="mt-5 max-w-3xl text-4xl font-extrabold leading-tight text-white sm:text-5xl">
              Công cụ hiện tại chỉ xử lý bài viết.
              <span className="block text-blue-300">Người học cần một vòng lặp.</span>
            </h2>
          </div>

          <p className="text-base leading-relaxed text-slate-300 sm:text-lg lg:pb-2">
            Người học tiếng Anh ở Việt Nam không thiếu app. Cái thiếu là một hệ thống
            nhìn thấy lỗi lặp lại, biến lỗi thành bài tập, rồi chứng minh bạn đang tiến bộ.
          </p>
        </div>

        <div className="grid grid-cols-1 gap-5 md:grid-cols-3">
          {problems.map((item) => {
            const { Icon } = item;

            return (
              <div
                key={item.tool}
                className={`group relative flex min-h-[310px] flex-col overflow-hidden rounded-2xl border ${item.border} bg-gradient-to-br ${item.bg} p-6 shadow-2xl shadow-black/10 transition duration-300 hover:-translate-y-1 hover:shadow-blue-950/20`}
              >
                <div className="absolute -right-10 -top-10 h-28 w-28 rounded-full bg-white/70 blur-2xl transition duration-300 group-hover:scale-125" />

                <div className="relative mb-6 flex items-start justify-between">
                  <div className={`flex h-12 w-12 items-center justify-center rounded-2xl ${item.iconColor}`}>
                    <Icon className="h-5 w-5" />
                  </div>
                  <span className={`rounded-full border px-2.5 py-1 text-[11px] font-bold uppercase tracking-wider ${item.tagColor}`}>
                    {item.tool}
                  </span>
                </div>

                <div className="relative flex flex-1 flex-col">
                  <p className="mb-2 text-xs font-bold uppercase tracking-[0.22em] text-slate-400">
                    {item.pain}
                  </p>
                  <h3 className="mb-3 text-xl font-extrabold leading-snug text-slate-900">
                    {item.problem}
                  </h3>
                  <p className="text-sm leading-relaxed text-slate-600">
                    {item.detail}
                  </p>

                  <div className="mt-auto pt-6">
                    <div className="flex items-center gap-2 rounded-xl bg-slate-900/5 px-3 py-2 text-xs font-bold text-slate-600">
                      <XCircle className="h-4 w-4 text-red-500" />
                      {item.metric}
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        <div className="relative my-10 flex justify-center">
          <div className="hidden h-px w-full bg-gradient-to-r from-transparent via-slate-700 to-transparent sm:block" />
          <div className="absolute -top-5 inline-flex h-10 w-10 items-center justify-center rounded-full border border-slate-700 bg-slate-950 text-slate-400">
            <ArrowRight className="h-4 w-4 rotate-90 sm:rotate-0" />
          </div>
        </div>

        <div className="grid gap-5 lg:grid-cols-[0.9fr_1.1fr]">
          <div className="rounded-2xl border border-white/10 bg-white/[0.06] p-6 backdrop-blur">
            <div className="mb-5 flex items-center gap-3">
              <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-blue-500/15 text-blue-200 ring-1 ring-blue-400/20">
                <BrainCircuit className="h-5 w-5" />
              </div>
              <div>
                <p className="text-xs font-bold uppercase tracking-widest text-blue-200">
                  Insight
                </p>
                <h3 className="font-extrabold text-white">Sửa bài không đủ để học viết</h3>
              </div>
            </div>

            <div className="space-y-3">
              {loopItems.map((item, index) => (
                <div key={item} className="flex items-center gap-3 rounded-xl bg-white/[0.05] p-3">
                  <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-white text-xs font-black text-slate-950">
                    {index + 1}
                  </span>
                  <span className="text-sm font-semibold text-slate-200">{item}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="relative overflow-hidden rounded-3xl border border-blue-300/20 bg-gradient-to-br from-blue-600 via-blue-700 to-violet-700 p-7 text-white shadow-2xl shadow-blue-950/30 sm:p-8">
            <div className="absolute right-0 top-0 h-40 w-40 translate-x-12 -translate-y-12 rounded-full bg-white/20 blur-2xl" />
            <div className="absolute bottom-0 left-10 h-28 w-28 translate-y-14 rounded-full bg-cyan-300/20 blur-2xl" />

            <div className="relative flex flex-col gap-6 sm:flex-row sm:items-center">
              <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-2xl border border-white/20 bg-white/15">
                <CheckCircle2 className="h-8 w-8" />
              </div>

              <div className="flex-1">
                <div className="mb-3 inline-flex items-center gap-2 rounded-full bg-white/15 px-3 py-1 text-xs font-bold text-blue-50 ring-1 ring-white/15">
                  <Sparkles className="h-3.5 w-3.5" />
                  NomiWrite giải quyết khoảng trống này
                </div>
                <h3 className="text-2xl font-extrabold leading-tight sm:text-3xl">
                  Từ một bài viết, tạo ra lộ trình luyện đúng lỗi của bạn.
                </h3>
                <p className="mt-3 max-w-2xl text-sm leading-relaxed text-blue-50 sm:text-base">
                  NomiWrite dùng bài viết thật làm dữ liệu, phân loại lỗi cố hữu,
                  gợi ý từ vựng theo ngữ cảnh, sinh quiz cá nhân hóa và theo dõi
                  xem lỗi cũ có giảm qua từng bài không.
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
