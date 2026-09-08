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
  },
  writing: {
    types: "/api/writing/types",
    prompts: "/api/writing/prompts",
  },
  submissions: {
    list: "/api/submissions",
    detail: (id: string) => `/api/submissions/${id}`,
    grade: (id: string) => `/api/submissions/${id}/grade`,
  },
  feedback: {
    detail: (submissionId: string) => `/api/feedback/${submissionId}`,
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
} as const;
