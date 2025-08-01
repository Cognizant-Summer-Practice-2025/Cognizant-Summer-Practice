'use client';

import React from 'react';
import { Button } from '@/components/ui/button';
import { 
  ChevronLeft, 
  ChevronRight, 
  ChevronsLeft, 
  ChevronsRight,
  RotateCcw
} from 'lucide-react';
import { useHomePageCache } from '@/lib/contexts/home-page-cache-context';
import { cn } from '@/lib/utils';

interface PaginationControlsProps {
  showTotal?: boolean;
  className?: string;
}

const PaginationControls: React.FC<PaginationControlsProps> = ({
  showTotal = true,
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

  
    loadPage
  } = useHomePageCache();



  const handlePageChange = (page: number) => {
    goToPage(page);
  };

  const handleRefresh = () => {
    loadPage(currentPage, false); // Force refresh without cache
  };



  if (!pagination) {
    return null;
  }

  const { totalCount, totalPages, hasNext, hasPrevious } = pagination;

  // Generate page numbers to show
  const getVisiblePages = () => {
    const delta = 2; // Number of pages to show on each side
    const range = [];
    const rangeWithDots = [];

    for (let i = Math.max(2, currentPage - delta); i <= Math.min(totalPages - 1, currentPage + delta); i++) {
      range.push(i);
    }

    if (currentPage - delta > 2) {
      rangeWithDots.push(1, '...');
    } else {
      rangeWithDots.push(1);
    }

    rangeWithDots.push(...range);

    if (currentPage + delta < totalPages - 1) {
      rangeWithDots.push('...', totalPages);
    } else if (totalPages > 1) {
      rangeWithDots.push(totalPages);
    }

    return rangeWithDots;
  };

  const visiblePages = getVisiblePages();

  return (
    <div className={cn("flex flex-col gap-4", className)}>


      {/* Main Pagination */}
      <div className="flex items-center justify-center gap-2">
        {/* First page button */}
        <Button
          variant="outline"
          size="sm"
          onClick={goToFirstPage}
          disabled={!hasPrevious || loading}
          className="h-9 w-9 p-0 rounded-full"
        >
          <ChevronsLeft className="h-4 w-4" />
        </Button>

        {/* Previous page button */}
        <Button
          variant="outline"
          size="sm"
          onClick={goToPreviousPage}
          disabled={!hasPrevious || loading}
          className="h-9 w-9 p-0 rounded-full"
        >
          <ChevronLeft className="h-4 w-4" />
        </Button>

        {/* Page number buttons */}
        <div className="flex items-center gap-1">
          {visiblePages.map((page, index) => {
            if (page === '...') {
              return (
                <span key={`dots-${index}`} className="px-2 text-muted-foreground">
                  ...
                </span>
              );
            }

            const pageNum = page as number;
            const isActive = pageNum === currentPage;

            return (
              <Button
                key={pageNum}
                variant={isActive ? "default" : "outline"}
                size="sm"
                onClick={() => handlePageChange(pageNum)}
                disabled={loading}
                className={cn(
                  "h-9 w-9 p-0 rounded-full transition-all",
                  isActive && "bg-primary text-primary-foreground shadow-md"
                )}
              >
                {pageNum}
              </Button>
            );
          })}
        </div>

        {/* Next page button */}
        <Button
          variant="outline"
          size="sm"
          onClick={goToNextPage}
          disabled={!hasNext || loading}
          className="h-9 w-9 p-0 rounded-full"
        >
          <ChevronRight className="h-4 w-4" />
        </Button>

        {/* Last page button */}
        <Button
          variant="outline"
          size="sm"
          onClick={goToLastPage}
          disabled={!hasNext || loading}
          className="h-9 w-9 p-0 rounded-full"
        >
          <ChevronsRight className="h-4 w-4" />
        </Button>

        {/* Refresh button */}
        <div className="ml-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={handleRefresh}
            disabled={loading}
            className="h-9 w-9 p-0 rounded-full"
          >
            <RotateCcw className={cn("h-4 w-4", loading && "animate-spin")} />
          </Button>
        </div>
      </div>

      {/* Page Info */}
      {showTotal && (
        <div className="flex flex-col items-center gap-1 text-sm text-muted-foreground">
          <span>Page {currentPage} of {totalPages}</span>
          <span className="text-xs">
            {totalCount} {totalCount === 1 ? 'portfolio' : 'portfolios'} total
          </span>
        </div>
      )}
    </div>
  );
};

export default PaginationControls;
