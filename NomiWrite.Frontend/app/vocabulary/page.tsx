"use client";

import { useState, useMemo } from "react";
import AppShell from "../components/AppShell";
import {
  Search,
  BookOpen,
  ArrowRight,
  CheckCircle2,
  Circle,
  X,
  Trophy,
  RotateCcw,
  Zap,
} from "lucide-react";

interface Word {
  id: number;
  original: string;
  suggested: string;
  example: string;
  category: string;
  type: "adj" | "adv" | "verb" | "noun" | "phrase" | "conj";
  synonyms: string[];
  mastered: boolean;
}

interface QuizQuestion {
  word: Word;
  options: string[];
  correctIdx: number;
}

const categories = [
  { key: "all",         label: "Tất cả",     color: "bg-slate-600"   },
  { key: "Health",      label: "Sức khoẻ",   color: "bg-emerald-500" },
  { key: "Environment", label: "Môi trường",  color: "bg-teal-500"    },
  { key: "Technology",  label: "Công nghệ",   color: "bg-blue-500"    },
  { key: "Education",   label: "Giáo dục",   color: "bg-violet-500"  },
  { key: "Academic",    label: "Học thuật",   color: "bg-indigo-500"  },
  { key: "Business",    label: "Kinh doanh",  color: "bg-orange-500"  },
  { key: "Society",     label: "Xã hội",      color: "bg-rose-500"    },
];

const typeLabel: Record<string, string> = {
  adj: "tính từ", adv: "trạng từ", verb: "động từ",
  noun: "danh từ", phrase: "cụm từ", conj: "liên từ",
};

