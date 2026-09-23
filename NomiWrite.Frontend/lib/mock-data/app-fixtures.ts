import type {
  DashboardSummary,
  Quiz,
  StudyGuide,
  Submission,
  VocabSuggestion,
  WritingFeedback,
  WritingPrompt,
  WritingType,
} from "../types";
import { mockUser } from "./auth";

export const mockStudyGuide: StudyGuide = {
  id: "study-guide-mock-0001",
  userId: mockUser.id,
  targetExam: "IELTS Academic - Writing Task 2",
  targetBand: 7,
  summary:
    "Your recent essays show clear structure and a strong, consistent position, which is holding your Task Response and Coherence at Band 7. Lexical Resource and Grammatical Range are the two criteria keeping your overall score below target.",
  estimatedBand: 6.5,
  strengths: [
    "Clear, consistent position on every question",
    "Coherent paragraph structure with linking logic",
    "Few repetition errors in essay organization",
  ],
  weaknesses: [
    "Lexical Resource is the lowest criterion (6.0)",
    "'Preposition' mistakes recur in almost every essay",
    "Limited range of complex sentence structures",
  ],
  nextSteps: [
    {
      title: "Raise Lexical Resource to Band 7",
      description:
        "Learn 5 academic collocations per week targeted at your recurring topics. Aim to use 2 unfamiliar words from your saved vocabulary in each new essay.",
      focus: "vocabulary",
    },
    {
      title: "Eliminate recurring 'Preposition' errors",
      description:
        "Review every flagged preposition error in History, note each corrected collocation, and drill them until they appear as corrections in the next essay.",
      focus: "grammar",
    },
    {
      title: "Add one complex structure per essay",
      description:
        "Draft at least one conditional or relative clause per body paragraph. Keep it short and correct rather than long and error-prone.",
      focus: "grammar",
    },
  ],
  recommendedTopic: {
    title: "Renewable energy and the future of employment",
    reason:
      "This topic forces 'cause and effect' writing, which exercises your weakest lexical area and challenges your sentence variety.",
    suggestedPrompt:
      "Some people think that the transition to renewable energy will create more jobs than it destroys. To what extent do you agree or disagree?",
  },
  analyzedEssayCount: 3,
  createdAt: new Date().toISOString(),
};

