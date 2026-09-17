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
  role: number;
  accountStatus: number;
  isEmailVerified: boolean;
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
};
