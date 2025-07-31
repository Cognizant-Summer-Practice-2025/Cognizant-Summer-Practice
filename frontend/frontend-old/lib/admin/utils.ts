import { AdminUser, AdminPortfolio, AdminStats, UserWithPortfolio, PortfolioWithOwner } from './interfaces';

/**
 * Date and time utility functions for admin analytics
 */
export class AdminDateUtils {
  /**
   * Check if a date is in the current month and year
   */
  static isInCurrentMonth(date: Date): boolean {
    const now = new Date();
    return date.getMonth() === now.getMonth() && date.getFullYear() === now.getFullYear();
  }

  /**
   * Check if a date is in a specific month and year
   */
  static isInMonth(date: Date, targetMonth: number, targetYear: number): boolean {
    return date.getMonth() === targetMonth && date.getFullYear() === targetYear;
  }

  /**
   * Generate month name for charts (e.g., "Jan 2025")
   */
  static getMonthName(date: Date): string {
    return date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  }

  /**
   * Get date for N months ago
   */
  static getMonthsAgo(monthsBack: number): Date {
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth() - monthsBack, 1);
  }
}

/**
 * Data processing utilities for admin statistics
 */
export class AdminDataUtils {
  /**
   * Filter active users from user list
   */
  static getActiveUsers(users: AdminUser[]): AdminUser[] {
    return users.filter(u => u.isActive);
  }

  /**
   * Filter published portfolios from portfolio list
   */
  static getPublishedPortfolios(portfolios: AdminPortfolio[]): AdminPortfolio[] {
    return portfolios.filter(p => p.isPublished);
  }

  /**
   * Filter draft portfolios from portfolio list
   */
  static getDraftPortfolios(portfolios: AdminPortfolio[]): AdminPortfolio[] {
    return portfolios.filter(p => !p.isPublished);
  }

  /**
   * Calculate total views across all portfolios
   */
  static getTotalViews(portfolios: AdminPortfolio[]): number {
    return portfolios.reduce((sum, p) => sum + p.viewCount, 0);
  }

  /**
   * Count users created in current month
   */
  static getNewUsersThisMonth(users: AdminUser[]): number {
    return users.filter(u => {
      const createdDate = new Date(u.createdAt);
      return AdminDateUtils.isInCurrentMonth(createdDate);
    }).length;
  }

  /**
   * Count entities created in a specific month
   */
  static getCountInMonth<T extends { createdAt: string }>(
    entities: T[], 
    month: number, 
    year: number
  ): number {
    return entities.filter(entity => {
      const createdDate = new Date(entity.createdAt);
      return AdminDateUtils.isInMonth(createdDate, month, year);
    }).length;
  }

  /**
   * Generate full name from user object
   */
  static getUserFullName(user: AdminUser): string {
    return `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.username;
  }
}

/**
 * Chart data generation utilities
 */
export class AdminChartUtils {
  /**
   * Generate growth data for the last N months
   */
  static generateGrowthData(
    users: AdminUser[],
    portfolios: AdminPortfolio[],
    monthsCount: number = 6
  ): { month: string; users: number; portfolios: number }[] {
    const months: { month: string; users: number; portfolios: number }[] = [];
    
    for (let i = monthsCount - 1; i >= 0; i--) {
      const date = AdminDateUtils.getMonthsAgo(i);
      const monthName = AdminDateUtils.getMonthName(date);
      
      const usersInMonth = AdminDataUtils.getCountInMonth(users, date.getMonth(), date.getFullYear());
      const portfoliosInMonth = AdminDataUtils.getCountInMonth(portfolios, date.getMonth(), date.getFullYear());

      months.push({
        month: monthName,
        users: usersInMonth,
        portfolios: portfoliosInMonth
      });
    }

    return months;
  }

  /**
   * Generate activity data for pie charts
   */
  static generateActivityData(
    users: AdminUser[],
    portfolios: AdminPortfolio[]
  ): { name: string; value: number }[] {
    const activeUsers = AdminDataUtils.getActiveUsers(users).length;
    const publishedPortfolios = AdminDataUtils.getPublishedPortfolios(portfolios).length;
    const draftPortfolios = AdminDataUtils.getDraftPortfolios(portfolios).length;

    return [
      { name: 'Active Users', value: activeUsers },
      { name: 'Published Portfolios', value: publishedPortfolios },
      { name: 'Draft Portfolios', value: draftPortfolios },
    ];
  }
}

/**
 * Statistics calculation utilities
 */
export class AdminStatsUtils {
  /**
   * Calculate complete admin statistics
   */
  static calculateStats(users: AdminUser[], portfolios: AdminPortfolio[]): AdminStats {
    const activeUsers = AdminDataUtils.getActiveUsers(users);
    const publishedPortfolios = AdminDataUtils.getPublishedPortfolios(portfolios);
    const draftPortfolios = AdminDataUtils.getDraftPortfolios(portfolios);
    const totalViews = AdminDataUtils.getTotalViews(portfolios);
    const newThisMonth = AdminDataUtils.getNewUsersThisMonth(users);

    return {
      totalUsers: activeUsers.length,
      activePortfolios: publishedPortfolios.length,
      totalProjects: 0, // To be implemented when endpoint is available
      newThisMonth,
      totalBlogPosts: 0, // To be implemented when endpoint is available
      publishedPortfolios: publishedPortfolios.length,
      draftPortfolios: draftPortfolios.length,
      totalViews,
    };
  }
}

/**
 * Data transformation utilities
 */
export class AdminTransformUtils {
  /**
   * Transform users with their portfolio information
   */
  static transformUsersWithPortfolios(
    users: AdminUser[],
    portfolios: AdminPortfolio[]
  ): UserWithPortfolio[] {
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
        joinedDate: user.createdAt,
      };
    });
  }

  /**
   * Transform portfolios with their owner information
   */
  static transformPortfoliosWithOwners(
    portfolios: AdminPortfolio[],
    users: AdminUser[]
  ): PortfolioWithOwner[] {
    return portfolios.map(portfolio => {
      const owner = users.find(u => u.id === portfolio.userId);
      return {
        ...portfolio,
        ownerName: owner ? AdminDataUtils.getUserFullName(owner) : 'Unknown User',
        ownerEmail: owner?.email || '',
        ownerAvatar: owner?.avatarUrl,
      };
    });
  }
}
