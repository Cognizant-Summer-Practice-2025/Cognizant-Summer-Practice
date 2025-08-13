import { AdminUser, AdminPortfolio, AdminStats, UserWithPortfolio, PortfolioWithOwner } from './interfaces';
import { AdminStatsUtils, AdminChartUtils, AdminTransformUtils } from './utils';
import { authenticatedClient } from '@/lib/authenticated-client';
import { Logger } from '@/lib/logger';

const USER_API_BASE = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';
const PORTFOLIO_API_BASE = process.env.NEXT_PUBLIC_PORTFOLIO_API_URL || 'http://localhost:5201';

export class AdminAPI {
  // User-related API calls
  static async getAllUsers(): Promise<AdminUser[]> {
    return authenticatedClient.get<AdminUser[]>(`${USER_API_BASE}/api/users`);
  }

  static async getUserById(id: string): Promise<AdminUser> {
    return authenticatedClient.get<AdminUser>(`${USER_API_BASE}/api/users/${id}`);
  }

  static async updateUser(id: string, data: Partial<AdminUser>): Promise<AdminUser> {
    return authenticatedClient.put<AdminUser>(`${USER_API_BASE}/api/users/${id}`, data);
  }

  // Portfolio-related API calls
  static async getAllPortfolios(): Promise<AdminPortfolio[]> {
    return authenticatedClient.get<AdminPortfolio[]>(`${PORTFOLIO_API_BASE}/api/Portfolio`);
  }

  static async getPortfolioById(id: string): Promise<AdminPortfolio> {
    return authenticatedClient.get<AdminPortfolio>(`${PORTFOLIO_API_BASE}/api/Portfolio/${id}`);
  }

  static async getPortfoliosByUserId(userId: string): Promise<AdminPortfolio[]> {
    return authenticatedClient.get<AdminPortfolio[]>(`${PORTFOLIO_API_BASE}/api/Portfolio/user/${userId}`);
  }

  static async deletePortfolio(portfolioId: string): Promise<boolean> {
    try {
      await authenticatedClient.deleteVoid(`${PORTFOLIO_API_BASE}/api/Portfolio/${portfolioId}`);
      return true;
    } catch (error) {
      Logger.error('🗑️ Admin API: Error deleting portfolio', error);
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
      Logger.error('Error fetching users with portfolios', error);
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
      Logger.error('Error fetching portfolios with owners', error);
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
      Logger.error('Error calculating admin stats', error);
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
      Logger.error('Error fetching growth data', error);
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
      Logger.error('Error fetching activity data', error);
      throw error;
    }
  }
} 