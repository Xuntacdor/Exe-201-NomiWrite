"use client";

import { useState } from "react";
import AppShell from "../components/AppShell";
import {
  Check, X, ArrowRight, BrainCircuit, Trophy, RotateCcw,
  BookOpen, Type, AlignLeft, AlertTriangle, Zap, Clock, BookMarked, Home,
} from "lucide-react";

interface Question {
  id: number;
  category: string;
  question: string;
  sentence: string;
  options: string[];
  correctIndex: number;
  explanation: string;
}

const allQuestions: Question[] = [
  // ── Mạo từ ──────────────────────────────────────────────
  {
    id: 1, category: "Mạo từ",
    question: "Chọn mạo từ đúng:",
    sentence: "______ environment is a shared responsibility of every citizen.",
    options: ["A", "An", "The", "—"],
    correctIndex: 2,
    explanation: "'Environment' ở đây chỉ môi trường của chúng ta — một khái niệm cụ thể mà cả người nói và người nghe đều biết → dùng 'the'.",
  },
  {
    id: 2, category: "Mạo từ",
    question: "Chọn mạo từ đúng:",
    sentence: "She is ______ honest person who always tells the truth.",
    options: ["a", "an", "the", "—"],
    correctIndex: 1,
    explanation: "'Honest' bắt đầu bằng âm nguyên âm /ɒ/ (chữ h câm) → dùng 'an', không phải 'a'.",
  },
  {
    id: 3, category: "Mạo từ",
    question: "Chọn mạo từ đúng:",
    sentence: "He plans to become ______ engineer working in renewable energy.",
    options: ["a", "an", "the", "—"],
    correctIndex: 1,
    explanation: "'Engineer' bắt đầu bằng âm nguyên âm /ɛ/ → dùng 'an engineer'.",
  },
  {
    id: 4, category: "Mạo từ",
    question: "Chọn mạo từ đúng:",
    sentence: "______ internet has fundamentally transformed global communication.",
    options: ["A", "An", "The", "—"],
    correctIndex: 2,
    explanation: "'The internet' — đây là danh từ chỉ vật duy nhất trên thế giới, luôn dùng 'the'.",
  },
  {
    id: 5, category: "Mạo từ",
    question: "Chọn mạo từ đúng:",
    sentence: "There has been ______ significant increase in renewable energy adoption.",
    options: ["a", "an", "the", "—"],
    correctIndex: 0,
    explanation: "'Significant' bắt đầu bằng phụ âm /s/ → dùng 'a significant increase'.",
  },
  {
    id: 6, category: "Mạo từ",
    question: "Chọn mạo từ đúng:",
    sentence: "In ______ long run, investing in education yields the greatest returns.",
    options: ["a", "an", "the", "—"],
    correctIndex: 2,
    explanation: "'In the long run' là cụm cố định — luôn dùng 'the'.",
  },
  {
    id: 7, category: "Mạo từ",
    question: "Chọn mạo từ đúng:",
    sentence: "The government has introduced ______ new policy to reduce carbon emissions.",
    options: ["a", "an", "the", "—"],
    correctIndex: 0,
    explanation: "Đây là lần đầu đề cập chính sách này, chưa xác định cụ thể → dùng 'a new policy'.",
  },
  {
    id: 8, category: "Mạo từ",
    question: "Chọn mạo từ đúng:",
    sentence: "______ poverty is a complex issue that requires coordinated global action.",
    options: ["A", "An", "The", "—"],
    correctIndex: 3,
    explanation: "Khi dùng danh từ trừu tượng theo nghĩa chung (poverty nói chung), không dùng mạo từ — còn gọi là 'zero article'.",
  },

  // ── Chia thì ─────────────────────────────────────────────
  {
    id: 9, category: "Chia thì",
    question: "Chọn dạng động từ đúng:",
    sentence: "Over the past decade, smartphones ______ how we communicate.",
    options: ["has changed", "have changed", "changed", "are changing"],
    correctIndex: 1,
    explanation: "'Smartphones' là danh từ số nhiều → phải dùng 'have changed'. 'Has changed' chỉ dùng cho số ít.",
  },
  {
    id: 10, category: "Chia thì",
    question: "Chọn dạng động từ đúng:",
    sentence: "Since 2010, global temperatures ______ by more than 1.2 degrees Celsius.",
    options: ["rise", "rose", "have risen", "had risen"],
    correctIndex: 2,
    explanation: "'Since 2010' là dấu hiệu của Present Perfect → dùng 'have risen'. 'Rose' là Simple Past.",
  },
  {
    id: 11, category: "Chia thì",
    question: "Chọn dạng động từ đúng:",
    sentence: "The government ______ a comprehensive climate strategy last year.",
    options: ["introduces", "introduced", "has introduced", "was introducing"],
    correctIndex: 1,
    explanation: "'Last year' chỉ thời điểm cụ thể trong quá khứ → dùng Simple Past 'introduced'.",
  },
  {
    id: 12, category: "Chia thì",
    question: "Chọn dạng động từ đúng:",
    sentence: "Research consistently ______ that regular exercise reduces the risk of heart disease.",
    options: ["show", "shows", "showed", "is showing"],
    correctIndex: 1,
    explanation: "Dùng Simple Present 'shows' cho sự thật chung/kết quả nghiên cứu tổng quát. 'Research' là danh từ số ít → 'shows'.",
  },
  {
    id: 13, category: "Chia thì",
    question: "Chọn dạng động từ đúng:",
    sentence: "While scientists ______ the data, they discovered an unexpected anomaly.",
    options: ["analysed", "were analysing", "have analysed", "analyse"],
    correctIndex: 1,
    explanation: "Hành động đang xảy ra khi một hành động khác xảy đến → Past Continuous 'were analysing'.",
  },
  {
    id: 14, category: "Chia thì",
    question: "Chọn dạng động từ đúng:",
    sentence: "Scientists ______ a solution to ocean plastic pollution for decades.",
    options: ["seek", "seeked", "are seeking", "have been seeking"],
    correctIndex: 3,
    explanation: "Hành động bắt đầu trong quá khứ và vẫn đang tiếp diễn → Present Perfect Continuous 'have been seeking'.",
  },
  {
    id: 15, category: "Chia thì",
    question: "Chọn dạng động từ đúng:",
    sentence: "If governments ______ act immediately, the consequences will be irreversible.",
    options: ["does not", "do not", "will not", "did not"],
    correctIndex: 1,
    explanation: "Câu điều kiện loại 1 (real condition): If + Simple Present → will + V. 'Governments' là số nhiều → 'do not'.",
  },
  {
    id: 16, category: "Chia thì",
    question: "Chọn dạng động từ đúng:",
    sentence: "By 2050, renewable energy ______ for over 80% of global electricity production.",
    options: ["accounts", "will account", "has accounted", "accounted"],
    correctIndex: 1,
    explanation: "'By 2050' là mốc tương lai → dùng Future Simple 'will account' hoặc Future Perfect 'will have accounted'.",
  },
  {
    id: 17, category: "Chia thì",
    question: "Chọn dạng động từ đúng:",
    sentence: "The policy had already been implemented before the new government ______ to power.",
    options: ["comes", "came", "had come", "has come"],
    correctIndex: 1,
    explanation: "Hai hành động trong quá khứ: hành động xảy ra trước dùng Past Perfect ('had been implemented'), sau dùng Simple Past 'came'.",
  },

  // ── Collocation ──────────────────────────────────────────
  {
    id: 18, category: "Collocation",
    question: "Chọn collocation đúng:",
    sentence: "We need to ______ efforts to combat climate change effectively.",
    options: ["do", "make", "take", "give"],
    correctIndex: 1,
    explanation: "'Make efforts' là collocation chuẩn. 'Do efforts' không tồn tại trong tiếng Anh.",
  },
  {
    id: 19, category: "Collocation",
    question: "Chọn collocation đúng:",
    sentence: "The new legislation will ______ a significant impact on small businesses.",
    options: ["do", "make", "have", "give"],
    correctIndex: 2,
    explanation: "'Have an impact' là collocation đúng. 'Make an impact' cũng có thể dùng nhưng 'have' phổ biến hơn trong văn học thuật.",
  },
  {
    id: 20, category: "Collocation",
    question: "Chọn collocation đúng:",
    sentence: "The research team ______ a comprehensive study on the effects of social media.",
    options: ["made", "conducted", "did", "performed"],
    correctIndex: 1,
    explanation: "'Conduct a study/research' là collocation chuẩn trong academic writing. 'Do a study' informal hơn.",
  },
  {
    id: 21, category: "Collocation",
    question: "Chọn collocation đúng:",
    sentence: "Policymakers need to ______ immediate action to address the housing crisis.",
    options: ["make", "do", "take", "have"],
    correctIndex: 2,
    explanation: "'Take action' là collocation cố định. 'Make action' hay 'do action' đều sai.",
  },
  {
    id: 22, category: "Collocation",
    question: "Chọn collocation đúng:",
    sentence: "These educational reforms ______ a crucial role in shaping future generations.",
    options: ["make", "do", "play", "have"],
    correctIndex: 2,
    explanation: "'Play a role' là collocation chuẩn. 'Have a role' cũng dùng được nhưng 'play a role' phổ biến hơn.",
  },
  {
    id: 23, category: "Collocation",
    question: "Chọn collocation đúng:",
    sentence: "The company plans to ______ a new strategy to increase market share.",
    options: ["make", "implement", "do", "bring"],
    correctIndex: 1,
    explanation: "'Implement a strategy' là collocation chính xác trong business/academic writing.",
  },
  {
    id: 24, category: "Collocation",
    question: "Chọn collocation đúng:",
    sentence: "The committee will ______ a final decision on the proposal next week.",
    options: ["do", "make", "take", "reach"],
    correctIndex: 3,
    explanation: "'Reach a decision' và 'make a decision' đều đúng, nhưng 'reach a decision' nhấn mạnh quá trình thảo luận hơn. 'Take a decision' (British English) cũng chấp nhận được.",
  },
  {
    id: 25, category: "Collocation",
    question: "Chọn collocation đúng:",
    sentence: "The athlete ______ a world record at the championship last month.",
    options: ["did", "achieved", "made", "reached"],
    correctIndex: 1,
    explanation: "'Achieve a record' hoặc 'break a record' — không nói 'do a record' hay 'reach a record'.",
  },

  // ── Từ vựng học thuật ────────────────────────────────────
  {
    id: 26, category: "Từ vựng học thuật",
    question: "Chọn từ học thuật phù hợp nhất thay thế từ gạch chân:",
    sentence: "Regular exercise is very [good] for both physical and mental health.",
    options: ["nice", "beneficial", "helpful", "fine"],
    correctIndex: 1,
    explanation: "'Beneficial' là từ học thuật chuẩn thay cho 'good'. 'Nice' và 'fine' quá informal, 'helpful' chưa đủ mạnh.",
  },
  {
    id: 27, category: "Từ vựng học thuật",
    question: "Chọn từ học thuật phù hợp nhất thay thế từ gạch chân:",
    sentence: "The data clearly [shows] a link between poverty and poor health outcomes.",
    options: ["tells", "demonstrates", "presents", "explains"],
    correctIndex: 1,
    explanation: "'Demonstrates' là từ học thuật chuẩn hơn 'shows'. 'Presents' cũng OK nhưng kém mạnh hơn.",
  },
  {
    id: 28, category: "Từ vựng học thuật",
    question: "Chọn từ học thuật phù hợp nhất thay thế từ gạch chân:",
    sentence: "Pollution has a [bad] effect on air quality in urban areas.",
    options: ["bad", "harmful", "detrimental", "negative"],
    correctIndex: 2,
    explanation: "'Detrimental' là từ học thuật cao nhất trong nhóm. 'Harmful' và 'negative' cũng tốt nhưng 'detrimental' phù hợp nhất với văn học thuật.",
  },
  {
    id: 29, category: "Từ vựng học thuật",
    question: "Chọn từ học thuật phù hợp nhất thay thế từ gạch chân:",
    sentence: "The government must [deal with] the issue of rising inequality urgently.",
    options: ["fix", "address", "handle", "solve"],
    correctIndex: 1,
    explanation: "'Address' là từ học thuật chuẩn. 'Fix' và 'handle' quá informal. 'Solve' hơi hẹp nghĩa (chỉ cho vấn đề có đáp án rõ ràng).",
  },
  {
    id: 30, category: "Từ vựng học thuật",
    question: "Chọn từ học thuật phù hợp nhất:",
    sentence: "Technology has the power to ______ the entire healthcare system.",
    options: ["change", "fix", "revolutionise", "improve"],
    correctIndex: 2,
    explanation: "'Revolutionise' (hoặc 'transform') thể hiện sự thay đổi căn bản và mạnh mẽ — phù hợp hơn 'change' hay 'improve' trong văn học thuật.",
  },
  {
    id: 31, category: "Từ vựng học thuật",
    question: "Chọn từ học thuật phù hợp nhất:",
    sentence: "There are ______ studies confirming the link between sleep and academic performance.",
    options: ["many", "a lot of", "numerous", "lots of"],
    correctIndex: 2,
    explanation: "'Numerous' là từ học thuật chuẩn thay thế 'many'. 'A lot of' và 'lots of' quá informal trong academic writing.",
  },
  {
    id: 32, category: "Từ vựng học thuật",
    question: "Chọn từ học thuật phù hợp nhất:",
    sentence: "Evidence ______ that early childhood education has long-term societal benefits.",
    options: ["says", "tells", "suggests", "shows"],
    correctIndex: 2,
    explanation: "'Evidence suggests' là collocation học thuật chuẩn. 'Says/tells' quá informal với chủ ngữ 'evidence'.",
  },
  {
    id: 33, category: "Từ vựng học thuật",
    question: "Chọn từ học thuật phù hợp nhất:",
    sentence: "Digital technology can ______ communication across geographical boundaries.",
    options: ["help", "make easier", "facilitate", "improve"],
    correctIndex: 2,
    explanation: "'Facilitate' là từ học thuật chính xác nghĩa 'làm cho dễ dàng hơn/cho phép xảy ra'. 'Help' và 'improve' ít chính xác hơn.",
  },

  // ── Cấu trúc câu ────────────────────────────────────────
  {
    id: 34, category: "Cấu trúc câu",
    question: "Chọn cấu trúc đúng:",
    sentence: "Despite ______ many obstacles, the research team completed the project on time.",
    options: ["face", "faces", "facing", "to face"],
    correctIndex: 2,
    explanation: "Sau 'despite' (giới từ) dùng V-ing: 'Despite facing many obstacles'. Không dùng động từ nguyên mẫu sau giới từ.",
  },
  {
    id: 35, category: "Cấu trúc câu",
    question: "Chọn cấu trúc đúng:",
    sentence: "It is essential that every student ______ the requirements before graduating.",
    options: ["meets", "meet", "has met", "to meet"],
    correctIndex: 1,
    explanation: "Sau 'It is essential/important/necessary that' dùng bare infinitive (subjunctive): 'meet', không phải 'meets'.",
  },
  {
    id: 36, category: "Cấu trúc câu",
    question: "Chọn cấu trúc đúng:",
    sentence: "Not only ______ the cost of production, but they also reduced waste significantly.",
    options: ["they reduced", "did they reduce", "they did reduce", "reducing"],
    correctIndex: 1,
    explanation: "Sau 'Not only' ở đầu câu, cần đảo ngữ: Not only + auxiliary + subject + verb. → 'did they reduce'.",
  },
  {
    id: 37, category: "Cấu trúc câu",
    question: "Chọn cấu trúc đúng:",
    sentence: "Neither the additional funding nor the new policies ______ been effective so far.",
    options: ["has", "have", "was", "were"],
    correctIndex: 1,
    explanation: "Quy tắc 'neither...nor': động từ chia theo danh từ gần nhất. 'Policies' (số nhiều) là danh từ gần động từ nhất → dùng 'have'.",
  },
  {
    id: 38, category: "Cấu trúc câu",
    question: "Chọn cấu trúc đúng:",
    sentence: "The government announced that new measures ______ implemented the following year.",
    options: ["will be", "would be", "are", "were"],
    correctIndex: 1,
    explanation: "Trong reported speech (câu tường thuật) với 'announced', thì tương lai 'will' chuyển thành 'would'. → 'would be implemented'.",
  },
  {
    id: 39, category: "Cấu trúc câu",
    question: "Chọn cấu trúc đúng:",
    sentence: "______ significant progress in technology, many routine jobs are at risk of automation.",
    options: ["Because of", "Despite", "Due to", "Given"],
    correctIndex: 3,
    explanation: "'Given + noun phrase' có nghĩa 'considering/in light of' — phù hợp nhất ở đây. 'Despite' trái nghĩa. 'Because of / Due to' đúng ngữ nghĩa nhưng 'Given' tự nhiên hơn trong academic writing.",
  },

  // ── Lỗi phổ biến ────────────────────────────────────────
  {
    id: 40, category: "Lỗi phổ biến",
    question: "Tìm lỗi sai và chọn cách sửa đúng:",
    sentence: "We must take into account ______ economic consequences of this policy.",
    options: ["a", "an", "the", "—"],
    correctIndex: 2,
    explanation: "'The economic consequences of this policy' — vì 'of this policy' đã xác định cụ thể nhóm hậu quả nào → dùng 'the'.",
  },
  {
    id: 41, category: "Lỗi phổ biến",
    question: "Chọn giới từ đúng:",
    sentence: "The report consists ______ three main sections and an appendix.",
    options: ["of", "with", "in", "from"],
    correctIndex: 0,
    explanation: "'Consist of' là phrasal verb cố định. Không nói 'consist with/in/from'.",
  },
  {
    id: 42, category: "Lỗi phổ biến",
    question: "Chọn giới từ đúng:",
    sentence: "Many students are interested ______ pursuing careers in environmental science.",
    options: ["on", "about", "in", "for"],
    correctIndex: 2,
    explanation: "'Interested in' là cụm tính từ cố định. 'Interested about' không đúng trong tiếng Anh chuẩn.",
  },
  {
    id: 43, category: "Lỗi phổ biến",
    question: "Câu nào ĐÚNG ngữ pháp?",
    sentence: "Chọn câu viết đúng về 'information':",
    options: [
      "There are many informations online.",
      "There is much information online.",
      "There are a lot of informations online.",
      "There is many information online.",
    ],
    correctIndex: 1,
    explanation: "'Information' là uncountable noun — không có số nhiều 'informations'. Dùng 'much information' hoặc 'a lot of information' (không có 's').",
  },
  {
    id: 44, category: "Lỗi phổ biến",
    question: "Chọn câu ĐÚNG về 'advice':",
    sentence: "Câu nào viết đúng?",
    options: [
      "She gave me a good advice.",
      "She gave me some good advices.",
      "She gave me some good advice.",
      "She gave me good advices.",
    ],
    correctIndex: 2,
    explanation: "'Advice' là uncountable noun — không có 'an advice' hay 'advices'. Dùng 'some advice / a piece of advice'.",
  },
];

