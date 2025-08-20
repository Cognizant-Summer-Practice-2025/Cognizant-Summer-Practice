"use client"

import * as React from "react"
import { ChevronLeft, ChevronRight } from "lucide-react"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"

interface Column<T> {
  title: string
  key: string
  dataIndex?: keyof T
  render?: (value: T[keyof T], record: T, index: number) => React.ReactNode
  className?: string
}

interface TableProps<T> {
  columns: Column<T>[]
  dataSource: T[]
  rowKey: keyof T | ((record: T) => string)
  loading?: boolean
  pagination?: {
    pageSize: number
    showSizeChanger?: boolean
    showQuickJumper?: boolean
    showTotal?: (total: number, range: [number, number]) => string
  }
  scroll?: { x?: number }
  className?: string
}

function Table<T extends Record<string, unknown>>({
  columns,
  dataSource,
  rowKey,
  loading = false,
  pagination,
  scroll,
  className
}: TableProps<T>) {
  const [currentPage, setCurrentPage] = React.useState(1)
  const [pageSize, setPageSize] = React.useState(pagination?.pageSize || 10)

  const getRowKey = (record: T): string => {
    if (typeof rowKey === 'function') {
      return rowKey(record)
    }
    return String(record[rowKey])
  }

  const totalItems = dataSource.length
  const totalPages = Math.ceil(totalItems / pageSize)
  const startIndex = (currentPage - 1) * pageSize
  const endIndex = Math.min(startIndex + pageSize, totalItems)
  const currentData = dataSource.slice(startIndex, endIndex)

  const handlePageChange = (page: number) => {
    setCurrentPage(page)
  }

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize)
    setCurrentPage(1)
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    )
  }

  return (
    <div className={cn("w-full", className)}>
      <div className="rounded-md border">
        <div className={cn("overflow-auto", scroll?.x && `min-w-[${scroll.x}px]`)}>
          <table className="w-full">
            <thead>
              <tr className="border-b bg-muted/50">
                {columns.map((column) => (
                  <th
                    key={column.key}
                    className={cn(
                      "h-12 px-4 text-left align-middle font-medium text-muted-foreground",
                      column.className
                    )}
                  >
                    {column.title}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {currentData.map((record, index) => (
                <tr
                  key={getRowKey(record)}
                  className="border-b transition-colors hover:bg-muted/50"
                >
                  {columns.map((column) => (
                    <td
                      key={column.key}
                      className={cn(
                        "p-4 align-middle",
                        column.className
                      )}
                    >
                      {column.render
                        ? column.render(
                            column.dataIndex ? record[column.dataIndex] : (record as T[keyof T]),
                            record,
                            index
                          )
                        : column.dataIndex
                        ? String(record[column.dataIndex] ?? '')
                        : ''}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {pagination && totalPages > 1 && (
        <div className="flex items-center justify-between px-2 py-4">
          <div className="text-sm text-muted-foreground">
            {pagination.showTotal
              ? pagination.showTotal(totalItems, [startIndex + 1, endIndex])
              : `Showing ${startIndex + 1}-${endIndex} of ${totalItems} results`}
          </div>
          
          <div className="flex items-center space-x-2">
            {pagination.showSizeChanger && (
              <div className="flex items-center space-x-2">
                <span className="text-sm text-muted-foreground">Rows per page:</span>
                <select
                  value={pageSize}
                  onChange={(e) => handlePageSizeChange(Number(e.target.value))}
                  className="h-8 w-16 rounded border border-input bg-background px-2 text-sm"
                >
                  <option value={10}>10</option>
                  <option value={20}>20</option>
                  <option value={50}>50</option>
                  <option value={100}>100</option>
                </select>
              </div>
            )}
            
            <div className="flex items-center space-x-1">
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage <= 1}
              >
                <ChevronLeft className="h-4 w-4" />
              </Button>
              
              {pagination.showQuickJumper && totalPages > 5 ? (
                <div className="flex items-center space-x-1">
                  <span className="text-sm text-muted-foreground">Page</span>
                  <input
                    type="number"
                    min={1}
                    max={totalPages}
                    value={currentPage}
                    onChange={(e) => {
                      const page = Number(e.target.value)
                      if (page >= 1 && page <= totalPages) {
                        handlePageChange(page)
                      }
                    }}
                    className="h-8 w-16 rounded border border-input bg-background px-2 text-sm text-center"
                  />
                  <span className="text-sm text-muted-foreground">of {totalPages}</span>
                </div>
              ) : (
                <div className="flex items-center space-x-1">
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum
                    if (totalPages <= 5) {
                      pageNum = i + 1
                    } else if (currentPage <= 3) {
                      pageNum = i + 1
                    } else if (currentPage >= totalPages - 2) {
                      pageNum = totalPages - 4 + i
                    } else {
                      pageNum = currentPage - 2 + i
                    }
                    
                    return (
                      <Button
                        key={pageNum}
                        variant={currentPage === pageNum ? "default" : "outline"}
                        size="sm"
                        onClick={() => handlePageChange(pageNum)}
                        className="h-8 w-8 p-0"
                      >
                        {pageNum}
                      </Button>
                    )
                  })}
                </div>
              )}
              
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage >= totalPages}
              >
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export { Table, type Column, type TableProps } 