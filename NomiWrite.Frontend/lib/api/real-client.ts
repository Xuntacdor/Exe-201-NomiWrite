import type {
  ApiClient,
  AuthResult,
  AuthResponse,
  CheckoutRequest,
  CheckoutResponse,
  DashboardSummary,
  FeedbackComparison,
  FeedbackFlagConfirmation,
  FlagFeedbackRequest,
  GradingHistoryItem,
  GenerateQuizRequest,
  LoginRequest,
  PaymentHistoryItem,
  Quiz,
  QuizAttempt,
  RegisterRequest,
  RefundRequest,
  RestructuringSuggestion,
  SubmitQuizAttemptRequest,
  Submission,
  SubmissionTimeRemaining,
  SubscriptionPlan,
  SubscriptionStatus,
  SubmitSubmissionRequest,
  TutorReviewRequest,
  UpdateVocabularyMasteredRequest,
  User,
  UserProgress,
  VocabSuggestion,
  WritingFeedback,
  WritingPrompt,
  WritingType,
} from "../types";
import { getSession } from "../auth/session";
import { apiRoutes } from "./routes";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5097";

interface ApiErrorBody {
  message?: string;
  errors?: string[];
}

interface BackendPaymentResponse {
  paymentId: string;
  orderReference: string;
  amount: number;
  currency: string;
  provider: "VNPay" | "Momo" | "VietQR";
  status: "Pending" | "Completed" | "Failed" | "Refunded";
  paymentUrl?: string;
  createdAt?: string;
  updatedAt?: string;
  appliedDiscountPercent?: number;
}

interface BackendUserProfile {
  userId: string;
  displayName: string;
  avatarUrl?: string | null;
  bio?: string | null;
  targetExam?: string | null;
  targetBand?: number | null;
  englishLevel?: string | null;
  hasActiveSubscription?: boolean;
  subscriptionPlanName?: string | null;
  subscriptionEndDate?: string | null;
}

interface BackendWritingType {
  id: string;
  name: string;
  category: string;
  description: string;
}

interface BackendWritingPrompt {
  id: string;
  writingTypeId: string;
  writingTypeName: string;
  title: string;
  instructions?: string;
  imageUrl?: string | null;
  difficulty: string;
}

interface BackendSubmission {
  id: string;
  writingPromptId: string;
  promptTitle: string;
  content: string;
  wordCount: number;
  status: "Draft" | "Submitted" | string;
  startedAt: string;
  submittedAt?: string | null;
}

interface BackendGradingResult {
  id: string;
  submissionId: string;
  overallBand: number;
  criterionScores: { criterionName: string; score: number; comment: string }[];
  overallFeedback: string;
  grammarErrors: { originalText: string; suggestion: string; explanation: string }[];
  vocabularySuggestions?: { originalWord: string; suggestedAlternatives: string[]; context: string }[];
  restructuringSuggestions?: { originalSentence: string; suggestedRewrite: string; reason: string }[];
  status: "Pending" | "Completed" | "Failed" | string;
}

interface BackendUserProgress {
  bandHistory: { date: string; band: number }[];
  strengthsWeaknesses?: string | null;
  currentStreak: number;
  totalSubmissions: number;
  badges: { name: string; achieved: boolean }[];
  targetExam?: string | null;
  targetBand?: number | null;
  targetExamDate?: string | null;
}

interface BackendPaymentHistoryItem {
  id: string;
  amount: number;
  currency: string;
  provider: string;
  status: string;
  planId?: string | null;
  createdAt: string;
}

interface BackendRefundRequest {
  id: string;
  paymentOrderId: string;
  reason: string;
  status: string;
  requestedAt: string;
  createdAt: string;
}

interface BackendSubscriptionStatus {
  planName: string;
  status: string;
  startDate: string;
  endDate: string;
  daysRemaining: number;
}

interface BackendApiEnvelope<T> {
  success: boolean;
  data: T;
  message?: string;
}

function getAuthHeaders(): HeadersInit {
  if (typeof window === "undefined") return {};

  const session = getSession();
  return session?.accessToken
    ? { Authorization: `Bearer ${session.accessToken}` }
    : {};
}

