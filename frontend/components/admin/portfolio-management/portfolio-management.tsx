'use client';

import React, { useState, useEffect, useMemo } from 'react';
import { Download, Eye, Edit, Trash2, AlertTriangle, Zap, Search, ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useAlert } from '@/components/ui/alert-dialog';
import { AdminAPI, PortfolioWithOwner } from '@/lib/admin';
import './style.css';

const PortfolioManagement: React.FC = () => {
  const [portfolios, setPortfolios] = useState<PortfolioWithOwner[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingPortfolioId, setDeletingPortfolioId] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 15;

  const { showAlert, showConfirm } = useAlert();

  useEffect(() => {
    fetchPortfolios();
  }, []);

  const fetchPortfolios = async () => {
    try {
      setLoading(true);
      const portfoliosData = await AdminAPI.getPortfoliosWithOwners();
      setPortfolios(portfoliosData);
      setError(null);
    } catch (err) {
      console.error('Error fetching portfolios:', err);
      setError('Failed to load portfolios');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  };

  const getStatusBadgeClass = (status: string): string => {
    return status.toLowerCase().replace(' ', '-');
  };

  const getPortfolioStatus = (portfolio: PortfolioWithOwner): string => {
    if (!portfolio.isPublished) return 'Draft';
    if (portfolio.visibility === 'Private') return 'Private';
    if (portfolio.visibility === 'Unlisted') return 'Unlisted';
    return 'Published';
  };

  const getThumbnail = (portfolio: PortfolioWithOwner): string => {
    // Use owner avatar as placeholder thumbnail, or generate a placeholder
    return portfolio.ownerAvatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(portfolio.title)}&size=40&background=f0f0f0&color=666`;
  };

  // Filter and paginate portfolios
  const filteredPortfolios = useMemo(() => {
    return portfolios.filter(portfolio => {
      const searchLower = searchTerm.toLowerCase();
      const title = portfolio.title.toLowerCase();
      const ownerName = portfolio.ownerName.toLowerCase();
      const ownerEmail = portfolio.ownerEmail.toLowerCase();
      const status = getPortfolioStatus(portfolio).toLowerCase();
      const bio = (portfolio.bio || '').toLowerCase();
      
      return title.includes(searchLower) || 
             ownerName.includes(searchLower) || 
             ownerEmail.includes(searchLower) ||
             status.includes(searchLower) ||
             bio.includes(searchLower);
    });
  }, [portfolios, searchTerm]);

  const totalPages = Math.ceil(filteredPortfolios.length / itemsPerPage);
  const paginatedPortfolios = useMemo(() => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    return filteredPortfolios.slice(startIndex, startIndex + itemsPerPage);
  }, [filteredPortfolios, currentPage, itemsPerPage]);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleSearchChange = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1); // Reset to first page when searching
  };

  const handleViewPortfolio = (portfolioId: string, _userId: string) => {
    // Open portfolio in new tab using the portfolio page URL structure
    const portfolioUrl = `/portfolio?portfolio=${portfolioId}`;
    window.open(portfolioUrl, '_blank', 'noopener,noreferrer');
  };

  const handleEditPortfolio = (_portfolioId: string) => {
    // TODO: Implement edit portfolio functionality
    showAlert({
      title: 'Edit Portfolio',
      description: 'Edit functionality will be implemented soon.',
      type: 'info',
    });
  };

  const handleDeletePortfolio = (portfolio: PortfolioWithOwner) => {
    const deleteMessage = `Are you sure you want to delete the portfolio "${portfolio.title}"?

This action will permanently delete:
• The portfolio and all its settings
• All associated projects
• All associated experiences
• All associated skills
• All associated blog posts
• All bookmarks pointing to this portfolio

This action cannot be undone.`;
    
    showConfirm({
      title: 'Delete Portfolio',
      description: deleteMessage,
      type: 'error',
      confirmText: 'Delete Portfolio',
      cancelText: 'Cancel',
      onConfirm: async () => {
        await performDeletePortfolio(portfolio.id, portfolio.title);
      },
    });
  };

  const performDeletePortfolio = async (portfolioId: string, portfolioTitle: string) => {
    try {
      setDeletingPortfolioId(portfolioId);
      
      await AdminAPI.deletePortfolio(portfolioId);
      
      // Remove the deleted portfolio from the local state
      setPortfolios(prev => prev.filter(p => p.id !== portfolioId));
      
      showAlert({
        title: 'Portfolio Deleted',
        description: `Portfolio "${portfolioTitle}" has been deleted successfully.`,
        type: 'success',
      });
    } catch (error) {
      console.error('🔴 PERFORM DELETE - Error occurred:', error);
      console.error('🔴 PERFORM DELETE - Error type:', typeof error);
      console.error('🔴 PERFORM DELETE - Error instance:', error instanceof Error);
      
      // Show a more detailed error message
      const errorMessage = error instanceof Error 
        ? error.message
        : 'Unknown error occurred';
      
      console.error('🔴 PERFORM DELETE - Showing error message:', errorMessage);
      showAlert({
        title: 'Delete Failed',
        description: `Failed to delete portfolio "${portfolioTitle}": ${errorMessage}`,
        type: 'error',
      });
    } finally {
      setDeletingPortfolioId(null);
    }
  };

  const handleExportPortfolios = () => {
    // TODO: Implement export functionality
    showAlert({
      title: 'Export Portfolios',
      description: 'Export functionality will be implemented soon.',
      type: 'info',
    });
  };

  const handleModeratePorfolios = () => {
    // TODO: Implement moderation functionality
    showAlert({
      title: 'Moderate Portfolios',
      description: 'Moderation functionality will be implemented soon.',
      type: 'info',
    });
  };

  if (loading) {
    return (
      <div className="management-section">
        <div className="section-header">
          <h2>Portfolio Management</h2>
        </div>
        <div className="loading-container">
          <p>Loading portfolios...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="management-section">
        <div className="section-header">
          <h2>Portfolio Management</h2>
        </div>
        <div className="error-container">
          <p>{error}</p>
          <Button onClick={fetchPortfolios} variant="outline">
            Retry
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="management-section">
      <div className="section-header">
        <h2>Portfolio Management</h2>
        <div className="section-actions">
          <Button 
            variant="outline"
            onClick={handleExportPortfolios}
            className="flex items-center gap-2"
          >
            <Download className="w-4 h-4" />
            Export
          </Button>
          <Button 
            onClick={handleModeratePorfolios}
            className="flex items-center gap-2"
          >
            <Zap className="w-4 h-4" />
            Moderate
          </Button>
        </div>
      </div>

      {/* Search Bar */}
      <div className="search-container">
        <div className="search-input-wrapper">
          <Search className="search-icon" />
          <Input
            type="text"
            placeholder="Search portfolios by title, owner, status, or description..."
            value={searchTerm}
            onChange={(e) => handleSearchChange(e.target.value)}
            className="search-input"
          />
        </div>
        <div className="search-results-info">
          {searchTerm && (
            <span className="text-sm text-gray-600">
              Found {filteredPortfolios.length} portfolios
            </span>
          )}
        </div>
      </div>
      
      <div className="table-container">
        <div className="table-header portfolio-header">
          <div className="col-portfolio">Portfolio</div>
          <div className="col-owner">Owner</div>
          <div className="col-created">Created</div>
          <div className="col-views">Views</div>
          <div className="col-p-status">Status</div>
          <div className="col-p-actions">Actions</div>
        </div>
        
        <div className="table-body scrollable-table-body">
          {paginatedPortfolios.map((portfolio) => (
            <div key={portfolio.id} className="table-row">
              <div className="col-portfolio">
                <div className="portfolio-cell">
                  <img 
                    src={getThumbnail(portfolio)} 
                    alt={portfolio.title} 
                    className="portfolio-thumbnail" 
                  />
                  <div className="portfolio-info">
                    <div className="portfolio-title">{portfolio.title}</div>
                    <div className="portfolio-description">
                      {portfolio.bio || 'No description available'}
                    </div>
                  </div>
                </div>
              </div>
              <div className="col-owner">{portfolio.ownerName}</div>
              <div className="col-created">{formatDate(portfolio.createdAt)}</div>
              <div className="col-views">{portfolio.viewCount.toLocaleString()}</div>
              <div className="col-p-status">
                <span className={`status-badge ${getStatusBadgeClass(getPortfolioStatus(portfolio))}`}>
                  {getPortfolioStatus(portfolio)}
                </span>
              </div>
              <div className="col-p-actions">
                <Button 
                  variant="ghost"
                  size="icon"
                  onClick={() => handleViewPortfolio(portfolio.id, portfolio.userId)}
                  title="View portfolio in new tab"
                >
                  <Eye className="w-4 h-4" />
                </Button>
                <Button 
                  variant="ghost"
                  size="icon"
                  onClick={() => handleEditPortfolio(portfolio.id)}
                  title="Edit portfolio"
                >
                  <Edit className="w-4 h-4" />
                </Button>
                <Button 
                  variant="ghost"
                  size="icon"
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    handleDeletePortfolio(portfolio);
                  }}
                  disabled={deletingPortfolioId === portfolio.id}
                  title="Delete portfolio and all related data"
                  className="text-red-600 hover:text-red-700 hover:bg-red-50"
                >
                  {deletingPortfolioId === portfolio.id ? (
                    <div className="w-4 h-4 animate-spin border-2 border-red-600 border-t-transparent rounded-full" />
                  ) : (
                    <Trash2 className="w-4 h-4" />
                  )}
                </Button>
              </div>
            </div>
          ))}
        </div>

        {/* Pagination Controls */}
        {filteredPortfolios.length > 0 && (
          <div className="pagination-container">
            <div className="pagination-info">
              Showing {Math.min((currentPage - 1) * itemsPerPage + 1, filteredPortfolios.length)} to{' '}
              {Math.min(currentPage * itemsPerPage, filteredPortfolios.length)} of {filteredPortfolios.length} portfolios
            </div>
            <div className="pagination-controls">
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1}
                className="flex items-center gap-1"
              >
                <ChevronLeft className="w-4 h-4" />
                Previous
              </Button>
              
              <div className="pagination-numbers">
                {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                  let pageNum;
                  if (totalPages <= 5) {
                    pageNum = i + 1;
                  } else if (currentPage <= 3) {
                    pageNum = i + 1;
                  } else if (currentPage >= totalPages - 2) {
                    pageNum = totalPages - 4 + i;
                  } else {
                    pageNum = currentPage - 2 + i;
                  }
                  
                  return (
                    <Button
                      key={pageNum}
                      variant={currentPage === pageNum ? "default" : "outline"}
                      size="sm"
                      onClick={() => handlePageChange(pageNum)}
                      className="pagination-number"
                    >
                      {pageNum}
                    </Button>
                  );
                })}
              </div>
              
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                className="flex items-center gap-1"
              >
                Next
                <ChevronRight className="w-4 h-4" />
              </Button>
            </div>
          </div>
        )}

        {paginatedPortfolios.length === 0 && !loading && (
          <div className="empty-state">
            {searchTerm ? (
              <>
                <p>No portfolios found matching &ldquo;{searchTerm}&rdquo;</p>
                <Button 
                  variant="outline" 
                  onClick={() => handleSearchChange('')}
                  className="mt-2"
                >
                  Clear search
                </Button>
              </>
            ) : (
              <p>No portfolios found.</p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default PortfolioManagement;