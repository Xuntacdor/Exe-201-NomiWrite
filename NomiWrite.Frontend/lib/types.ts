export type ApiMode = "mock" | "real";

export type UserPlan = "free" | "premium";

export interface User {
  id: string;
  email?: string;
  displayName: string;
  avatarUrl?: string;
  bio?: string;
  currentLevel?: string;
  targetType?: string;
  targetBand?: number;
  plan: UserPlan;
  subscriptionEndDate?: string;
  createdAt?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  fullName: string;
  role: string;
}

export interface WritingType {
  id: string;
  label: string;
  badge: string;
  description?: string;
  minWords: number;
}

export interface WritingPrompt {
  id: string;
  writingTypeId: string;
  writingType: string;
  topic: string;
  prompt: string;
  difficulty?: "Beginner" | "Intermediate" | "Advanced" | string;
}

export interface SubmitSubmissionRequest {
  writingPromptId: string;
  isTimed?: boolean;
  content: string;
}

export interface Submission {
  id: string;
  userId: string;
  writingType: string;
  topic: string;
  prompt: string;
  content: string;
  wordCount: number;
  overallScore?: number;
  overallFeedback?: string;
  submittedAt: string;
  status: "draft" | "submitted" | "grading" | "graded" | "failed";
}

export interface CriteriaScores {
  taskResponse?: number;
  taskAchievement?: number;
  coherenceCohesion?: number;
  lexicalResource?: number;
  grammaticalRangeAccuracy?: number;
  contentIdeas?: number;
  organization?: number;
  toneRegister?: number;
  format?: number;
}

export interface GrammarError {
  id: string;
  submissionId: string;
  userId: string;
  grammarCategory: string;
  sentence: string;
  errorPart: string;
  suggestion: string;
  explanation: string;
}

export interface VocabSuggestion {
  id: string;
  submissionId: string;
  userId: string;
  topic: string;
  originalWord: string;
  suggestedWord: string;
  exampleSentence: string;
  isMastered: boolean;
}

export interface WritingFeedback {
  submission: Submission;
  criteriaScores: CriteriaScores;
  grammarErrors: GrammarError[];
  vocabSuggestions: VocabSuggestion[];
}

export interface QuizQuestion {
  id: string;
  category: string;
  type: "multiple_choice" | "fill_blank" | "rewrite";
  question: string;
  sentence: string;
  options?: string[];
  correctAnswer: string;
  explanation: string;
}

export interface Quiz {
  id: string;
  userId: string;
  sourceSubmissionId?: string;
  questions: QuizQuestion[];
  createdAt: string;
}

export interface QuizAttempt {
  id: string;
  quizId: string;
  userId: string;
  answers: Record<string, string>;
  score: number;
  attemptedAt: string;
}

export interface DashboardSummary {
  totalSubmissions: number;
  averageScore?: number;
  strongestCategory?: string;
  weakestCategory?: string;
  recentSubmissions: Submission[];
  scoreTrend: number[];
  grammarErrorProfile: { category: string; count: number }[];
}

export interface UpdateVocabularyMasteredRequest {
  isMastered: boolean;
}

export interface GenerateQuizRequest {
  sourceSubmissionId?: string;
  categories?: string[];
  vocabularyIds?: string[];
}

export interface SubmitQuizAttemptRequest {
  quizId: string;
  answers: Record<string, string>;
}

export interface CheckoutRequest {
  plan: UserPlan;
  billingCycle: "monthly" | "yearly";
  paymentMethod: "vnpay" | "vietqr" | "momo" | "card";
  amount?: number;
  currency?: string;
  planId?: string;
}

export interface CheckoutResponse {
  id: string;
  orderReference?: string;
  amount?: number;
  currency?: string;
  provider?: "VNPay" | "Momo" | "VietQR";
  status: "pending" | "success" | "failed" | "refunded";
  checkoutUrl?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface ApiClient {
  login(request: LoginRequest): Promise<AuthResponse>;
  register(request: RegisterRequest): Promise<AuthResponse>;
  refresh(refreshToken: string): Promise<AuthResponse>;
  logout(): Promise<void>;
  getMe(): Promise<User>;
  updateMe(request: Partial<Pick<User, "displayName" | "currentLevel" | "targetType" | "targetBand">>): Promise<User>;
  getMyAccount(): Promise<User>;
  listWritingTypes(): Promise<WritingType[]>;
  listWritingPrompts(writingType?: string, topic?: string): Promise<WritingPrompt[]>;
  submitSubmission(request: SubmitSubmissionRequest): Promise<Submission>;
  listSubmissions(): Promise<Submission[]>;
  getSubmission(id: string): Promise<Submission>;
  gradeSubmission(id: string): Promise<WritingFeedback>;
  getFeedback(submissionId: string): Promise<WritingFeedback>;
  getDashboardSummary(): Promise<DashboardSummary>;
  listVocabulary(): Promise<VocabSuggestion[]>;
  updateVocabularyMastered(id: string, request: UpdateVocabularyMasteredRequest): Promise<VocabSuggestion>;
  generateQuiz(request: GenerateQuizRequest): Promise<Quiz>;
  getQuiz(id: string): Promise<Quiz>;
  submitQuizAttempt(request: SubmitQuizAttemptRequest): Promise<QuizAttempt>;
  createCheckout(request: CheckoutRequest): Promise<CheckoutResponse>;
  getPaymentStatus(id: string): Promise<CheckoutResponse>;
  listSubscriptionPlans(): Promise<SubscriptionPlan[]>;
  getCurrentSubscription(): Promise<SubscriptionStatus>;
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  billingCycle: "Monthly" | "Yearly" | string;
  durationDays: number;
}

export interface SubscriptionStatus {
  hasSubscription: boolean;
  status?: {
    planName: string;
    status: string;
    startDate: string;
    endDate: string;
    daysRemaining: number;
  } | null;
}
