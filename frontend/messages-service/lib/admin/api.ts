import { AdminUser, AdminPortfolio, AdminStats, UserWithPortfolio, PortfolioWithOwner } from './interfaces';
import { AdminStatsUtils, AdminChartUtils, AdminTransformUtils } from './utils';

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

      return AdminTransformUtils.transformUsersWithPortfolios(users, portfolios);
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

      return AdminTransformUtils.transformPortfoliosWithOwners(portfolios, users);
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

      return AdminStatsUtils.calculateStats(users, portfolios);
    } catch (error) {
      console.error('Error calculating admin stats:', error);
      throw error;
    }
  }

  // Growth data for charts
  static async getUserGrowthData(): Promise<{ month: string; users: number; portfolios: number }[]> {
    try {
      const [users, portfolios] = await Promise.all([
        this.getAllUsers(),
        this.getAllPortfolios()
      ]);

      return AdminChartUtils.generateGrowthData(users, portfolios, 6);
    } catch (error) {
      console.error('Error fetching growth data:', error);
      throw error;
    }
  }

  static async getActivityData(): Promise<{ name: string; value: number }[]> {
    try {
      const [users, portfolios] = await Promise.all([
        this.getAllUsers(),
        this.getAllPortfolios()
      ]);

      return AdminChartUtils.generateActivityData(users, portfolios);
    } catch (error) {
      console.error('Error fetching activity data:', error);
      throw error;
    }
  }
} 