const categoryDefs = [
  { key: "all",          label: "Tất cả",           icon: Zap,          gradient: "from-violet-500 to-purple-600", bg: "bg-violet-50", text: "text-violet-700", badge: "bg-violet-100 text-violet-700" },
  { key: "Mạo từ",       label: "Mạo từ (Articles)", icon: Type,         gradient: "from-blue-500 to-indigo-600",  bg: "bg-blue-50",   text: "text-blue-700",   badge: "bg-blue-100 text-blue-700"   },
  { key: "Chia thì",     label: "Chia thì động từ",  icon: Clock,        gradient: "from-violet-500 to-fuchsia-600", bg: "bg-violet-50", text: "text-violet-700", badge: "bg-violet-100 text-violet-700" },
  { key: "Collocation",  label: "Collocation",       icon: AlignLeft,    gradient: "from-emerald-500 to-teal-600", bg: "bg-emerald-50",text: "text-emerald-700",badge: "bg-emerald-100 text-emerald-700"},
  { key: "Từ vựng học thuật", label: "Từ vựng học thuật", icon: BookOpen, gradient: "from-orange-500 to-amber-500", bg: "bg-orange-50", text: "text-orange-700", badge: "bg-orange-100 text-orange-700" },
  { key: "Cấu trúc câu", label: "Cấu trúc câu",      icon: BookMarked,   gradient: "from-rose-500 to-pink-600",   bg: "bg-rose-50",   text: "text-rose-700",   badge: "bg-rose-100 text-rose-700"   },
  { key: "Lỗi phổ biến", label: "Lỗi phổ biến",      icon: AlertTriangle,gradient: "from-amber-500 to-orange-600",bg: "bg-amber-50",  text: "text-amber-700",  badge: "bg-amber-100 text-amber-700" },
];

