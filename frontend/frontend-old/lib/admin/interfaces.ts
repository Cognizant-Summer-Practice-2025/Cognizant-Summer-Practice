export interface AdminUser {
  id: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  avatarUrl?: string;
  isActive: boolean;
  isAdmin: boolean;
  lastLoginAt?: string;
  createdAt: string; // Now required from backend
  updatedAt: string; // Added from backend
}

export interface AdminPortfolio {
  id: string;
  userId: string;
  templateId: string;
  title: string;
  bio?: string;
  viewCount: number;
  likeCount: number;
  visibility: 'Public' | 'Private' | 'Unlisted';
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
  template?: {
    id: string;
    name: string;
  };
}

export interface AdminBlogPost {
  id: string;
  portfolioId: string;
  title: string;
  excerpt?: string;
  content?: string;
  featuredImageUrl?: string;
  tags?: string[];
  isPublished: boolean;
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface AdminStats {
  totalUsers: number;
  activePortfolios: number;
  totalProjects: number;
  newThisMonth: number;
  totalBlogPosts: number;
  publishedPortfolios: number;
  draftPortfolios: number;
  totalViews: number;
}

export interface UserWithPortfolio extends AdminUser {
  portfolioStatus: 'Published' | 'Draft' | 'None';
  portfolioId?: string;
  portfolioTitle?: string;
  joinedDate: string;
}

export interface PortfolioWithOwner extends AdminPortfolio {
  ownerName: string;
  ownerEmail: string;
  ownerAvatar?: string;
} 