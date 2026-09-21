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
};
