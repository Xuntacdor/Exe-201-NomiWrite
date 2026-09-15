const auth = "/api/auth";

export const apiRoutes = {
  auth: {
    login: `${auth}/login`,
    register: `${auth}/register`,
    google: `${auth}/google`,
    refresh: `${auth}/refresh`,
    logout: `${auth}/logout`,
    verifyEmail: `${auth}/verify-email`,
    resendVerificationEmail: `${auth}/resend-verification-email`,
    forgotPassword: `${auth}/forgot-password`,
    resetPassword: `${auth}/reset-password`,
    deactivate: `${auth}/deactivate`,
  },
  users: {
    me: "/api/users/me",
    account: "/api/users/me/account",
    progress: "/api/users/me/progress",
  },
  writing: {
    types: "/api/writing/types",
    prompts: "/api/writing/prompts",
    promptDetail: (id: string) => `/api/writing/prompts/${id}`,
    sampleAnswer: (id: string) => `/api/writing/prompts/${id}/sample-answer`,
  },
  submissions: {
    list: "/api/writing/submissions",
    detail: (id: string) => `/api/writing/submissions/${id}`,
    submit: (id: string) => `/api/writing/submissions/${id}/submit`,
    timeRemaining: (id: string) => `/api/writing/submissions/${id}/time-remaining`,
  },
  feedback: {
    detail: (submissionId: string) => `/api/grading/submissions/${submissionId}`,
    history: "/api/grading/history",
    compare: (submissionId: string) => `/api/grading/submissions/${submissionId}/compare`,
    requestTutorReview: (submissionId: string) => `/api/grading/submissions/${submissionId}/request-tutor-review`,
    tutorReviewRequests: "/api/grading/tutor-review-requests",
    flag: (gradingResultId: string) => `/api/grading/results/${gradingResultId}/flag`,
  },
  dashboard: {
    summary: "/api/dashboard/summary",
  },
  vocabulary: {
    list: "/api/vocabulary",
    mastered: (id: string) => `/api/vocabulary/${id}/mastered`,
  },
  quizzes: {
    generate: "/api/quizzes/generate",
    detail: (id: string) => `/api/quizzes/${id}`,
  },
  quizAttempts: {
    create: "/api/quizzes/attempts",
  },
  payments: {
    checkout: "/api/payment",
    status: (id: string) => `/api/payment/${id}`,
    history: "/api/payment/history",
    refundRequest: (id: string) => `/api/payment/${id}/refund-request`,
    refundRequests: "/api/payment/refund-requests",
  },
  subscriptions: {
    plans: "/api/subscriptions/plans",
    me: "/api/subscriptions/me",
    cancel: "/api/subscriptions/me/cancel",
    validatePromoCode: (code: string) => `/api/subscriptions/promo-codes/${encodeURIComponent(code)}/validate`,
  },
} as const;
