import AppShell from "../components/AppShell";
import { Clock, ArrowRight, Filter, ChevronRight, PenLine, Check, AlertCircle } from "lucide-react";

const submissions = [
  { id: 1, topic: "Technology & Society",       type: "IELTS Task 2", band: 6.5, errors: 8,  date: "Hôm nay",        status: "graded" },
  { id: 2, topic: "Environment & Climate",       type: "IELTS Task 2", band: 6.0, errors: 12, date: "2 ngày trước",   status: "graded" },
  { id: 3, topic: "Email xin việc",             type: "Email",        band: 7.0, errors: 4,  date: "5 ngày trước",   status: "graded" },
  { id: 4, topic: "Education System",            type: "IELTS Task 2", band: 5.5, errors: 15, date: "1 tuần trước",   status: "graded" },
  { id: 5, topic: "Urbanisation & City Living",  type: "IELTS Task 2", band: 6.5, errors: 9,  date: "10 ngày trước",  status: "graded" },
  { id: 6, topic: "Health & Lifestyle",          type: "VSTEP",        band: 7.0, errors: 6,  date: "2 tuần trước",   status: "graded" },
  { id: 7, topic: "Work & Economy",             type: "IELTS Task 2", band: 6.0, errors: 11, date: "3 tuần trước",   status: "graded" },
  { id: 8, topic: "Globalisation",              type: "Học thuật",    band: 6.5, errors: 7,  date: "1 tháng trước",  status: "graded" },
];

function bandColor(band: number) {
  if (band >= 7.0) return "text-emerald-600";
  if (band >= 6.0) return "text-blue-600";
  return "text-orange-500";
}

function bandBg(band: number) {
  if (band >= 7.0) return "bg-emerald-50 border-emerald-200";
  if (band >= 6.0) return "bg-blue-50 border-blue-200";
  return "bg-orange-50 border-orange-200";
}

export default function HistoryPage() {
  const avg = (submissions.reduce((sum, s) => sum + s.band, 0) / submissions.length).toFixed(1);
  const best = Math.max(...submissions.map(s => s.band));

  return (
    <AppShell activePath="/history">
      {/* Top bar */}
      <div className="sticky top-0 z-10 bg-white/90 backdrop-blur border-b border-slate-100 px-6 h-14 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Clock className="w-4 h-4 text-slate-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Lịch sử bài viết</h1>
        </div>
        <div className="flex items-center gap-2">
          <button className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 bg-slate-100 rounded-lg hover:bg-slate-200 transition-colors">
            <Filter className="w-3.5 h-3.5" />
            Lọc
          </button>
          <a href="/write" className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors">
            <PenLine className="w-3.5 h-3.5" />
            Viết bài mới
          </a>
        </div>
      </div>

      <div className="p-6 w-full space-y-5">
        {/* Stats */}
        <div className="grid grid-cols-3 gap-4">
          {[
            { label: "Tổng bài",        value: submissions.length, unit: "bài",  color: "text-slate-900" },
            { label: "Band trung bình", value: avg,                unit: "",     color: "text-blue-600"  },
            { label: "Band cao nhất",   value: best,               unit: "",     color: "text-emerald-600" },
          ].map(({ label, value, unit, color }) => (
            <div key={label} className="bg-white rounded-2xl border border-slate-100 p-4 text-center shadow-sm">
              <p className={`text-2xl font-extrabold ${color}`}>{value}<span className="text-base font-semibold ml-0.5">{unit}</span></p>
              <p className="text-xs text-slate-500 mt-0.5">{label}</p>
            </div>
          ))}
        </div>

        {/* Submission list */}
        <div className="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
          <div className="grid grid-cols-12 gap-4 px-5 py-3 text-xs font-bold text-slate-400 uppercase tracking-widest border-b border-slate-100">
            <span className="col-span-5">Bài viết</span>
            <span className="col-span-2">Loại</span>
            <span className="col-span-2 text-center">Band</span>
            <span className="col-span-2 text-center">Lỗi</span>
            <span className="col-span-1" />
          </div>

          <div className="divide-y divide-slate-50">
            {submissions.map((s) => (
              <a
                key={s.id}
                href="/result"
                className="grid grid-cols-12 gap-4 items-center px-5 py-4 hover:bg-slate-50 transition-colors group"
              >
                {/* Topic */}
                <div className="col-span-5">
                  <p className="text-sm font-semibold text-slate-800 group-hover:text-blue-600 transition-colors">{s.topic}</p>
                  <p className="text-xs text-slate-400 mt-0.5">{s.date}</p>
                </div>

                {/* Type */}
                <div className="col-span-2">
                  <span className="text-xs font-semibold px-2 py-1 bg-slate-100 text-slate-600 rounded-full">
                    {s.type}
                  </span>
                </div>

                {/* Band */}
                <div className="col-span-2 flex justify-center">
                  <span className={`text-sm font-extrabold px-3 py-1 rounded-xl border ${bandBg(s.band)} ${bandColor(s.band)}`}>
                    {s.band}
                  </span>
                </div>

                {/* Errors */}
                <div className="col-span-2 flex justify-center">
                  <div className="flex items-center gap-1.5">
                    <AlertCircle className="w-3.5 h-3.5 text-red-400" />
                    <span className="text-sm font-semibold text-slate-700">{s.errors}</span>
                  </div>
                </div>

                {/* Arrow */}
                <div className="col-span-1 flex justify-end">
                  <ChevronRight className="w-4 h-4 text-slate-300 group-hover:text-blue-500 transition-colors" />
                </div>
              </a>
            ))}
          </div>
        </div>

        {/* Progress note */}
        <div className="flex items-start gap-3 p-4 bg-blue-50 border border-blue-100 rounded-2xl">
          <Check className="w-4 h-4 text-blue-500 mt-0.5 shrink-0" />
          <div>
            <p className="text-sm font-semibold text-blue-800">Band của bạn tăng 1.5 trong 3 tháng</p>
            <p className="text-xs text-blue-600 mt-0.5">Từ 5.5 → 7.0 — tiếp tục duy trì streak luyện tập mỗi ngày.</p>
          </div>
          <a href="/write" className="ml-auto flex items-center gap-1 text-xs font-bold text-blue-600 hover:text-blue-700 shrink-0">
            Viết thêm <ArrowRight className="w-3 h-3" />
          </a>
        </div>
      </div>
    </AppShell>
  );
}
