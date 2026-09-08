"use client";

import Link from "next/link";
import AppShell from "../components/AppShell";
import {
  AlertCircle,
  ArrowRight,
  BookOpen,
  CheckCircle2,
  Database,
  FileText,
  Filter,
  ListChecks,
  Search,
  Server,
  Sparkles,
} from "lucide-react";

const backendNeeds = [
  "GET /api/vocabulary - đọc danh sách từ vựng đã lưu của user",
  "PATCH /api/vocabulary/{id}/mastered - lưu trạng thái đã thuộc",
  "POST /api/vocabulary/from-feedback - tạo từ vựng từ AI feedback",
  "GET /api/vocabulary?search=&category=&mastered= - lọc và tìm kiếm phía server",
];

const readyUi = [
  "Route /vocabulary và sidebar redirect đã sẵn sàng",
  "Frontend contract đã có type VocabSuggestion và mastered request",
  "API client đã có listVocabulary và updateVocabularyMastered",
  "Quiz có thể nối tiếp bằng vocabularyIds sau khi backend có dữ liệu thật",
];

const plannedControls = [
  {
    title: "Tìm kiếm",
    description: "Tìm từ gốc, từ gợi ý, topic hoặc ví dụ khi backend trả dữ liệu.",
    icon: Search,
  },
  {
    title: "Bộ lọc",
    description: "Lọc theo topic, nguồn submission và trạng thái đã thuộc.",
    icon: Filter,
  },
  {
    title: "Mastered state",
    description: "Đánh dấu đã thuộc và đồng bộ với tài khoản người dùng.",
    icon: CheckCircle2,
  },
];

export default function VocabularyPage() {
  return (
    <AppShell activePath="/vocabulary">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2">
          <BookOpen className="h-4 w-4 text-emerald-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Sổ từ vựng</h1>
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
                  Trang vocabulary đã bỏ toàn bộ danh sách từ mẫu.
                </h2>
                <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-500">
                  Sổ từ vựng phải được sinh từ kết quả chấm bài thật, gắn với submission và tài khoản.
                  Backend hiện chưa có module vocabulary riêng, nên UI được giữ ở trạng thái 1/2 để chờ API.
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
                    Xem bài đã chấm
                    <FileText className="h-4 w-4" />
                  </Link>
                </div>
              </div>
              <div className="border-t border-slate-100 bg-slate-50 p-6 lg:border-l lg:border-t-0">
                <div className="rounded-2xl bg-slate-950 p-5 text-white">
                  <div className="mb-4 flex items-center gap-2">
                    <Server className="h-4 w-4 text-emerald-300" />
                    <p className="text-xs font-extrabold uppercase tracking-widest text-emerald-200">Backend needed</p>
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
            {plannedControls.map(item => {
              const Icon = item.icon;
              return (
                <div key={item.title} className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
                  <div className="mb-4 flex h-11 w-11 items-center justify-center rounded-xl bg-emerald-50 text-emerald-600">
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
                <ListChecks className="h-4 w-4 text-blue-500" />
                <h3 className="text-sm font-extrabold text-slate-900">Usecase đang chờ API</h3>
              </div>
              <div className="space-y-2 text-xs text-slate-600">
                <p className="rounded-xl bg-slate-50 p-3">UC25: lưu vocabulary suggestions từ AI feedback.</p>
                <p className="rounded-xl bg-slate-50 p-3">UC30-UC32: dùng lịch sử để phát hiện từ vựng yếu.</p>
                <p className="rounded-xl bg-slate-50 p-3">UC58-UC60: mở rộng kho từ và model answer cho VIP.</p>
                <p className="rounded-xl bg-slate-50 p-3">UC63: xuất báo cáo PDF kèm vocabulary sau này.</p>
              </div>
            </div>
          </section>

          <div className="rounded-2xl border border-blue-100 bg-blue-50 p-4 text-sm font-semibold text-blue-800">
            <div className="flex items-start gap-2">
              <Database className="mt-0.5 h-4 w-4 shrink-0" />
              Không còn danh sách từ mẫu trong production UI. Khi backend có dữ liệu, nối trực tiếp bằng `apiClient.listVocabulary()`.
            </div>
          </div>

          <div className="rounded-2xl border border-emerald-100 bg-emerald-50 p-4 text-sm font-semibold text-emerald-800">
            <div className="flex items-start gap-2">
              <Sparkles className="mt-0.5 h-4 w-4 shrink-0" />
              Màn này đã sẵn layout cho search, filter, mastered state và generate quiz, nhưng sẽ chỉ bật khi API thật hoàn thành.
            </div>
          </div>
        </div>
      </div>
    </AppShell>
  );
}