type Mode = "home" | "quiz" | "result";
type AnswerMap = Record<number, number>;

function shuffle<T>(arr: T[]): T[] {
  return [...arr].sort(() => Math.random() - 0.5);
}

export default function QuizPage() {
  const [mode, setMode] = useState<Mode>("home");
  const [categoryKey, setCategoryKey] = useState("all");
  const [quizQuestions, setQuizQuestions] = useState<Question[]>([]);
  const [current, setCurrent] = useState(0);
  const [answers, setAnswers] = useState<AnswerMap>({});
  const [showExplanation, setShowExplanation] = useState(false);

  const startQuiz = (key: string) => {
    const pool = key === "all" ? allQuestions : allQuestions.filter(q => q.category === key);
    setCategoryKey(key);
    setQuizQuestions(shuffle(pool));
    setCurrent(0);
    setAnswers({});
    setShowExplanation(false);
    setMode("quiz");
  };

  const goHome = () => setMode("home");

  // ── Home screen ──────────────────────────────────────────
  if (mode === "home") {
    return (
      <AppShell activePath="/quiz">
        <div className="sticky top-0 z-10 bg-white/90 backdrop-blur border-b border-slate-100 px-6 h-14 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <BrainCircuit className="w-4 h-4 text-violet-500" />
            <h1 className="text-sm font-extrabold text-slate-900">Quiz ôn tập ngữ pháp</h1>
          </div>
          <span className="text-xs text-slate-400 font-semibold">{allQuestions.length} câu · {categoryDefs.length - 1} chủ đề</span>
        </div>

        <div className="p-6 w-full space-y-5 max-w-2xl mx-auto">
          {/* Hero */}
          <div className="relative rounded-2xl overflow-hidden">
            <div className="absolute inset-0 bg-gradient-to-br from-violet-600 to-purple-700" />
            <div className="relative p-6 text-white">
              <p className="text-xs font-extrabold text-violet-200 uppercase tracking-widest mb-1">AI Grammar Quiz</p>
              <h2 className="text-xl font-extrabold mb-1">Chọn chủ đề để ôn tập</h2>
              <p className="text-sm text-violet-100">Mỗi chủ đề tập trung vào 1 điểm ngữ pháp — làm từng phần để nắm chắc hơn làm tổng hợp.</p>
            </div>
          </div>

          {/* "Làm tất cả" */}
          <button
            onClick={() => startQuiz("all")}
            className="w-full flex items-center justify-between px-5 py-4 rounded-2xl bg-gradient-to-r from-violet-500 to-purple-600 text-white shadow-md shadow-violet-200 hover:shadow-lg hover:-translate-y-0.5 transition-all"
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-white/15 flex items-center justify-center">
                <Zap className="w-5 h-5" />
              </div>
              <div className="text-left">
                <p className="text-sm font-extrabold">Làm tất cả</p>
                <p className="text-xs text-violet-200">{allQuestions.length} câu · Tổng hợp mọi chủ đề</p>
              </div>
            </div>
            <ArrowRight className="w-5 h-5 text-violet-200" />
          </button>

          {/* Category grid */}
          <p className="text-[10px] font-extrabold text-slate-400 uppercase tracking-widest">Hoặc chọn theo chủ đề</p>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {categoryDefs.slice(1).map(cat => {
              const count = allQuestions.filter(q => q.category === cat.key).length;
              const Icon = cat.icon;
              return (
                <button
                  key={cat.key}
                  onClick={() => startQuiz(cat.key)}
                  className="flex items-center gap-3 px-4 py-4 rounded-2xl bg-white border border-slate-100 hover:border-slate-200 hover:shadow-md transition-all text-left group"
                >
                  <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${cat.gradient} flex items-center justify-center shrink-0 shadow-sm`}>
                    <Icon className="w-5 h-5 text-white" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-extrabold text-slate-900">{cat.label}</p>
                    <p className="text-xs text-slate-500">{count} câu hỏi</p>
                  </div>
                  <ArrowRight className="w-4 h-4 text-slate-300 group-hover:text-slate-500 group-hover:translate-x-0.5 transition-all shrink-0" />
                </button>
              );
            })}
          </div>
        </div>
      </AppShell>
    );
  }

  // ── Quiz & Result shared state ───────────────────────────
  const q = quizQuestions[current];
  const selected = q ? answers[q.id] : undefined;
  const isAnswered = selected !== undefined;
  const isCorrect = isAnswered && selected === q?.correctIndex;
  const score = quizQuestions.filter(q => answers[q.id] === q.correctIndex).length;
  const catDef = categoryDefs.find(c => c.key === categoryKey) ?? categoryDefs[0];
  const CatIcon = catDef.icon;

  const handleSelect = (idx: number) => {
    if (isAnswered || !q) return;
    setAnswers(prev => ({ ...prev, [q.id]: idx }));
    setShowExplanation(true);
  };

  const handleNext = () => {
    if (current < quizQuestions.length - 1) {
      setCurrent(i => i + 1);
      setShowExplanation(false);
    } else {
      setMode("result");
    }
  };

  // ── Result screen ────────────────────────────────────────
  if (mode === "result") {
    const pct = Math.round((score / quizQuestions.length) * 100);
    return (
      <AppShell activePath="/quiz">
        <div className="sticky top-0 z-10 bg-white/90 backdrop-blur border-b border-slate-100 px-6 h-14 flex items-center gap-2">
          <button onClick={goHome} className="text-slate-400 hover:text-slate-600 transition-colors">
            <Home className="w-4 h-4" />
          </button>
          <span className="text-slate-300">/</span>
          <span className="text-sm font-extrabold text-slate-900">Kết quả — {catDef.label}</span>
        </div>

        <div className="p-6 w-full max-w-md mx-auto space-y-5">
          {/* Score hero */}
          <div className={`rounded-3xl bg-gradient-to-br ${catDef.gradient} p-7 text-white text-center`}>
            <div className="w-20 h-20 rounded-2xl bg-white/15 border border-white/20 flex items-center justify-center mx-auto mb-4">
              <Trophy className="w-9 h-9" />
            </div>
            <p className="text-4xl font-extrabold mb-1">{score}/{quizQuestions.length}</p>
            <p className="text-sm opacity-80 mb-3">{pct}% chính xác · {catDef.label}</p>
            <p className="text-base font-bold">
              {pct === 100 ? "Hoàn hảo! Xuất sắc 🎉"
                : pct >= 75 ? "Rất tốt! Tiếp tục luyện tập"
                : pct >= 50 ? "Tạm ổn — ôn thêm phần sai"
                : "Cần ôn nhiều hơn — thử lại nhé!"}
            </p>
          </div>

          {/* Breakdown */}
          <div className="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
            <p className="px-5 py-3.5 text-xs font-extrabold text-slate-400 uppercase tracking-widest border-b border-slate-50">Chi tiết từng câu</p>
            <div className="divide-y divide-slate-50">
              {quizQuestions.map((q, i) => {
                const ans = answers[q.id];
                const correct = ans === q.correctIndex;
                return (
                  <div key={q.id} className={`flex items-start gap-3 px-5 py-3 ${correct ? "" : "bg-red-50/50"}`}>
                    <div className={`w-5 h-5 rounded-full flex items-center justify-center shrink-0 mt-0.5 ${correct ? "bg-emerald-500" : "bg-red-400"}`}>
                      {correct ? <Check className="w-3 h-3 text-white" /> : <X className="w-3 h-3 text-white" />}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-xs font-semibold text-slate-700">Câu {i + 1} — {q.category}</p>
                      {!correct && (
                        <p className="text-xs text-red-600 mt-0.5">
                          Đáp án đúng: <span className="font-bold">{q.options[q.correctIndex]}</span>
                        </p>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Actions */}
          <div className="flex gap-3">
            <button
              onClick={() => startQuiz(categoryKey)}
              className="flex-1 flex items-center justify-center gap-2 py-3 rounded-full border-2 border-slate-200 text-slate-700 text-sm font-bold hover:bg-slate-50 transition-all"
            >
              <RotateCcw className="w-4 h-4" />
              Làm lại
            </button>
            <button
              onClick={goHome}
              className="flex-1 flex items-center justify-center gap-2 py-3 rounded-full bg-violet-600 text-white text-sm font-bold hover:bg-violet-700 transition-all"
            >
              <Home className="w-4 h-4" />
              Chủ đề khác
            </button>
          </div>
        </div>
      </AppShell>
    );
  }

  // ── Quiz screen ──────────────────────────────────────────
  if (!q) return null;
  const progress = ((current + (isAnswered ? 1 : 0)) / quizQuestions.length) * 100;

  return (
    <AppShell activePath="/quiz">
      {/* Top bar */}
      <div className="sticky top-0 z-10 bg-white/90 backdrop-blur border-b border-slate-100 px-6 h-14 flex items-center gap-3">
        <button onClick={goHome} className="text-slate-400 hover:text-slate-600 transition-colors shrink-0">
          <Home className="w-4 h-4" />
        </button>
        <div className={`flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[10px] font-extrabold ${catDef.badge}`}>
          <CatIcon className="w-3 h-3" />
          {catDef.label}
        </div>
        <div className="flex-1" />
        <span className="text-xs text-slate-500 font-semibold shrink-0">{current + 1} / {quizQuestions.length}</span>
      </div>

      {/* Progress */}
      <div className="h-1 bg-slate-100">
        <div
          className={`h-full bg-gradient-to-r ${catDef.gradient} transition-all duration-500`}
          style={{ width: `${progress}%` }}
        />
      </div>

      <div className="flex items-start justify-center p-6 pt-8 min-h-[calc(100vh-4rem)]">
        <div className="w-full max-w-xl space-y-5">
          {/* Category + count */}
          <div className="flex items-center gap-2">
            <span className={`px-3 py-1 text-xs font-bold rounded-full border ${catDef.badge} ${catDef.bg} border-current`}>
              {q.category}
            </span>
            <span className="text-xs text-slate-400">Câu {current + 1}/{quizQuestions.length}</span>
          </div>

          {/* Question card */}
          <div className="bg-white rounded-2xl border border-slate-100 shadow-sm p-6">
            <p className="text-sm font-semibold text-slate-500 mb-4">{q.question}</p>
            <p className="text-base text-slate-800 leading-relaxed font-medium">
              {q.sentence.split("______").map((part, i, arr) => (
                <span key={i}>
                  {part}
                  {i < arr.length - 1 && (
                    <span className={`inline-block min-w-16 px-2 mx-1 rounded-lg border-b-2 text-center font-bold transition-all ${
                      !isAnswered
                        ? "border-blue-400 bg-blue-50 text-blue-400"
                        : isCorrect
                        ? "border-emerald-500 bg-emerald-50 text-emerald-700"
                        : "border-red-400 bg-red-50 text-red-600"
                    }`}>
                      {isAnswered ? q.options[selected!] : "______"}
                    </span>
                  )}
                </span>
              ))}
            </p>
          </div>

          {/* Options */}
          <div className="grid grid-cols-1 gap-2.5">
            {q.options.map((opt, idx) => {
              let style = "border-slate-200 bg-white text-slate-700 hover:border-violet-400 hover:bg-violet-50";
              if (isAnswered) {
                if (idx === q.correctIndex) style = "border-emerald-500 bg-emerald-50 text-emerald-700";
                else if (idx === selected) style = "border-red-400 bg-red-50 text-red-600";
                else style = "border-slate-100 bg-slate-50 text-slate-400 opacity-60";
              }
              return (
                <button
                  key={idx}
                  onClick={() => handleSelect(idx)}
                  disabled={isAnswered}
                  className={`flex items-center gap-3 px-4 py-3.5 rounded-xl border-2 text-sm font-semibold transition-all ${style} ${!isAnswered ? "cursor-pointer" : "cursor-default"}`}
                >
                  <span className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-extrabold shrink-0 ${
                    isAnswered && idx === q.correctIndex ? "bg-emerald-500 text-white" :
                    isAnswered && idx === selected      ? "bg-red-400 text-white" :
                    "bg-slate-100 text-slate-500"
                  }`}>
                    {isAnswered && idx === q.correctIndex ? <Check className="w-3.5 h-3.5" /> :
                     isAnswered && idx === selected       ? <X className="w-3.5 h-3.5" /> :
                     String.fromCharCode(65 + idx)}
                  </span>
                  {opt}
                </button>
              );
            })}
          </div>

          {/* Explanation */}
          {showExplanation && (
            <div className={`rounded-xl p-4 border ${isCorrect ? "bg-emerald-50 border-emerald-200" : "bg-red-50 border-red-200"}`}>
              <div className="flex items-start gap-2">
                <div className={`w-5 h-5 rounded-full flex items-center justify-center shrink-0 mt-0.5 ${isCorrect ? "bg-emerald-500" : "bg-red-500"}`}>
                  {isCorrect ? <Check className="w-3 h-3 text-white" /> : <X className="w-3 h-3 text-white" />}
                </div>
                <div>
                  <p className={`text-xs font-bold mb-1 ${isCorrect ? "text-emerald-700" : "text-red-700"}`}>
                    {isCorrect ? "Chính xác!" : `Chưa đúng — Đáp án: ${q.options[q.correctIndex]}`}
                  </p>
                  <p className={`text-xs leading-relaxed ${isCorrect ? "text-emerald-600" : "text-red-600"}`}>
                    {q.explanation}
                  </p>
                </div>
              </div>
            </div>
          )}

          {/* Next */}
          {isAnswered && (
            <button
              onClick={handleNext}
              className={`w-full flex items-center justify-center gap-2 py-3.5 rounded-full text-white text-sm font-bold transition-all shadow-lg hover:-translate-y-0.5 bg-gradient-to-r ${catDef.gradient}`}
            >
              {current < quizQuestions.length - 1 ? "Câu tiếp theo" : "Xem kết quả"}
              <ArrowRight className="w-4 h-4" />
            </button>
          )}
        </div>
      </div>
    </AppShell>
  );
}
