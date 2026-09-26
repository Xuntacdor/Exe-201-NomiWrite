import { Check, Minus, X } from "lucide-react";

type CellValue = boolean | "partial";

interface Row {
  feature: string;
  nomiwrite: CellValue;
  correctionTools: CellValue;
  scoringTools: CellValue;
  reviewTools: CellValue;
}

const rows: Row[] = [
  {
    feature: "Chấm theo đúng loại văn bản",
    nomiwrite: true,
    correctionTools: false,
    scoringTools: "partial",
    reviewTools: false,
  },
  {
    feature: "Phân loại lỗi ngữ pháp cố định",
    nomiwrite: true,
    correctionTools: false,
    scoringTools: false,
    reviewTools: false,
  },
  {
    feature: "Gợi ý nâng cấp từ vựng theo ngữ cảnh",
    nomiwrite: true,
    correctionTools: true,
    scoringTools: "partial",
    reviewTools: false,
  },
  {
    feature: "Tạo quiz từ chính bài viết của bạn",
    nomiwrite: true,
    correctionTools: false,
    scoringTools: false,
    reviewTools: false,
  },
  {
    feature: "Đo tiến bộ theo từng nhóm lỗi",
    nomiwrite: true,
    correctionTools: false,
    scoringTools: false,
    reviewTools: false,
  },
  {
    feature: "Hỗ trợ nhiều mục tiêu viết",
    nomiwrite: true,
    correctionTools: true,
    scoringTools: false,
    reviewTools: false,
  },
  {
    feature: "Nhận diện lỗi phổ biến của người Việt",
    nomiwrite: true,
    correctionTools: false,
    scoringTools: "partial",
    reviewTools: false,
  },
  {
    feature: "Biến lỗi thành bài học tiếp theo",
    nomiwrite: true,
    correctionTools: false,
    scoringTools: false,
    reviewTools: false,
  },
];

function Cell({ value, highlight }: { value: CellValue; highlight?: boolean }) {
  if (value === true) {
    return (
      <div className={`flex justify-center ${highlight ? "text-accent-ink" : "text-success-ink"}`}>
        <Check className="h-5 w-5" strokeWidth={2.5} />
      </div>
    );
  }

  if (value === "partial") {
    return (
      <div className="flex justify-center text-warning-ink">
        <Minus className="h-5 w-5" strokeWidth={2.5} />
      </div>
    );
  }

  return (
    <div className="flex justify-center text-muted">
      <X className="h-5 w-5" strokeWidth={2} />
    </div>
  );
}

export default function Comparison() {
  return (
    <section className="bg-canvas py-20 sm:py-28">
      <div className="mx-auto max-w-5xl px-4 sm:px-6">
        <div className="mb-12 text-center">
          <span className="mb-3 inline-block rounded-full bg-success px-3 py-1 text-xs font-semibold uppercase tracking-wide text-success-ink">
            So sánh
          </span>
          <h2 className="text-3xl font-bold leading-tight text-ink sm:text-4xl">
            Không chỉ sửa bài. NomiWrite tạo vòng lặp học thật.
          </h2>
          <p className="mx-auto mt-4 max-w-2xl text-lg text-muted">
            Nhiều công cụ chỉ giải quyết một phần: sửa lỗi, cho điểm, hoặc giúp ôn tập.
            NomiWrite nối các bước đó thành một hành trình luyện viết cá nhân hóa.
          </p>
        </div>

        <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
          <div className="grid grid-cols-5 gap-0 border-b border-line">
            <div className="col-span-1 px-4 py-4 text-xs font-semibold uppercase tracking-wide text-muted">
              Năng lực
            </div>
            <div className="col-span-1 border-x border-line bg-accent px-2 py-4 text-center">
              <p className="text-sm font-bold text-accent-ink">NomiWrite</p>
            </div>
            <div className="col-span-1 px-2 py-4 text-center">
              <p className="text-xs font-semibold text-muted">Công cụ sửa lỗi</p>
            </div>
            <div className="col-span-1 px-2 py-4 text-center">
              <p className="text-xs font-semibold text-muted">Công cụ chấm điểm</p>
            </div>
            <div className="col-span-1 px-2 py-4 text-center">
              <p className="text-xs font-semibold text-muted">Công cụ ôn tập</p>
            </div>
          </div>

          {rows.map((row, idx) => (
            <div
              key={row.feature}
              className={`grid grid-cols-5 gap-0 border-b border-line ${
                idx % 2 === 0 ? "bg-surface" : "bg-canvas"
              }`}
            >
              <div className="col-span-1 px-4 py-3.5">
                <p className="text-sm text-ink">{row.feature}</p>
              </div>
              <div className="col-span-1 border-x border-line bg-accent px-2 py-3.5">
                <Cell value={row.nomiwrite} highlight />
              </div>
              <div className="col-span-1 px-2 py-3.5">
                <Cell value={row.correctionTools} />
              </div>
              <div className="col-span-1 px-2 py-3.5">
                <Cell value={row.scoringTools} />
              </div>
              <div className="col-span-1 px-2 py-3.5">
                <Cell value={row.reviewTools} />
              </div>
            </div>
          ))}
        </div>

        <div className="mt-4 flex items-center justify-center gap-5 text-xs text-muted">
          <div className="flex items-center gap-1.5">
            <Check className="h-3.5 w-3.5 text-success-ink" />
            <span>Có đầy đủ</span>
          </div>
          <div className="flex items-center gap-1.5">
            <Minus className="h-3.5 w-3.5 text-warning-ink" />
            <span>Có một phần</span>
          </div>
          <div className="flex items-center gap-1.5">
            <X className="h-3.5 w-3.5 text-muted" />
            <span>Không có</span>
          </div>
        </div>
      </div>
    </section>
  );
}
