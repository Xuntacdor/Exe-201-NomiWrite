import { ApiRequestError } from "../api/real-client";
import { clearSession } from "./session";

type RouterLike = {
  replace(path: string): void;
};

export function handleAuthFailure(error: unknown, router: RouterLike) {
  const isUnauthorized =
    (error instanceof ApiRequestError && error.status === 401) ||
    (error instanceof Error && error.message.includes("status 401"));

  if (!isUnauthorized) return false;

  clearSession();
  router.replace("/login");
  return true;
}
