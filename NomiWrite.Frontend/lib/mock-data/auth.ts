import type { AuthResponse, User } from "../types";

export const mockUser: User = {
  id: "00000000-0000-0000-0000-000000000001",
  email: "student@nomiwrite.local",
  displayName: "NomiWrite Student",
  currentLevel: "B1",
  targetType: "IELTS Writing",
  targetBand: 7,
  plan: "free",
  createdAt: "2026-09-05T00:00:00.000Z",
};

export function createMockAuthResponse(email: string): AuthResponse {
  return {
    accessToken: "mock-access-token",
    refreshToken: "mock-refresh-token",
    expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    userId: mockUser.id,
    email,
    fullName: mockUser.displayName,
    role: "Student",
  };
}
