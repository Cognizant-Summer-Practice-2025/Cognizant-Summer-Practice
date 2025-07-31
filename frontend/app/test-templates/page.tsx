"use client";

import React, { useState, useEffect, useCallback } from 'react';
import { PortfolioDataFromDB, TemplateConfig } from '@/lib/portfolio';
import { getPortfolioById, getUserPortfolioInfo, UserPortfolioInfo } from '@/lib/portfolio/api';
import { getMockPortfolioData } from '@/lib/portfolio/mock-data';
import { loadTemplateComponent, getPortfolioTemplates } from '@/lib/templates';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Loader2, RefreshCw, Eye, EyeOff } from 'lucide-react';

// Test portfolio IDs
const TEST_PORTFOLIO_IDS = [
  '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
  'mock-data', // Use mock data
  'minimal', // Use minimal mock data
  // Add more test portfolio IDs here as needed
];

export default function TestTemplatesPage() {
  const [portfolioData, setPortfolioData] = useState<PortfolioDataFromDB | null>(null);
  const [userInfo, setUserInfo] = useState<UserPortfolioInfo | null>(null);
  const [selectedTemplate, setSelectedTemplate] = useState('modern');
  const [selectedPortfolioId, setSelectedPortfolioId] = useState(TEST_PORTFOLIO_IDS[0]);
  const [loading, setLoading] = useState(false);
  const [templatesLoading, setTemplatesLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [TemplateComponent, setTemplateComponent] = useState<React.ComponentType<any> | null>(null);
  const [showRawData, setShowRawData] = useState(false);
  const [availableTemplates, setAvailableTemplates] = useState<TemplateConfig[]>([]);

  // Helper function to get display name from user info
  const getDisplayName = (user: UserPortfolioInfo | null) => {
    if (!user) return '';
    return user.name || user.username || 'Unknown User';
  };

  // Create portfolio data with user info like the real portfolio page does
  const createPortfolioDataWithUser = useCallback((rawData: PortfolioDataFromDB, user: UserPortfolioInfo | null): PortfolioDataFromDB => {
    if (!user) return rawData;

    return {
      ...rawData,
      profile: {
        id: user.userId,
        name: getDisplayName(user),
        title: user.professionalTitle || rawData.profile.title,
        bio: rawData.portfolio.bio || rawData.profile.bio,
        profileImage: user.avatarUrl || rawData.profile.profileImage,
        location: user.location || rawData.profile.location,
        email: user.email || rawData.profile.email
      },
      contacts: {
        email: user.email || rawData.contacts.email,
        location: user.location || rawData.contacts.location
      },
      quotes: rawData.quotes.length > 0 ? rawData.quotes : [
        {
          id: 'default-1',
          text: rawData.portfolio.bio || 'Passionate about creating amazing experiences and solving complex problems.',
          author: getDisplayName(user),
          position: user.professionalTitle
        }
      ]
    };
  }, []);

  // Fetch portfolio data
  const fetchPortfolioData = useCallback(async (portfolioId: string) => {
    setLoading(true);
    setError(null);
    setUserInfo(null);
    
    try {
      console.log(`Fetching portfolio data for ID: ${portfolioId}`);
      
      // Use mock data for testing if portfolioId starts with 'mock' or 'minimal'
      if (portfolioId.startsWith('mock') || portfolioId === 'minimal') {
        console.log('Using mock data for testing');
        const mockData = getMockPortfolioData(portfolioId);
        setPortfolioData(mockData);
        setUserInfo(null); // Mock data already has complete user info
        return;
      }
      
      // Fetch portfolio data
      const data = await getPortfolioById(portfolioId);
      console.log('Portfolio data received:', data);
      
      // Fetch user information
      try {
        const user = await getUserPortfolioInfo(data.portfolio.userId);
        console.log('User info received:', user);
        setUserInfo(user);
        
        // Create complete portfolio data with user info
        const completeData = createPortfolioDataWithUser(data, user);
        setPortfolioData(completeData);
      } catch (userErr) {
        console.warn('Failed to fetch user info, using portfolio data as-is:', userErr);
        setPortfolioData(data);
        setUserInfo(null);
      }
    } catch (err) {
      console.error('Error fetching portfolio data:', err);
      setError(err instanceof Error ? err.message : 'Failed to fetch portfolio data');
      
      // Fallback to mock data if API fails
      console.log('Falling back to mock data due to API error');
      const mockData = getMockPortfolioData();
      setPortfolioData(mockData);
      setUserInfo(null);
    } finally {
      setLoading(false);
    }
  }, [createPortfolioDataWithUser]);

  // Load template component
  const loadTemplate = async (templateId: string) => {
    try {
      console.log(`Loading template: ${templateId}`);
      const templateModule = await loadTemplateComponent(templateId);
      setTemplateComponent(() => templateModule.default);
    } catch (err) {
      console.error('Error loading template:', err);
      setError(`Failed to load template: ${templateId}`);
    }
  };

  // Load available templates
  useEffect(() => {
    async function loadTemplates() {
      try {
        setTemplatesLoading(true);
        const templates = await getPortfolioTemplates();
        setAvailableTemplates(templates);
        
        // Set default template if current selection doesn't exist
        if (!templates.find(t => t.id === selectedTemplate)) {
          setSelectedTemplate(templates[0]?.id || 'modern');
        }
      } catch (err) {
        console.error('Error loading templates:', err);
        setError('Failed to load available templates');
        // Fallback to basic templates
        setAvailableTemplates([
          { id: 'modern', name: 'Modern', description: 'Modern template', previewImage: '' },
          { id: 'creative', name: 'Creative', description: 'Creative template', previewImage: '' }
        ]);
      } finally {
        setTemplatesLoading(false);
      }
    }

    loadTemplates();
  }, [selectedTemplate]);

  // Initial load
  useEffect(() => {
    fetchPortfolioData(selectedPortfolioId);
  }, [selectedPortfolioId, fetchPortfolioData]);

  // Load template when selection changes
  useEffect(() => {
    loadTemplate(selectedTemplate);
  }, [selectedTemplate]);

  const handleRefresh = () => {
    fetchPortfolioData(selectedPortfolioId);
  };

  const renderPortfolioInfo = () => {
    if (!portfolioData) return null;

    return (
      <Card className="mb-6">
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            Portfolio Information
            <Button 
              variant="outline" 
              size="sm"
              onClick={() => setShowRawData(!showRawData)}
            >
              {showRawData ? <EyeOff size={16} /> : <Eye size={16} />}
              {showRawData ? 'Hide' : 'Show'} Raw Data
            </Button>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-4">
            <div>
              <p className="text-sm text-muted-foreground">Portfolio ID</p>
              <p className="font-mono text-sm">{portfolioData.portfolio.id}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">User ID</p>
              <p className="font-mono text-sm">{portfolioData.portfolio.userId}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Template ID</p>
              <p className="font-mono text-sm">{portfolioData.portfolio.templateId}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Profile Name</p>
              <p className="font-semibold">{portfolioData.profile.name || 'Not set'}</p>
              {userInfo && (
                <p className="text-xs text-muted-foreground">From user: {getDisplayName(userInfo)}</p>
              )}
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Title</p>
              <p>{portfolioData.profile.title || 'Not set'}</p>
              {userInfo?.professionalTitle && (
                <p className="text-xs text-muted-foreground">From user: {userInfo.professionalTitle}</p>
              )}
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Published</p>
              <Badge variant={portfolioData.portfolio.isPublished ? 'default' : 'secondary'}>
                {portfolioData.portfolio.isPublished ? 'Published' : 'Draft'}
              </Badge>
            </div>
            {userInfo && (
              <div>
                <p className="text-sm text-muted-foreground">User Email</p>
                <p className="text-sm">{userInfo.email}</p>
              </div>
            )}
            {userInfo && (
              <div>
                <p className="text-sm text-muted-foreground">User Location</p>
                <p className="text-sm">{userInfo.location || 'Not set'}</p>
              </div>
            )}
          </div>

          {/* Data counts */}
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-4">
            <div className="text-center p-3 bg-muted rounded-lg">
              <p className="text-2xl font-bold text-primary">{portfolioData.experience?.length || 0}</p>
              <p className="text-sm text-muted-foreground">Experience</p>
            </div>
            <div className="text-center p-3 bg-muted rounded-lg">
              <p className="text-2xl font-bold text-primary">{portfolioData.projects?.length || 0}</p>
              <p className="text-sm text-muted-foreground">Projects</p>
            </div>
            <div className="text-center p-3 bg-muted rounded-lg">
              <p className="text-2xl font-bold text-primary">{portfolioData.skills?.length || 0}</p>
              <p className="text-sm text-muted-foreground">Skills</p>
            </div>
            <div className="text-center p-3 bg-muted rounded-lg">
              <p className="text-2xl font-bold text-primary">{portfolioData.blogPosts?.length || 0}</p>
              <p className="text-sm text-muted-foreground">Blog Posts</p>
            </div>
            <div className="text-center p-3 bg-muted rounded-lg">
              <Badge variant={userInfo ? 'default' : 'secondary'} className="w-full h-full flex items-center justify-center">
                {userInfo ? 'User Data ✓' : 'User Data ✗'}
              </Badge>
            </div>
          </div>

          {showRawData && (
            <details className="mt-4">
              <summary className="cursor-pointer text-sm font-medium mb-2">Raw Portfolio Data</summary>
              <pre className="bg-muted p-4 rounded-lg text-xs overflow-auto max-h-96">
                {JSON.stringify(portfolioData, null, 2)}
              </pre>
            </details>
          )}
        </CardContent>
      </Card>
    );
  };

  return (
    <div className="container mx-auto py-8 px-4">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Portfolio Template Tester</h1>
        <p className="text-muted-foreground">
          Test different portfolio templates with real data from the backend
        </p>
      </div>

      {/* Controls */}
      <Card className="mb-6">
        <CardHeader>
          <CardTitle>Test Controls</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Portfolio ID Selection */}
            <div>
              <label className="text-sm font-medium mb-2 block">Portfolio ID</label>
              <Select value={selectedPortfolioId} onValueChange={setSelectedPortfolioId}>
                <SelectTrigger>
                  <SelectValue placeholder="Select portfolio" />
                </SelectTrigger>
                <SelectContent>
                  {TEST_PORTFOLIO_IDS.map((id) => (
                    <SelectItem key={id} value={id}>
                      {id === 'mock-data' ? 'Mock Data (Full)' : 
                       id === 'minimal' ? 'Mock Data (Minimal)' : 
                       `${id.slice(0, 8)}...`}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Template Selection */}
            <div>
              <label className="text-sm font-medium mb-2 block">Template</label>
              <Select value={selectedTemplate} onValueChange={setSelectedTemplate} disabled={templatesLoading}>
                <SelectTrigger>
                  <SelectValue placeholder={templatesLoading ? "Loading templates..." : "Select template"} />
                </SelectTrigger>
                <SelectContent>
                  {availableTemplates.map((template) => (
                    <SelectItem key={template.id} value={template.id}>
                      {template.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {templatesLoading && (
                <p className="text-xs text-muted-foreground mt-1">Loading available templates...</p>
              )}
              {!templatesLoading && availableTemplates.length > 0 && (
                <p className="text-xs text-muted-foreground mt-1">
                  {availableTemplates.length} templates available
                </p>
              )}
            </div>

            {/* Actions */}
            <div className="flex items-end">
              <Button onClick={handleRefresh} disabled={loading} className="w-full">
                {loading ? (
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                ) : (
                  <RefreshCw className="w-4 h-4 mr-2" />
                )}
                Refresh Data
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Error Display */}
      {error && (
        <Card className="mb-6 border-destructive">
          <CardContent className="pt-6">
            <div className="flex items-center gap-2 text-destructive">
              <span className="font-medium">Error:</span>
              <span>{error}</span>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Portfolio Info */}
      {renderPortfolioInfo()}

      {/* Template Preview */}
      {portfolioData && TemplateComponent && (
        <Card>
          <CardHeader>
            <CardTitle>
              Template Preview: {availableTemplates.find(t => t.id === selectedTemplate)?.name || selectedTemplate}
            </CardTitle>
            {availableTemplates.find(t => t.id === selectedTemplate)?.description && (
              <p className="text-sm text-muted-foreground">
                {availableTemplates.find(t => t.id === selectedTemplate)?.description}
              </p>
            )}
          </CardHeader>
          <CardContent>
            <div className="border rounded-lg overflow-hidden">
              <TemplateComponent data={portfolioData} />
            </div>
          </CardContent>
        </Card>
      )}

      {/* Loading State */}
      {loading && (
        <Card>
          <CardContent className="py-12">
            <div className="flex items-center justify-center">
              <Loader2 className="w-8 h-8 mr-2 animate-spin" />
              <span>Loading portfolio data...</span>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
} 