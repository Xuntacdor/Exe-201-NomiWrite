const auth = "/api/auth";

export const apiRoutes = {
  auth: {
    login: `${auth}/login`,
    register: `${auth}/register`,
    refresh: `${auth}/refresh`,
    logout: `${auth}/logout`,
  },
  users: {
    me: "/api/users/me",
    account: "/api/users/me/account",
  },
  writing: {
    types: "/api/writing/types",
    prompts: "/api/writing/prompts",
  },
  submissions: {
    list: "/api/writing/submissions",
    detail: (id: string) => `/api/writing/submissions/${id}`,
    submit: (id: string) => `/api/writing/submissions/${id}/submit`,
  },
  feedback: {
    detail: (submissionId: string) => `/api/grading/submissions/${submissionId}`,
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
    create: "/api/quiz-attempts",
  },
  payments: {
    checkout: "/api/payment",
    status: (id: string) => `/api/payment/${id}`,
  },
  subscriptions: {
    plans: "/api/subscriptions/plans",
    me: "/api/subscriptions/me",
  },
} as const;
