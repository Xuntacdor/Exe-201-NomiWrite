import type {
  ApiClient,
  AuthResponse,
  CheckoutResponse,
  LoginRequest,
  RegisterRequest,
  SubmitSubmissionRequest,
  UpdateVocabularyMasteredRequest,
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

  async getMe() {
    await delay();
    return mockUser;
  },

  async updateMe(request) {
    await delay();
    return { ...mockUser, ...request };
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

  async submitSubmission(request: SubmitSubmissionRequest) {
    await delay();
    const submittedAt = new Date().toISOString();
    return {
      id: `sub_${Date.now()}`,
      userId: mockUser.id,
      wordCount: request.content.trim() ? request.content.trim().split(/\s+/).length : 0,
      submittedAt,
      status: "submitted",
      ...request,
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

  async gradeSubmission() {
    await delay();
    return mockFeedback;
  },

  async getFeedback() {
    await delay();
    return mockFeedback;
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
};
