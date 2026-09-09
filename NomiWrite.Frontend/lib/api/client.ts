import type { ApiClient } from "../types";
import { realClient } from "./real-client";

export const apiClient: ApiClient = realClient;
