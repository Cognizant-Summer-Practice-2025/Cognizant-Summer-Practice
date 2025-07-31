export interface Bookmark {
  id: string;
  userId: string;
  portfolioId: string;
  portfolioTitle?: string;
  portfolioOwnerName?: string;
  createdAt: string;
}

export interface AddBookmarkRequest {
  portfolioId: string;
  portfolioTitle?: string;
  portfolioOwnerName?: string;
}

export interface IsBookmarkedResponse {
  isBookmarked: boolean;
} 