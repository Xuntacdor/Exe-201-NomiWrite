"use client";

import Link from "next/link";
import AppShell from "../components/AppShell";
import {
  AlertCircle,
  ArrowRight,
  BookOpenCheck,
  BrainCircuit,
  CheckCircle2,
  FileText,
  History,
  ListChecks,
  Server,
  Sparkles,
} from "lucide-react";

const backendNeeds = [
  "POST /api/quizzes/generate - tạo quiz từ lỗi ngữ pháp, từ vựng hoặc submission cụ thể",
  "GET /api/quizzes/{id} - đọc câu hỏi, đáp án và giải thích",
  "POST /api/quiz-attempts - lưu lượt làm bài, điểm và đáp án đã chọn",
  "GET /api/quiz-attempts?userId=me - xem lịch sử luyện quiz",
];

const readyUi = [
  "Route /quiz và navigation đã có sẵn trong app shell",
  "Frontend contract đã có type Quiz, QuizQuestion, QuizAttempt",
  "API client đã có hàm generateQuiz, getQuiz, submitQuizAttempt",
  "Có thể nối quiz từ trang Result bằng sourceSubmissionId khi backend mở API",
];

const plannedModes = [
  {
    title: "Quiz từ bài viết",
    description: "Sinh câu hỏi từ grammar errors và vocabulary suggestions của một submission.",
    icon: FileText,
  },
  {
    title: "Quiz theo điểm yếu",
    description: "Luyện các nhóm lỗi thường gặp dựa trên lịch sử chấm bài của người dùng.",
    icon: BrainCircuit,
  },
  {
    title: "Review đáp án sai",
    description: "Hiển thị đáp án, giải thích và lưu kết quả để theo dõi tiến bộ.",
    icon: BookOpenCheck,
  },
];

export default function QuizPage() {
  return (
    <AppShell activePath="/quiz">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2">
          <BrainCircuit className="h-4 w-4 text-violet-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Quiz luyện tập</h1>
        </div>
        <span className="rounded-full border border-amber-200 bg-amber-50 px-3 py-1 text-xs font-extrabold text-amber-700">
          Status 1/2
        </span>
      </div>

      <div className="w-full p-6">
        <div className="mx-auto max-w-6xl space-y-5">
          <section className="overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm">
            <div className="grid gap-0 lg:grid-cols-[1.2fr_0.8fr]">
              <div className="p-6 sm:p-8">
                <div className="mb-4 inline-flex items-center gap-2 rounded-full border border-amber-200 bg-amber-50 px-3 py-1 text-xs font-bold text-amber-700">
                  <AlertCircle className="h-3.5 w-3.5" />
                  Frontend ready, waiting backend API
                </div>
                <h2 className="max-w-2xl text-2xl font-extrabold text-slate-950 sm:text-3xl">
                  Trang quiz đã bỏ toàn bộ câu hỏi mẫu.
                </h2>
                <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-500">
                  Theo usecase UC25, UC27 và UC58-UC66, quiz cần sinh từ dữ liệu feedback thật,
                  vocabulary thật và lịch sử attempt. Backend hiện chưa có module quiz/vocabulary,
                  nên màn này giữ UI sẵn ở trạng thái 1/2 thay vì hiển thị dữ liệu mẫu.
                </p>
                <div className="mt-6 flex flex-wrap gap-3">
                  <Link
                    href="/write"
                    className="inline-flex items-center gap-2 rounded-full bg-blue-600 px-4 py-2.5 text-sm font-bold text-white shadow-sm shadow-blue-200 transition-all hover:bg-blue-700"
                  >
                    Viết bài mới
                    <ArrowRight className="h-4 w-4" />
                  </Link>
                  <Link
                    href="/history"
                    className="inline-flex items-center gap-2 rounded-full border border-slate-200 bg-white px-4 py-2.5 text-sm font-bold text-slate-700 transition-all hover:bg-slate-50"
                  >
                    Xem lịch sử
                    <History className="h-4 w-4" />
                  </Link>
                </div>
              </div>
              <div className="border-t border-slate-100 bg-slate-50 p-6 lg:border-l lg:border-t-0">
                <div className="rounded-2xl bg-slate-950 p-5 text-white">
                  <div className="mb-4 flex items-center gap-2">
                    <Server className="h-4 w-4 text-blue-300" />
                    <p className="text-xs font-extrabold uppercase tracking-widest text-blue-200">Backend needed</p>
                  </div>
                  <div className="space-y-3">
                    {backendNeeds.map(item => (
                      <div key={item} className="rounded-xl bg-white/10 p-3 text-xs leading-5 text-slate-200">
                        {item}
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          </section>

          <section className="grid gap-4 lg:grid-cols-3">
            {plannedModes.map(item => {
              const Icon = item.icon;
              return (
                <div key={item.title} className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
                  <div className="mb-4 flex h-11 w-11 items-center justify-center rounded-xl bg-blue-50 text-blue-600">
                    <Icon className="h-5 w-5" />
                  </div>
                  <h3 className="text-sm font-extrabold text-slate-900">{item.title}</h3>
                  <p className="mt-2 text-xs leading-5 text-slate-500">{item.description}</p>
                </div>
              );
            })}
          </section>

          <section className="grid gap-4 lg:grid-cols-2">
            <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
              <div className="mb-4 flex items-center gap-2">
                <CheckCircle2 className="h-4 w-4 text-emerald-500" />
                <h3 className="text-sm font-extrabold text-slate-900">Frontend đã chuẩn bị</h3>
              </div>
              <div className="space-y-2">
                {readyUi.map(item => (
                  <div key={item} className="flex items-start gap-2 rounded-xl bg-emerald-50 px-3 py-2 text-xs font-medium text-emerald-800">
                    <CheckCircle2 className="mt-0.5 h-3.5 w-3.5 shrink-0" />
                    {item}
                  </div>
                ))}
              </div>
            </div>

            <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
              <div className="mb-4 flex items-center gap-2">
                <ListChecks className="h-4 w-4 text-violet-500" />
                <h3 className="text-sm font-extrabold text-slate-900">Usecase đang chờ API</h3>
              </div>
              <div className="space-y-2 text-xs text-slate-600">
                <p className="rounded-xl bg-slate-50 p-3">UC25: sinh gợi ý từ vựng để làm quiz.</p>
                <p className="rounded-xl bg-slate-50 p-3">UC27: so sánh điểm hiện tại với các attempt trước.</p>
                <p className="rounded-xl bg-slate-50 p-3">UC32: dùng strengths/weaknesses để chọn nhóm lỗi yếu.</p>
                <p className="rounded-xl bg-slate-50 p-3">UC58-UC66: mở rộng quiz nâng cao cho VIP.</p>
              </div>
            </div>
          </section>

          <div className="rounded-2xl border border-blue-100 bg-blue-50 p-4 text-sm font-semibold text-blue-800">
            <div className="flex items-start gap-2">
              <Sparkles className="mt-0.5 h-4 w-4 shrink-0" />
              Khi backend có API, trang này chỉ cần đổi từ status shell sang load/generate quiz bằng `apiClient`.
            </div>
          </div>
        </div>
      </div>
    </AppShell>
  );
}
