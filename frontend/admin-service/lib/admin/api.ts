import { AdminUser, AdminPortfolio, AdminStats, UserWithPortfolio, PortfolioWithOwner } from './interfaces';
import { AdminStatsUtils, AdminChartUtils, AdminTransformUtils } from './utils';
import { authenticatedClient } from '@/lib/authenticated-client';
import { Logger } from '@/lib/logger';

const USER_API_BASE = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5002'; // Fixed port
const PORTFOLIO_API_BASE = process.env.NEXT_PUBLIC_PORTFOLIO_API_URL || 'http://localhost:5201';
const MESSAGES_API_BASE = process.env.NEXT_PUBLIC_MESSAGES_API_URL || 'http://localhost:5003'; // Added messages service

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

  static async deleteUser(userId: string): Promise<void> {
    try {
      // The user service now handles cascade deletion across all services
      await authenticatedClient.deleteVoid(`${USER_API_BASE}/api/users/admin/${userId}`);
      Logger.info('Successfully deleted user and all related data', { userId });
    } catch (error) {
      Logger.error('Error deleting user', error);
      throw error;
    }
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

  // Reports-related API calls
  static async getAllUserReports(): Promise<any[]> {
    try {
      return authenticatedClient.get<any[]>(`${USER_API_BASE}/api/users/admin/reports`);
    } catch (error) {
      Logger.error('Error fetching user reports', error);
      throw error;
    }
  }

  static async getAllMessageReports(): Promise<any[]> {
    try {
      return authenticatedClient.get<any[]>(`${MESSAGES_API_BASE}/api/messages/admin/reports`);
    } catch (error) {
      Logger.error('Error fetching message reports', error);
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

  // Chart data
  static async getChartData() {
    try {
      const [users, portfolios] = await Promise.all([
        this.getAllUsers(),
        this.getAllPortfolios()
      ]);

      return {
        growthData: AdminChartUtils.generateGrowthData(users, portfolios),
        activityData: AdminChartUtils.generateActivityData(users, portfolios)
      };
    } catch (error) {
      Logger.error('Error generating chart data', error);
      throw error;
    }
  }

  // User growth data for charts
  static async getUserGrowthData(): Promise<{ month: string; users: number; portfolios: number }[]> {
    try {
      const [users, portfolios] = await Promise.all([
        this.getAllUsers(),
        this.getAllPortfolios()
      ]);

      return AdminChartUtils.generateGrowthData(users, portfolios);
    } catch (error) {
      Logger.error('Error generating user growth data', error);
      throw error;
    }
  }
} 