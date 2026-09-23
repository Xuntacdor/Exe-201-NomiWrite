"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import AppShell from "../components/AppShell";
import {
  ArrowRight,
  Bell,
  ChevronRight,
  Flame,
  Loader2,
  PenLine,
  Sparkles,
  TrendingUp,
  BarChart,
  MessageSquare,
  Award,
} from "lucide-react";
import { apiClient, apiMode } from "@/lib/api/client";
import { getSession } from "@/lib/auth/session";
import type { Submission, User } from "@/lib/types";

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en-GB", { 
    day: 'numeric', 
    month: 'short', 
    year: 'numeric' 
  }).format(new Date(value));
}

export default function DashboardPage() {
  const router = useRouter();
  const [user, setUser] = useState<User | null>(null);
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (apiMode === "real" && !getSession()?.accessToken) {
      router.replace("/login");
      return;
    }

    let ignore = false;
    const loadTimer = setTimeout(() => {
      setLoading(true);
      Promise.all([
        apiClient.getMyAccount().catch(() => apiClient.getMe()),
        apiClient.listSubmissions(),
        apiClient.listGradingHistory().catch(() => []),
      ])
        .then(([profile, items, gradingHistory]) => {
          if (ignore) return;
          const scoreBySubmission = new Map(gradingHistory.map(item => [item.submissionId, item.overallBand]));
          const merged = items.map(item => ({
            ...item,
            overallScore: item.overallScore ?? scoreBySubmission.get(item.id),
            status: scoreBySubmission.has(item.id) ? "graded" as const : item.status,
          }));
          setUser(profile);
          setSubmissions(merged);
        })
        .catch(err => {
          if (!ignore) setError(err instanceof Error ? err.message : "Could not load dashboard.");
        })
        .finally(() => {
          if (!ignore) setLoading(false);
        });
    }, 0);

    return () => {
      ignore = true;
      clearTimeout(loadTimer);
    };
  }, [router]);

  const gradedScores = submissions.map(item => item.overallScore).filter((score): score is number => typeof score === "number");
  const averageScore = gradedScores.length
    ? (gradedScores.reduce((sum, score) => sum + score, 0) / gradedScores.length).toFixed(1)
    : "--";
  const recentSubmissions = useMemo(
    () => [...submissions].sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime()).slice(0, 5),
    [submissions],
  );
  const bandHistory = gradedScores.slice(-12);
  const maxBand = 9.0;

  return (
    <AppShell activePath="/dashboard">
      <div className="sticky top-0 z-50 flex h-[72px] items-center justify-between border-b border-slate-200/50 bg-white/80 px-8 backdrop-blur-xl">
        <div>
          <h1 className="text-xl font-extrabold tracking-tight text-slate-900">
            Welcome back, {user?.displayName ?? "Writer"}! 👋
          </h1>
        </div>
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2 rounded-full border border-orange-200 bg-orange-50 px-3.5 py-1.5 text-[13px] font-bold text-orange-600 shadow-sm">
            <Flame className="h-4 w-4" />
            <span>{submissions.length} Essays</span>
          </div>
          <button className="relative flex h-10 w-10 items-center justify-center rounded-full border border-slate-200 bg-white text-slate-500 shadow-sm transition-all hover:bg-slate-50 hover:text-slate-900">
            <Bell className="h-5 w-5" />
            <span className="absolute right-2.5 top-2.5 h-2 w-2 rounded-full bg-red-500 ring-2 ring-white"></span>
          </button>
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-blue-600 to-blue-800 text-sm font-bold text-white shadow-md">
            {(user?.displayName ?? "N").slice(0, 1).toUpperCase()}
          </div>
        </div>
      </div>

      <div className="mx-auto max-w-6xl space-y-8 p-8">
        {loading && (
          <div className="flex items-center justify-center gap-3 rounded-3xl border border-slate-100 bg-white p-12 text-[15px] font-semibold text-slate-500 shadow-sm">
            <Loader2 className="h-5 w-5 animate-spin text-blue-600" />
            Loading your writing dashboard...
          </div>
        )}

        {!loading && error && (
          <div className="rounded-2xl border border-red-200 bg-red-50 p-6 text-[15px] font-semibold text-red-700 shadow-sm">
            {error}
          </div>
        )}

        {!loading && !error && (
          <>
            {/* Stats Row */}
            <div className="grid grid-cols-2 gap-5 lg:grid-cols-4">
              {[
                { label: "Avg. Writing Band", value: averageScore, sub: "Based on AI grading", color: "text-blue-600", bg: "bg-blue-50", Icon: TrendingUp },
                { label: "Essays Graded", value: gradedScores.length, sub: "Total completed", color: "text-emerald-600", bg: "bg-emerald-50", Icon: Award },
                { label: "Current Plan", value: user?.plan === "premium" ? "PRO" : "Free", sub: "Upgrade for full AI feedback", color: "text-violet-600", bg: "bg-violet-50", Icon: Sparkles },
                { label: "Target Band", value: "7.0+", sub: "Set your goal in Profile", color: "text-orange-600", bg: "bg-orange-50", Icon: Flame },
              ].map(({ label, value, sub, color, bg, Icon }) => (
                <div key={label} className="relative overflow-hidden rounded-3xl border border-slate-100 bg-white p-6 shadow-[0_2px_12px_rgba(0,0,0,0.02)] transition-all hover:shadow-[0_4px_24px_rgba(0,0,0,0.06)]">
                  <div className={`mb-4 flex h-12 w-12 items-center justify-center rounded-2xl ${bg}`}>
                    <Icon className={color} strokeWidth={2.5} style={{ width: 22, height: 22 }} />
                  </div>
                  <p className={`text-3xl font-extrabold tracking-tight ${color}`}>{value}</p>
                  <p className="mt-1 text-[15px] font-bold text-slate-800">{label}</p>
                  <p className="mt-1 text-[13px] font-medium text-slate-500">{sub}</p>
                </div>
              ))}
            </div>

            {/* Practice Modules */}
            <div>
              <div className="mb-5 flex items-center justify-between">
                <h2 className="text-xl font-extrabold text-slate-900">Luyện Tập IELTS Writing</h2>
                <Link href="/write" className="text-sm font-bold text-blue-600 hover:text-blue-700">View all prompts →</Link>
              </div>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                {/* Task 1 Card */}
                <div className="group relative flex flex-col justify-between overflow-hidden rounded-3xl border border-slate-100 bg-white p-8 shadow-sm transition-all hover:border-blue-100 hover:shadow-lg hover:shadow-blue-900/5">
                  <div className="absolute -right-12 -top-12 h-40 w-40 rounded-full bg-blue-50/50 blur-3xl transition-all group-hover:bg-blue-100/50"></div>
                  <div className="relative z-10">
                    <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-blue-50 text-blue-600">
                      <BarChart strokeWidth={2.5} className="h-6 w-6" />
                    </div>
                    <h3 className="mb-2 text-2xl font-extrabold text-slate-900">Writing Task 1</h3>
                    <p className="text-[15px] leading-relaxed text-slate-500">
                      Summarize, describe or explain visual information (graphs, charts, tables or diagrams) in at least 150 words.
                    </p>
                  </div>
                  <Link 
                    href="/write?task=1" 
                    className="relative z-10 mt-8 inline-flex items-center justify-center gap-2 rounded-2xl bg-slate-900 px-6 py-3.5 text-[15px] font-bold text-white transition-all hover:bg-slate-800"
                  >
                    Bắt đầu làm bài <ArrowRight className="h-4 w-4" />
                  </Link>
                </div>

                {/* Task 2 Card */}
                <div className="group relative flex flex-col justify-between overflow-hidden rounded-3xl border border-slate-100 bg-white p-8 shadow-sm transition-all hover:border-violet-100 hover:shadow-lg hover:shadow-violet-900/5">
                  <div className="absolute -right-12 -top-12 h-40 w-40 rounded-full bg-violet-50/50 blur-3xl transition-all group-hover:bg-violet-100/50"></div>
                  <div className="relative z-10">
                    <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-violet-50 text-violet-600">
                      <MessageSquare strokeWidth={2.5} className="h-6 w-6" />
                    </div>
                    <h3 className="mb-2 text-2xl font-extrabold text-slate-900">Writing Task 2</h3>
                    <p className="text-[15px] leading-relaxed text-slate-500">
                      Write an essay in response to a point of view, argument or problem in at least 250 words. High score impact.
                    </p>
                  </div>
                  <Link 
                    href="/write?task=2" 
                    className="relative z-10 mt-8 inline-flex items-center justify-center gap-2 rounded-2xl bg-blue-600 px-6 py-3.5 text-[15px] font-bold text-white shadow-md shadow-blue-200 transition-all hover:-translate-y-0.5 hover:bg-blue-700"
                  >
                    Bắt đầu làm bài <ArrowRight className="h-4 w-4" />
                  </Link>
                </div>
              </div>
            </div>

            {/* Bottom Section */}
            <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
              {/* Recent History */}
              <div className="col-span-2 overflow-hidden rounded-3xl border border-slate-100 bg-white shadow-sm">
                <div className="flex items-center justify-between border-b border-slate-100 p-6">
                  <div>
                    <h3 className="text-lg font-extrabold text-slate-900">Lịch sử làm bài</h3>
                    <p className="mt-1 text-sm text-slate-500">Your latest essay submissions</p>
                  </div>
                  <Link href="/history" className="rounded-full bg-slate-50 px-4 py-2 text-sm font-bold text-slate-600 transition-colors hover:bg-slate-100">
                    Xem tất cả
                  </Link>
                </div>
                {recentSubmissions.length ? (
                  <div className="divide-y divide-slate-100">
                    {recentSubmissions.map(submission => (
                      <Link key={submission.id} href={`/result?submissionId=${submission.id}`} className="flex items-center justify-between p-6 transition-colors hover:bg-slate-50/80 group">
                        <div className="flex items-center gap-4">
                          <div className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl font-bold ${submission.overallScore ? "bg-emerald-50 text-emerald-700" : "bg-slate-100 text-slate-500"}`}>
                            {submission.overallScore ? submission.overallScore.toFixed(1) : "-"}
                          </div>
                          <div>
                            <p className="line-clamp-1 font-bold text-slate-900 group-hover:text-blue-600 transition-colors">{submission.topic}</p>
                            <div className="mt-1 flex items-center gap-2 text-sm text-slate-500">
                              <span className="font-medium text-slate-700">{submission.writingType}</span>
                              <span>•</span>
                              <span>{formatDate(submission.submittedAt)}</span>
                              <span>•</span>
                              <span className={submission.status === "graded" ? "text-emerald-600 font-medium" : "text-amber-600 font-medium"}>
                                {submission.status.charAt(0).toUpperCase() + submission.status.slice(1)}
                              </span>
                            </div>
                          </div>
                        </div>
                        <ChevronRight className="ml-4 h-5 w-5 shrink-0 text-slate-300 transition-all group-hover:text-blue-600 group-hover:translate-x-1" />
                      </Link>
                    ))}
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center py-16 text-center">
                    <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-slate-50 text-slate-400">
                      <PenLine strokeWidth={2} className="h-8 w-8" />
                    </div>
                    <p className="text-lg font-bold text-slate-900">Chưa có bài làm nào</p>
                    <p className="mt-2 text-sm text-slate-500">Hãy bắt đầu viết bài đầu tiên của bạn để nhận đánh giá chi tiết.</p>
                  </div>
                )}
              </div>

              {/* PRO Banner */}
              <div className="flex flex-col gap-6">
                <div className="rounded-3xl bg-gradient-to-br from-slate-900 to-slate-800 p-6 text-white shadow-sm">
                  <div className="flex items-center gap-2 mb-3">
                    <Sparkles className="h-5 w-5 text-yellow-400" />
                    <h4 className="font-bold text-base">NomiWrite PRO</h4>
                  </div>
                  <p className="text-[13px] text-slate-300 mb-5 leading-relaxed">
                    Mở khóa tính năng chấm chữa chi tiết từng câu (Line-by-line grading) và nhận xét theo tiêu chí IELTS.
                  </p>
                  <Link href="/upgrade" className="block w-full rounded-xl bg-white/10 px-4 py-3 text-center text-sm font-bold transition-colors hover:bg-white/20">
                    Tìm hiểu thêm
                  </Link>
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </AppShell>
  );
}
