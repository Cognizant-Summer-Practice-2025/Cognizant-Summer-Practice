import { UserWithPortfolio, PortfolioWithOwner } from '@/lib/admin/interfaces';
import { Logger } from '@/lib/logger';

/**
 * Export utilities for admin data management
 */
export class ExportUtils {
  
  /**
   * Convert user data to XML format
   */
  static exportUsersToXML(users: UserWithPortfolio[]): string {
    const timestamp = new Date().toISOString();
    
    let xml = `<?xml version="1.0" encoding="UTF-8"?>\n`;
    xml += `<users export_date="${timestamp}" count="${users.length}">\n`;
    
    users.forEach(user => {
      xml += `  <user>\n`;
      xml += `    <id>${this.escapeXml(user.id)}</id>\n`;
      xml += `    <username>${this.escapeXml(user.username)}</username>\n`;
      xml += `    <email>${this.escapeXml(user.email)}</email>\n`;
      xml += `    <firstName>${this.escapeXml(user.firstName || '')}</firstName>\n`;
      xml += `    <lastName>${this.escapeXml(user.lastName || '')}</lastName>\n`;
      xml += `    <professionalTitle>${this.escapeXml(user.professionalTitle || '')}</professionalTitle>\n`;
      xml += `    <bio>${this.escapeXml(user.bio || '')}</bio>\n`;
      xml += `    <location>${this.escapeXml(user.location || '')}</location>\n`;
      xml += `    <isActive>${user.isActive}</isActive>\n`;
      xml += `    <isAdmin>${user.isAdmin || false}</isAdmin>\n`;
      xml += `    <joinedDate>${this.escapeXml(user.joinedDate)}</joinedDate>\n`;
      xml += `    <lastLoginAt>${this.escapeXml(user.lastLoginAt || '')}</lastLoginAt>\n`;
      xml += `    <portfolioStatus>${this.escapeXml(user.portfolioStatus)}</portfolioStatus>\n`;
      xml += `    <portfolioId>${this.escapeXml(user.portfolioId || '')}</portfolioId>\n`;
      xml += `    <portfolioTitle>${this.escapeXml(user.portfolioTitle || '')}</portfolioTitle>\n`;
      xml += `    <createdAt>${this.escapeXml(user.createdAt)}</createdAt>\n`;
      xml += `    <updatedAt>${this.escapeXml(user.updatedAt)}</updatedAt>\n`;
      xml += `  </user>\n`;
    });
    
    xml += `</users>`;
    return xml;
  }

  /**
   * Convert portfolio data to XML format
   */
  static exportPortfoliosToXML(portfolios: PortfolioWithOwner[]): string {
    const timestamp = new Date().toISOString();
    
    let xml = `<?xml version="1.0" encoding="UTF-8"?>\n`;
    xml += `<portfolios export_date="${timestamp}" count="${portfolios.length}">\n`;
    
    portfolios.forEach(portfolio => {
      xml += `  <portfolio>\n`;
      xml += `    <id>${this.escapeXml(portfolio.id)}</id>\n`;
      xml += `    <userId>${this.escapeXml(portfolio.userId)}</userId>\n`;
      xml += `    <templateId>${this.escapeXml(portfolio.templateId)}</templateId>\n`;
      xml += `    <title>${this.escapeXml(portfolio.title)}</title>\n`;
      xml += `    <bio>${this.escapeXml(portfolio.bio || '')}</bio>\n`;
      xml += `    <viewCount>${portfolio.viewCount}</viewCount>\n`;
      xml += `    <likeCount>${portfolio.likeCount}</likeCount>\n`;
      xml += `    <visibility>${this.escapeXml(portfolio.visibility)}</visibility>\n`;
      xml += `    <isPublished>${portfolio.isPublished}</isPublished>\n`;
      xml += `    <createdAt>${this.escapeXml(portfolio.createdAt)}</createdAt>\n`;
      xml += `    <updatedAt>${this.escapeXml(portfolio.updatedAt)}</updatedAt>\n`;
      xml += `    <owner>\n`;
      xml += `      <name>${this.escapeXml(portfolio.ownerName)}</name>\n`;
      xml += `      <email>${this.escapeXml(portfolio.ownerEmail)}</email>\n`;
      xml += `      <avatar>${this.escapeXml(portfolio.ownerAvatar || '')}</avatar>\n`;
      xml += `    </owner>\n`;
      xml += `  </portfolio>\n`;
    });
    
    xml += `</portfolios>`;
    return xml;
  }

