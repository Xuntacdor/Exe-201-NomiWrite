import type { ApiClient, ApiMode } from "../types";
import { realClient } from "./real-client";

export const apiMode: ApiMode = "real";

export const apiClient: ApiClient = realClient;
