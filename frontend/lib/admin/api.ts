import { AdminUser, AdminPortfolio, AdminStats, UserWithPortfolio, PortfolioWithOwner } from './interfaces';

const USER_API_BASE = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200/api/users';
const PORTFOLIO_API_BASE = process.env.NEXT_PUBLIC_PORTFOLIO_API_URL || 'http://localhost:5201';

export class AdminAPI {
  // User-related API calls
  static async getAllUsers(): Promise<AdminUser[]> {
    const response = await fetch(`${USER_API_BASE}`);
    if (!response.ok) {
      throw new Error('Failed to fetch users');
    }
    return response.json();
  }

  static async getUserById(id: string): Promise<AdminUser> {
    const response = await fetch(`${USER_API_BASE}/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch user');
    }
    return response.json();
  }

  static async updateUser(id: string, data: Partial<AdminUser>): Promise<AdminUser> {
    const response = await fetch(`${USER_API_BASE}/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error('Failed to update user');
    }
    return response.json();
  }

  // Portfolio-related API calls
  static async getAllPortfolios(): Promise<AdminPortfolio[]> {
    const response = await fetch(`${PORTFOLIO_API_BASE}/api/Portfolio`);
    if (!response.ok) {
      throw new Error('Failed to fetch portfolios');
    }
    return response.json();
  }

  static async getPortfolioById(id: string): Promise<AdminPortfolio> {
    const response = await fetch(`${PORTFOLIO_API_BASE}/api/Portfolio/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch portfolio');
    }
    return response.json();
  }

  static async getPortfoliosByUserId(userId: string): Promise<AdminPortfolio[]> {
    const response = await fetch(`${PORTFOLIO_API_BASE}/api/Portfolio/user/${userId}`);
    if (!response.ok) {
      throw new Error('Failed to fetch user portfolios');
    }
    return response.json();
  }

  static async deletePortfolio(portfolioId: string): Promise<boolean> {
    const deleteUrl = `${PORTFOLIO_API_BASE}/api/Portfolio/${portfolioId}`;
    
    try {
      const response = await fetch(deleteUrl, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('🗑️ Admin API: Delete error response:', errorText);
        console.error('🗑️ Admin API: Delete error status text:', response.statusText);
        throw new Error(`Failed to delete portfolio (${response.status}): ${errorText || response.statusText}`);
      }
      
      return true;
    } catch (error) {
      console.error('🗑️ Admin API: Network or other error:', error);
      throw error;
    }
  }

  // Combined data for admin views
  static async getUsersWithPortfolios(): Promise<UserWithPortfolio[]> {
    try {
      const [users, portfolios] = await Promise.all([
        this.getAllUsers(),
        this.getAllPortfolios()
      ]);

      return users.map(user => {
        const userPortfolios = portfolios.filter(p => p.userId === user.id);
        const publishedPortfolio = userPortfolios.find(p => p.isPublished);
        const draftPortfolio = userPortfolios.find(p => !p.isPublished);
        
        let portfolioStatus: 'Published' | 'Draft' | 'None' = 'None';
        let portfolioId: string | undefined;
        let portfolioTitle: string | undefined;

        if (publishedPortfolio) {
          portfolioStatus = 'Published';
          portfolioId = publishedPortfolio.id;
          portfolioTitle = publishedPortfolio.title;
        } else if (draftPortfolio) {
          portfolioStatus = 'Draft';
          portfolioId = draftPortfolio.id;
          portfolioTitle = draftPortfolio.title;
        }

        return {
          ...user,
          portfolioStatus,
          portfolioId,
          portfolioTitle,
          joinedDate: new Date().toISOString(), // Since we don't have this in the user model
        };
      });
    } catch (error) {
      console.error('Error fetching users with portfolios:', error);
      throw error;
    }
  }

  static async getPortfoliosWithOwners(): Promise<PortfolioWithOwner[]> {
    try {
      const [portfolios, users] = await Promise.all([
        this.getAllPortfolios(),
        this.getAllUsers()
      ]);

      return portfolios.map(portfolio => {
        const owner = users.find(u => u.id === portfolio.userId);
        return {
          ...portfolio,
          ownerName: owner ? `${owner.firstName || ''} ${owner.lastName || ''}`.trim() || owner.username : 'Unknown User',
          ownerEmail: owner?.email || '',
          ownerAvatar: owner?.avatarUrl,
        };
      });
    } catch (error) {
      console.error('Error fetching portfolios with owners:', error);
      throw error;
    }
  }

  // Statistics calculation
  static async getAdminStats(): Promise<AdminStats> {
    try {
      const [users, portfolios] = await Promise.all([
        this.getAllUsers(),
        this.getAllPortfolios()
      ]);

      const activeUsers = users.filter(u => u.isActive);
      const publishedPortfolios = portfolios.filter(p => p.isPublished);
      const draftPortfolios = portfolios.filter(p => !p.isPublished);
      const totalViews = portfolios.reduce((sum, p) => sum + p.viewCount, 0);

      // Calculate new users this month (approximation since we don't have createdAt for users)
      const currentMonth = new Date().getMonth();
      const currentYear = new Date().getFullYear();
      const newPortfoliosThisMonth = portfolios.filter(p => {
        const createdDate = new Date(p.createdAt);
        return createdDate.getMonth() === currentMonth && createdDate.getFullYear() === currentYear;
      }).length;

      return {
        totalUsers: activeUsers.length,
        activePortfolios: publishedPortfolios.length,
        totalProjects: 0, // We'll need to add project counting when we have the endpoint
        newThisMonth: newPortfoliosThisMonth,
        totalBlogPosts: 0, // We'll need to add blog post counting when we have the endpoint
        publishedPortfolios: publishedPortfolios.length,
        draftPortfolios: draftPortfolios.length,
        totalViews,
      };
    } catch (error) {
      console.error('Error calculating admin stats:', error);
      throw error;
    }
  }
} 