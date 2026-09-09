import type {
  ApiClient,
  AuthResponse,
  CheckoutResponse,
  GradingHistoryItem,
  LoginRequest,
  RegisterRequest,
  SubmitSubmissionRequest,
  SubmissionTimeRemaining,
  UpdateVocabularyMasteredRequest,
  UserProgress,
} from "../types";
import {
  mockDashboardSummary,
  mockFeedback,
  mockQuiz,
  mockSubmissions,
  mockVocabulary,
  mockWritingPrompts,
  mockWritingTypes,
} from "../mock-data/app-fixtures";
import { createMockAuthResponse, mockUser } from "../mock-data/auth";

const delay = (ms = 350) => new Promise(resolve => setTimeout(resolve, ms));

export const mockClient: ApiClient = {
  async login(request: LoginRequest): Promise<AuthResponse> {
    await delay();

    if (!request.email || !request.password) {
      throw new Error("Email and password are required.");
    }

    return createMockAuthResponse(request.email);
  },

  async register(request: RegisterRequest): Promise<AuthResponse> {
    await delay();

    if (!request.email || !request.password || !request.fullName) {
      throw new Error("Name, email, and password are required.");
    }

    return createMockAuthResponse(request.email);
  },

  async refresh(refreshToken: string): Promise<AuthResponse> {
    await delay();
    return createMockAuthResponse(refreshToken ? "student@nomiwrite.local" : "");
  },

  async logout(): Promise<void> {
    await delay(150);
  },

  async verifyEmail() {
    await delay();
    return { success: true, message: "Email verified in mock mode." };
  },

  async resendVerificationEmail(request) {
    await delay();
    return { success: true, message: `Verification email queued for ${request.email}.` };
  },

  async forgotPassword(request) {
    await delay();
    return { success: true, message: `Password reset instructions queued for ${request.email}.` };
  },

  async resetPassword() {
    await delay();
    return { success: true, message: "Password reset in mock mode." };
  },

  async deactivateAccount() {
    await delay();
    return { success: true, message: "Account deactivation requested in mock mode." };
  },

  async getMe() {
    await delay();
    return mockUser;
  },

  async updateMe(request) {
    await delay();
    return { ...mockUser, ...request };
  },

  async getMyAccount() {
    await delay();
    return mockUser;
  },

  async getUserProgress(): Promise<UserProgress> {
    await delay();
    return {
      bandHistory: mockDashboardSummary.scoreTrend.map((band, index) => ({
        date: new Date(Date.UTC(2026, 8, 1 + index)).toISOString(),
        band,
      })),
      strengthsWeaknesses: "Strong idea control; keep building lexical range.",
      currentStreak: 3,
      totalSubmissions: mockSubmissions.length,
      badges: [
        { name: "First submission", achieved: true },
        { name: "Three-day streak", achieved: true },
        { name: "Band 7", achieved: false },
      ],
      targetExam: mockUser.targetType ?? "IELTS",
      targetBand: mockUser.targetBand ?? 7,
      targetExamDate: null,
    };
  },

  async listWritingTypes() {
    await delay();
    return mockWritingTypes;
  },

  async listWritingPrompts(writingType?: string, topic?: string) {
    await delay();
    return mockWritingPrompts.filter(prompt => {
      if (writingType && prompt.writingType !== writingType) return false;
      if (topic && prompt.topic !== topic) return false;
      return true;
    });
  },

  async getWritingPrompt(id: string) {
    await delay();
    const prompt = mockWritingPrompts.find(item => item.id === id);
    if (!prompt) throw new Error("Prompt not found.");
    return prompt;
  },

  async getPromptSampleAnswer(id: string) {
    await delay();
    const prompt = mockWritingPrompts.find(item => item.id === id);
    if (!prompt) return null;
    return "A clear response should state a position, develop two main ideas, and close with a concise conclusion.";
  },

  async submitSubmission(request: SubmitSubmissionRequest) {
    await delay();
    const submittedAt = new Date().toISOString();
    return {
      id: `sub_${Date.now()}`,
      userId: mockUser.id,
      wordCount: request.content.trim() ? request.content.trim().split(/\s+/).length : 0,
      submittedAt,
      status: "submitted",
      writingType: "Mock writing",
      topic: "Mock prompt",
      prompt: "Mock prompt",
      content: request.content,
    };
  },

  async listSubmissions() {
    await delay();
    return mockSubmissions;
  },

  async getSubmission(id: string) {
    await delay();
    const submission = mockSubmissions.find(item => item.id === id);
    if (!submission) throw new Error("Submission not found.");
    return submission;
  },

  async getSubmissionTimeRemaining(): Promise<SubmissionTimeRemaining> {
    await delay();
    return {
      deadlineAt: null,
      secondsRemaining: 0,
      isTimed: false,
    };
  },

  async gradeSubmission() {
    await delay();
    return mockFeedback;
  },

  async getFeedback() {
    await delay();
    return mockFeedback;
  },

  async listGradingHistory(): Promise<GradingHistoryItem[]> {
    await delay();
    return mockSubmissions
      .filter(item => typeof item.overallScore === "number")
      .map(item => ({
        id: `grade_${item.id}`,
        submissionId: item.id,
        overallBand: item.overallScore ?? 0,
        createdAt: item.submittedAt,
      }));
  },

  async compareSubmissionFeedback() {
    await delay();
    return {
      current: mockFeedback,
      previous: null,
      bandDifference: null,
    };
  },

  async requestTutorReview(submissionId: string) {
    await delay();
    return {
      id: `tutor_${Date.now()}`,
      submissionId,
      status: "Pending",
      requestedAt: new Date().toISOString(),
    };
  },

  async listTutorReviewRequests() {
    await delay();
    return [];
  },

  async flagFeedback(gradingResultId: string, request) {
    await delay();
    return {
      id: `flag_${Date.now()}`,
      gradingResultId,
      reason: request.reason,
      createdAt: new Date().toISOString(),
    };
  },

  async getDashboardSummary() {
    await delay();
    return mockDashboardSummary;
  },

  async listVocabulary() {
    await delay();
    return mockVocabulary;
  },

  async updateVocabularyMastered(id: string, request: UpdateVocabularyMasteredRequest) {
    await delay();
    const word = mockVocabulary.find(item => item.id === id);
    if (!word) throw new Error("Vocabulary item not found.");
    return { ...word, isMastered: request.isMastered };
  },

  async generateQuiz() {
    await delay();
    return mockQuiz;
  },

  async getQuiz() {
    await delay();
    return mockQuiz;
  },

  async submitQuizAttempt(request) {
    await delay();
    return {
      id: `attempt_${Date.now()}`,
      quizId: request.quizId,
      userId: mockUser.id,
      answers: request.answers,
      score: 0,
      attemptedAt: new Date().toISOString(),
    };
  },

  async createCheckout(): Promise<CheckoutResponse> {
    await delay();
    return {
      id: `pay_${Date.now()}`,
      status: "pending",
    };
  },

  async getPaymentStatus(id: string): Promise<CheckoutResponse> {
    await delay();
    return {
      id,
      status: "pending",
    };
  },

  async listPaymentHistory() {
    await delay();
    return [];
  },

  async createRefundRequest(paymentOrderId: string, reason: string) {
    await delay();
    return {
      id: `refund_${Date.now()}`,
      paymentOrderId,
      reason,
      status: "Pending",
      requestedAt: new Date().toISOString(),
      createdAt: new Date().toISOString(),
    };
  },

  async listRefundRequests() {
    await delay();
    return [];
  },

  async listSubscriptionPlans() {
    await delay();
    return [
      {
        id: "mock-monthly-plan",
        name: "Premium Monthly",
        description: "Mock monthly premium plan.",
        price: 199000,
        currency: "VND",
        billingCycle: "Monthly",
        durationDays: 30,
      },
      {
        id: "mock-yearly-plan",
        name: "Premium Yearly",
        description: "Mock yearly premium plan.",
        price: 1908000,
        currency: "VND",
        billingCycle: "Yearly",
        durationDays: 365,
      },
    ];
  },

  async getCurrentSubscription() {
    await delay();
    return { hasSubscription: false, status: null };
  },

  async cancelSubscription() {
    await delay();
    return { hasSubscription: false, status: null };
  },

  async validatePromoCode(code: string) {
    await delay();
    return {
      valid: code.trim().toUpperCase() === "NOMI20",
      discountPercent: code.trim().toUpperCase() === "NOMI20" ? 20 : null,
    };
  },
};