function buildErrorMessage(body: string, fallback: string) {
  if (!body) return fallback;

  try {
    const parsed = JSON.parse(body) as ApiErrorBody;
    if (parsed.errors?.length) return parsed.errors.join(" ");
    if (parsed.message) return parsed.message;
  } catch {
    return body;
  }

  return fallback;
}

function toPaymentProvider(method: CheckoutRequest["paymentMethod"]): BackendPaymentResponse["provider"] {
  if (method === "momo") return "Momo";
  if (method === "vietqr") return "VietQR";
  return "VNPay";
}

function toCheckoutStatus(status: BackendPaymentResponse["status"]): CheckoutResponse["status"] {
  if (status === "Completed") return "success";
  if (status === "Failed") return "failed";
  if (status === "Refunded") return "refunded";
  return "pending";
}

function toCheckoutResponse(response: BackendPaymentResponse): CheckoutResponse {
  return {
    id: response.paymentId,
    orderReference: response.orderReference,
    amount: response.amount,
    currency: response.currency,
    provider: response.provider,
    status: toCheckoutStatus(response.status),
    checkoutUrl: response.paymentUrl,
    createdAt: response.createdAt,
    updatedAt: response.updatedAt,
    appliedDiscountPercent: response.appliedDiscountPercent,
  };
}

function badgeForCategory(category: string) {
  if (category === "Professional") return "Work";
  if (category === "Academic") return "Academic";
  return "Exam";
}

function minWordsForWritingType(name: string) {
  if (/task 1|letter/i.test(name)) return 150;
  if (/email|minutes/i.test(name)) return 100;
  if (/cover|statement|sop/i.test(name)) return 250;
  return 250;
}

function toUser(profile: BackendUserProfile): User {
  return {
    id: profile.userId,
    displayName: profile.displayName,
    avatarUrl: profile.avatarUrl ?? undefined,
    bio: profile.bio ?? undefined,
    currentLevel: profile.englishLevel ?? undefined,
    targetType: profile.targetExam ?? undefined,
    targetBand: profile.targetBand ?? undefined,
    plan: profile.hasActiveSubscription ? "premium" : "free",
    subscriptionEndDate: profile.subscriptionEndDate ?? undefined,
  };
}

function toBackendProfile(request: Partial<Pick<User, "displayName" | "currentLevel" | "targetType" | "targetBand">>) {
  return {
    displayName: request.displayName,
    englishLevel: request.currentLevel,
    targetExam: request.targetType,
    targetBand: request.targetBand,
  };
}

function toWritingType(item: BackendWritingType): WritingType {
  return {
    id: item.id,
    label: item.name,
    badge: badgeForCategory(item.category),
    description: item.description,
    minWords: minWordsForWritingType(item.name),
  };
}

function toWritingPrompt(item: BackendWritingPrompt): WritingPrompt {
  return {
    id: item.id,
    writingTypeId: item.writingTypeId,
    writingType: item.writingTypeName,
    topic: item.title,
    prompt: item.instructions ?? item.title,
    imageUrl: item.imageUrl ?? undefined,
    difficulty: item.difficulty,
  };
}

function toSubmission(item: BackendSubmission, userId = ""): Submission {
  const isSubmitted = item.status === "Submitted";
  const status: Submission["status"] =
    item.status === "Graded" ? "graded" :
    item.status === "Failed" ? "failed" :
    isSubmitted ? "submitted" : "draft";

  return {
    id: item.id,
    userId,
    writingType: "",
    topic: item.promptTitle,
    prompt: item.promptTitle,
    content: item.content,
    wordCount: item.wordCount,
    submittedAt: item.submittedAt ?? item.startedAt,
    status,
  };
}

