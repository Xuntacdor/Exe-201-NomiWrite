export interface AdminSession {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  fullName: string;
  role: string;
}

const tokenKey = "token";
const sessionKey = "nomiwrite_admin_session";
const adminRoles = new Set(["admin", "moderator"]);

export function saveAdminSession(session: AdminSession) {
  localStorage.setItem(tokenKey, session.accessToken);
  localStorage.setItem(sessionKey, JSON.stringify(session));
}

export function getAdminSession(): AdminSession | null {
  const raw = localStorage.getItem(sessionKey);
  if (!raw) return null;

  try {
    return JSON.parse(raw) as AdminSession;
  } catch {
    clearAdminSession();
    return null;
  }
}

export function clearAdminSession() {
  localStorage.removeItem(tokenKey);
  localStorage.removeItem(sessionKey);
}

export function isAdminRole(role: string) {
  return adminRoles.has(role.toLowerCase());
}

export function hasAdminSession() {
  const session = getAdminSession();
  return Boolean(localStorage.getItem(tokenKey) && session && isAdminRole(session.role));
}

export function importSessionFromHash() {
  const hash = window.location.hash;
  if (!hash.startsWith("#session=")) return false;

  const params = new URLSearchParams(hash.slice("#session=".length));
  const session: AdminSession = {
    accessToken: params.get("accessToken") ?? "",
    refreshToken: params.get("refreshToken") ?? "",
    expiresAt: params.get("expiresAt") ?? "",
    userId: params.get("userId") ?? "",
    email: params.get("email") ?? "",
    fullName: params.get("fullName") ?? "",
    role: params.get("role") ?? "",
  };

  if (!session.accessToken || !isAdminRole(session.role)) {
    clearAdminSession();
    window.history.replaceState(null, "", window.location.pathname);
    return false;
  }

  saveAdminSession(session);
  window.history.replaceState(null, "", window.location.pathname);
  return true;
}
