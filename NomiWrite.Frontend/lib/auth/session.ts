import type { AuthResponse } from "../types";

const sessionKey = "nomiwrite_session";

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