function toWritingFeedback(result: BackendGradingResult, submission?: Submission): WritingFeedback {
  const fallbackSubmission: Submission = submission ?? {
    id: result.submissionId,
    userId: "",
    writingType: "",
    topic: "Submitted writing",
    prompt: "Submitted writing",
    content: "",
    wordCount: 0,
    overallScore: result.overallBand,
    overallFeedback: result.overallFeedback,
    submittedAt: new Date().toISOString(),
    status: result.status === "Completed" ? "graded" : "grading",
  };

  return {
    id: result.id,
    submission: {
      ...fallbackSubmission,
      overallScore: result.overallBand,
      overallFeedback: result.overallFeedback,
      status: result.status === "Completed" ? "graded" : fallbackSubmission.status,
    },
    criteriaScores: Object.fromEntries(
      result.criterionScores.map(score => [
        score.criterionName.replace(/\s+/g, ""),
        score.score,
      ]),
    ) as WritingFeedback["criteriaScores"],
    grammarErrors: result.grammarErrors.map((error, index) => ({
      id: `${result.id}-${index}`,
      submissionId: result.submissionId,
      userId: fallbackSubmission.userId,
      grammarCategory: "AI feedback",
      sentence: error.originalText,
      errorPart: error.originalText,
      suggestion: error.suggestion,
      explanation: error.explanation,
    })),
    vocabSuggestions: (result.vocabularySuggestions ?? []).flatMap((suggestion, index) => {
      const alternatives = suggestion.suggestedAlternatives.length
        ? suggestion.suggestedAlternatives
        : ["Review word choice"];

      return alternatives.map((alternative, altIndex) => ({
        id: `${result.id}-vocab-${index}-${altIndex}`,
        submissionId: result.submissionId,
        userId: fallbackSubmission.userId,
        topic: "AI feedback",
        originalWord: suggestion.originalWord,
        suggestedWord: alternative,
        exampleSentence: suggestion.context,
        isMastered: false,
      }));
    }),
    restructuringSuggestions: (result.restructuringSuggestions ?? []).map((suggestion, index): RestructuringSuggestion => ({
      id: `${result.id}-rewrite-${index}`,
      submissionId: result.submissionId,
      originalSentence: suggestion.originalSentence,
      suggestedRewrite: suggestion.suggestedRewrite,
      reason: suggestion.reason,
    })),
  };
}

function toUserProgress(progress: BackendUserProgress): UserProgress {
  return progress;
}

function toPaymentHistory(item: BackendPaymentHistoryItem): PaymentHistoryItem {
  return {
    ...item,
    status: toCheckoutStatus(item.status as BackendPaymentResponse["status"]),
  };
}

function toRefundRequest(item: BackendRefundRequest): RefundRequest {
  return item;
}

function toSubscriptionStatus(status: BackendSubscriptionStatus | null): SubscriptionStatus {
  return {
    hasSubscription: Boolean(status),
    status,
  };
}

