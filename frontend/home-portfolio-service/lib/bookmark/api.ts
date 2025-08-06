import { authenticatedClient } from '@/lib/authenticated-client';

const API_BASE_URL = process.env.NEXT_PUBLIC_PORTFOLIO_API_URL || 'http://localhost:5201';

export interface BookmarkToggleRequest {
  userId: string;
  portfolioId: string;
  collectionName?: string;
  notes?: string;
}

export interface BookmarkResponse {
  id: string;
  userId: string;
  portfolioId: string;
  collectionName?: string;
  notes?: string;
  createdAt: string;
}

export interface BookmarkToggleResponse {
  isBookmarked: boolean;
  bookmark?: BookmarkResponse;
  message: string;
}

export interface BookmarkCheckResponse {
  isBookmarked: boolean;
}

export const bookmarkApi = {
  /**
   * Toggle bookmark state (add/remove)
   */
  async toggleBookmark(request: BookmarkToggleRequest): Promise<BookmarkToggleResponse> {
    try {
      return await authenticatedClient.post<BookmarkToggleResponse>(
        `${API_BASE_URL}/api/bookmark/toggle`,
        request
      );
    } catch (error) {
      console.error('Error toggling bookmark:', error);
      throw error;
    }
  },

  /**
   * Check if a portfolio is bookmarked by a user
   */
  async checkBookmark(userId: string, portfolioId: string): Promise<BookmarkCheckResponse> {
    try {
      return await authenticatedClient.get<BookmarkCheckResponse>(
        `${API_BASE_URL}/api/bookmark/check/${userId}/${portfolioId}`
      );
    } catch (error) {
      console.error('Error checking bookmark:', error);
      throw error;
    }
  },

  /**
   * Get all bookmarks for a user
   */
  async getUserBookmarks(userId: string): Promise<BookmarkResponse[]> {
    try {
      return await authenticatedClient.get<BookmarkResponse[]>(
        `${API_BASE_URL}/api/bookmark/user/${userId}`
      );
    } catch (error) {
      console.error('Error getting user bookmarks:', error);
      throw error;
    }
  },
}; 