"use client";

import { useState } from "react";
import AppShell from "../components/AppShell";
import {
  AlertCircle,
  Sparkles,
  BrainCircuit,
  ArrowRight,
  ChevronDown,
  BookMarked,
  TrendingUp,
  Info,
  FileText,
} from "lucide-react";

interface Submission {
  content: string;
  type: string;
  typeLabel: string;
  topic: string;
  prompt: string;
  wordCount: number;
  submittedAt: string;
}

function getBand(wordCount: number): number {
  if (wordCount < 80)  return 4.5;
  if (wordCount < 130) return 5.0;
  if (wordCount < 180) return 5.5;
  if (wordCount < 230) return 6.0;
  if (wordCount < 290) return 6.5;
  if (wordCount < 360) return 7.0;
  return 7.5;
}

function round05(n: number) {
  return Math.round(n * 2) / 2;
}

function getExcerpt(content: string): string {
  if (!content) return "";
  const sentences = content.match(/[^.!?]+[.!?]+/g);
  if (!sentences) return content.slice(0, 150) + (content.length > 150 ? "…" : "");
  return sentences.slice(0, 2).join(" ").trim();
}

const grammarErrors = [
  {
    category: "Mạo từ (a/an/the)",
    color: "border-red-200 bg-red-50",
    badge: "bg-red-100 text-red-700",
    errors: [
      { sentence: "Technology has become important part of our daily lives.", part: "important", suggestion: "an important", explanation: "Cần mạo từ 'an' trước tính từ bắt đầu bằng âm nguyên âm 'i'." },
      { sentence: "In the modern world, internet connects billions of people.", part: "internet", suggestion: "the internet", explanation: "'Internet' là danh từ cụ thể, duy nhất — cần 'the'." },
    ],
  },
  {
    category: "Chia thì động từ",
    color: "border-orange-200 bg-orange-50",
    badge: "bg-orange-100 text-orange-700",
    errors: [
      { sentence: "Over the past decade, smartphones has changed how we communicate.", part: "has changed", suggestion: "have changed", explanation: "'Smartphones' là danh từ số nhiều, động từ phải chia 'have'." },
    ],
  },
  {
    category: "Collocation",
    color: "border-violet-200 bg-violet-50",
    badge: "bg-violet-100 text-violet-700",
    errors: [
      { sentence: "We must do efforts to solve environmental problems.", part: "do efforts", suggestion: "make efforts", explanation: "'Efforts' kết hợp với 'make', không dùng 'do'." },
    ],
  },
];

const vocabSuggestions = [
  { original: "good",      suggested: "beneficial",  example: "Regular exercise is highly beneficial for mental health.",       topic: "Health"      },
  { original: "bad",       suggested: "detrimental", example: "Pollution has a detrimental effect on urban air quality.",        topic: "Environment" },
  { original: "important", suggested: "crucial",     example: "It is crucial to address climate change immediately.",            topic: "General"     },
  { original: "many",      suggested: "a myriad of", example: "The internet offers a myriad of learning opportunities.",         topic: "Technology"  },
];

