import type { ApiClient, ApiMode } from "../types";
import { mockClient } from "./mock-client";
import { realClient } from "./real-client";

export const apiMode: ApiMode =
  process.env.NEXT_PUBLIC_API_MODE === "real" ? "real" : "mock";

export const apiClient: ApiClient = apiMode === "real" ? realClient : mockClient;