const initialWords: Word[] = [
  // ── Health ──────────────────────────────────────────────
  { id: 1,  original: "good",        suggested: "beneficial",          example: "Regular exercise is highly beneficial for mental health.",                   category: "Health",      type: "adj",    synonyms: ["advantageous", "favourable"],              mastered: false },
  { id: 2,  original: "bad",         suggested: "detrimental",         example: "A sedentary lifestyle is detrimental to cardiovascular health.",              category: "Health",      type: "adj",    synonyms: ["harmful", "adverse"],                      mastered: false },
  { id: 3,  original: "fix",         suggested: "address",             example: "Governments must address the rising rates of obesity.",                       category: "Health",      type: "verb",   synonyms: ["tackle", "combat"],                        mastered: false },
  { id: 4,  original: "help",        suggested: "alleviate",           example: "Exercise can alleviate symptoms of depression and anxiety.",                  category: "Health",      type: "verb",   synonyms: ["relieve", "mitigate"],                     mastered: false },
  { id: 5,  original: "get worse",   suggested: "deteriorate",         example: "Without treatment, the patient's condition will deteriorate rapidly.",         category: "Health",      type: "verb",   synonyms: ["worsen", "decline"],                       mastered: false },
  { id: 6,  original: "make better", suggested: "enhance",             example: "A nutritious diet can enhance both physical and cognitive performance.",       category: "Health",      type: "verb",   synonyms: ["improve", "boost"],                        mastered: false },
  { id: 7,  original: "increase",    suggested: "escalate",            example: "Healthcare costs have escalated significantly over the past decade.",          category: "Health",      type: "verb",   synonyms: ["surge", "soar"],                           mastered: false },
  { id: 8,  original: "sick",        suggested: "afflicted",           example: "Millions of people are afflicted with preventable diseases each year.",        category: "Health",      type: "adj",    synonyms: ["suffering", "stricken"],                   mastered: false },

  // ── Environment ──────────────────────────────────────────
  { id: 9,  original: "destroy",     suggested: "devastate",           example: "Deforestation has devastated biodiversity in tropical regions.",               category: "Environment", type: "verb",   synonyms: ["decimate", "obliterate"],                 mastered: false },
  { id: 10, original: "damage",      suggested: "degrade",             example: "Industrial waste continues to degrade water quality in rivers.",               category: "Environment", type: "verb",   synonyms: ["contaminate", "pollute"],                 mastered: false },
  { id: 11, original: "pollution",   suggested: "contamination",       example: "Air contamination in major cities poses serious health risks.",                 category: "Environment", type: "noun",   synonyms: ["toxicity", "emissions"],                  mastered: false },
  { id: 12, original: "use up",      suggested: "deplete",             example: "Fossil fuels are being depleted at an alarming rate.",                         category: "Environment", type: "verb",   synonyms: ["exhaust", "consume"],                      mastered: false },
  { id: 13, original: "reduce",      suggested: "mitigate",            example: "Renewable energy can mitigate the effects of climate change.",                 category: "Environment", type: "verb",   synonyms: ["alleviate", "curtail"],                   mastered: false },
  { id: 14, original: "rubbish",     suggested: "waste",               example: "Excessive waste generation is a hallmark of consumerist societies.",           category: "Environment", type: "noun",   synonyms: ["refuse", "debris"],                        mastered: false },
  { id: 15, original: "recover",     suggested: "rehabilitate",        example: "Reforestation efforts aim to rehabilitate degraded ecosystems.",               category: "Environment", type: "verb",   synonyms: ["restore", "regenerate"],                  mastered: false },
  { id: 16, original: "worry about", suggested: "address",             example: "World leaders must address environmental degradation urgently.",                category: "Environment", type: "verb",   synonyms: ["tackle", "combat"],                        mastered: false },

  // ── Technology ───────────────────────────────────────────
  { id: 17, original: "use",         suggested: "utilise",             example: "Students should utilise digital tools to enhance their learning.",             category: "Technology",  type: "verb",   synonyms: ["employ", "apply"],                         mastered: false },
  { id: 18, original: "spread",      suggested: "proliferate",         example: "Smartphones have proliferated across both developed and developing nations.",   category: "Technology",  type: "verb",   synonyms: ["expand", "disseminate"],                   mastered: false },
  { id: 19, original: "change",      suggested: "revolutionise",       example: "Artificial intelligence has the potential to revolutionise healthcare.",        category: "Technology",  type: "verb",   synonyms: ["transform", "reshape"],                    mastered: false },
  { id: 20, original: "connect",     suggested: "integrate",           example: "IoT devices integrate seamlessly into modern household systems.",              category: "Technology",  type: "verb",   synonyms: ["link", "incorporate"],                     mastered: false },
  { id: 21, original: "new idea",    suggested: "innovation",          example: "Technological innovation is a key driver of economic growth.",                 category: "Technology",  type: "noun",   synonyms: ["breakthrough", "advancement"],              mastered: false },
  { id: 22, original: "fast",        suggested: "exponentially",       example: "Computing power has grown exponentially over the past 50 years.",              category: "Technology",  type: "adv",    synonyms: ["rapidly", "dramatically"],                 mastered: false },
  { id: 23, original: "many",        suggested: "a myriad of",         example: "The internet offers a myriad of learning opportunities for self-study.",       category: "Technology",  type: "phrase", synonyms: ["numerous", "countless"],                   mastered: false },
  { id: 24, original: "help",        suggested: "facilitate",          example: "Digital platforms facilitate communication across geographical boundaries.",   category: "Technology",  type: "verb",   synonyms: ["enable", "streamline"],                    mastered: false },

  // ── Education ────────────────────────────────────────────
  { id: 25, original: "important",   suggested: "pivotal",             example: "Critical thinking is a pivotal skill in the modern workplace.",                category: "Education",   type: "adj",    synonyms: ["crucial", "essential"],                    mastered: false },
  { id: 26, original: "teach",       suggested: "instruct",            example: "Effective teachers instruct students through experiential learning.",          category: "Education",   type: "verb",   synonyms: ["educate", "mentor"],                       mastered: false },
  { id: 27, original: "big",         suggested: "substantial",         example: "There has been a substantial improvement in global literacy rates.",           category: "Education",   type: "adj",    synonyms: ["considerable", "significant"],             mastered: false },
  { id: 28, original: "skill",       suggested: "competency",          example: "Digital competency is now considered an essential life skill.",                category: "Education",   type: "noun",   synonyms: ["proficiency", "capability"],               mastered: false },
  { id: 29, original: "learn",       suggested: "acquire",             example: "Students acquire knowledge through both formal and informal settings.",        category: "Education",   type: "verb",   synonyms: ["attain", "develop"],                       mastered: false },
  { id: 30, original: "change",      suggested: "reform",              example: "Educational reform is needed to prepare students for the digital economy.",    category: "Education",   type: "verb",   synonyms: ["restructure", "overhaul"],                 mastered: false },
  { id: 31, original: "success",     suggested: "achievement",         example: "Academic achievement should not be measured by grades alone.",                 category: "Education",   type: "noun",   synonyms: ["accomplishment", "attainment"],            mastered: false },

  // ── Academic ─────────────────────────────────────────────
  { id: 32, original: "show",        suggested: "demonstrate",         example: "The data clearly demonstrates a correlation between poverty and health outcomes.", category: "Academic", type: "verb",   synonyms: ["illustrate", "indicate"],                  mastered: false },
  { id: 33, original: "think",       suggested: "contend",             example: "Some scholars contend that social media exacerbates political polarisation.",  category: "Academic",    type: "verb",   synonyms: ["argue", "assert", "maintain"],             mastered: false },
  { id: 34, original: "also",        suggested: "furthermore",         example: "Furthermore, the evidence suggests a causal link between diet and cognition.", category: "Academic",    type: "adv",    synonyms: ["moreover", "in addition"],                 mastered: false },
  { id: 35, original: "but",         suggested: "nevertheless",        example: "Nevertheless, the policy has faced significant opposition from stakeholders.",  category: "Academic",    type: "conj",   synonyms: ["however", "nonetheless"],                  mastered: false },
  { id: 36, original: "because",     suggested: "given that",          example: "Given that resources are limited, prioritisation is essential.",               category: "Academic",    type: "phrase", synonyms: ["since", "due to the fact that"],           mastered: false },
  { id: 37, original: "it seems",    suggested: "evidence suggests",   example: "Evidence suggests that economic inequality has widened in recent decades.",    category: "Academic",    type: "phrase", synonyms: ["data indicates", "research shows"],        mastered: false },
  { id: 38, original: "many studies","suggested": "a body of research", example: "A body of research supports the link between sleep deprivation and poor health.", category: "Academic", type: "phrase", synonyms: ["empirical evidence", "scholarly literature"], mastered: false },

  // ── Business ─────────────────────────────────────────────
  { id: 39, original: "grow",        suggested: "expand",              example: "The company plans to expand its operations into Southeast Asian markets.",    category: "Business",    type: "verb",   synonyms: ["scale", "flourish"],                       mastered: false },
  { id: 40, original: "make money",  suggested: "generate revenue",    example: "Subscription models generate revenue more predictably than one-off sales.",  category: "Business",    type: "phrase", synonyms: ["yield profit", "produce income"],           mastered: false },
  { id: 41, original: "workers",     suggested: "workforce",           example: "A skilled workforce is critical to sustaining competitive advantage.",        category: "Business",    type: "noun",   synonyms: ["personnel", "employees"],                  mastered: false },
  { id: 42, original: "plan",        suggested: "strategy",            example: "The company's long-term strategy focuses on sustainable growth.",             category: "Business",    type: "noun",   synonyms: ["initiative", "approach"],                  mastered: false },
  { id: 43, original: "problem",     suggested: "predicament",         example: "Rising inflation presents a significant predicament for small businesses.",  category: "Business",    type: "noun",   synonyms: ["challenge", "obstacle"],                   mastered: false },
  { id: 44, original: "buy",         suggested: "procure",             example: "The team procures materials at the most competitive prices available.",      category: "Business",    type: "verb",   synonyms: ["acquire", "obtain"],                       mastered: false },

  // ── Society ───────────────────────────────────────────────
  { id: 45, original: "people",      suggested: "individuals",         example: "Individuals have a responsibility to contribute to societal wellbeing.",       category: "Society",     type: "noun",   synonyms: ["citizens", "members of society"],          mastered: false },
  { id: 46, original: "government",  suggested: "policymakers",        example: "Policymakers must balance economic growth with social equity.",                category: "Society",     type: "noun",   synonyms: ["authorities", "legislators"],              mastered: false },
  { id: 47, original: "poor people", suggested: "marginalised communities", example: "Marginalised communities often lack access to quality healthcare.",      category: "Society",     type: "phrase", synonyms: ["disadvantaged groups", "underserved populations"], mastered: false },
  { id: 48, original: "help",        suggested: "empower",             example: "Education can empower individuals to break the cycle of poverty.",             category: "Society",     type: "verb",   synonyms: ["uplift", "support"],                       mastered: false },
  { id: 49, original: "rules",       suggested: "legislation",         example: "Robust legislation is needed to protect workers' rights in the gig economy.", category: "Society",     type: "noun",   synonyms: ["regulations", "policy"],                   mastered: false },
  { id: 50, original: "fair",        suggested: "equitable",           example: "An equitable distribution of wealth is fundamental to social cohesion.",       category: "Society",     type: "adj",    synonyms: ["impartial", "just"],                       mastered: false },
];

