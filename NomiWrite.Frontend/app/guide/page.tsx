"use client";

import { useState } from "react";
import AppShell from "../components/AppShell";
import {
  BookOpen, Briefcase, GraduationCap, FileText, Mail, Star,
  ChevronRight, Copy, Check, Lightbulb, AlertTriangle, AlignLeft,
  MessageSquare, Zap, Target, Clock, ArrowRight, X,
} from "lucide-react";

type Tab = "structure" | "connectors" | "tips" | "mistakes";

const types = [
  {
    id: "ielts2", icon: GraduationCap,
    gradient: "from-blue-500 to-indigo-600", gradientLight: "from-blue-50 to-indigo-50",
    accent: "text-blue-600", bg: "bg-blue-50", border: "border-blue-200", pill: "bg-blue-100 text-blue-700",
    tag: "IELTS", label: "IELTS Writing Task 2",
    desc: "Bài luận phân tích quan điểm, thảo luận hai chiều hoặc đề xuất giải pháp cho vấn đề xã hội.",
    minWords: 250, time: "40 phút",
    criteria: [
      { label: "Task Response",        weight: "25%", tip: "Address đầy đủ đề bài, quan điểm rõ ràng và nhất quán" },
      { label: "Coherence & Cohesion", weight: "25%", tip: "Mỗi đoạn có main idea riêng, từ nối đa dạng và logic" },
      { label: "Lexical Resource",     weight: "25%", tip: "Tránh lặp từ, dùng academic vocabulary và collocations" },
      { label: "Grammatical Range",    weight: "25%", tip: "Mix câu đơn/phức, ít lỗi thì/mạo từ/giới từ" },
    ],
    structure: [
      { step: "Introduction", detail: "Paraphrase đề + thesis statement rõ ràng", example: "While some argue that X, I contend that Y is more significant due to A and B." },
      { step: "Body 1",       detail: "Luận điểm chính + evidence + ví dụ cụ thể", example: "The primary reason for this is… For instance, studies have found that…" },
      { step: "Body 2",       detail: "Luận điểm hai hoặc counterargument + refutation", example: "Admittedly, critics argue that… However, this overlooks the fact that…" },
      { step: "Conclusion",   detail: "Restate thesis + tóm tắt ý chính, không có ý mới", example: "In conclusion, although X presents merit, Y remains stronger because…" },
    ],
    connectors: [
      { label: "Mở bài",     phrases: ["It is widely argued that…", "There is growing debate over…", "In recent years, … has become a contentious issue."] },
      { label: "Bổ sung",    phrases: ["Furthermore,…", "Moreover,…", "In addition to this,…", "What is more,…"] },
      { label: "Tương phản", phrases: ["However,…", "On the other hand,…", "In contrast,…", "Admittedly,…", "Nevertheless,…"] },
      { label: "Ví dụ",      phrases: ["For instance,…", "For example,…", "A case in point is…", "This is evident in…"] },
      { label: "Kết luận",   phrases: ["In conclusion,…", "To summarise,…", "Overall, it is clear that…"] },
    ],
    tips: [
      { tip: "Paraphrase hoàn toàn — không copy đề",         why: "Copy nguyên đề không được tính vào word count và bị trừ điểm Lexical Resource." },
      { tip: "Dùng 'I contend / I maintain' thay 'I think'",  why: "'I think' quá informal với IELTS — examiners sẽ đánh giá thấp Lexical Resource." },
      { tip: "Mỗi body paragraph chỉ 1 main idea",            why: "Nhiều ý trong 1 đoạn làm mất điểm Coherence vì thiếu focus và development." },
      { tip: "Aim 280–320 từ, không cần dài hơn",             why: "Bài dài không đồng nghĩa điểm cao — chất lượng luận điểm quan trọng hơn." },
      { tip: "Ít nhất 1 example cụ thể mỗi body paragraph",   why: "Examiners đánh giá 'extended and supported ideas' — example = điểm Task Response." },
    ],
    mistakes: [
      { wrong: "Technology is very important in our modern world today.", fix: "Digital technology has become indispensable to contemporary society.", note: "Tránh 'very + adjective' — dùng từ mạnh hơn trực tiếp." },
      { wrong: "I think this is good because it is helpful for people.", fix: "I contend that this policy is beneficial as it significantly reduces urban unemployment.", note: "Cụ thể hóa: 'good for people' → 'giảm thất nghiệp đô thị'." },
      { wrong: "Firstly,… Secondly,… Thirdly,… Lastly,…", fix: "The primary advantage is… Furthermore,… A further consideration is…", note: "Ordinal connectors bị coi là repetitive — dùng logical connectors thay thế." },
    ],
    bandTips: [
      { band: "Band 6", color: "bg-orange-100 text-orange-700", req: "Có quan điểm, có luận điểm nhưng thiếu development. Từ vựng đủ nhưng lặp lại." },
      { band: "Band 7", color: "bg-blue-100 text-blue-700",     req: "Luận điểm tốt, extended với examples cụ thể. Từ vựng đa dạng, ít lỗi ngữ pháp." },
      { band: "Band 8", color: "bg-emerald-100 text-emerald-700", req: "Lập luận tinh tế, fully developed, vocabulary sophisticated, gần như không lỗi." },
    ],
  },
  {
    id: "ielts1", icon: FileText,
    gradient: "from-violet-500 to-purple-600", gradientLight: "from-violet-50 to-purple-50",
    accent: "text-violet-600", bg: "bg-violet-50", border: "border-violet-200", pill: "bg-violet-100 text-violet-700",
    tag: "IELTS", label: "IELTS Writing Task 1",
    desc: "Mô tả và phân tích dữ liệu từ biểu đồ, bảng biểu, sơ đồ hoặc quy trình.",
    minWords: 150, time: "20 phút",
    criteria: [
      { label: "Task Achievement",     weight: "25%", tip: "Có overview, select key features, không liệt kê mọi số liệu" },
      { label: "Coherence & Cohesion", weight: "25%", tip: "Nhóm dữ liệu theo trend, dùng paragraphs rõ ràng" },
      { label: "Lexical Resource",     weight: "25%", tip: "Paraphrase units & labels, dùng từ miêu tả trend đa dạng" },
      { label: "Grammatical Range",    weight: "25%", tip: "Dùng passive voice, comparative structures đúng" },
    ],
    structure: [
      { step: "Introduction", detail: "Paraphrase câu hỏi — mô tả ngắn gọn biểu đồ là gì", example: "The bar chart compares the proportion of electricity generated from renewable sources in five countries between 2000 and 2020." },
      { step: "Overview",     detail: "2–3 xu hướng nổi bật nhất — KHÔNG có số liệu ở đây", example: "Overall, renewable energy usage rose in all five nations, with Country A showing the most dramatic increase." },
      { step: "Details 1",    detail: "Nhóm dữ liệu cao nhất/thấp nhất + số liệu cụ thể", example: "Country A led the group with 78% in 2020, a rise of 45 percentage points from 2000." },
      { step: "Details 2",    detail: "So sánh, ngoại lệ, biến động đáng chú ý + số liệu", example: "By contrast, Country E showed the smallest increase, from 12% to only 19% over the same period." },
    ],
    connectors: [
      { label: "Giới thiệu", phrases: ["The graph illustrates…", "The chart compares…", "The diagram shows…"] },
      { label: "Overview",   phrases: ["Overall, it can be seen that…", "In general,…", "The most notable trend is…"] },
      { label: "Tăng/Giảm", phrases: ["rose significantly to…", "declined sharply from… to…", "increased by… percentage points", "fell by approximately…"] },
      { label: "So sánh",   phrases: ["By contrast,…", "Similarly,…", "Compared to…", "while… on the other hand,…"] },
      { label: "Đỉnh/Đáy",  phrases: ["peaked at…", "reached its lowest point of…", "remained stable at around…", "fluctuated between… and…"] },
    ],
    tips: [
      { tip: "Overview phải đứng trước Details — luôn luôn",      why: "Không có Overview là lý do phổ biến nhất bị điểm thấp Task Achievement trong Task 1." },
      { tip: "Nhóm data theo xu hướng, không liệt kê từng con số", why: "Liệt kê là mô tả, không phải phân tích — Task 1 yêu cầu 'report main features'." },
      { tip: "Overview không có số liệu",                           why: "Overview là tổng quan xu hướng — số liệu cụ thể thuộc về body paragraphs." },
      { tip: "Dùng passive voice: 'was recorded', 'is shown'",      why: "Passive voice phù hợp hơn với data report và giúp đa dạng grammatical range." },
      { tip: "Không bày tỏ ý kiến cá nhân",                        why: "Task 1 là data description — ý kiến chủ quan bị coi là off-topic và trừ điểm." },
    ],
    mistakes: [
      { wrong: "The graph shows that in 2000, Country A had 33%, Country B had 41%, Country C had 12%…", fix: "Countries A and B showed higher initial figures (33% and 41% respectively), while C and D lagged behind at 12–25%.", note: "Liệt kê tuần tự là lỗi cổ điển nhất — hãy nhóm và so sánh." },
      { wrong: "Overall, there are many changes in the chart.", fix: "Overall, renewable energy usage increased across all five nations, with Country A demonstrating the most significant growth.", note: "Overview phải specific — nêu xu hướng CHÍNH, không vague." },
      { wrong: "I think the increase is due to government policies.", fix: "The data shows a steady upward trend in renewable energy adoption across all five countries.", note: "Task 1 không dùng 'I think' hoặc giải thích nguyên nhân — chỉ mô tả data." },
    ],
    bandTips: [
      { band: "Band 6", color: "bg-orange-100 text-orange-700", req: "Có overview, có details nhưng liệt kê nhiều số liệu, chưa nhóm tốt." },
      { band: "Band 7", color: "bg-blue-100 text-blue-700",     req: "Overview rõ ràng, data được nhóm theo trend, comparisons logic." },
      { band: "Band 8", color: "bg-emerald-100 text-emerald-700", req: "Overview tinh tế, fully analysed, ngôn ngữ đa dạng và chính xác." },
    ],
  },
  {
    id: "vstep", icon: Star,
    gradient: "from-emerald-500 to-teal-600", gradientLight: "from-emerald-50 to-teal-50",
    accent: "text-emerald-600", bg: "bg-emerald-50", border: "border-emerald-200", pill: "bg-emerald-100 text-emerald-700",
    tag: "VSTEP", label: "VSTEP Writing",
    desc: "Bài luận tiếng Anh học thuật theo định dạng kỳ thi VSTEP của Việt Nam.",
    minWords: 200, time: "35 phút",
    criteria: [
      { label: "Nội dung & Ý tưởng", weight: "40%", tip: "Thesis rõ, luận điểm đủ và relevant, có dẫn chứng" },
      { label: "Tổ chức bài viết",   weight: "25%", tip: "Paragraphs rõ ràng, từ nối logic, introduction/conclusion hoàn chỉnh" },
      { label: "Từ vựng",            weight: "20%", tip: "Đa dạng, chính xác, collocation đúng" },
      { label: "Ngữ pháp",           weight: "15%", tip: "Cấu trúc câu đa dạng, ít lỗi cơ bản" },
    ],
    structure: [
      { step: "Introduction", detail: "Hook → background → thesis statement trực tiếp", example: "In an era where… it is essential to consider… This essay argues that…" },
      { step: "Body 1",       detail: "Luận điểm chính + dẫn chứng + phân tích link về thesis", example: "To begin with, … This is evident in the fact that… Consequently, this supports the argument that…" },
      { step: "Body 2",       detail: "Luận điểm phụ + dẫn chứng thực tế", example: "Furthermore, … A clear example of this can be seen in… This demonstrates that…" },
      { step: "Conclusion",   detail: "Restate thesis (paraphrase) + tổng kết", example: "In conclusion, this essay has demonstrated that… It is therefore recommended that…" },
    ],
    connectors: [
      { label: "Mở đầu",     phrases: ["To begin with,…", "First and foremost,…", "It is undeniable that…"] },
      { label: "Bổ sung",    phrases: ["In addition,…", "Furthermore,…", "Apart from this,…", "Another key point is…"] },
      { label: "Nhân quả",   phrases: ["As a result,…", "Therefore,…", "Consequently,…", "This leads to…"] },
      { label: "Tương phản", phrases: ["On the other hand,…", "However,…", "In contrast,…", "Despite this,…"] },
      { label: "Kết luận",   phrases: ["To summarise,…", "In conclusion,…", "All things considered,…"] },
    ],
    tips: [
      { tip: "Thesis statement phải xuất hiện ở cuối Introduction",  why: "Examiner VSTEP đọc thesis trước — nếu không rõ sẽ mất điểm Tổ chức ngay từ đầu." },
      { tip: "Mỗi luận điểm cần có ví dụ thực tế cụ thể",           why: "VSTEP đánh giá 40% vào nội dung — ví dụ chung chung bị trừ điểm." },
      { tip: "Kết luận không bao giờ nêu ý mới",                      why: "Ý mới trong conclusion cho thấy bài thiếu lập kế hoạch — mất điểm Tổ chức." },
      { tip: "Aim 220–270 từ là ideal",                               why: "Quá ngắn (dưới 200) rõ ràng mất điểm. Quá dài (400+) thường có nhiều lỗi hơn." },
    ],
    mistakes: [
      { wrong: "Technology is good. It helps people. People like it.", fix: "Technology has significantly improved quality of life by enabling instant global communication and access to knowledge.", note: "Câu ngắn liên tiếp = choppy writing — nối bằng complex sentences." },
      { wrong: "In conclusion, I think we should also consider the economic impact...", fix: "In conclusion, this essay has argued that technology, while disruptive, brings net benefits through enhanced connectivity and education.", note: "Ý mới trong conclusion là lỗi cấu trúc nghiêm trọng." },
      { wrong: "This is very important and very helpful for society today.", fix: "This proves particularly significant for contemporary societies striving to balance growth with sustainability.", note: "Tránh 'very + adjective' — chọn từ mạnh hơn trực tiếp." },
    ],
    bandTips: [
      { band: "Level 3", color: "bg-orange-100 text-orange-700", req: "Có thesis, có luận điểm nhưng ví dụ còn chung chung, từ nối đơn giản." },
      { band: "Level 4", color: "bg-blue-100 text-blue-700",     req: "Luận điểm tốt, ví dụ cụ thể, từ vựng đa dạng, ít lỗi ngữ pháp cơ bản." },
      { band: "Level 5", color: "bg-emerald-100 text-emerald-700", req: "Fully developed, academic vocabulary, sophisticated structure, near error-free." },
    ],
  },
  {
    id: "email", icon: Mail,
    gradient: "from-orange-500 to-amber-500", gradientLight: "from-orange-50 to-amber-50",
    accent: "text-orange-600", bg: "bg-orange-50", border: "border-orange-200", pill: "bg-orange-100 text-orange-700",
    tag: "Workplace", label: "Email công việc",
    desc: "Email tiếng Anh chuyên nghiệp dùng trong môi trường công sở: xin việc, đề xuất, phản hồi.",
    minWords: 100, time: "10–15 phút",
    criteria: [
      { label: "Mục đích rõ ràng",   weight: "30%", tip: "Người đọc hiểu ngay bạn cần gì sau câu đầu tiên" },
      { label: "Tone & Register",    weight: "30%", tip: "Formal/semi-formal phù hợp với đối tượng nhận" },
      { label: "Cấu trúc & Format",  weight: "25%", tip: "Subject rõ, greeting đúng, CTA cụ thể, sign-off phù hợp" },
      { label: "Từ vựng & Ngữ pháp", weight: "15%", tip: "Chính xác, lịch sự, không lỗi typo cơ bản" },
    ],
    structure: [
      { step: "Subject line",   detail: "Ngắn gọn, nêu đúng mục đích: 'Request for…', 'Follow-up: [dự án]'", example: "Subject: Interview Request — Marketing Executive Position" },
      { step: "Greeting",       detail: "'Dear Mr./Ms. [Họ],' (formal) hoặc 'Hi [Tên],' (semi-formal)", example: "Dear Ms. Nguyen," },
      { step: "Opening line",   detail: "Nêu mục đích NGAY, không dài dòng", example: "I am writing to express my interest in the Marketing Executive position advertised on your website." },
      { step: "Body",           detail: "Thông tin chi tiết, ngắn gọn — dùng bullet points nếu có nhiều hơn 3 items", example: "Please find enclosed my CV and portfolio. I would be happy to provide any additional information." },
      { step: "Call to action", detail: "Hành động cụ thể bạn muốn người nhận làm, kèm deadline nếu có", example: "Could you please confirm your availability for a brief call next week?" },
      { step: "Sign-off",       detail: "'Best regards,' / 'Sincerely,' + Họ tên", example: "Best regards, Minh Quang Nguyen" },
    ],
    connectors: [
      { label: "Mở đầu email", phrases: ["I am writing to…", "I hope this email finds you well.", "Further to our conversation,…", "With reference to…"] },
      { label: "Nội dung",     phrases: ["Please find attached…", "I would like to…", "Could you please…", "I would be grateful if…"] },
      { label: "Yêu cầu",      phrases: ["I would appreciate it if…", "Would it be possible to…", "Please do not hesitate to contact me if…"] },
      { label: "Kết thúc",     phrases: ["I look forward to hearing from you.", "Thank you for your time and consideration.", "Please let me know if you have any questions."] },
    ],
    tips: [
      { tip: "Subject line: người đọc phải hiểu nội dung mà không cần mở email", why: "Busy professionals quyết định ưu tiên email dựa trên subject — subject mơ hồ = email bị bỏ qua." },
      { tip: "Một email = một mục đích duy nhất",                                  why: "Email nhiều chủ đề thường không được xử lý đầy đủ — người nhận xử lý phần dễ nhất rồi bỏ qua phần còn lại." },
      { tip: "Dùng 'would' thay 'will' để lịch sự hơn",                            why: "'Will you send me' nghe như mệnh lệnh — 'Would you please send me' tạo tone respectful hơn rõ rệt." },
      { tip: "CTA phải cụ thể: ai làm gì, khi nào",                                why: "CTA mơ hồ như 'Let me know' không tạo được sense of urgency và thường bị trễ deadline." },
    ],
    mistakes: [
      { wrong: "Hi, I want to ask about the job. Can you help me? Thanks.", fix: "Dear Ms. Johnson, I am writing to enquire about the Marketing Executive vacancy. Could you please share details regarding the application process? Best regards, Minh Quang", note: "Thiếu context, tên, và closing chuyên nghiệp." },
      { wrong: "Subject: Hello / Subject: Important / Subject: (trống)", fix: "Subject: Job Application — Marketing Executive | ABC Company", note: "Subject phải đủ thông tin để nhận ra ngay không cần mở email." },
      { wrong: "I will need the report by tomorrow or else the project will fail.", fix: "Could you please send the report by end of day tomorrow? The project timeline depends on this milestone.", note: "Tone aggressive không phù hợp workplace — diễn đạt urgency mà vẫn respectful." },
    ],
    bandTips: [
      { band: "Cơ bản",        color: "bg-orange-100 text-orange-700", req: "Có đủ các phần, nhưng tone chưa phù hợp, CTA còn mơ hồ." },
      { band: "Chuyên nghiệp", color: "bg-blue-100 text-blue-700",     req: "Subject rõ, tone phù hợp, CTA cụ thể, format đẹp." },
      { band: "Xuất sắc",      color: "bg-emerald-100 text-emerald-700", req: "Mỗi câu có mục đích, reader-centric, persuasive mà không aggressive." },
    ],
  },
  {
    id: "cover", icon: Briefcase,
    gradient: "from-pink-500 to-rose-600", gradientLight: "from-pink-50 to-rose-50",
    accent: "text-pink-600", bg: "bg-pink-50", border: "border-pink-200", pill: "bg-pink-100 text-pink-700",
    tag: "Career", label: "Cover Letter / Luận học bổng",
    desc: "Thư xin việc hoặc bài luận học bổng thể hiện động lực, kinh nghiệm và sự phù hợp.",
    minWords: 250, time: "30–60 phút",
    criteria: [
      { label: "Hook & First impression", weight: "25%", tip: "Câu đầu tiên phải khiến reader muốn đọc tiếp — không bắt đầu bằng 'My name is'" },
      { label: "Relevance & Specificity", weight: "35%", tip: "Mọi ví dụ phải liên quan trực tiếp đến vị trí/học bổng cụ thể này" },
      { label: "Motivation & Fit",        weight: "25%", tip: "Tại sao VỊ TRÍ NÀY, tại sao CÔNG TY/TRƯỜNG NÀY — phải thuyết phục và cá nhân" },
      { label: "Tone & Language",         weight: "15%", tip: "Confident nhưng không kiêu ngạo, formal nhưng không cứng nhắc" },
    ],
    structure: [
      { step: "Hook",       detail: "Câu mở ấn tượng: thành tích, tình huống — KHÔNG phải 'My name is'", example: "When I led a cross-functional team of 8 to deliver a product that reached 50,000 users in 3 months, I realised that scaling impact is what drives me." },
      { step: "Background", detail: "2–3 kinh nghiệm liên quan TRỰC TIẾP, kèm số liệu", example: "During my 3 years at XYZ Corp, I grew our social media engagement by 120% and managed a quarterly budget of $50,000." },
      { step: "Motivation", detail: "Tại sao vị trí NÀY + tại sao CÔNG TY/TRƯỜNG NÀY — phải cụ thể", example: "I am drawn to Anthropic specifically because of your Constitutional AI research — I believe safety-first AI development is the only sustainable path forward." },
      { step: "Value fit",  detail: "Bạn đem lại gì cụ thể — không chung chung 'I am hardworking'", example: "I would bring proven experience in rapid experimentation cycles and a network of 200+ engineers in the fintech space." },
      { step: "Closing",    detail: "Chủ động bày tỏ mong muốn — không thụ động 'Hope to hear from you'", example: "I would welcome the opportunity to discuss how my background aligns with your goals. I am available for an interview at your earliest convenience." },
    ],
    connectors: [
      { label: "Mở bài",      phrases: ["Throughout my time at…", "Having spent X years in…", "My journey into… began when…"] },
      { label: "Kinh nghiệm", phrases: ["I have consistently…", "This experience equipped me with…", "A key achievement was…", "I successfully…"] },
      { label: "Động lực",    phrases: ["What draws me to [Company] is…", "I am particularly excited about…", "This role aligns with my goal of…"] },
      { label: "Giá trị",     phrases: ["I would bring…", "My background in… would enable me to…", "I am confident that…"] },
      { label: "Kết thúc",    phrases: ["I would welcome the opportunity to…", "I am eager to contribute to…", "I look forward to discussing…"] },
    ],
    tips: [
      { tip: "Không bao giờ bắt đầu bằng 'My name is' hay 'I am writing to apply'", why: "Hiring managers đọc hàng trăm cover letters — câu mở mà ai cũng dùng là dấu hiệu bạn không đầu tư." },
      { tip: "Mỗi ví dụ phải có số liệu: %, số người, doanh thu, thời gian",        why: "'I improved performance' là claim — 'I improved performance by 40% in 3 months' là evidence." },
      { tip: "Nhắc tên công ty/trường ít nhất 1 lần và giải thích tại sao",          why: "Generic cover letter bị nhận ra ngay — cá nhân hóa cho thấy bạn thực sự muốn vị trí này." },
      { tip: "Tối đa 400 từ — một trang A4",                                          why: "Hiring manager không đọc cover letter dài — nếu bạn không thể tóm gọn, đó là dấu hiệu bạn không filter được ý chính." },
    ],
    mistakes: [
      { wrong: "I am a hardworking, dedicated, and passionate person who loves challenges.", fix: "In my previous role, I led a 5-person team to deliver the project 2 weeks ahead of schedule — an achievement that required both strategic planning and adaptability under pressure.", note: "Adjectives tự mô tả không có giá trị — chỉ có evidence mới thuyết phục." },
      { wrong: "I am writing to apply for the Software Engineer position at your esteemed company.", fix: "When our startup's infrastructure failed during peak traffic, I rebuilt the backend architecture in 48 hours — keeping 10,000 users online and earning the trust of our CTO.", note: "Câu mở generic = first impression kém. Hook = story/achievement = memorable." },
      { wrong: "I believe I am a good fit for this position and would love to join your team.", fix: "With 3 years of React experience and a track record of shipping products used by 200K+ users, I am confident I can contribute meaningfully to [Company]'s engineering team from day one.", note: "Vague belief vs. specific evidence — luôn chọn evidence." },
    ],
    bandTips: [
      { band: "Trung bình", color: "bg-orange-100 text-orange-700", req: "Có đủ các phần, nhưng generic — không đề cập tên công ty, không có số liệu." },
      { band: "Tốt",        color: "bg-blue-100 text-blue-700",     req: "Cá nhân hóa, có số liệu, động lực thuyết phục, hook tốt." },
      { band: "Xuất sắc",   color: "bg-emerald-100 text-emerald-700", req: "Từng câu có purpose, hook memorable, story-driven, vừa confident vừa humble." },
    ],
  },
  {
    id: "academic", icon: BookOpen,
    gradient: "from-slate-600 to-slate-800", gradientLight: "from-slate-50 to-slate-100",
    accent: "text-slate-600", bg: "bg-slate-50", border: "border-slate-200", pill: "bg-slate-100 text-slate-700",
    tag: "Academic", label: "Văn bản học thuật",
    desc: "Bài báo, tiểu luận, research paper theo chuẩn học thuật quốc tế.",
    minWords: 300, time: "Linh hoạt",
    criteria: [
      { label: "Research question",    weight: "25%", tip: "Câu hỏi nghiên cứu rõ ràng, thesis trả lời được câu hỏi đó" },
      { label: "Evidence & Citations", weight: "30%", tip: "Mọi claim cần có nguồn — trích dẫn đúng format (APA/MLA)" },
      { label: "Critical Analysis",    weight: "30%", tip: "Không chỉ mô tả nguồn — phải phân tích, so sánh, đánh giá" },
      { label: "Academic Style",       weight: "15%", tip: "Formal, objective, passive voice, không contractions, không slang" },
    ],
    structure: [
      { step: "Abstract",          detail: "150–250 từ: mục tiêu, phương pháp, kết quả chính, kết luận", example: "This paper examines… Drawing on… the study finds that… These findings suggest…" },
      { step: "Introduction",      detail: "Background → Research gap → Research question → Thesis + outline", example: "While extensive research exists on X (Smith, 2020), limited attention has been paid to Y. This paper addresses this gap by arguing that…" },
      { step: "Literature Review", detail: "Tổng hợp, so sánh, đánh giá nghiên cứu trước — không liệt kê", example: "Contrary to Johnson (2019), who argues that…, Chen (2022) demonstrates that… This discrepancy suggests…" },
      { step: "Discussion",        detail: "Phân tích kết quả, link về research question, so sánh với literature", example: "These findings are consistent with… However, they contradict… This may be explained by…" },
      { step: "Conclusion",        detail: "Đóng góp chính, hạn chế của nghiên cứu, hướng tiếp theo", example: "This paper has demonstrated that… However, this study is limited by… Future research should explore…" },
    ],
    connectors: [
      { label: "Giới thiệu", phrases: ["This paper aims to…", "The purpose of this study is to…", "This paper argues that…"] },
      { label: "Tham chiếu", phrases: ["Previous studies have shown… (Author, Year)", "According to…", "As argued by…", "Research suggests that…"] },
      { label: "Tương phản", phrases: ["However,…", "Contrary to…", "In contrast to previous findings,…", "Despite…"] },
      { label: "Phân tích",  phrases: ["The findings suggest…", "It can be inferred that…", "This implies that…", "This is consistent with…"] },
      { label: "Giới hạn",   phrases: ["It should be noted that…", "This study is limited by…", "Further research is needed to…"] },
    ],
    tips: [
      { tip: "Mọi claim đều cần citation",                        why: "Unsupported claims là lý do phổ biến nhất bị điểm thấp hoặc reject trong academic writing." },
      { tip: "Dùng passive voice để tăng tính khách quan",        why: "'I found that' → 'It was found that' — academic writing cần objective tone, không personal." },
      { tip: "Phân tích nguồn, không chỉ tóm tắt",               why: "Literature review là critical synthesis — so sánh, đánh giá điểm mạnh/yếu của các nghiên cứu." },
      { tip: "Abstract viết SAU KHI có bài hoàn chỉnh",           why: "Abstract là miniature của cả bài — viết trước thường không reflect đúng nội dung bài." },
      { tip: "Tránh contractions: don't → do not, can't → cannot", why: "Contractions là informal — academic writing yêu cầu formal register tuyệt đối." },
    ],
    mistakes: [
      { wrong: "Many researchers have studied this topic and found interesting results.", fix: "Smith (2020) demonstrated a 23% reduction in error rates, while Chen and Liu (2022) found contradictory results in high-noise environments, suggesting the relationship is context-dependent.", note: "Vague summary vs. specific, critical engagement with sources." },
      { wrong: "I think this is a very important finding that proves my point.", fix: "These findings provide compelling evidence that… (Author, Year), further supporting the thesis that…", note: "Academic writing: 'I think' → objective language; 'very important' → specific justification." },
      { wrong: "In conclusion, we should all work together to solve this problem in the future.", fix: "In conclusion, this paper has demonstrated that… Future research should investigate… to address the limitation of…", note: "Academic conclusion không phải call-to-action chung chung — phải link về research contribution và gap." },
    ],
    bandTips: [
      { band: "Pass",        color: "bg-orange-100 text-orange-700", req: "Có thesis, có sources, nhưng analysis còn descriptive, ít critical thinking." },
      { band: "Merit",       color: "bg-blue-100 text-blue-700",     req: "Thesis rõ, sources được phân tích critically, structure mạch lạc." },
      { band: "Distinction", color: "bg-emerald-100 text-emerald-700", req: "Argument tinh tế, critical synthesis mạnh, contribution rõ ràng, near error-free." },
    ],
  },
];

