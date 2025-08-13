'use client';

import React, { useState } from 'react';
import { Download, FileText, Users, FolderOpen, Database } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useAlert } from '@/components/ui/alert-dialog';
import { AdminAPI, UserWithPortfolio, PortfolioWithOwner } from '@/lib/admin';
import { ExportUtils } from '@/lib/utils/export-utils';
import { Logger } from '@/lib/logger';

const AdminExport: React.FC = () => {
  const [isExporting, setIsExporting] = useState(false);
  const { showAlert } = useAlert();

  const exportUsers = async () => {
    try {
      setIsExporting(true);
      const users: UserWithPortfolio[] = await AdminAPI.getUsersWithPortfolios();
      
      if (users.length === 0) {
        showAlert({
          title: 'No Data to Export',
          description: 'There are no users to export.',
          type: 'info',
        });
        return;
      }

      const xmlContent = ExportUtils.exportUsersToXML(users);
      const filename = ExportUtils.generateFilename('goalkeeper_users');
      ExportUtils.downloadXMLFile(xmlContent, filename);
      
      showAlert({
        title: 'Export Successful',
        description: `Successfully exported ${users.length} users to ${filename}`,
        type: 'success',
      });
    } catch (error) {
      Logger.error('Error exporting users', error);
      showAlert({
        title: 'Export Failed',
        description: 'Failed to export users. Please try again.',
        type: 'error',
      });
    } finally {
      setIsExporting(false);
    }
  };

  const exportPortfolios = async () => {
    try {
      setIsExporting(true);
      const portfolios: PortfolioWithOwner[] = await AdminAPI.getPortfoliosWithOwners();
      
      if (portfolios.length === 0) {
        showAlert({
          title: 'No Data to Export',
          description: 'There are no portfolios to export.',
          type: 'info',
        });
        return;
      }

      const xmlContent = ExportUtils.exportPortfoliosToXML(portfolios);
      const filename = ExportUtils.generateFilename('goalkeeper_portfolios');
      ExportUtils.downloadXMLFile(xmlContent, filename);
      
      showAlert({
        title: 'Export Successful',
        description: `Successfully exported ${portfolios.length} portfolios to ${filename}`,
        type: 'success',
      });
    } catch (error) {
      Logger.error('Error exporting portfolios', error);
      showAlert({
        title: 'Export Failed',
        description: 'Failed to export portfolios. Please try again.',
        type: 'error',
      });
    } finally {
      setIsExporting(false);
    }
  };

  const exportAllData = async () => {
    try {
      setIsExporting(true);
      
      showAlert({
        title: 'Starting Export',
        description: 'Fetching all data for export. This may take a moment...',
        type: 'info',
      });

      const [users, portfolios] = await Promise.all([
        AdminAPI.getUsersWithPortfolios(),
        AdminAPI.getPortfoliosWithOwners()
      ]);

      if (users.length === 0 && portfolios.length === 0) {
        showAlert({
          title: 'No Data to Export',
          description: 'There is no data available for export.',
          type: 'info',
        });
        return;
      }

      await ExportUtils.exportAllData(users, portfolios);
      
      showAlert({
        title: 'Export Complete',
        description: `Successfully exported all data: ${users.length} users and ${portfolios.length} portfolios in 3 XML files.`,
        type: 'success',
      });
    } catch (error) {
      Logger.error('Error exporting all data', error);
      showAlert({
        title: 'Export Failed',
        description: 'Failed to export all data. Please try again.',
        type: 'error',
      });
    } finally {
      setIsExporting(false);
    }
  };

  return (
    <div className="export-section space-y-6">
      <div className="section-header">
        <h2 className="text-2xl font-bold text-gray-900">Data Export</h2>
        <p className="text-gray-600 mt-2">
          Export system data in XML format for backup, analysis, or migration purposes.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {/* Users Export */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="w-5 h-5 text-blue-600" />
              Users Export
            </CardTitle>
            <CardDescription>
              Export all user data including profiles, account information, and portfolio statistics.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button 
              onClick={exportUsers}
              disabled={isExporting}
              className="w-full flex items-center gap-2"
              variant="outline"
            >
              <Download className="w-4 h-4" />
              Export Users XML
            </Button>
          </CardContent>
        </Card>

        {/* Portfolios Export */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FolderOpen className="w-5 h-5 text-green-600" />
              Portfolios Export
            </CardTitle>
            <CardDescription>
              Export all portfolio data including templates, publication status, and owner information.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button 
              onClick={exportPortfolios}
              disabled={isExporting}
              className="w-full flex items-center gap-2"
              variant="outline"
            >
              <Download className="w-4 h-4" />
              Export Portfolios XML
            </Button>
          </CardContent>
        </Card>

        {/* Complete Export */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Database className="w-5 h-5 text-purple-600" />
              Complete Export
            </CardTitle>
            <CardDescription>
              Export all system data in separate files plus a combined summary report.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button 
              onClick={exportAllData}
              disabled={isExporting}
              className="w-full flex items-center gap-2"
            >
              <FileText className="w-4 h-4" />
              {isExporting ? 'Exporting...' : 'Export All Data'}
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Export Information */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <h3 className="text-lg font-semibold text-blue-900 mb-2">Export Information</h3>
        <ul className="text-blue-800 space-y-1 text-sm">
          <li>• All exports are generated in XML format for easy parsing and compatibility</li>
          <li>• Individual exports contain specific data for users or portfolios</li>
          <li>• Complete export generates 3 files: users.xml, portfolios.xml, and combined_summary.xml</li>
          <li>• Export files include timestamps and are automatically named with the current date</li>
          <li>• All sensitive data is included - ensure proper handling of exported files</li>
        </ul>
      </div>
    </div>
  );
};

export default AdminExport;
