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
      const response = await fetch(`${API_BASE_URL}/api/bookmark/toggle`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to toggle bookmark: ${response.status} ${errorText}`);
      }

      return await response.json();
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
      const response = await fetch(`${API_BASE_URL}/api/bookmark/check/${userId}/${portfolioId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to check bookmark: ${response.status} ${errorText}`);
      }

      return await response.json();
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
      const response = await fetch(`${API_BASE_URL}/api/bookmark/user/${userId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to get user bookmarks: ${response.status} ${errorText}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Error getting user bookmarks:', error);
      throw error;
    }
  },
}; 