export const mockWritingTypes: WritingType[] = [
  { id: "11111111-1111-1111-1111-111111111111", label: "IELTS Writing Task 1 Academic", badge: "Exam", minWords: 150 },
  { id: "22222222-2222-2222-2222-222222222222", label: "IELTS Writing Task 1 General Training", badge: "Exam", minWords: 150 },
  { id: "33333333-3333-3333-3333-333333333333", label: "IELTS Writing Task 2", badge: "Exam", minWords: 250 },
  { id: "44444444-4444-4444-4444-444444444444", label: "TOEFL iBT Integrated Writing", badge: "Exam", minWords: 150 },
  { id: "55555555-5555-5555-5555-555555555555", label: "TOEFL iBT Independent Writing", badge: "Exam", minWords: 300 },
  { id: "66666666-6666-6666-6666-666666666666", label: "Cover Letter", badge: "Work", minWords: 250 },
  { id: "77777777-7777-7777-7777-777777777777", label: "Business Email", badge: "Work", minWords: 100 },
  { id: "88888888-8888-8888-8888-888888888888", label: "Meeting Minutes", badge: "Work", minWords: 180 },
  { id: "99999999-9999-9999-9999-999999999999", label: "Paragraph Writing", badge: "Academic", minWords: 120 },
  { id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", label: "Personal Statement / SOP", badge: "Academic", minWords: 350 },
  { id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", label: "PTE Academic Summarize Written Text", badge: "Exam", minWords: 50 },
  { id: "cccccccc-cccc-cccc-cccc-cccccccccccc", label: "Cambridge B2 First Essay", badge: "Exam", minWords: 140 },
  { id: "dddddddd-dddd-dddd-dddd-dddddddddddd", label: "VSTEP Task 2 Essay", badge: "Exam", minWords: 250 },
  { id: "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee", label: "Academic Essay", badge: "Academic", minWords: 350 },
  { id: "ffffffff-ffff-ffff-ffff-ffffffffffff", label: "Research Abstract", badge: "Academic", minWords: 150 },
];

export const mockWritingPrompts: WritingPrompt[] = [
  {
    id: "10000000-0000-0000-0000-000000000001",
    writingTypeId: "11111111-1111-1111-1111-111111111111",
    writingType: "IELTS Writing Task 1 Academic",
    topic: "Urban Transport Modes",
    prompt: "The chart shows changes in the percentage of commuters using different transport modes in a city. Summarize the information by selecting and reporting the main features, and make comparisons where relevant.",
  },
  {
    id: "10000000-0000-0000-0000-000000000002",
    writingTypeId: "22222222-2222-2222-2222-222222222222",
    writingType: "IELTS Writing Task 1 General Training",
    topic: "Request A Course Refund",
    prompt: "Write a letter to a course provider requesting a refund. Explain why you joined the course, why you are dissatisfied, and what action you want them to take.",
  },
  {
    id: "10000000-0000-0000-0000-000000000003",
    writingTypeId: "33333333-3333-3333-3333-333333333333",
    writingType: "IELTS Writing Task 2",
    topic: "Remote Work And Productivity",
    prompt: "Some people believe remote work improves productivity, while others think it creates communication problems. Discuss both views and give your opinion.",
  },
  {
    id: "fcc2aa21-4654-4b1e-8521-4ccec45f79fb",
    writingTypeId: "33333333-3333-3333-3333-333333333333",
    writingType: "IELTS Writing Task 2",
    topic: "Technology & Society",
    prompt: "Some people believe technology makes life more complicated, while others think it makes life easier. Discuss both views and give your opinion.",
  },
  {
    id: "10000000-0000-0000-0000-000000000005",
    writingTypeId: "55555555-5555-5555-5555-555555555555",
    writingType: "TOEFL iBT Independent Writing",
    topic: "Learning Through Mistakes",
    prompt: "Do you agree or disagree that students learn more from making mistakes than from successful experiences? Use reasons and examples to support your answer.",
  },
  {
    id: "10000000-0000-0000-0000-000000000006",
    writingTypeId: "66666666-6666-6666-6666-666666666666",
    writingType: "Cover Letter",
    topic: "Junior Marketing Associate Cover Letter",
    prompt: "Write a cover letter for a junior marketing associate position, highlighting relevant experience, motivation, and fit for the role.",
  },
  {
    id: "10000000-0000-0000-0000-000000000007",
    writingTypeId: "77777777-7777-7777-7777-777777777777",
    writingType: "Business Email",
    topic: "Project Deadline Update",
    prompt: "Write a professional email to a client explaining that a project deadline needs to be adjusted and proposing a revised timeline.",
  },
  {
    id: "10000000-0000-0000-0000-000000000009",
    writingTypeId: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    writingType: "PTE Academic Summarize Written Text",
    topic: "Digital Textbooks In Universities",
    prompt: "Summarize a passage about the use of digital textbooks in universities in one clear sentence.",
  },
  {
    id: "10000000-0000-0000-0000-000000000010",
    writingTypeId: "cccccccc-cccc-cccc-cccc-cccccccccccc",
    writingType: "Cambridge B2 First Essay",
    topic: "School Trips And Learning",
    prompt: "Write an essay discussing whether schools should organize more educational trips for students.",
  },
  {
    id: "10000000-0000-0000-0000-000000000011",
    writingTypeId: "dddddddd-dddd-dddd-dddd-dddddddddddd",
    writingType: "VSTEP Task 2 Essay",
    topic: "Public Parks In Cities",
    prompt: "Write an essay discussing whether cities should invest more money in public parks and green spaces.",
  },
  {
    id: "10000000-0000-0000-0000-000000000013",
    writingTypeId: "99999999-9999-9999-9999-999999999999",
    writingType: "Paragraph Writing",
    topic: "Benefits Of Peer Feedback",
    prompt: "Write one well-structured academic paragraph explaining how peer feedback can improve student writing.",
  },
  {
    id: "10000000-0000-0000-0000-000000000016",
    writingTypeId: "44444444-4444-4444-4444-444444444444",
    writingType: "TOEFL iBT Integrated Writing",
    topic: "Online Course Announcement",
    prompt: "Summarize the relationship between a reading passage about a new online course policy and a lecture that questions its benefits.",
  },
  {
    id: "10000000-0000-0000-0000-000000000017",
    writingTypeId: "88888888-8888-8888-8888-888888888888",
    writingType: "Meeting Minutes",
    topic: "Weekly Planning Meeting Minutes",
    prompt: "Write concise meeting minutes from notes about a weekly planning meeting, including decisions, owners, action items, and deadlines.",
  },
  {
    id: "10000000-0000-0000-0000-000000000018",
    writingTypeId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    writingType: "Personal Statement / SOP",
    topic: "Computer Science Scholarship Statement",
    prompt: "Write a personal statement for a computer science scholarship, focusing on motivation, relevant achievements, and future contribution.",
  },
  {
    id: "10000000-0000-0000-0000-000000000019",
    writingTypeId: "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
    writingType: "Academic Essay",
    topic: "Group Work In University Courses",
    prompt: "Write a formal academic essay discussing whether group work should be used more often in university courses.",
  },
  {
    id: "10000000-0000-0000-0000-000000000020",
    writingTypeId: "ffffffff-ffff-ffff-ffff-ffffffffffff",
    writingType: "Research Abstract",
    topic: "Abstract For A Study On Study Habits",
    prompt: "Write a research abstract for a small study investigating the relationship between study habits and exam performance among university students.",
  },
];

export const mockSubmissions: Submission[] = [
  {
    id: "sub_mock_001",
    userId: mockUser.id,
    writingType: "IELTS Writing Task 2",
    topic: "Technology & Society",
    prompt: mockWritingPrompts[0].prompt,
    content: "Technology changes how people work, study, and communicate.",
    wordCount: 8,
    overallScore: 6.5,
    overallFeedback: "Clear position, but the argument needs more specific support.",
    submittedAt: "2026-09-05T08:00:00.000Z",
    status: "graded",
  },
];

export const mockVocabulary: VocabSuggestion[] = [
  {
    id: "vocab_mock_001",
    submissionId: mockSubmissions[0].id,
    userId: mockUser.id,
    topic: "Technology",
    originalWord: "good",
    suggestedWord: "beneficial",
    exampleSentence: "Digital tools can be beneficial when they improve access to education.",
    isMastered: false,
  },
];

export const mockFeedback: WritingFeedback = {
  submission: mockSubmissions[0],
  criteriaScores: {
    taskResponse: 6.5,
    coherenceCohesion: 6,
    lexicalResource: 6,
    grammaticalRangeAccuracy: 6.5,
  },
  grammarErrors: [],
  vocabSuggestions: mockVocabulary,
};

export const mockQuiz: Quiz = {
  id: "quiz_mock_001",
  userId: mockUser.id,
  sourceSubmissionId: mockSubmissions[0].id,
  createdAt: "2026-09-05T08:05:00.000Z",
  questions: [
    {
      id: "question_mock_001",
      category: "Collocation",
      type: "multiple_choice",
      question: "Choose the stronger academic word.",
      sentence: "Technology can be ___ for students.",
      options: ["good", "beneficial", "nice", "big"],
      correctAnswer: "beneficial",
      explanation: "Beneficial is more precise and formal than good.",
    },
  ],
};

export const mockDashboardSummary: DashboardSummary = {
  totalSubmissions: mockSubmissions.length,
  averageScore: 6.5,
  strongestCategory: "Task Response",
  weakestCategory: "Lexical Resource",
  recentSubmissions: mockSubmissions,
  scoreTrend: [5.5, 6, 6.5],
  grammarErrorProfile: [{ category: "Collocation", count: 1 }],
};
