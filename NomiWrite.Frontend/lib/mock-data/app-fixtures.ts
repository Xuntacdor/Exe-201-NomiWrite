import type {
  DashboardSummary,
  Quiz,
  Submission,
  VocabSuggestion,
  WritingFeedback,
  WritingPrompt,
  WritingType,
} from "../types";
import { mockUser } from "./auth";

export const mockWritingTypes: WritingType[] = [
  { id: "ielts2", label: "IELTS Writing Task 2", badge: "IELTS", minWords: 250 },
  { id: "ielts1", label: "IELTS Writing Task 1", badge: "IELTS", minWords: 150 },
  { id: "vstep", label: "VSTEP Writing", badge: "VSTEP", minWords: 200 },
  { id: "email", label: "Work email", badge: "Email", minWords: 100 },
  { id: "cover", label: "Cover letter", badge: "Letter", minWords: 250 },
  { id: "academic", label: "Academic writing", badge: "Academic", minWords: 300 },
];

export const mockWritingPrompts: WritingPrompt[] = [
  {
    writingType: "ielts2",
    topic: "Technology & Society",
    prompt: "Some people believe technology makes life more complicated, while others think it makes life easier. Discuss both views and give your opinion.",
  },
  {
    writingType: "email",
    topic: "Project Proposal",
    prompt: "Write an email to your manager proposing a project to improve employee onboarding.",
  },
];

export const mockSubmissions: Submission[] = [
  {
    id: "sub_mock_001",
    userId: mockUser.id,
    writingType: "ielts2",
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
