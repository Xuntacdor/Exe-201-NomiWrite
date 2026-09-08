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

  getMe() {
    return request<User>(apiRoutes.users.me, { method: "GET" }, true);
  },

  updateMe(requestBody) {
    return request<User>(apiRoutes.users.me, {
      method: "PATCH",
      body: JSON.stringify(requestBody),
    }, true);
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
    const amount = requestBody.amount ?? (requestBody.billingCycle === "monthly" ? 199_000 : 159_000 * 12);

    return request<BackendPaymentResponse>(apiRoutes.payments.checkout, {
      method: "POST",
      body: JSON.stringify({
        amount,
        currency: requestBody.currency ?? "VND",
        provider: toPaymentProvider(requestBody.paymentMethod),
      }),
    }, true).then(toCheckoutResponse);
  },

  getPaymentStatus(id: string) {
    return request<BackendPaymentResponse>(apiRoutes.payments.status(id), { method: "GET" }, true)
      .then(toCheckoutResponse);
  },
};
