"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
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
  const recentSubmissions = [...submissions]
    .sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime())
    .slice(0, 5);

  return (
    <AppShell activePath="/dashboard">
      <div className="sticky top-0 z-50 flex h-[72px] items-center justify-between border-b border-line bg-surface px-8">
        <div>
          <h1 className="text-xl font-bold tracking-tight text-ink">
            Welcome back, {user?.displayName ?? "Writer"}! 👋
          </h1>
        </div>
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2 rounded-full border border-line bg-accent px-3.5 py-1.5 text-[13px] font-bold text-accent-ink shadow-sm">
            <Flame className="h-4 w-4" />
            <span>{submissions.length} Essays</span>
          </div>
          <button className="relative flex h-10 w-10 items-center justify-center rounded-full border border-line bg-surface text-muted shadow-sm transition-all hover:bg-canvas hover:text-ink">
            <Bell className="h-5 w-5" />
            <span className="absolute right-2.5 top-2.5 h-2 w-2 rounded-full bg-danger ring-2 ring-focus"></span>
          </button>
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-accent text-sm font-bold text-ink shadow-sm">
            {(user?.displayName ?? "N").slice(0, 1).toUpperCase()}
          </div>
        </div>
      </div>

      <div className="mx-auto max-w-6xl space-y-8 p-8">
        {loading && (
          <div className="flex items-center justify-center gap-3 rounded-xl border border-line bg-surface p-12 text-[15px] font-semibold text-muted shadow-sm">
            <Loader2 className="h-5 w-5 animate-spin text-accent-ink" />
            Loading your writing dashboard...
          </div>
        )}

        {!loading && error && (
          <div className="rounded-xl border border-line bg-danger p-6 text-[15px] font-semibold text-danger-ink shadow-sm">
            {error}
          </div>
        )}

        {!loading && !error && (
          <>
            {/* Stats Row */}
            <div className="grid grid-cols-2 gap-5 lg:grid-cols-4">
              {[
                { label: "Avg. Writing Band", value: averageScore, sub: "Based on AI grading", color: "text-accent-ink", bg: "bg-accent", Icon: TrendingUp },
                { label: "Essays Graded", value: gradedScores.length, sub: "Total completed", color: "text-success-ink", bg: "bg-success", Icon: Award },
                { label: "Current Plan", value: user?.plan === "premium" ? "PRO" : "Free", sub: "Upgrade for full AI feedback", color: "text-rose-ink", bg: "bg-rose", Icon: Sparkles },
                { label: "Target Band", value: "7.0+", sub: "Set your goal in Profile", color: "text-accent-ink", bg: "bg-accent", Icon: Flame },
              ].map(({ label, value, sub, color, bg, Icon }) => (
                <div key={label} className="relative overflow-hidden rounded-xl border border-line bg-surface p-6 shadow-sm transition-all hover:shadow-sm">
                  <div className={`mb-4 flex h-12 w-12 items-center justify-center rounded-xl ${bg}`}>
                    <Icon className={color} strokeWidth={2.5} style={{ width: 22, height: 22 }} />
                  </div>
                  <p className={`text-3xl font-bold tracking-tight ${color}`}>{value}</p>
                  <p className="mt-1 text-[15px] font-bold text-ink">{label}</p>
                  <p className="mt-1 text-[13px] font-medium text-muted">{sub}</p>
                </div>
              ))}
            </div>

            {/* Practice Modules */}
            <div>
              <div className="mb-5 flex items-center justify-between">
                <h2 className="text-xl font-bold text-ink">Luyện Tập IELTS Writing</h2>
                <Link href="/write" className="text-sm font-bold text-accent-ink hover:text-accent-ink">View all prompts →</Link>
              </div>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                {/* Task 1 Card */}
                <div className="group relative flex flex-col justify-between overflow-hidden rounded-xl border border-line bg-surface p-8 shadow-sm transition-all hover:border-line hover:shadow-sm">
                  <div className="relative z-10">
                    <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-accent text-accent-ink">
                      <BarChart strokeWidth={2.5} className="h-6 w-6" />
                    </div>
                    <h3 className="mb-2 text-2xl font-bold text-ink">Writing Task 1</h3>
                    <p className="text-[15px] leading-relaxed text-muted">
                      Summarize, describe or explain visual information (graphs, charts, tables or diagrams) in at least 150 words.
                    </p>
                  </div>
                  <Link
                    href="/write?task=1"
                    className="relative z-10 mt-8 inline-flex items-center justify-center gap-2 rounded-xl bg-surface-muted px-6 py-3.5 text-[15px] font-bold text-ink transition-all hover:bg-surface-muted"
                  >
                    Bắt đầu làm bài <ArrowRight className="h-4 w-4" />
                  </Link>
                </div>

                {/* Task 2 Card */}
                <div className="group relative flex flex-col justify-between overflow-hidden rounded-xl border border-line bg-surface p-8 shadow-sm transition-all hover:border-line hover:shadow-sm">
                  <div className="relative z-10">
                    <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-rose text-rose-ink">
                      <MessageSquare strokeWidth={2.5} className="h-6 w-6" />
                    </div>
                    <h3 className="mb-2 text-2xl font-bold text-ink">Writing Task 2</h3>
                    <p className="text-[15px] leading-relaxed text-muted">
                      Write an essay in response to a point of view, argument or problem in at least 250 words. High score impact.
                    </p>
                  </div>
                  <Link
                    href="/write?task=2"
                    className="relative z-10 mt-8 inline-flex items-center justify-center gap-2 rounded-xl bg-accent px-6 py-3.5 text-[15px] font-bold text-ink shadow-sm transition-all  hover:bg-accent-hover"
                  >
                    Bắt đầu làm bài <ArrowRight className="h-4 w-4" />
                  </Link>
                </div>
              </div>
            </div>

            {/* Bottom Section */}
            <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
              {/* Recent History */}
              <div className="col-span-2 overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
                <div className="flex items-center justify-between border-b border-line p-6">
                  <div>
                    <h3 className="text-lg font-bold text-ink">Lịch sử làm bài</h3>
                    <p className="mt-1 text-sm text-muted">Your latest essay submissions</p>
                  </div>
                  <Link href="/history" className="rounded-full bg-canvas px-4 py-2 text-sm font-bold text-muted transition-colors hover:bg-surface-muted">
                    Xem tất cả
                  </Link>
                </div>
                {recentSubmissions.length ? (
                  <div className="divide-y divide-line">
                    {recentSubmissions.map(submission => (
                      <Link key={submission.id} href={`/result?submissionId=${submission.id}`} className="flex items-center justify-between p-6 transition-colors hover:bg-canvas group">
                        <div className="flex items-center gap-4">
                          <div className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-xl font-bold ${submission.overallScore ? "bg-success text-success-ink" : "bg-surface-muted text-muted"}`}>
                            {submission.overallScore ? submission.overallScore.toFixed(1) : "-"}
                          </div>
                          <div>
                            <p className="line-clamp-1 font-bold text-ink group-hover:text-accent-ink transition-colors">{submission.topic}</p>
                            <div className="mt-1 flex items-center gap-2 text-sm text-muted">
                              <span className="font-medium text-ink">{submission.writingType}</span>
                              <span>•</span>
                              <span>{formatDate(submission.submittedAt)}</span>
                              <span>•</span>
                              <span className={submission.status === "graded" ? "text-success-ink font-medium" : "text-warning-ink font-medium"}>
                                {submission.status.charAt(0).toUpperCase() + submission.status.slice(1)}
                              </span>
                            </div>
                          </div>
                        </div>
                        <ChevronRight className="ml-4 h-5 w-5 shrink-0 text-muted transition-all group-hover:text-accent-ink " />
                      </Link>
                    ))}
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center py-16 text-center">
                    <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-canvas text-muted">
                      <PenLine strokeWidth={2} className="h-8 w-8" />
                    </div>
                    <p className="text-lg font-bold text-ink">Chưa có bài làm nào</p>
                    <p className="mt-2 text-sm text-muted">Hãy bắt đầu viết bài đầu tiên của bạn để nhận đánh giá chi tiết.</p>
                  </div>
                )}
              </div>

              {/* PRO Banner */}
              <div className="flex flex-col gap-6">
                <div className="rounded-xl bg-surface-muted p-6 text-ink shadow-sm">
                  <div className="flex items-center gap-2 mb-3">
                    <Sparkles className="h-5 w-5 text-warning-ink" />
                    <h4 className="font-bold text-base">NomiWrite PRO</h4>
                  </div>
                  <p className="text-[13px] text-muted mb-5 leading-relaxed">
                    Mở khóa tính năng chấm chữa chi tiết từng câu (Line-by-line grading) và nhận xét theo tiêu chí IELTS.
                  </p>
                  <Link href="/upgrade" className="block w-full rounded-xl bg-surface px-4 py-3 text-center text-sm font-bold transition-colors hover:bg-surface">
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
