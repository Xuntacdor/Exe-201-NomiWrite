import type {
  ApiClient,
  AuthResponse,
  CheckoutResponse,
  GoogleLoginRequest,
  GradingHistoryItem,
  LoginRequest,
  RegisterRequest,
  StudyGuide,
  Submission,
  WritingFeedback,
  SubmitSubmissionRequest,
  SubmissionTimeRemaining,
  UpdateVocabularyMasteredRequest,
  UserProgress,
} from "../types";
import {
  mockDashboardSummary,
  mockQuiz,
  mockStudyGuide,
  mockSubmissions,
  mockVocabulary,
  mockWritingPrompts,
  mockWritingTypes,
} from "../mock-data/app-fixtures";
import { createMockAuthResponse, mockUser } from "../mock-data/auth";

const delay = (ms = 350) => new Promise(resolve => setTimeout(resolve, ms));
const runtimeSubmissions: Submission[] = [...mockSubmissions];

function createFeedbackForSubmission(submission: Submission): WritingFeedback {
  return {
    id: `grade_${submission.id}`,
    submission: {
      ...submission,
      status: "graded",
      overallScore: submission.overallScore ?? 6.5,
      overallFeedback: submission.overallFeedback ?? "Clear response to the selected prompt. Add more specific examples and tighten sentence control to improve the score.",
    },
    criteriaScores: {
      taskResponse: 6.5,
      coherenceCohesion: 6,
      lexicalResource: 6,
      grammaticalRangeAccuracy: 6.5,
    },
    grammarErrors: [],
    vocabSuggestions: mockVocabulary.map(item => ({
      ...item,
      submissionId: submission.id,
      userId: submission.userId,
      topic: submission.topic,
    })),
  };
}

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

  async googleLogin(request: GoogleLoginRequest): Promise<AuthResponse> {
    await delay();

    if (!request.idToken) {
      throw new Error("Google id token is required.");
    }

    return createMockAuthResponse("google.user@nomiwrite.local");
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
      if (writingType && prompt.writingTypeId !== writingType && prompt.writingType !== writingType) return false;
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
    const prompt = mockWritingPrompts.find(item => item.id === request.writingPromptId);
    if (!prompt) throw new Error("Prompt not found.");

    const submittedAt = new Date().toISOString();
    const submission: Submission = {
      id: `sub_${Date.now()}`,
      userId: mockUser.id,
      wordCount: request.content.trim() ? request.content.trim().split(/\s+/).length : 0,
      submittedAt,
      status: "graded",
      writingType: prompt.writingType,
      topic: prompt.topic,
      prompt: prompt.prompt,
      content: request.content,
    };
    runtimeSubmissions.unshift(submission);
    return submission;
  },

  async listSubmissions() {
    await delay();
    return runtimeSubmissions;
  },

  async getSubmission(id: string) {
    await delay();
    const submission = runtimeSubmissions.find(item => item.id === id);
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

  async gradeSubmission(id: string) {
    await delay();
    const submission = runtimeSubmissions.find(item => item.id === id);
    if (!submission) throw new Error("Submission not found.");
    return createFeedbackForSubmission(submission);
  },

  async getFeedback(submissionId: string) {
    await delay();
    const submission = runtimeSubmissions.find(item => item.id === submissionId);
    if (!submission) throw new Error("Submission not found.");
    return createFeedbackForSubmission(submission);
  },

  async listGradingHistory(): Promise<GradingHistoryItem[]> {
    await delay();
    return runtimeSubmissions
      .filter(item => typeof item.overallScore === "number")
      .map(item => ({
        id: `grade_${item.id}`,
        submissionId: item.id,
        overallBand: item.overallScore ?? 0,
        createdAt: item.submittedAt,
      }));
  },

  async compareSubmissionFeedback(submissionId: string) {
    await delay();
    const submission = runtimeSubmissions.find(item => item.id === submissionId);
    if (!submission) throw new Error("Submission not found.");
    return {
      current: createFeedbackForSubmission(submission),
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

  async listQuizzes() {
    await delay();
    return [{
      id: mockQuiz.id,
      userId: mockQuiz.userId,
      sourceSubmissionId: mockQuiz.sourceSubmissionId,
      category: "Grammar, Vocabulary",
      questionCount: mockQuiz.questions.length,
      attemptCount: 1,
      latestScore: 0,
      latestTotalQuestions: mockQuiz.questions.length,
      latestAttemptedAt: new Date().toISOString(),
      createdAt: mockQuiz.createdAt,
    }];
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

  async getStudyGuide(): Promise<StudyGuide | null> {
    await delay();
    return mockStudyGuide;
  },

  async generateStudyGuide() {
    await delay(1400);
    return {
      ...mockStudyGuide,
      id: `study-guide-mock-${Date.now()}`,
      createdAt: new Date().toISOString(),
    };
  },
};
