import { apiClient } from '../lib/apiClient';

export interface UserAnalyticsDto {
  totalUsers: number;
  activeUsers: number;
  bannedUsers: number;
}

export interface AdminUserListItemDto {
  id: string;
  email: string;
  fullName: string;
  role: number | string;
  accountStatus: number | string;
  isEmailVerified: boolean;
  createdAt: string;
}

export interface AdminPromptListItemDto {
  id: string;
  writingTypeId: string;
  writingTypeName?: string;
  title: string;
  difficulty: number | string;
  isActive: boolean;
  timeLimitMinutes?: number | null;
  minWords?: number | null;
  maxWords?: number | null;
  imageUrl?: string | null;
  isVipOnly: boolean;
  createdAt: string;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AiGradingConfigDto {
  id: string;
  providerName: string;
  modelName: string;
  fallbackModelName?: string | null;
  hasStoredApiKey: boolean;
  apiKeyHint?: string | null;
  temperature?: number | null;
  systemPromptTemplate?: string | null;
  maxOutputTokens?: number | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface UpdateAiGradingConfigRequest {
  providerName: string;
  modelName: string;
  fallbackModelName?: string | null;
  apiKey?: string;
  clearApiKey?: boolean;
  temperature?: number | null;
  systemPromptTemplate?: string | null;
  maxOutputTokens?: number | null;
}

export const adminService = {
  getAnalytics: async (): Promise<UserAnalyticsDto> => {
    const response = await apiClient.get('/users/analytics');
    return response.data;
  },

  getUsers: async (page = 1, pageSize = 20): Promise<PagedResultDto<AdminUserListItemDto>> => {
    const response = await apiClient.get('/users', {
      params: { page, pageSize },
    });
    return response.data;
  },

  getPrompts: async (page = 1, pageSize = 50): Promise<PagedResultDto<AdminPromptListItemDto>> => {
    const response = await apiClient.get('/prompts', {
      params: { page, pageSize },
    });
    return response.data;
  },

  updateUserStatus: async (id: string, status: number | string): Promise<void> => {
    await apiClient.patch(`/users/${id}/status`, { status });
  },

  updateUserRole: async (id: string, role: number | string): Promise<void> => {
    await apiClient.patch(`/users/${id}/role`, { role });
  },

  getAiConfig: async (): Promise<AiGradingConfigDto> => {
    const response = await apiClient.get('/ai-config');
    return response.data;
  },

  updateAiConfig: async (request: UpdateAiGradingConfigRequest): Promise<AiGradingConfigDto> => {
    const response = await apiClient.put('/ai-config', request);
    return response.data;
  },
};