const tabDefs: { key: Tab; icon: typeof AlignLeft; label: string }[] = [
  { key: "structure",  icon: AlignLeft,     label: "Cấu trúc"    },
  { key: "connectors", icon: MessageSquare, label: "Từ nối"      },
  { key: "tips",       icon: Lightbulb,     label: "Mẹo viết"   },
  { key: "mistakes",   icon: AlertTriangle, label: "Lỗi hay gặp" },
];

export default function GuidePage() {
  const [selected, setSelected] = useState<string | null>(() => {
    if (typeof window === "undefined") return null;

    const params = new URLSearchParams(window.location.search);
    const openParam = params.get("open");
    return openParam && types.find(t => t.id === openParam) ? openParam : null;
  });
  const [activeTab, setActiveTab] = useState<Tab>("structure");
  const [copied, setCopied] = useState<string | null>(null);

  const openType = types.find(t => t.id === selected);
  const open = (id: string) => { setSelected(id); setActiveTab("structure"); };
  const close = () => setSelected(null);
  const copyPhrase = (phrase: string) => {
    navigator.clipboard?.writeText(phrase);
    setCopied(phrase);
    setTimeout(() => setCopied(null), 1500);
  };

  return (
    <AppShell activePath="/guide">
      <div className="sticky top-0 z-10 bg-white/90 backdrop-blur border-b border-slate-100 px-6 h-14 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <GraduationCap className="w-4 h-4 text-blue-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Hướng dẫn viết</h1>
        </div>
        <span className="text-xs text-slate-400">{types.length} loại văn bản</span>
      </div>

      <div className="p-6 w-full space-y-6">
        {/* Hero */}
        <div className="relative rounded-3xl overflow-hidden">
          <div className="absolute inset-0 bg-gradient-to-br from-blue-600 via-violet-600 to-purple-700" />
          <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_left,rgba(255,255,255,0.08),transparent_60%)]" />
          <div className="relative p-7 flex flex-col sm:flex-row items-start sm:items-center gap-8">
            <div className="flex-1">
              <p className="text-[11px] font-extrabold text-blue-200 uppercase tracking-widest mb-2">AI Writing Coach</p>
              <h2 className="text-2xl font-extrabold text-white mb-2 leading-tight">Học cách viết từng loại văn bản</h2>
              <p className="text-sm text-blue-100 leading-relaxed max-w-xl">
                {/* eslint-disable-next-line react/no-unescaped-entities */}
                Bấm vào loại văn bản để xem cấu trúc chi tiết, từ nối có thể copy ngay, mẹo viết có giải thích "tại sao", và ví dụ before/after từ AI Coach.
              </p>
            </div>
            <div className="flex gap-4 shrink-0">
              {([
                { val: "6",   label: "Loại văn bản", icon: FileText     },
                { val: "50+", label: "Từ nối mẫu",   icon: MessageSquare },
                { val: "25+", label: "Mẹo viết",     icon: Lightbulb    },
              ] as const).map(({ val, label, icon: Icon }) => (
                <div key={label} className="bg-white/10 border border-white/15 rounded-2xl p-4 text-center min-w-[70px]">
                  <Icon className="w-4 h-4 text-blue-200 mx-auto mb-1.5" />
                  <p className="text-xl font-extrabold text-white">{val}</p>
                  <p className="text-[10px] text-blue-200 font-semibold leading-tight mt-0.5">{label}</p>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Card grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4">
          {types.map((t) => {
            const Icon = t.icon;
            return (
              <button key={t.id} onClick={() => open(t.id)}
                className="group text-left bg-white rounded-2xl border border-slate-100 shadow-sm hover:shadow-md hover:-translate-y-1 transition-all duration-200 overflow-hidden"
              >
                <div className={`h-1.5 w-full bg-gradient-to-r ${t.gradient}`} />
                <div className="p-5">
                  <div className="flex items-start justify-between mb-3">
                    <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${t.gradient} flex items-center justify-center shadow-sm`}>
                      <Icon className="w-5 h-5 text-white" />
                    </div>
                    <span className={`text-[10px] font-extrabold px-2 py-1 rounded-full uppercase tracking-wide ${t.pill}`}>{t.tag}</span>
                  </div>
                  <h3 className="text-sm font-extrabold text-slate-900 mb-1">{t.label}</h3>
                  <p className="text-xs text-slate-500 leading-relaxed mb-4">{t.desc}</p>
                  <div className="flex items-center gap-3 text-xs text-slate-400 mb-4">
                    <span className="flex items-center gap-1"><Clock className="w-3.5 h-3.5" />{t.time}</span>
                    <span className="w-px h-3 bg-slate-200" />
                    <span className="flex items-center gap-1"><Target className="w-3.5 h-3.5" />≥ {t.minWords} từ</span>
                  </div>
                  <div className="flex items-center gap-1.5 flex-wrap mb-4">
                    {t.structure.map((s, i) => (
                      <span key={i} className={`text-[10px] font-semibold px-2 py-1 rounded-lg ${t.bg} border ${t.border} ${t.accent}`}>{s.step}</span>
                    ))}
                  </div>
                  <div className={`flex items-center gap-1.5 text-xs font-bold ${t.accent} group-hover:gap-2.5 transition-all`}>
                    Xem hướng dẫn chi tiết <ChevronRight className="w-3.5 h-3.5" />
                  </div>
                </div>
              </button>
            );
          })}
        </div>

        <div className="flex items-start gap-3 p-4 bg-blue-50 border border-blue-200 rounded-2xl">
          <Zap className="w-4 h-4 text-blue-500 mt-0.5 shrink-0" />
          <p className="text-xs text-blue-700 leading-relaxed">
            <span className="font-bold">Mẹo dùng AI Coach:</span> Khi viết bài, mở tab <span className="font-bold">Cấu trúc / Từ nối / Checklist</span> trong panel bên phải để xem hướng dẫn ngay trong lúc viết.
          </p>
        </div>
      </div>

      {/* Modal */}
      {openType && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-slate-900/50 backdrop-blur-sm" onClick={close} />
          <div className="relative bg-white rounded-3xl shadow-2xl w-full max-w-2xl max-h-[88vh] flex flex-col overflow-hidden">

            {/* Modal header */}
            <div className={`bg-gradient-to-r ${openType.gradientLight} border-b border-slate-100 p-5 shrink-0`}>
              <div className="flex items-start justify-between gap-3">
                <div className="flex items-center gap-3">
                  <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${openType.gradient} flex items-center justify-center shadow-md shrink-0`}>
                    {(() => { const Icon = openType.icon; return <Icon className="w-5 h-5 text-white" />; })()}
                  </div>
                  <div>
                    <div className="flex items-center gap-2 mb-0.5">
                      <h2 className="text-sm font-extrabold text-slate-900">{openType.label}</h2>
                      <span className={`text-[10px] font-extrabold px-2 py-0.5 rounded-full ${openType.pill}`}>{openType.tag}</span>
                    </div>
                    <div className="flex items-center gap-3 text-xs text-slate-500">
                      <span className="flex items-center gap-1"><Clock className="w-3 h-3" />{openType.time}</span>
                      <span className="w-px h-3 bg-slate-200" />
                      <span className="flex items-center gap-1"><Target className="w-3 h-3" />≥ {openType.minWords} từ</span>
                    </div>
                  </div>
                </div>
                <button onClick={close} className="w-8 h-8 rounded-full bg-white/80 border border-slate-200 flex items-center justify-center text-slate-400 hover:bg-white hover:text-slate-600 transition-all shrink-0">
                  <X className="w-4 h-4" />
                </button>
              </div>
              <div className="flex flex-wrap gap-2 mt-3">
                {openType.criteria.map((c) => (
                  <div key={c.label} title={c.tip} className="flex items-center gap-1.5 px-2.5 py-1.5 bg-white/70 border border-white/80 rounded-xl cursor-help hover:bg-white transition-colors">
                    <span className={`text-[10px] font-extrabold ${openType.accent}`}>{c.weight}</span>
                    <span className="text-[10px] font-semibold text-slate-600">{c.label}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Tabs */}
            <div className="flex border-b border-slate-100 bg-slate-50/50 shrink-0">
              {tabDefs.map(({ key, icon: TabIcon, label }) => (
                <button key={key} onClick={() => setActiveTab(key)}
                  className={`flex items-center gap-1.5 px-4 py-3 text-xs font-bold transition-all border-b-2 ${
                    activeTab === key ? `border-current ${openType.accent} bg-white` : "border-transparent text-slate-400 hover:text-slate-600 hover:bg-white/60"
                  }`}
                >
                  <TabIcon className="w-3.5 h-3.5" />{label}
                </button>
              ))}
            </div>

            {/* Scrollable content */}
            <div className="overflow-y-auto flex-1 p-5">
              {activeTab === "structure" && (
                <div className="space-y-4">
                  <div className="space-y-3">
                    {openType.structure.map((s, i) => (
                      <div key={i} className="flex gap-4">
                        <div className="flex flex-col items-center gap-1">
                          <div className={`w-7 h-7 rounded-xl bg-gradient-to-br ${openType.gradient} text-white text-[11px] font-extrabold flex items-center justify-center shrink-0 shadow-sm`}>{i + 1}</div>
                          {i < openType.structure.length - 1 && <div className="w-px flex-1 bg-slate-100 min-h-[16px]" />}
                        </div>
                        <div className="pb-3 flex-1">
                          <div className="flex items-baseline gap-2 mb-1.5">
                            <p className="text-sm font-extrabold text-slate-900">{s.step}</p>
                            <p className="text-xs text-slate-500">{s.detail}</p>
                          </div>
                          <div className={`p-3 rounded-xl ${openType.bg} border ${openType.border}`}>
                            <p className="text-[10px] font-extrabold text-slate-400 uppercase tracking-widest mb-1">Ví dụ mẫu</p>
                            <p className={`text-xs ${openType.accent} font-medium italic leading-relaxed`}>&ldquo;{s.example}&rdquo;</p>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                  <div className="pt-2 border-t border-slate-100">
                    <p className="text-[10px] font-extrabold text-slate-400 uppercase tracking-widest mb-2.5 flex items-center gap-1.5"><Zap className="w-3 h-3" />Yêu cầu theo mức điểm</p>
                    <div className="grid grid-cols-3 gap-2">
                      {openType.bandTips.map((b) => (
                        <div key={b.band} className={`px-3 py-2.5 rounded-xl ${b.color}`}>
                          <p className="text-[10px] font-extrabold uppercase tracking-wide mb-1">{b.band}</p>
                          <p className="text-[11px] leading-snug">{b.req}</p>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              )}

              {activeTab === "connectors" && (
                <div className="space-y-4">
                  {openType.connectors.map((group) => (
                    <div key={group.label}>
                      <p className="text-[10px] font-extrabold text-slate-400 uppercase tracking-widest mb-2">{group.label}</p>
                      <div className="flex flex-wrap gap-2">
                        {group.phrases.map((phrase) => (
                          <button key={phrase} onClick={() => copyPhrase(phrase)}
                            className={`flex items-center gap-1.5 px-3 py-2 rounded-xl text-xs font-medium transition-all ${
                              copied === phrase ? "bg-emerald-50 border border-emerald-200 text-emerald-700" : `${openType.bg} border ${openType.border} text-slate-700 hover:shadow-sm`
                            }`}
                          >
                            {copied === phrase ? <><Check className="w-3 h-3" />Đã copy</> : <><Copy className="w-3 h-3 opacity-50" />{phrase}</>}
                          </button>
                        ))}
                      </div>
                    </div>
                  ))}
                  <p className="text-[10px] text-slate-400 text-center pt-2 flex items-center justify-center gap-1"><Copy className="w-3 h-3" />Bấm vào phrase để copy vào clipboard</p>
                </div>
              )}

              {activeTab === "tips" && (
                <div className="space-y-3">
                  {openType.tips.map((item, i) => (
                    <div key={i} className="flex gap-3 p-4 rounded-2xl bg-slate-50 border border-slate-100">
                      <div className={`w-6 h-6 rounded-full bg-gradient-to-br ${openType.gradient} flex items-center justify-center shrink-0 mt-0.5 shadow-sm`}>
                        <Check className="w-3.5 h-3.5 text-white" />
                      </div>
                      <div>
                        <p className="text-sm font-bold text-slate-800 mb-1">{item.tip}</p>
                        <p className="text-xs text-slate-500 leading-relaxed"><span className="font-semibold text-slate-600">Tại sao: </span>{item.why}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {activeTab === "mistakes" && (
                <div className="space-y-4">
                  {openType.mistakes.map((item, i) => (
                    <div key={i} className="rounded-2xl border border-slate-100 overflow-hidden">
                      <div className="flex items-start gap-3 p-4 bg-red-50 border-b border-red-100">
                        <div className="w-5 h-5 rounded-full bg-red-100 flex items-center justify-center shrink-0 mt-0.5">
                          <span className="text-red-500 text-[10px] font-extrabold">✕</span>
                        </div>
                        <div>
                          <p className="text-[10px] font-extrabold text-red-400 uppercase tracking-widest mb-1">Cách viết kém</p>
                          <p className="text-sm text-red-700 italic leading-relaxed">&ldquo;{item.wrong}&rdquo;</p>
                        </div>
                      </div>
                      <div className="flex items-start gap-3 p-4 bg-emerald-50">
                        <div className="w-5 h-5 rounded-full bg-emerald-100 flex items-center justify-center shrink-0 mt-0.5">
                          <Check className="w-3 h-3 text-emerald-600" />
                        </div>
                        <div>
                          <p className="text-[10px] font-extrabold text-emerald-500 uppercase tracking-widest mb-1">Cách viết tốt hơn</p>
                          <p className="text-sm text-emerald-800 font-medium italic leading-relaxed">&ldquo;{item.fix}&rdquo;</p>
                        </div>
                      </div>
                      <div className="px-4 py-2.5 bg-amber-50 border-t border-amber-100 flex items-start gap-2">
                        <Lightbulb className="w-3.5 h-3.5 text-amber-500 shrink-0 mt-0.5" />
                        <p className="text-xs text-amber-700 leading-relaxed">{item.note}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Modal footer */}
            <div className="shrink-0 px-5 py-4 border-t border-slate-100 flex items-center justify-between bg-white">
              <button onClick={close} className="text-xs text-slate-400 hover:text-slate-600 font-semibold transition-colors">← Quay lại</button>
              <a href={`/write?type=${openType.id}`}
                className={`flex items-center gap-2 px-5 py-2.5 rounded-xl bg-gradient-to-r ${openType.gradient} text-white text-xs font-bold hover:opacity-90 transition-opacity shadow-sm`}
              >
                Bắt đầu viết loại này <ArrowRight className="w-3.5 h-3.5" />
              </a>
            </div>
          </div>
        </div>
      )}
    </AppShell>
  );
}