async function request<T>(path: string, init: RequestInit, authenticated = false): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(authenticated ? getAuthHeaders() : {}),
      ...init.headers,
    },
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(buildErrorMessage(message, `Request failed with status ${response.status}`));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export const realClient: ApiClient = {
  login(requestBody: LoginRequest) {
    return request<AuthResponse>(apiRoutes.auth.login, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  register(requestBody: RegisterRequest) {
    return request<AuthResponse>(apiRoutes.auth.register, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  refresh(refreshToken: string) {
    return request<AuthResponse>(apiRoutes.auth.refresh, {
      method: "POST",
      body: JSON.stringify({ refreshToken }),
    });
  },

  logout() {
    return request<void>(apiRoutes.auth.logout, {
      method: "POST",
    }, true);
  },

  verifyEmail(requestBody) {
    return request<AuthResult>(apiRoutes.auth.verifyEmail, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  resendVerificationEmail(requestBody) {
    return request<AuthResult>(apiRoutes.auth.resendVerificationEmail, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  forgotPassword(requestBody) {
    return request<AuthResult>(apiRoutes.auth.forgotPassword, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  resetPassword(requestBody) {
    return request<AuthResult>(apiRoutes.auth.resetPassword, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  deactivateAccount() {
    return request<AuthResult>(apiRoutes.auth.deactivate, { method: "POST" }, true);
  },

  getMe() {
    return request<BackendUserProfile>(apiRoutes.users.me, { method: "GET" }, true).then(toUser);
  },

  updateMe(requestBody) {
    return request<BackendUserProfile>(apiRoutes.users.me, {
      method: "PUT",
      body: JSON.stringify(toBackendProfile(requestBody)),
    }, true).then(toUser);
  },

  getMyAccount() {
    return request<BackendUserProfile>(apiRoutes.users.account, { method: "GET" }, true).then(toUser);
  },

  getUserProgress() {
    return request<BackendUserProgress>(apiRoutes.users.progress, { method: "GET" }, true).then(toUserProgress);
  },

  listWritingTypes() {
    return request<BackendWritingType[]>(apiRoutes.writing.types, { method: "GET" }).then(items => items.map(toWritingType));
  },

  listWritingPrompts(writingType?: string, difficulty?: string) {
    const params = new URLSearchParams();
    if (writingType) params.set("typeId", writingType);
    if (difficulty) params.set("difficulty", difficulty);
    const query = params.toString();
    return request<BackendWritingPrompt[]>(`${apiRoutes.writing.prompts}${query ? `?${query}` : ""}`, { method: "GET" })
      .then(items => items.map(toWritingPrompt));
  },

  getWritingPrompt(id: string) {
    return request<BackendWritingPrompt>(apiRoutes.writing.promptDetail(id), { method: "GET" }).then(toWritingPrompt);
  },

  getPromptSampleAnswer(id: string) {
    return request<{ sampleAnswer?: string | null }>(apiRoutes.writing.sampleAnswer(id), { method: "GET" })
      .then(result => result.sampleAnswer ?? null);
  },

  async submitSubmission(requestBody: SubmitSubmissionRequest) {
    const created = await request<BackendSubmission>(apiRoutes.submissions.list, {
      method: "POST",
      body: JSON.stringify({
        writingPromptId: requestBody.writingPromptId,
        isTimed: requestBody.isTimed ?? false,
      }),
    }, true);

    const updated = await request<BackendSubmission>(apiRoutes.submissions.detail(created.id), {
      method: "PUT",
      body: JSON.stringify({ content: requestBody.content }),
    }, true);

    const submitted = await request<BackendSubmission>(apiRoutes.submissions.submit(created.id), {
      method: "POST",
    }, true);

    return toSubmission({ ...updated, ...submitted });
  },

  listSubmissions() {
    return request<BackendSubmission[]>(apiRoutes.submissions.list, { method: "GET" }, true)
      .then(items => items.map(item => toSubmission(item)));
  },

  getSubmission(id: string) {
    return request<BackendSubmission>(apiRoutes.submissions.detail(id), { method: "GET" }, true).then(toSubmission);
  },

  getSubmissionTimeRemaining(id: string): Promise<SubmissionTimeRemaining> {
    return request<SubmissionTimeRemaining>(apiRoutes.submissions.timeRemaining(id), { method: "GET" }, true);
  },

  async gradeSubmission(id: string) {
    const [submission, envelope] = await Promise.all([
      realClient.getSubmission(id).catch(() => undefined),
      request<BackendApiEnvelope<BackendGradingResult>>(apiRoutes.feedback.detail(id), { method: "GET" }, true),
    ]);
    return toWritingFeedback(envelope.data, submission);
  },

  async getFeedback(submissionId: string) {
    const [submission, envelope] = await Promise.all([
      realClient.getSubmission(submissionId).catch(() => undefined),
      request<BackendApiEnvelope<BackendGradingResult>>(apiRoutes.feedback.detail(submissionId), { method: "GET" }, true),
    ]);
    return toWritingFeedback(envelope.data, submission);
  },

  listGradingHistory() {
    return request<BackendApiEnvelope<GradingHistoryItem[]>>(apiRoutes.feedback.history, { method: "GET" }, true)
      .then(envelope => envelope.data);
  },

  async compareSubmissionFeedback(submissionId: string): Promise<FeedbackComparison> {
    const [submission, envelope] = await Promise.all([
      realClient.getSubmission(submissionId).catch(() => undefined),
      request<BackendApiEnvelope<{
        current: BackendGradingResult;
        previous?: BackendGradingResult | null;
        bandDifference?: number | null;
      }>>(apiRoutes.feedback.compare(submissionId), { method: "GET" }, true),
    ]);

    return {
      current: toWritingFeedback(envelope.data.current, submission),
      previous: envelope.data.previous ? toWritingFeedback(envelope.data.previous) : null,
      bandDifference: envelope.data.bandDifference,
    };
  },

  requestTutorReview(submissionId: string) {
    return request<BackendApiEnvelope<TutorReviewRequest>>(apiRoutes.feedback.requestTutorReview(submissionId), { method: "POST" }, true)
      .then(envelope => envelope.data);
  },

  listTutorReviewRequests() {
    return request<BackendApiEnvelope<TutorReviewRequest[]>>(apiRoutes.feedback.tutorReviewRequests, { method: "GET" }, true)
      .then(envelope => envelope.data);
  },

  flagFeedback(gradingResultId: string, requestBody: FlagFeedbackRequest) {
    return request<BackendApiEnvelope<FeedbackFlagConfirmation>>(apiRoutes.feedback.flag(gradingResultId), {
      method: "POST",
      body: JSON.stringify(requestBody),
    }, true).then(envelope => envelope.data);
  },

  getDashboardSummary() {
    return request<DashboardSummary>(apiRoutes.dashboard.summary, { method: "GET" });
  },

  listVocabulary() {
    return request<VocabSuggestion[]>(apiRoutes.vocabulary.list, { method: "GET" });
  },

  updateVocabularyMastered(id: string, requestBody: UpdateVocabularyMasteredRequest) {
    return request<VocabSuggestion>(apiRoutes.vocabulary.mastered(id), {
      method: "PATCH",
      body: JSON.stringify(requestBody),
    });
  },

  generateQuiz(requestBody: GenerateQuizRequest) {
    return request<Quiz>(apiRoutes.quizzes.generate, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  getQuiz(id: string) {
    return request<Quiz>(apiRoutes.quizzes.detail(id), { method: "GET" });
  },

  submitQuizAttempt(requestBody: SubmitQuizAttemptRequest) {
    return request<QuizAttempt>(apiRoutes.quizAttempts.create, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  createCheckout(requestBody: CheckoutRequest) {
    const amount = requestBody.amount ?? (requestBody.billingCycle === "monthly" ? 199_000 : 159_000 * 12);

    return request<BackendPaymentResponse>(apiRoutes.payments.checkout, {
      method: "POST",
      body: JSON.stringify({
        amount,
        currency: requestBody.currency ?? "VND",
        provider: toPaymentProvider(requestBody.paymentMethod),
        planId: requestBody.planId,
        promoCode: requestBody.promoCode,
      }),
    }, true).then(toCheckoutResponse);
  },

  getPaymentStatus(id: string) {
    return request<BackendPaymentResponse>(apiRoutes.payments.status(id), { method: "GET" }, true)
      .then(toCheckoutResponse);
  },

  listPaymentHistory() {
    return request<BackendPaymentHistoryItem[]>(apiRoutes.payments.history, { method: "GET" }, true)
      .then(items => items.map(toPaymentHistory));
  },

  createRefundRequest(paymentOrderId: string, reason: string) {
    return request<BackendRefundRequest>(apiRoutes.payments.refundRequest(paymentOrderId), {
      method: "POST",
      body: JSON.stringify({ reason }),
    }, true).then(toRefundRequest);
  },

  listRefundRequests() {
    return request<BackendRefundRequest[]>(apiRoutes.payments.refundRequests, { method: "GET" }, true)
      .then(items => items.map(toRefundRequest));
  },

  listSubscriptionPlans() {
    return request<SubscriptionPlan[]>(apiRoutes.subscriptions.plans, { method: "GET" });
  },

  getCurrentSubscription() {
    return request<BackendSubscriptionStatus | null>(apiRoutes.subscriptions.me, { method: "GET" }, true)
      .then(toSubscriptionStatus);
  },

  cancelSubscription() {
    return request<BackendSubscriptionStatus | null>(apiRoutes.subscriptions.cancel, { method: "POST" }, true)
      .then(toSubscriptionStatus);
  },

  validatePromoCode(code: string) {
    return request<{ valid: boolean; discountPercent?: number | null }>(
      apiRoutes.subscriptions.validatePromoCode(code),
      { method: "GET" },
      true,
    );
  },
};