  /**
   * Export combined user and portfolio data to XML
   */
  static exportCombinedDataToXML(users: UserWithPortfolio[], portfolios: PortfolioWithOwner[]): string {
    const timestamp = new Date().toISOString();
    
    let xml = `<?xml version="1.0" encoding="UTF-8"?>\n`;
    xml += `<goalkeeper_admin_export export_date="${timestamp}">\n`;
    xml += `  <summary>\n`;
    xml += `    <total_users>${users.length}</total_users>\n`;
    xml += `    <total_portfolios>${portfolios.length}</total_portfolios>\n`;
    xml += `    <active_users>${users.filter(u => u.isActive).length}</active_users>\n`;
    xml += `    <published_portfolios>${portfolios.filter(p => p.isPublished).length}</published_portfolios>\n`;
    xml += `  </summary>\n`;
    
    // Add users section
    xml += `  <users count="${users.length}">\n`;
    users.forEach(user => {
      xml += `    <user>\n`;
      xml += `      <id>${this.escapeXml(user.id)}</id>\n`;
      xml += `      <username>${this.escapeXml(user.username)}</username>\n`;
      xml += `      <email>${this.escapeXml(user.email)}</email>\n`;
      xml += `      <firstName>${this.escapeXml(user.firstName || '')}</firstName>\n`;
      xml += `      <lastName>${this.escapeXml(user.lastName || '')}</lastName>\n`;
      xml += `      <professionalTitle>${this.escapeXml(user.professionalTitle || '')}</professionalTitle>\n`;
      xml += `      <isActive>${user.isActive}</isActive>\n`;
      xml += `      <isAdmin>${user.isAdmin || false}</isAdmin>\n`;
      xml += `      <joinedDate>${this.escapeXml(user.joinedDate)}</joinedDate>\n`;
      xml += `      <portfolioStatus>${this.escapeXml(user.portfolioStatus)}</portfolioStatus>\n`;
      xml += `    </user>\n`;
    });
    xml += `  </users>\n`;
    
    // Add portfolios section
    xml += `  <portfolios count="${portfolios.length}">\n`;
    portfolios.forEach(portfolio => {
      xml += `    <portfolio>\n`;
      xml += `      <id>${this.escapeXml(portfolio.id)}</id>\n`;
      xml += `      <userId>${this.escapeXml(portfolio.userId)}</userId>\n`;
      xml += `      <templateId>${this.escapeXml(portfolio.templateId)}</templateId>\n`;
      xml += `      <title>${this.escapeXml(portfolio.title)}</title>\n`;
      xml += `      <isPublished>${portfolio.isPublished}</isPublished>\n`;
      xml += `      <viewCount>${portfolio.viewCount}</viewCount>\n`;
      xml += `      <ownerName>${this.escapeXml(portfolio.ownerName)}</ownerName>\n`;
      xml += `      <ownerEmail>${this.escapeXml(portfolio.ownerEmail)}</ownerEmail>\n`;
      xml += `    </portfolio>\n`;
    });
    xml += `  </portfolios>\n`;
    
    xml += `</goalkeeper_admin_export>`;
    return xml;
  }

  /**
   * Download data as XML file
   */
  static downloadXMLFile(xmlContent: string, filename: string): void {
    try {
      const blob = new Blob([xmlContent], { type: 'application/xml' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
      
      Logger.info(`XML file downloaded: ${filename}`);
    } catch (error) {
      Logger.error('Error downloading XML file', error);
      throw new Error('Failed to download XML file');
    }
  }

  /**
   * Generate filename with timestamp
   */
  static generateFilename(prefix: string): string {
    const timestamp = new Date().toISOString().split('T')[0]; // YYYY-MM-DD
    return `${prefix}_${timestamp}.xml`;
  }

  /**
   * Escape XML special characters
   */
  private static escapeXml(text: string): string {
    if (typeof text !== 'string') {
      return String(text || '');
    }
    
    return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&apos;');
  }

  /**
   * Export users and portfolios to separate XML files
   */
  static async exportAllData(users: UserWithPortfolio[], portfolios: PortfolioWithOwner[]): Promise<void> {
    try {
      // Export users
      const usersXML = this.exportUsersToXML(users);
      const usersFilename = this.generateFilename('goalkeeper_users');
      this.downloadXMLFile(usersXML, usersFilename);

      // Small delay to prevent browser blocking multiple downloads
      await new Promise(resolve => setTimeout(resolve, 500));

      // Export portfolios
      const portfoliosXML = this.exportPortfoliosToXML(portfolios);
      const portfoliosFilename = this.generateFilename('goalkeeper_portfolios');
      this.downloadXMLFile(portfoliosXML, portfoliosFilename);

      // Small delay to prevent browser blocking multiple downloads
      await new Promise(resolve => setTimeout(resolve, 500));

      // Export combined data
      const combinedXML = this.exportCombinedDataToXML(users, portfolios);
      const combinedFilename = this.generateFilename('goalkeeper_admin_export');
      this.downloadXMLFile(combinedXML, combinedFilename);

      Logger.info('Successfully exported all admin data to XML files');
    } catch (error) {
      Logger.error('Error exporting admin data', error);
      throw error;
    }
  }
}