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

export interface GoogleLoginRequest {
  idToken: string;
}

export interface AuthResult {
  success: boolean;
  message: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface VerifyEmailRequest {
  token: string;
}

export interface ResendVerificationEmailRequest {
  email: string;
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
  imageUrl?: string;
  difficulty?: "Beginner" | "Intermediate" | "Advanced" | string;
}

export interface SubmissionTimeRemaining {
  deadlineAt?: string | null;
  secondsRemaining: number;
  isTimed: boolean;
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

export interface RestructuringSuggestion {
  id: string;
  submissionId: string;
  originalSentence: string;
  suggestedRewrite: string;
  reason: string;
}

export interface WritingFeedback {
  id?: string;
  submission: Submission;
  criteriaScores: CriteriaScores;
  grammarErrors: GrammarError[];
  vocabSuggestions: VocabSuggestion[];
  restructuringSuggestions?: RestructuringSuggestion[];
}

export interface GradingHistoryItem {
  id: string;
  submissionId: string;
  overallBand: number;
  createdAt: string;
}

export interface FeedbackComparison {
  current: WritingFeedback;
  previous?: WritingFeedback | null;
  bandDifference?: number | null;
}

export interface TutorReviewRequest {
  id: string;
  submissionId: string;
  status: string;
  requestedAt: string;
}

export interface FlagFeedbackRequest {
  reason: string;
}

export interface FeedbackFlagConfirmation {
  id: string;
  gradingResultId: string;
  reason: string;
  createdAt: string;
}

export interface UserProgress {
  bandHistory: { date: string; band: number }[];
  strengthsWeaknesses?: string | null;
  currentStreak: number;
  totalSubmissions: number;
  badges: { name: string; achieved: boolean }[];
  targetExam?: string | null;
  targetBand?: number | null;
  targetExamDate?: string | null;
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
  promoCode?: string;
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
  appliedDiscountPercent?: number;
}

export interface PaymentHistoryItem {
  id: string;
  amount: number;
  currency: string;
  provider: "VNPay" | "Momo" | "VietQR" | string;
  status: "pending" | "success" | "failed" | "refunded" | string;
  planId?: string | null;
  createdAt: string;
}

export interface RefundRequest {
  id: string;
  paymentOrderId: string;
  reason: string;
  status: string;
  requestedAt: string;
  createdAt: string;
}

export interface PromoCodeValidation {
  valid: boolean;
  discountPercent?: number | null;
}

export interface ApiClient {
  login(request: LoginRequest): Promise<AuthResponse>;
  register(request: RegisterRequest): Promise<AuthResponse>;
  googleLogin(request: GoogleLoginRequest): Promise<AuthResponse>;
  refresh(refreshToken: string): Promise<AuthResponse>;
  logout(): Promise<void>;
  verifyEmail(request: VerifyEmailRequest): Promise<AuthResult>;
  resendVerificationEmail(request: ResendVerificationEmailRequest): Promise<AuthResult>;
  forgotPassword(request: ForgotPasswordRequest): Promise<AuthResult>;
  resetPassword(request: ResetPasswordRequest): Promise<AuthResult>;
  deactivateAccount(): Promise<AuthResult>;
  getMe(): Promise<User>;
  updateMe(request: Partial<Pick<User, "displayName" | "currentLevel" | "targetType" | "targetBand">>): Promise<User>;
  getMyAccount(): Promise<User>;
  getUserProgress(): Promise<UserProgress>;
  listWritingTypes(): Promise<WritingType[]>;
  listWritingPrompts(writingType?: string, topic?: string): Promise<WritingPrompt[]>;
  getWritingPrompt(id: string): Promise<WritingPrompt>;
  getPromptSampleAnswer(id: string): Promise<string | null>;
  submitSubmission(request: SubmitSubmissionRequest): Promise<Submission>;
  listSubmissions(): Promise<Submission[]>;
  getSubmission(id: string): Promise<Submission>;
  getSubmissionTimeRemaining(id: string): Promise<SubmissionTimeRemaining>;
  gradeSubmission(id: string): Promise<WritingFeedback>;
  getFeedback(submissionId: string): Promise<WritingFeedback>;
  listGradingHistory(): Promise<GradingHistoryItem[]>;
  compareSubmissionFeedback(submissionId: string): Promise<FeedbackComparison>;
  requestTutorReview(submissionId: string): Promise<TutorReviewRequest>;
  listTutorReviewRequests(): Promise<TutorReviewRequest[]>;
  flagFeedback(gradingResultId: string, request: FlagFeedbackRequest): Promise<FeedbackFlagConfirmation>;
  getDashboardSummary(): Promise<DashboardSummary>;
  listVocabulary(): Promise<VocabSuggestion[]>;
  updateVocabularyMastered(id: string, request: UpdateVocabularyMasteredRequest): Promise<VocabSuggestion>;
  generateQuiz(request: GenerateQuizRequest): Promise<Quiz>;
  getQuiz(id: string): Promise<Quiz>;
  submitQuizAttempt(request: SubmitQuizAttemptRequest): Promise<QuizAttempt>;
  createCheckout(request: CheckoutRequest): Promise<CheckoutResponse>;
  getPaymentStatus(id: string): Promise<CheckoutResponse>;
  listPaymentHistory(): Promise<PaymentHistoryItem[]>;
  createRefundRequest(paymentOrderId: string, reason: string): Promise<RefundRequest>;
  listRefundRequests(): Promise<RefundRequest[]>;
  listSubscriptionPlans(): Promise<SubscriptionPlan[]>;
  getCurrentSubscription(): Promise<SubscriptionStatus>;
  cancelSubscription(): Promise<SubscriptionStatus>;
  validatePromoCode(code: string): Promise<PromoCodeValidation>;
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
