"use client";

import { useLocale } from "@/lib/i18n/locale";


import { useEffect, useState } from "react";
import AppShell from "../components/AppShell";
import {
  BookOpen, Briefcase, GraduationCap, FileText, Mail, Star,
  Copy, Check, Lightbulb, AlertTriangle, AlignLeft,
  MessageSquare, Zap, X,
} from "lucide-react";

type Tab = "structure" | "connectors" | "tips" | "mistakes";

const types = [
  {
    id: "ielts2", icon: GraduationCap,
    gradient: "bg-accent ", gradientLight: "bg-accent ",
    accent: "text-accent-ink", bg: "bg-accent", border: "border-line", pill: "bg-accent text-accent-ink",
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
      { band: "Band 6", color: "bg-accent text-accent-ink", req: "Có quan điểm, có luận điểm nhưng thiếu development. Từ vựng đủ nhưng lặp lại." },
      { band: "Band 7", color: "bg-accent text-accent-ink",     req: "Luận điểm tốt, extended với examples cụ thể. Từ vựng đa dạng, ít lỗi ngữ pháp." },
      { band: "Band 8", color: "bg-success text-success-ink", req: "Lập luận tinh tế, fully developed, vocabulary sophisticated, gần như không lỗi." },
    ],
  },
  {
    id: "ielts1", icon: FileText,
    gradient: "bg-rose ", gradientLight: "bg-rose ",
    accent: "text-rose-ink", bg: "bg-rose", border: "border-line", pill: "bg-rose text-rose-ink",
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
      { band: "Band 6", color: "bg-accent text-accent-ink", req: "Có overview, có details nhưng liệt kê nhiều số liệu, chưa nhóm tốt." },
      { band: "Band 7", color: "bg-accent text-accent-ink",     req: "Overview rõ ràng, data được nhóm theo trend, comparisons logic." },
      { band: "Band 8", color: "bg-success text-success-ink", req: "Overview tinh tế, fully analysed, ngôn ngữ đa dạng và chính xác." },
    ],
  },
  {
    id: "vstep", icon: Star,
    gradient: "bg-success ", gradientLight: "bg-success ",
    accent: "text-success-ink", bg: "bg-success", border: "border-line", pill: "bg-success text-success-ink",
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
      { band: "Level 3", color: "bg-accent text-accent-ink", req: "Có thesis, có luận điểm nhưng ví dụ còn chung chung, từ nối đơn giản." },
      { band: "Level 4", color: "bg-accent text-accent-ink",     req: "Luận điểm tốt, ví dụ cụ thể, từ vựng đa dạng, ít lỗi ngữ pháp cơ bản." },
      { band: "Level 5", color: "bg-success text-success-ink", req: "Fully developed, academic vocabulary, sophisticated structure, near error-free." },
    ],
  },
  {
    id: "email", icon: Mail,
    gradient: "bg-accent ", gradientLight: "bg-accent ",
    accent: "text-accent-ink", bg: "bg-accent", border: "border-line", pill: "bg-accent text-accent-ink",
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
      { band: "Cơ bản",        color: "bg-accent text-accent-ink", req: "Có đủ các phần, nhưng tone chưa phù hợp, CTA còn mơ hồ." },
      { band: "Chuyên nghiệp", color: "bg-accent text-accent-ink",     req: "Subject rõ, tone phù hợp, CTA cụ thể, format đẹp." },
      { band: "Xuất sắc",      color: "bg-success text-success-ink", req: "Mỗi câu có mục đích, reader-centric, persuasive mà không aggressive." },
    ],
  },
  {
    id: "cover", icon: Briefcase,
    gradient: "bg-rose ", gradientLight: "bg-rose ",
    accent: "text-rose-ink", bg: "bg-rose", border: "border-line", pill: "bg-rose text-rose-ink",
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
      { band: "Trung bình", color: "bg-accent text-accent-ink", req: "Có đủ các phần, nhưng generic — không đề cập tên công ty, không có số liệu." },
      { band: "Tốt",        color: "bg-accent text-accent-ink",     req: "Cá nhân hóa, có số liệu, động lực thuyết phục, hook tốt." },
      { band: "Xuất sắc",   color: "bg-success text-success-ink", req: "Từng câu có purpose, hook memorable, story-driven, vừa confident vừa humble." },
    ],
  },
  {
    id: "academic", icon: BookOpen,
    gradient: "bg-surface-muted ", gradientLight: "bg-canvas ",
    accent: "text-muted", bg: "bg-canvas", border: "border-line", pill: "bg-surface-muted text-ink",
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
      { band: "Pass",        color: "bg-accent text-accent-ink", req: "Có thesis, có sources, nhưng analysis còn descriptive, ít critical thinking." },
      { band: "Merit",       color: "bg-accent text-accent-ink",     req: "Thesis rõ, sources được phân tích critically, structure mạch lạc." },
      { band: "Distinction", color: "bg-success text-success-ink", req: "Argument tinh tế, critical synthesis mạnh, contribution rõ ràng, near error-free." },
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
  const { t: translateUi } = useLocale();
  const [selectedTypeId, setSelectedTypeId] = useState<string>("ielts2");
  const [activeTab, setActiveTab] = useState<Tab>("structure");
  const [copied, setCopied] = useState<string | null>(null);

  useEffect(() => {
    const timer = setTimeout(() => {
      const params = new URLSearchParams(window.location.search);
      const openParam = params.get("open");
      if (openParam && types.find(t => t.id === openParam)) {
        setSelectedTypeId(openParam);
      }
    }, 0);

    return () => clearTimeout(timer);
  }, []);

  const openType = types.find(t => t.id === selectedTypeId) || types[0];
  const copyPhrase = (phrase: string) => {
    navigator.clipboard?.writeText(phrase);
    setCopied(phrase);
    setTimeout(() => setCopied(null), 1500);
  };

  return (
    <AppShell activePath="/guide">
      {/* Sticky Header */}
      <div className="sticky top-0 z-50 flex h-[72px] items-center justify-between border-b border-line bg-surface px-6">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent text-accent-ink">
            <GraduationCap className="h-5 w-5" strokeWidth={2.5} />
          </div>
          <div>
            <h1 className="text-lg font-bold text-ink">{translateUi("Writing Guides")}</h1>
            <p className="text-xs font-semibold text-muted">{translateUi("Comprehensive resources & structures")}</p>
          </div>
        </div>
      </div>

      <div className="guide-layout">
        {/* Left Sidebar Navigation */}
        <div className="guide-library">
          <h2 className="mb-4 px-2 text-[11px] font-bold uppercase tracking-widest text-muted">{translateUi("Library")}</h2>
          <div className="space-y-1">
            {types.map((t) => {
              const Icon = t.icon;
              const isActive = selectedTypeId === t.id;
              return (
                <button
                  key={t.id}
                  onClick={() => {
                    setSelectedTypeId(t.id);
                    setActiveTab("structure");
                  }}
                  className={`flex w-full items-center gap-3 rounded-xl px-3 py-3 text-left transition-all ${
                    isActive
                      ? "bg-accent text-accent-ink"
                      : "text-muted hover:bg-canvas hover:text-ink"
                  }`}
                >
                  <div className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-lg ${isActive ? "bg-accent text-ink shadow-sm " : "bg-surface-muted text-muted"}`}>
                    <Icon className="h-4 w-4" />
                  </div>
                  <div>
                    <p className={`text-[13px] font-bold ${isActive ? "text-accent-ink" : "text-ink"}`}>{translateUi(t.label)}</p>
                    <p className={`text-[10px] font-semibold ${isActive ? "text-accent-ink" : "text-muted"}`}>{translateUi(t.tag)}</p>
                  </div>
                </button>
              );
            })}
          </div>

          <div className="guide-tip mt-4 rounded-xl border border-line bg-accent p-4">
            <div className="mb-2 flex items-center gap-2">
              <Zap className="h-4 w-4 text-accent-ink" />
              <p className="text-xs font-bold text-accent-ink">{translateUi("Pro Tip")}</p>
            </div>
            <p className="text-[11px] leading-relaxed text-accent-ink">
              {translateUi("When taking a practice test, you can open the guide in the side panel to quickly reference structures and connectors!")}</p>
          </div>
        </div>

        {/* Main Content Area */}
        <div className="guide-content">
          {/* Content Header */}
          <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
            <div className="mb-4 flex items-start gap-4">
              <div className={`flex h-16 w-16 shrink-0 items-center justify-center rounded-xl  ${openType.gradient} shadow-sm`}>
                {(() => { const Icon = openType.icon; return <Icon className="h-8 w-8 text-ink" />; })()}
              </div>
              <div>
                <div className="mb-2 flex flex-wrap items-center gap-3">
                  <h2 className="text-2xl font-bold text-ink">{translateUi(openType.label)}</h2>
                  <span className={`rounded-full px-3 py-1 text-[10px] font-bold uppercase tracking-widest ${openType.pill}`}>
                    {translateUi(openType.tag)}
                  </span>
                </div>
                <p className="text-[15px] leading-relaxed text-muted">{translateUi(openType.desc)}</p>
              </div>
            </div>

            {/* Criteria Grid */}
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 2xl:grid-cols-4">
              {openType.criteria.map((c) => (
                <div key={c.label} className="rounded-xl border border-line bg-canvas p-4">
                  <div className={`mb-1 text-lg font-bold ${openType.accent}`}>{translateUi(c.weight)}</div>
                  <div className="mb-1.5 text-xs font-bold text-ink">{translateUi(c.label)}</div>
                  <p className="text-[10px] font-medium text-muted">{translateUi(c.tip)}</p>
                </div>
              ))}
            </div>
          </div>

          {/* Tabs */}
          <div className="guide-tabs">
            {tabDefs.map(({ key, icon: TabIcon, label }) => {
              const isActive = activeTab === key;
              return (
                <button
                  key={key}
                  onClick={() => setActiveTab(key)}
                  className={`flex items-center gap-2 border-b-2 px-6 py-4 text-[13px] font-bold transition-all ${
                    isActive
                      ? `border-focus text-accent-ink`
                      : "border-transparent text-muted hover:text-ink"
                  }`}
                >
                  <TabIcon className="h-4 w-4" />
                  {translateUi(label)}
                </button>
              );
            })}
          </div>

          {/* Tab Content Panels */}
          <div className="min-w-0">
            {/* Structure Tab */}
            {activeTab === "structure" && (
              <div className="space-y-5">
                <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                  <h3 className="mb-4 text-lg font-bold text-ink">{translateUi("Recommended Paragraph Structure")}</h3>
                  <div className="space-y-6">
                    {openType.structure.map((s, i) => (
                      <div key={i} className="flex gap-5">
                        <div className="flex flex-col items-center gap-2">
                          <div className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full  ${openType.gradient} text-xs font-bold text-ink shadow-sm`}>
                            {translateUi(i + 1)}
                          </div>
                          {i < openType.structure.length - 1 && <div className="w-px flex-1 bg-surface-muted min-h-[24px]" />}
                        </div>
                        <div className="pt-1 min-w-0 flex-1">
                          <div className="mb-2 flex flex-wrap items-baseline gap-x-3 gap-y-1">
                            <h4 className="text-[15px] font-bold text-ink">{translateUi(s.step)}</h4>
                            <p className="text-[13px] text-muted">{translateUi(s.detail)}</p>
                          </div>
                          <div className={`rounded-xl border ${openType.border} ${openType.bg} p-4`}>
                            <p className="mb-1 text-[10px] font-bold uppercase tracking-widest text-muted">{translateUi("Example")}</p>
                            <p className={`text-[13px] italic font-medium leading-relaxed ${openType.accent}`}>
                              &ldquo;{s.example}&rdquo;
                            </p>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                  <h3 className="mb-4 text-lg font-bold text-ink">{translateUi("Scoring Requirements")}</h3>
                  <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
                    {openType.bandTips.map((b) => (
                      <div key={b.band} className={`rounded-xl px-5 py-4 ${b.color}`}>
                        <p className="mb-2 text-xs font-bold uppercase tracking-widest">{translateUi(b.band)}</p>
                        <p className="text-[13px] font-medium leading-relaxed">{translateUi(b.req)}</p>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            )}

            {/* Connectors Tab */}
            {activeTab === "connectors" && (
              <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                <div className="mb-6 flex items-center justify-between">
                  <div>
                    <h3 className="text-lg font-bold text-ink">{translateUi("Vocabulary & Connectors")}</h3>
                    <p className="mt-1 text-[13px] text-muted">{translateUi("Click any phrase to copy it to your clipboard.")}</p>
                  </div>
                </div>
                <div className="space-y-5">
                  {openType.connectors.map((group) => (
                    <div key={group.label}>
                      <h4 className="mb-3 text-[11px] font-bold uppercase tracking-widest text-muted">
                        {translateUi(group.label)}
                      </h4>
                      <div className="flex flex-wrap gap-2.5">
                        {group.phrases.map((phrase) => (
                          <button
                            key={phrase}
                            onClick={() => copyPhrase(phrase)}
                            className={`flex items-center gap-2 rounded-xl px-4 py-2.5 text-[13px] font-semibold transition-all ${
                              copied === phrase
                                ? "border border-line bg-success text-success-ink shadow-sm"
                                : `border ${openType.border} ${openType.bg} text-ink hover:scale-[1.02] hover:shadow-sm`
                            }`}
                          >
                            {copied === phrase ? (
                              <><Check className="h-4 w-4" /> {translateUi(" Copied!")}</>
                            ) : (
                              <><Copy className="h-4 w-4 opacity-40" /> {phrase}</>
                            )}
                          </button>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Tips Tab */}
            {activeTab === "tips" && (
              <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                <h3 className="mb-4 text-lg font-bold text-ink">{translateUi("Pro Tips for ")}{translateUi(openType.tag)}</h3>
                <div className="space-y-4">
                  {openType.tips.map((item, i) => (
                    <div key={i} className="flex gap-4 rounded-xl border border-line bg-canvas p-5 transition-all hover:border-line hover:shadow-sm">
                      <div className={`mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full  ${openType.gradient} shadow-sm`}>
                        <Check className="h-3 w-3 text-ink" />
                      </div>
                      <div>
                        <p className="mb-1 text-[15px] font-bold text-ink">{translateUi(item.tip)}</p>
                        <p className="text-[13px] leading-relaxed text-muted">
                          <span className="font-bold text-ink">{translateUi("Why: ")}</span>{translateUi(item.why)}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Mistakes Tab */}
            {activeTab === "mistakes" && (
              <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                <h3 className="mb-4 text-lg font-bold text-ink">{translateUi("Common Mistakes to Avoid")}</h3>
                <div className="space-y-6">
                  {openType.mistakes.map((item, i) => (
                    <div key={i} className="overflow-hidden rounded-xl border border-line shadow-sm">
                      <div className="flex items-start gap-4 border-b border-line bg-danger p-5">
                        <div className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-danger">
                          <X className="h-3.5 w-3.5 text-danger-ink" />
                        </div>
                        <div>
                          <p className="text-[11px] font-bold uppercase tracking-widest text-danger-ink mb-1">{translateUi("Don't write this")}</p>
                          <p className="text-[14px] font-medium leading-relaxed text-danger-ink line-through decoration-red-300">
                            {item.wrong}
                          </p>
                        </div>
                      </div>
                      <div className="flex items-start gap-4 border-b border-line bg-success p-5">
                        <div className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-success">
                          <Check className="h-3.5 w-3.5 text-success-ink" />
                        </div>
                        <div>
                          <p className="text-[11px] font-bold uppercase tracking-widest text-success-ink mb-1">{translateUi("Instead, do this")}</p>
                          <p className="text-[14px] font-bold leading-relaxed text-success-ink">
                            {item.fix}
                          </p>
                        </div>
                      </div>
                      <div className="bg-canvas p-4 px-5">
                        <p className="text-[13px] text-muted">
                          <span className="font-bold text-ink">{translateUi("Explanation: ")}</span>
                          {translateUi(item.note)}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </AppShell>
  );
}
