import type {
  ApiClient,
  AuthResponse,
  CheckoutRequest,
  CheckoutResponse,
  DashboardSummary,
  GenerateQuizRequest,
  LoginRequest,
  Quiz,
  QuizAttempt,
  RegisterRequest,
  SubmitQuizAttemptRequest,
  Submission,
  SubmitSubmissionRequest,
  UpdateVocabularyMasteredRequest,
  User,
  VocabSuggestion,
  WritingFeedback,
  WritingPrompt,
  WritingType,
} from "../types";
import { apiRoutes } from "./routes";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";

async function request<T>(path: string, init: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init.headers,
    },
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with status ${response.status}`);
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

  logout(userId: string) {
    return request<void>(apiRoutes.auth.logout(userId), {
      method: "POST",
    });
  },

  getMe() {
    return request<User>(apiRoutes.users.me, { method: "GET" });
  },

  updateMe(requestBody) {
    return request<User>(apiRoutes.users.me, {
      method: "PATCH",
      body: JSON.stringify(requestBody),
    });
  },

  listWritingTypes() {
    return request<WritingType[]>(apiRoutes.writing.types, { method: "GET" });
  },

  listWritingPrompts(writingType?: string, topic?: string) {
    const params = new URLSearchParams();
    if (writingType) params.set("type", writingType);
    if (topic) params.set("topic", topic);
    const query = params.toString();
    return request<WritingPrompt[]>(`${apiRoutes.writing.prompts}${query ? `?${query}` : ""}`, { method: "GET" });
  },

  submitSubmission(requestBody: SubmitSubmissionRequest) {
    return request<Submission>(apiRoutes.submissions.list, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  listSubmissions() {
    return request<Submission[]>(apiRoutes.submissions.list, { method: "GET" });
  },

  getSubmission(id: string) {
    return request<Submission>(apiRoutes.submissions.detail(id), { method: "GET" });
  },

  gradeSubmission(id: string) {
    return request<WritingFeedback>(apiRoutes.submissions.grade(id), { method: "POST" });
  },

  getFeedback(submissionId: string) {
    return request<WritingFeedback>(apiRoutes.feedback.detail(submissionId), { method: "GET" });
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
    return request<CheckoutResponse>(apiRoutes.payments.checkout, {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  getPaymentStatus(id: string) {
    return request<CheckoutResponse>(apiRoutes.payments.status(id), { method: "GET" });
  },
};
