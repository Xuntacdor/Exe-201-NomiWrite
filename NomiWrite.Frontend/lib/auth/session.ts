import type { AuthResponse } from "../types";

const sessionKey = "nomiwrite_session";
const adminRoles = new Set(["admin", "moderator"]);

type RouterLike = {
  push: (href: string) => void;
};

export function saveSession(session: AuthResponse) {
  localStorage.setItem(sessionKey, JSON.stringify(session));
}

export function getSession(): AuthResponse | null {
  const raw = localStorage.getItem(sessionKey);
  if (!raw) return null;

  try {
    return JSON.parse(raw) as AuthResponse;
  } catch {
    localStorage.removeItem(sessionKey);
    return null;
  }
}

export function clearSession() {
  localStorage.removeItem(sessionKey);
}

export function isAdminSession(session: AuthResponse) {
  return adminRoles.has(session.role.toLowerCase());
}

export function redirectAfterAuth(session: AuthResponse, router: RouterLike) {
  if (!isAdminSession(session)) {
    router.push("/dashboard");
    return;
  }

  const adminUrl = new URL(process.env.NEXT_PUBLIC_ADMIN_APP_URL ?? "http://localhost:5173");
  const params = new URLSearchParams({
    accessToken: session.accessToken,
    refreshToken: session.refreshToken,
    expiresAt: session.expiresAt,
    userId: session.userId,
    email: session.email,
    fullName: session.fullName,
    role: session.role,
  });

  adminUrl.hash = `session=${params.toString()}`;
  window.location.assign(adminUrl.toString());
}