function buildQuiz(words: Word[], categoryKey: string): QuizQuestion[] {
  const pool = categoryKey === "all" ? [...words] : words.filter(w => w.category === categoryKey);
  const shuffled = [...pool].sort(() => Math.random() - 0.5);
  const allSuggested = words.map(w => w.suggested);
  return shuffled.map(w => {
    const wrongs = allSuggested
      .filter(s => s !== w.suggested)
      .sort(() => Math.random() - 0.5)
      .slice(0, 3);
    const options = [w.suggested, ...wrongs].sort(() => Math.random() - 0.5);
    return { word: w, options, correctIdx: options.indexOf(w.suggested) };
  });
}

export default function VocabularyPage() {
  const [words, setWords] = useState<Word[]>(initialWords);
  const [search, setSearch] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("all");
  const [masteredFilter, setMasteredFilter] = useState<"all" | "mastered" | "learning">("all");

  // Quiz state
  const [quizOpen, setQuizOpen] = useState(false);
  const [quizCategoryKey, setQuizCategoryKey] = useState("all");
  const [questions, setQuestions] = useState<QuizQuestion[]>([]);
  const [qIndex, setQIndex] = useState(0);
  const [picked, setPicked] = useState<number | null>(null);
  const [score, setScore] = useState(0);
  const [quizDone, setQuizDone] = useState(false);

  const toggleMastered = (id: number) =>
    setWords(prev => prev.map(w => w.id === id ? { ...w, mastered: !w.mastered } : w));

  const masteredCount = words.filter(w => w.mastered).length;

  const filtered = useMemo(() => words.filter(w => {
    const q = search.toLowerCase();
    const matchSearch = !q || w.original.includes(q) || w.suggested.includes(q) || w.synonyms.some(s => s.includes(q));
    const matchCat = selectedCategory === "all" || w.category === selectedCategory;
    const matchMastered = masteredFilter === "all" || (masteredFilter === "mastered" ? w.mastered : !w.mastered);
    return matchSearch && matchCat && matchMastered;
  }), [words, search, selectedCategory, masteredFilter]);

  const startQuiz = (categoryKey: string) => {
    const qs = buildQuiz(words, categoryKey);
    setQuizCategoryKey(categoryKey);
    setQuestions(qs);
    setQIndex(0);
    setPicked(null);
    setScore(0);
    setQuizDone(false);
    setQuizOpen(true);
  };

  const handlePick = (idx: number) => {
    if (picked !== null) return;
    setPicked(idx);
    if (idx === questions[qIndex].correctIdx) setScore(s => s + 1);
  };

  const handleNext = () => {
    if (qIndex + 1 >= questions.length) {
      setQuizDone(true);
    } else {
      setQIndex(i => i + 1);
      setPicked(null);
    }
  };

  const resetQuiz = () => {
    const qs = buildQuiz(words, quizCategoryKey);
    setQuestions(qs);
    setQIndex(0);
    setPicked(null);
    setScore(0);
    setQuizDone(false);
  };

  const catLabel = categories.find(c => c.key === quizCategoryKey)?.label ?? "Tất cả";
  const currentQ = questions[qIndex];

  return (
    <AppShell activePath="/vocabulary">
      {/* Top bar */}
      <div className="sticky top-0 z-10 bg-white/90 backdrop-blur border-b border-slate-100 px-6 h-14 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <BookOpen className="w-4 h-4 text-emerald-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Sổ từ vựng</h1>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-1.5 text-xs text-slate-500">
            <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" />
            <span className="font-semibold">{masteredCount}/{words.length} thành thạo</span>
          </div>
          <button
            onClick={() => startQuiz(selectedCategory)}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-violet-600 text-white text-xs font-bold rounded-full hover:bg-violet-700 transition-all"
          >
            <Zap className="w-3 h-3" />
            Quiz {selectedCategory === "all" ? "tất cả" : categories.find(c => c.key === selectedCategory)?.label}
          </button>
        </div>
      </div>

      <div className="p-5 w-full space-y-4">
        {/* Stats */}
        <div className="grid grid-cols-3 gap-3">
          {[
            { label: "Tổng từ",     value: words.length,              color: "text-slate-900" },
            { label: "Đang học",    value: words.length - masteredCount, color: "text-blue-600"  },
            { label: "Thành thạo", value: masteredCount,              color: "text-emerald-600"},
          ].map(({ label, value, color }) => (
            <div key={label} className="bg-white rounded-2xl border border-slate-100 p-4 text-center shadow-sm">
              <p className={`text-2xl font-extrabold ${color}`}>{value}</p>
              <p className="text-xs text-slate-500 mt-0.5">{label}</p>
            </div>
          ))}
        </div>

        {/* Search */}
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <input
            type="text"
            placeholder="Tìm từ gốc, từ học thuật, synonym…"
            value={search}
            onChange={e => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 text-sm bg-white border border-slate-200 rounded-xl focus:outline-none focus:border-blue-400 text-slate-700 placeholder-slate-400"
          />
        </div>

        {/* Category pills */}
        <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
          {categories.map(cat => {
            const count = cat.key === "all" ? words.length : words.filter(w => w.category === cat.key).length;
            const active = selectedCategory === cat.key;
            return (
              <button
                key={cat.key}
                onClick={() => setSelectedCategory(cat.key)}
                className={`flex items-center gap-1.5 px-3.5 py-2 rounded-full text-xs font-bold whitespace-nowrap transition-all border ${
                  active
                    ? `${cat.color} text-white border-transparent shadow-sm`
                    : "bg-white text-slate-600 border-slate-200 hover:border-slate-300"
                }`}
              >
                {cat.label}
                <span className={`text-[10px] px-1.5 py-0.5 rounded-full font-extrabold ${active ? "bg-white/20" : "bg-slate-100"}`}>
                  {count}
                </span>
              </button>
            );
          })}
        </div>

        {/* Mastered filter + quiz button row */}
        <div className="flex items-center gap-3 flex-wrap">
          <div className="flex rounded-xl border border-slate-200 bg-white overflow-hidden text-xs font-semibold">
            {(["all", "learning", "mastered"] as const).map((f) => (
              <button
                key={f}
                onClick={() => setMasteredFilter(f)}
                className={`px-3 py-2 transition-all ${masteredFilter === f ? "bg-blue-600 text-white" : "text-slate-500 hover:bg-slate-50"}`}
              >
                {f === "all" ? "Tất cả" : f === "learning" ? "Đang học" : "Thành thạo"}
              </button>
            ))}
          </div>
          <span className="text-xs text-slate-400 ml-auto">{filtered.length} từ</span>
        </div>

        {/* Word cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          {filtered.map((word) => (
            <div
              key={word.id}
              className={`rounded-2xl border p-5 transition-all hover:shadow-md ${
                word.mastered
                  ? "border-emerald-200 bg-emerald-50/40"
                  : "border-slate-100 bg-white hover:border-blue-100"
              }`}
            >
              {/* Header */}
              <div className="flex items-start gap-2 mb-3">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className={`text-sm line-through font-medium ${word.mastered ? "text-emerald-400" : "text-slate-400"}`}>
                      {word.original}
                    </span>
                    <ArrowRight className={`w-3.5 h-3.5 shrink-0 ${word.mastered ? "text-emerald-400" : "text-slate-300"}`} />
                    <span className={`text-base font-extrabold ${word.mastered ? "text-emerald-700" : "text-slate-900"}`}>
                      {word.suggested}
                    </span>
                  </div>
                  <div className="flex items-center gap-1.5 mt-1.5 flex-wrap">
                    <span className="text-[10px] px-1.5 py-0.5 bg-blue-50 text-blue-600 rounded font-bold">
                      {typeLabel[word.type]}
                    </span>
                    <span className="text-[10px] px-1.5 py-0.5 bg-slate-100 text-slate-500 rounded font-semibold">
                      {word.category}
                    </span>
                  </div>
                </div>
              </div>

              {/* Synonyms */}
              <div className="flex flex-wrap gap-1 mb-3">
                {word.synonyms.map(s => (
                  <span key={s} className="text-[10px] px-2 py-0.5 bg-slate-50 border border-slate-200 text-slate-500 rounded-full font-medium">
                    {s}
                  </span>
                ))}
              </div>

              {/* Example */}
              <p className="text-xs text-slate-500 italic leading-relaxed mb-4">
                &ldquo;{word.example}&rdquo;
              </p>

              {/* Toggle mastered */}
              <button
                onClick={() => toggleMastered(word.id)}
                className={`flex items-center gap-2 text-xs font-semibold transition-all ${
                  word.mastered
                    ? "text-emerald-600 hover:text-emerald-700"
                    : "text-slate-400 hover:text-blue-600"
                }`}
              >
                {word.mastered
                  ? <CheckCircle2 className="w-4 h-4" />
                  : <Circle className="w-4 h-4" />}
                {word.mastered ? "Đã thành thạo" : "Đánh dấu thành thạo"}
              </button>
            </div>
          ))}

          {filtered.length === 0 && (
            <div className="col-span-2 text-center py-12 text-slate-400">
              <BookOpen className="w-8 h-8 mx-auto mb-3 opacity-40" />
              <p className="text-sm">Không tìm thấy từ nào phù hợp</p>
            </div>
          )}
        </div>
      </div>

      {/* Quiz Modal */}
      {quizOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-slate-900/60 backdrop-blur-sm" onClick={() => !quizDone && setQuizOpen(false)} />
          <div className="relative w-full max-w-md bg-white rounded-3xl shadow-2xl overflow-hidden">

            {quizDone ? (
              /* Score screen */
              <div className="p-8 text-center">
                <div className="w-20 h-20 rounded-2xl bg-violet-100 flex items-center justify-center mx-auto mb-4">
                  <Trophy className="w-9 h-9 text-violet-600" />
                </div>
                <h2 className="text-2xl font-extrabold text-slate-900 mb-1">
                  {score}/{questions.length}
                </h2>
                <p className="text-slate-500 text-sm mb-2">Quiz: {catLabel}</p>
                <p className="text-lg font-bold mb-6">
                  {score === questions.length
                    ? "Hoàn hảo! Xuất sắc 🎉"
                    : score >= questions.length * 0.7
                    ? "Rất tốt! Tiếp tục luyện tập"
                    : "Cần ôn thêm — thử lại nhé!"}
                </p>
                <div className="flex gap-3 justify-center">
                  <button
                    onClick={resetQuiz}
                    className="flex items-center gap-2 px-5 py-2.5 rounded-full border border-slate-200 text-slate-600 text-sm font-bold hover:bg-slate-50 transition-all"
                  >
                    <RotateCcw className="w-4 h-4" />
                    Làm lại
                  </button>
                  <button
                    onClick={() => setQuizOpen(false)}
                    className="px-5 py-2.5 rounded-full bg-violet-600 text-white text-sm font-bold hover:bg-violet-700 transition-all"
                  >
                    Quay về
                  </button>
                </div>
              </div>
            ) : currentQ ? (
              /* Question screen */
              <>
                {/* Header */}
                <div className="flex items-center justify-between px-6 pt-6 pb-4">
                  <div>
                    <p className="text-xs font-extrabold text-slate-400 uppercase tracking-widest mb-0.5">Quiz · {catLabel}</p>
                    <p className="text-xs text-slate-500">{qIndex + 1} / {questions.length}</p>
                  </div>
                  <button onClick={() => setQuizOpen(false)} className="p-1.5 rounded-full hover:bg-slate-100 transition-all">
                    <X className="w-4 h-4 text-slate-400" />
                  </button>
                </div>

                {/* Progress bar */}
                <div className="mx-6 h-1.5 bg-slate-100 rounded-full overflow-hidden mb-6">
                  <div
                    className="h-full bg-violet-500 rounded-full transition-all"
                    style={{ width: `${((qIndex + 1) / questions.length) * 100}%` }}
                  />
                </div>

                {/* Question */}
                <div className="px-6 pb-4">
                  <p className="text-xs text-slate-500 mb-2 font-semibold">Từ học thuật của từ này là gì?</p>
                  <div className="bg-slate-50 rounded-2xl px-5 py-4 mb-6 text-center">
                    <span className="text-2xl font-extrabold text-slate-900">{currentQ.word.original}</span>
                    <span className="ml-3 text-[11px] text-slate-400 font-semibold">{typeLabel[currentQ.word.type]}</span>
                  </div>

                  {/* Options */}
                  <div className="space-y-2.5">
                    {currentQ.options.map((opt, idx) => {
                      let cls = "border-slate-200 bg-white text-slate-700 hover:border-violet-300 hover:bg-violet-50";
                      if (picked !== null) {
                        if (idx === currentQ.correctIdx) cls = "border-emerald-400 bg-emerald-50 text-emerald-800";
                        else if (idx === picked) cls = "border-red-400 bg-red-50 text-red-700";
                        else cls = "border-slate-100 bg-slate-50 text-slate-400";
                      }
                      return (
                        <button
                          key={idx}
                          onClick={() => handlePick(idx)}
                          className={`w-full text-left px-4 py-3 rounded-xl border text-sm font-semibold transition-all ${cls}`}
                        >
                          {opt}
                        </button>
                      );
                    })}
                  </div>

                  {/* Explanation after answer */}
                  {picked !== null && (
                    <div className={`mt-4 p-3 rounded-xl text-xs leading-relaxed ${
                      picked === currentQ.correctIdx
                        ? "bg-emerald-50 border border-emerald-200 text-emerald-700"
                        : "bg-red-50 border border-red-200 text-red-700"
                    }`}>
                      {picked === currentQ.correctIdx
                        ? `Đúng! "${currentQ.word.suggested}" là từ học thuật phù hợp.`
                        : `Sai. Đáp án đúng là "${currentQ.word.suggested}". ${currentQ.word.synonyms.length ? `Synonyms: ${currentQ.word.synonyms.join(", ")}.` : ""}`}
                    </div>
                  )}
                </div>

                {/* Footer */}
                <div className="px-6 pb-6 mt-2">
                  <button
                    onClick={handleNext}
                    disabled={picked === null}
                    className="w-full py-3 rounded-full bg-violet-600 text-white text-sm font-bold hover:bg-violet-700 transition-all disabled:opacity-30 disabled:cursor-not-allowed"
                  >
                    {qIndex + 1 >= questions.length ? "Xem kết quả" : "Câu tiếp theo →"}
                  </button>
                </div>
              </>
            ) : null}
          </div>
        </div>
      )}
    </AppShell>
  );
}
