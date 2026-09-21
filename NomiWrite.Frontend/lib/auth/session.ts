import type { AuthResponse } from "../types";

const sessionKey = "nomiwrite_session";
const adminRoles = new Set(["admin", "moderator", "1", "2"]);

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
  return adminRoles.has(resolveRole(session).toLowerCase());
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
    role: resolveRole(session),
  });

  adminUrl.hash = `session=${params.toString()}`;
  window.location.assign(adminUrl.toString());
}

function resolveRole(session: AuthResponse) {
  const directRole = normalizeRole(session.role);
  if (directRole) return directRole;

  const payload = decodeJwtPayload(session.accessToken);
  const role =
    payload?.role ??
    payload?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

  return normalizeRole(role);
}

function normalizeRole(role: unknown) {
  return String(role ?? "").trim();
}

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const [, payload] = token.split(".");
  if (!payload) return null;

  try {
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = normalized.padEnd(normalized.length + ((4 - (normalized.length % 4)) % 4), "=");
    return JSON.parse(window.atob(padded)) as Record<string, unknown>;
  } catch {
    return null;
  }
}