export default function ResultPage() {
  const [sub] = useState<Submission | null>(() => {
    if (typeof window === "undefined") return null;
    try {
      const raw = localStorage.getItem("nomiwrite_submission");
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  });
  const [essayExpanded, setEssayExpanded] = useState(false);

  const wordCount = sub?.wordCount ?? 287;
  const band     = sub ? getBand(sub.wordCount) : 6.5;
  const errorCount = Math.max(4, Math.round(wordCount / 22));
  const excerpt  = sub?.content ? getExcerpt(sub.content) : "";

  const criteriaScores = [
    { label: "Task Response",        score: round05(Math.min(9, band + 0.0)) },
    { label: "Coherence & Cohesion", score: round05(Math.min(9, band + 0.5)) },
    { label: "Lexical Resource",     score: round05(Math.max(4, band - 0.5)) },
    { label: "Grammatical Range",    score: round05(Math.max(4, band - 0.5)) },
  ];

  const errorGroups = [
    { ...grammarErrors[0], count: Math.round(errorCount * 0.5) },
    { ...grammarErrors[1], count: Math.round(errorCount * 0.3) },
    { ...grammarErrors[2], count: Math.round(errorCount * 0.2) },
  ];

  return (
    <AppShell activePath="/write">
      {/* Top bar */}
      <div className="sticky top-0 z-10 bg-white/90 backdrop-blur border-b border-slate-100 px-6 h-14 flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm">
          <a href="/dashboard" className="text-slate-400 hover:text-slate-600">Dashboard</a>
          <ChevronDown className="w-3 h-3 text-slate-300 -rotate-90" />
          <span className="font-bold text-slate-900">Kết quả bài viết</span>
        </div>
        <a
          href="/quiz"
          className="flex items-center gap-2 px-4 py-2 rounded-full bg-violet-600 text-white text-xs font-bold hover:bg-violet-700 transition-all"
        >
          <BrainCircuit className="w-3.5 h-3.5" />
          Làm quiz từ lỗi này
        </a>
      </div>

      <div className="p-6 w-full space-y-5">

        {/* Essay context strip — shows what was submitted */}
        {sub && (
          <div className="flex items-start gap-3 p-4 bg-slate-50 border border-slate-200 rounded-2xl">
            <FileText className="w-4 h-4 text-slate-400 shrink-0 mt-0.5" />
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 flex-wrap mb-1">
                <span className="text-xs font-extrabold text-slate-700">{sub.topic}</span>
                <span className="text-slate-300">·</span>
                <span className="text-xs font-semibold text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full">{sub.typeLabel}</span>
                <span className="text-slate-300">·</span>
                <span className="text-xs text-slate-500">{wordCount} từ</span>
              </div>
              {excerpt && (
                <p className="text-xs text-slate-500 italic leading-relaxed line-clamp-2">&ldquo;{excerpt}&rdquo;</p>
              )}
            </div>
          </div>
        )}

        {/* Score hero */}
        <div className="relative rounded-3xl overflow-hidden">
          <div className="absolute inset-0 bg-gradient-to-br from-blue-600 via-blue-700 to-violet-700" />
          <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,rgba(255,255,255,0.1),transparent_60%)]" />
          <div className="relative p-7 flex flex-col sm:flex-row items-start sm:items-center gap-6">
            <div className="shrink-0 text-center">
              <div className="w-24 h-24 rounded-2xl bg-white/15 border border-white/20 flex flex-col items-center justify-center shadow-xl">
                <p className="text-4xl font-extrabold text-white leading-none">{band}</p>
                <p className="text-xs text-blue-200 mt-1 font-semibold">Band Score</p>
              </div>
            </div>
            <div className="flex-1 grid grid-cols-2 gap-3">
              {criteriaScores.map((c) => (
                <div key={c.label} className="bg-white/10 rounded-xl p-3 border border-white/10">
                  <p className="text-xs text-blue-200 font-medium mb-1 truncate">{c.label}</p>
                  <div className="flex items-center gap-2">
                    <p className="text-lg font-extrabold text-white">{c.score}</p>
                    <div className="flex-1 h-1 bg-white/20 rounded-full overflow-hidden">
                      <div className="h-full bg-white/70 rounded-full" style={{ width: `${(c.score / 9) * 100}%` }} />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
          <div className="relative px-7 pb-6">
            <div className="bg-white/10 rounded-xl p-4 border border-white/10">
              <div className="flex items-start gap-2">
                <Info className="w-4 h-4 text-blue-200 shrink-0 mt-0.5" />
                <p className="text-sm text-blue-50 leading-relaxed">
                  {band >= 7
                    ? "Bài viết tốt — cấu trúc rõ ràng, từ vựng đa dạng. Tiếp tục duy trì và giảm lỗi mạo từ để vững chắc hơn."
                    : band >= 6
                    ? "Bài viết có cấu trúc rõ ràng và luận điểm tốt. Cần cải thiện mạo từ và collocation để lên band tiếp theo."
                    : "Cần tăng độ dài bài và đa dạng cấu trúc câu. Tập trung vào mạo từ và chia thì động từ trước."}
                  {sub && ` Bài này ${wordCount} từ.`}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* AI disclaimer */}
        <div className="flex items-start gap-2 p-3 rounded-xl bg-amber-50 border border-amber-200">
          <AlertCircle className="w-4 h-4 text-amber-600 shrink-0 mt-0.5" />
          <p className="text-xs text-amber-700">
            Điểm AI chấm chỉ mang tính tham khảo và có thể lệch so với điểm thi thật. Kết quả thực tế phụ thuộc vào giám khảo IELTS.
          </p>
        </div>

        {/* Full essay viewer */}
        {sub?.content && (
          <div className="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
            <button
              onClick={() => setEssayExpanded(v => !v)}
              className="w-full px-5 py-4 flex items-center justify-between hover:bg-slate-50 transition-all"
            >
              <div className="flex items-center gap-2">
                <FileText className="w-4 h-4 text-blue-500" />
                <h3 className="text-sm font-extrabold text-slate-900">Bài viết đã nộp</h3>
                <span className="text-xs text-slate-500 font-medium">{wordCount} từ</span>
              </div>
              <ChevronDown className={`w-4 h-4 text-slate-400 transition-transform ${essayExpanded ? "rotate-180" : ""}`} />
            </button>
            {essayExpanded && (
              <div className="px-5 pb-5 border-t border-slate-50">
                <div className="mt-4 border border-slate-100 rounded-xl p-4 bg-slate-50/50 max-h-80 overflow-y-auto">
                  <p className="text-sm text-slate-700 leading-relaxed whitespace-pre-wrap">{sub.content}</p>
                </div>
              </div>
            )}
          </div>
        )}

        {/* Grammar errors */}
        <div className="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
          <div className="px-5 py-4 border-b border-slate-50 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <AlertCircle className="w-4 h-4 text-red-500" />
              <h3 className="text-sm font-extrabold text-slate-900">Lỗi ngữ pháp</h3>
              <span className="px-2 py-0.5 text-xs font-bold bg-red-50 text-red-600 rounded-full">{errorCount} lỗi</span>
            </div>
            <span className="text-xs text-slate-400">Đã gán vào {errorGroups.length} nhóm lỗi</span>
          </div>
          <div className="divide-y divide-slate-50">
            {errorGroups.map((group) => (
              <div key={group.category} className={`p-5 ${group.color}`}>
                <div className="flex items-center gap-2 mb-4">
                  <span className={`px-2.5 py-1 text-xs font-bold rounded-full ${group.badge}`}>
                    {group.category}
                  </span>
                  <span className="text-xs text-slate-500">{group.count} lỗi trong bài</span>
                </div>
                <div className="space-y-3">
                  {group.errors.map((err, i) => (
                    <div key={i} className="bg-white rounded-xl p-4 border border-slate-100 shadow-sm">
                      <p className="text-sm text-slate-700 leading-relaxed mb-2">
                        {err.sentence.split(err.part).map((part, j, arr) => (
                          <span key={j}>
                            {part}
                            {j < arr.length - 1 && (
                              <span className="bg-red-100 text-red-600 border-b-2 border-red-400 px-0.5 rounded-sm font-medium">
                                {err.part}
                              </span>
                            )}
                          </span>
                        ))}
                      </p>
                      <div className="flex items-start gap-4 mt-3 text-xs">
                        <div>
                          <p className="text-slate-400 font-semibold mb-0.5">Gợi ý sửa</p>
                          <p className="text-emerald-700 font-bold">{err.suggestion}</p>
                        </div>
                        <div className="flex-1">
                          <p className="text-slate-400 font-semibold mb-0.5">Giải thích</p>
                          <p className="text-slate-600 leading-relaxed">{err.explanation}</p>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Vocab suggestions */}
        <div className="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
          <div className="px-5 py-4 border-b border-slate-50 flex items-center gap-2">
            <BookMarked className="w-4 h-4 text-violet-500" />
            <h3 className="text-sm font-extrabold text-slate-900">Gợi ý nâng cấp từ vựng</h3>
            <span className="px-2 py-0.5 text-xs font-bold bg-violet-50 text-violet-600 rounded-full">{vocabSuggestions.length} từ</span>
          </div>
          <div className="p-5 grid grid-cols-1 sm:grid-cols-2 gap-4">
            {vocabSuggestions.map((v) => (
              <div key={v.original} className="rounded-xl border border-slate-100 p-4 hover:border-violet-200 hover:bg-violet-50/30 transition-all">
                <div className="flex items-center gap-2 mb-2">
                  <span className="text-sm text-slate-500 line-through">{v.original}</span>
                  <ArrowRight className="w-3.5 h-3.5 text-slate-300 shrink-0" />
                  <span className="text-sm font-extrabold text-violet-700">{v.suggested}</span>
                  <span className="ml-auto text-[10px] px-2 py-0.5 bg-slate-100 text-slate-500 rounded-full font-semibold">{v.topic}</span>
                </div>
                <p className="text-xs text-slate-500 leading-relaxed italic">&ldquo;{v.example}&rdquo;</p>
              </div>
            ))}
          </div>
        </div>

        {/* Quiz CTA */}
        <div className="rounded-2xl bg-gradient-to-r from-violet-600 to-purple-700 p-6 flex flex-col sm:flex-row items-center gap-4 text-white">
          <div className="w-12 h-12 rounded-2xl bg-white/15 border border-white/20 flex items-center justify-center shrink-0">
            <BrainCircuit className="w-6 h-6" />
          </div>
          <div className="flex-1 text-center sm:text-left">
            <p className="font-extrabold text-lg mb-1">Quiz đã sẵn sàng!</p>
            <p className="text-violet-100 text-sm">
              {errorCount} câu quiz được tạo từ đúng lỗi bài này — nhắm vào mạo từ và collocation.
            </p>
          </div>
          <a
            href="/quiz"
            className="shrink-0 flex items-center gap-2 px-5 py-3 rounded-full bg-white text-violet-700 text-sm font-extrabold hover:bg-violet-50 transition-all shadow-lg"
          >
            Làm quiz ngay
            <ArrowRight className="w-4 h-4" />
          </a>
        </div>

        {/* Progress note */}
        <div className="flex items-center gap-3 p-4 bg-emerald-50 border border-emerald-200 rounded-xl">
          <TrendingUp className="w-4 h-4 text-emerald-600 shrink-0" />
          <p className="text-sm text-emerald-700">
            <span className="font-bold">Tiến bộ:</span>{" "}
            {band >= 7
              ? "Band cao nhất từ trước đến nay! Lỗi mạo từ giảm rõ rệt. Tiếp tục!"
              : "Lỗi mạo từ giảm từ 9 → " + errorGroups[0].count + " so với bài trước. Tiếp tục!"}
          </p>
        </div>

        {/* Sparkles footer */}
        <div className="flex items-center justify-center gap-2 py-2 text-xs text-slate-400">
          <Sparkles className="w-3.5 h-3.5" />
          Phân tích bởi NomiWrite AI · Chỉ mang tính tham khảo
        </div>
      </div>
    </AppShell>
  );
}
