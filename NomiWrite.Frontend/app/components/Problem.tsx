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
    border: "border-line",
    bg: "bg-rose ",
    iconColor: "text-rose-ink bg-rose",
    tagColor: "text-rose-ink bg-rose border-line",
  },
  {
    tool: "App chấm IELTS",
    Icon: BarChart2,
    problem: "Có điểm, nhưng thiếu đường đi",
    detail:
      "Biết mình đang band 6.0 hay 7.0 là chưa đủ. Điều cần hơn là biết nhóm lỗi nào đang kéo điểm xuống và nên luyện gì tiếp.",
    pain: "Phản hồi rời rạc",
    metric: "Không thấy lỗi lặp lại",
    border: "border-line",
    bg: "bg-accent ",
    iconColor: "text-accent-ink bg-accent",
    tagColor: "text-accent-ink bg-accent border-line",
  },
  {
    tool: "Quizlet / Anki",
    Icon: Layers,
    problem: "Ôn từ vựng tách khỏi bài viết",
    detail:
      "Flashcard giúp nhớ từ, nhưng không nối trực tiếp với câu bạn vừa viết sai, chủ đề bạn đang dùng, hay mục tiêu bài tiếp theo.",
    pain: "Thiếu ngữ cảnh",
    metric: "Khó áp dụng vào bài thật",
    border: "border-line",
    bg: "bg-warning ",
    iconColor: "text-warning-ink bg-warning",
    tagColor: "text-warning-ink bg-warning border-line",
  },
];

const loopItems = [
  "Lỗi được gom thành nhóm cố định",
  "Quiz sinh từ đúng điểm yếu",
  "Bài sau đo tiến bộ so với bài trước",
];

export default function Problem() {
  return (
    <section className="relative overflow-hidden bg-surface-muted py-20 sm:py-28">
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute inset-0 bg-surface-muted bg-[size:44px_44px] [mask-image:radial-gradient(ellipse_at_top,black,transparent_70%)]" />
      </div>

      <div className="relative max-w-6xl mx-auto px-4 sm:px-6">
        <div className="grid items-end gap-8 lg:grid-cols-[1.05fr_0.95fr] mb-12">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-focus bg-danger px-3 py-1 text-xs font-bold uppercase tracking-widest text-danger-ink">
              <XCircle className="h-3.5 w-3.5" />
              Vấn đề
            </span>
            <h2 className="mt-5 max-w-3xl text-4xl font-bold leading-tight text-ink sm:text-5xl">
              Công cụ hiện tại chỉ xử lý bài viết.
              <span className="block text-accent-ink">Người học cần một vòng lặp.</span>
            </h2>
          </div>

          <p className="text-base leading-relaxed text-muted sm:text-lg lg:pb-2">
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
                className={`group relative flex min-h-[310px] flex-col overflow-hidden rounded-xl border ${item.border}  ${item.bg} p-6 shadow-sm  transition duration-300  `}
              >

                <div className="relative mb-6 flex items-start justify-between">
                  <div className={`flex h-12 w-12 items-center justify-center rounded-xl ${item.iconColor}`}>
                    <Icon className="h-5 w-5" />
                  </div>
                  <span className={`rounded-full border px-2.5 py-1 text-[11px] font-bold uppercase tracking-wider ${item.tagColor}`}>
                    {item.tool}
                  </span>
                </div>

                <div className="relative flex flex-1 flex-col">
                  <p className="mb-2 text-xs font-bold uppercase tracking-[0.22em] text-muted">
                    {item.pain}
                  </p>
                  <h3 className="mb-3 text-xl font-bold leading-snug text-ink">
                    {item.problem}
                  </h3>
                  <p className="text-sm leading-relaxed text-muted">
                    {item.detail}
                  </p>

                  <div className="mt-auto pt-6">
                    <div className="flex items-center gap-2 rounded-xl bg-surface-muted px-3 py-2 text-xs font-bold text-muted">
                      <XCircle className="h-4 w-4 text-danger-ink" />
                      {item.metric}
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        <div className="relative my-10 flex justify-center">
          <div className="hidden h-px w-full from-transparent to-transparent sm:block" />
          <div className="absolute -top-5 inline-flex h-10 w-10 items-center justify-center rounded-full border border-line bg-surface-muted text-muted">
            <ArrowRight className="h-4 w-4 rotate-90 sm:rotate-0" />
          </div>
        </div>

        <div className="grid gap-5 lg:grid-cols-[0.9fr_1.1fr]">
          <div className="rounded-xl border border-line bg-surface/[0.06] p-6">
            <div className="mb-5 flex items-center gap-3">
              <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-accent text-accent-ink ring-1 ring-focus">
                <BrainCircuit className="h-5 w-5" />
              </div>
              <div>
                <p className="text-xs font-bold uppercase tracking-widest text-accent-ink">
                  Insight
                </p>
                <h3 className="font-bold text-ink">Sửa bài không đủ để học viết</h3>
              </div>
            </div>

            <div className="space-y-3">
              {loopItems.map((item, index) => (
                <div key={item} className="flex items-center gap-3 rounded-xl bg-surface/[0.05] p-3">
                  <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-surface text-xs font-black text-ink">
                    {index + 1}
                  </span>
                  <span className="text-sm font-semibold text-muted">{item}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="relative overflow-hidden rounded-xl border border-focus bg-accent p-7 text-ink shadow-sm sm:p-8">

            <div className="relative flex flex-col gap-6 sm:flex-row sm:items-center">
              <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-xl border border-line bg-surface">
                <CheckCircle2 className="h-8 w-8" />
              </div>

              <div className="flex-1">
                <div className="mb-3 inline-flex items-center gap-2 rounded-full bg-surface px-3 py-1 text-xs font-bold text-accent-ink ring-1 ring-focus">
                  <Sparkles className="h-3.5 w-3.5" />
                  NomiWrite giải quyết khoảng trống này
                </div>
                <h3 className="text-2xl font-bold leading-tight sm:text-3xl">
                  Từ một bài viết, tạo ra lộ trình luyện đúng lỗi của bạn.
                </h3>
                <p className="mt-3 max-w-2xl text-sm leading-relaxed text-accent-ink sm:text-base">
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
