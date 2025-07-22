'use client';

import React from 'react';
import { Pagination, Space, Typography, Button, Tooltip } from 'antd';
import { 
  LeftOutlined, 
  RightOutlined, 
  DoubleLeftOutlined, 
  DoubleRightOutlined,
  ReloadOutlined 
} from '@ant-design/icons';
import { useHomePageCache } from '@/lib/contexts/home-page-cache-context';
import './pagination-controls.css';

const { Text } = Typography;

interface PaginationControlsProps {
  showQuickJumper?: boolean;
  showSizeChanger?: boolean;
  showTotal?: boolean;
  showCacheStats?: boolean;
  className?: string;
}

const PaginationControls: React.FC<PaginationControlsProps> = ({
  showQuickJumper = true,
  showSizeChanger = false,
  showTotal = true,
  showCacheStats = true,
  className = ''
}) => {
  const {
    currentPage,
    pagination,
    loading,
    goToPage,
    goToNextPage,
    goToPreviousPage,
    goToFirstPage,
    goToLastPage,
    clearCache,
    getCacheStats,
    loadPage
  } = useHomePageCache();

  const cacheStats = getCacheStats();

  const handlePageChange = (page: number) => {
    goToPage(page);
  };

  const handleRefresh = () => {
    loadPage(currentPage, false); // Force refresh without cache
  };

  const handleClearCache = () => {
    clearCache();
    loadPage(currentPage, false); // Reload current page
  };

  if (!pagination) {
    return null;
  }

  const { totalCount, totalPages, hasNext, hasPrevious } = pagination;

  return (
    <div className={`pagination-controls ${className}`}>
      {/* Cache Stats */}
      {showCacheStats && (
        <div className="cache-stats">
          <Space size="small">
            <Text type="secondary" className="cache-stat">
              Cache: {cacheStats.totalEntries} pages
            </Text>
            <Text type="secondary" className="cache-stat">
              Hit Rate: {(cacheStats.hitRate * 100).toFixed(1)}%
            </Text>
            <Tooltip title="Clear cache and refresh">
              <Button 
                size="small" 
                icon={<ReloadOutlined />} 
                onClick={handleClearCache}
                type="text"
              >
                Clear Cache
              </Button>
            </Tooltip>
          </Space>
        </div>
      )}

      {/* Main Pagination Controls */}
      <div className="pagination-main">
        <Space size="middle" align="center">
          {/* Quick Navigation */}
          <Space size="small">
            <Tooltip title="First page">
              <Button
                icon={<DoubleLeftOutlined />}
                disabled={!hasPrevious || loading}
                onClick={goToFirstPage}
                size="small"
              />
            </Tooltip>
            <Tooltip title="Previous page">
              <Button
                icon={<LeftOutlined />}
                disabled={!hasPrevious || loading}
                onClick={goToPreviousPage}
                size="small"
              />
            </Tooltip>
          </Space>

          {/* Ant Design Pagination */}
          <Pagination
            current={currentPage}
            total={totalCount}
            pageSize={pagination.pageSize}
            onChange={handlePageChange}
            showQuickJumper={showQuickJumper}
            showSizeChanger={showSizeChanger}
            disabled={loading}
            showLessItems
            size="small"
            className="main-pagination"
          />

          {/* Quick Navigation */}
          <Space size="small">
            <Tooltip title="Next page">
              <Button
                icon={<RightOutlined />}
                disabled={!hasNext || loading}
                onClick={goToNextPage}
                size="small"
              />
            </Tooltip>
            <Tooltip title="Last page">
              <Button
                icon={<DoubleRightOutlined />}
                disabled={!hasNext || loading}
                onClick={goToLastPage}
                size="small"
              />
            </Tooltip>
          </Space>

          {/* Refresh Button */}
          <Tooltip title="Refresh current page">
            <Button
              icon={<ReloadOutlined />}
              onClick={handleRefresh}
              loading={loading}
              size="small"
              type="text"
            />
          </Tooltip>
        </Space>
      </div>

      {/* Page Info */}
      {showTotal && (
        <div className="pagination-info">
          <Space direction="vertical" size={2}>
            <Text type="secondary" className="page-info">
              Page {currentPage} of {totalPages}
            </Text>
            <Text type="secondary" className="total-info">
              {totalCount} {totalCount === 1 ? 'portfolio' : 'portfolios'} total
            </Text>
          </Space>
        </div>
      )}
    </div>
  );
};

export default PaginationControls;
