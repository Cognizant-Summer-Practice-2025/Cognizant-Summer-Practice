// Deployment API functions
// This file contains API calls for portfolio deployment operations

import { AuthenticatedApiClient } from '../api/authenticated-client';

// Create authenticated API client for portfolio service
const portfolioClient = AuthenticatedApiClient.createPortfolioClient();

export interface DeploymentRequest {
  userId: string;
  portfolioId?: string;
  templateName: string;
  includeAllComponents: boolean;
  projectName?: string;
  customDomain?: string;
  environment: 'Development' | 'Staging' | 'Production';
}

export interface DeploymentResponse {
  deploymentId: string;
  portfolioId: string;
  userId: string;
  projectName: string;
  deploymentUrl: string;
  customDomain?: string;
  status: 'Pending' | 'InProgress' | 'Building' | 'Deploying' | 'Completed' | 'Failed' | 'Cancelled';
  errorMessage?: string;
  createdAt: string;
  completedAt?: string;
  vercelInfo?: {
    vercelProjectId: string;
    vercelDeploymentId: string;
    vercelUrl: string;
    vercelAlias?: string;
    buildDuration: string;
    vercelRegion: string;
  };
}

export interface DeploymentSummary {
  deploymentId: string;
  portfolioId: string;
  projectName: string;
  deploymentUrl: string;
  status: string;
  createdAt: string;
  completedAt?: string;
}

export interface TemplateInfo {
  templates: string[];
}

export interface TemplateExtractionRequest {
  templateName: string;
  includeStyles: boolean;
  includeComponents: boolean;
  minifyCode: boolean;
}

export interface TemplateExtractionResponse {
  templateName: string;
  mainComponent: string;
  components: Record<string, string>;
  styles: Record<string, string>;
  dependencies: string[];
  nextConfigJs: string;
  packageJson: string;
  totalSizeBytes: number;
  extractedAt: string;
}

// Deploy a portfolio to Vercel
export async function deployPortfolio(request: DeploymentRequest): Promise<DeploymentResponse> {
  try {
    console.log('📤 Deploying portfolio:', request);
    
    const response = await portfolioClient.post<DeploymentResponse>(
      '/api/deployment/deploy',
      request,
      true // requireAuth
    );
    
    console.log('📥 Deployment response:', response);
    return response;
  } catch (error) {
    console.error('❌ Error deploying portfolio:', error);
    throw error;
  }
}

// Get deployment status by ID
export async function getDeploymentStatus(deploymentId: string): Promise<DeploymentResponse> {
  try {
    return await portfolioClient.get<DeploymentResponse>(
      `/api/deployment/${deploymentId}/status`,
      true // requireAuth
    );
  } catch (error) {
    console.error('❌ Error getting deployment status:', error);
    throw error;
  }
}

// Get all deployments for current user
export async function getUserDeployments(userId: string): Promise<DeploymentSummary[]> {
  try {
    return await portfolioClient.get<DeploymentSummary[]>(
      `/api/deployment/user/${userId}`,
      true // requireAuth
    );
  } catch (error) {
    console.error('❌ Error getting user deployments:', error);
    throw error;
  }
}

// Get all deployments for a specific portfolio
export async function getPortfolioDeployments(portfolioId: string): Promise<DeploymentSummary[]> {
  try {
    return await portfolioClient.get<DeploymentSummary[]>(
      `/api/deployment/portfolio/${portfolioId}`,
      true // requireAuth
    );
  } catch (error) {
    console.error('❌ Error getting portfolio deployments:', error);
    throw error;
  }
}

// Cancel a deployment
export async function cancelDeployment(deploymentId: string): Promise<{ message: string }> {
  try {
    return await portfolioClient.post<{ message: string }>(
      `/api/deployment/${deploymentId}/cancel`,
      undefined,
      true // requireAuth
    );
  } catch (error) {
    console.error('❌ Error cancelling deployment:', error);
    throw error;
  }
}

// Delete a deployment
export async function deleteDeployment(deploymentId: string): Promise<{ message: string }> {
  try {
    return await portfolioClient.delete<{ message: string }>(
      `/api/deployment/${deploymentId}`,
      true // requireAuth
    );
  } catch (error) {
    console.error('❌ Error deleting deployment:', error);
    throw error;
  }
}

// Update deployment domain
export async function updateDeploymentDomain(
  deploymentId: string, 
  customDomain: string
): Promise<DeploymentResponse> {
  try {
    return await portfolioClient.put<DeploymentResponse>(
      `/api/deployment/${deploymentId}/domain`,
      { customDomain },
      true // requireAuth
    );
  } catch (error) {
    console.error('❌ Error updating deployment domain:', error);
    throw error;
  }
}

// Get available templates
export async function getAvailableTemplates(): Promise<TemplateInfo> {
  try {
    return await portfolioClient.get<TemplateInfo>(
      '/api/deployment/templates',
      true // requireAuth
    );
  } catch (error) {
    console.error('❌ Error getting available templates:', error);
    throw error;
  }
}

// Extract template (for preview)
export async function extractTemplate(request: TemplateExtractionRequest): Promise<TemplateExtractionResponse> {
  try {
    return await portfolioClient.post<TemplateExtractionResponse>(
      '/api/deployment/templates/extract',
      request,
      true // requireAuth
    );
  } catch (error) {
    console.error('❌ Error extracting template:', error);
    throw error;
  }
}

// Validate template
export async function validateTemplate(templateName: string): Promise<{ isValid: boolean; templateName: string }> {
  try {
    return await portfolioClient.get<{ isValid: boolean; templateName: string }>(
      `/api/deployment/templates/${templateName}/validate`,
      true // requireAuth
    );
  } catch (error) {
    console.error('❌ Error validating template:', error);
    throw error;
  }
}
