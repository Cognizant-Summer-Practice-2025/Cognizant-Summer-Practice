// AI API functions for portfolio generation
import { getSession } from "next-auth/react";

const AI_API_BASE_URL = 'http://localhost:5134'; // AI service URL

// Portfolio data returned by AI service
export interface AIGeneratedPortfolio {
  id: string;
  userId: string;
  templateId: string;
  title: string;
  bio?: string;
  viewCount: number;
  likeCount: number;
  visibility: 0 | 1 | 2;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
  components?: string;
}

// Response from AI generate-best-portfolio endpoint
export interface AIPortfolioResponse {
  response: any[]; // eslint-disable-line @typescript-eslint/no-explicit-any - Backend returns JsonElement[], not structured objects
}

// Helper function to handle API responses
async function handleApiResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ 
      message: `HTTP error! status: ${response.status}` 
    }));
    throw new Error(errorData.message || errorData.error || `HTTP error! status: ${response.status}`);
  }
  return response.json();
}

// Generate best portfolios using AI
export async function generateBestPortfolios(): Promise<any[]> { // eslint-disable-line @typescript-eslint/no-explicit-any
  // Get session for authentication
  const session = await getSession();
  
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  // Add OAuth 2.0 Bearer token if available
  if (session?.accessToken) {
    headers['Authorization'] = `Bearer ${session.accessToken}`;
  } else {
    throw new Error('Authentication required. Please sign in to generate portfolios.');
  }

  const response = await fetch(`${AI_API_BASE_URL}/api/ai/generate-best-portfolio`, {
    method: 'GET',
    headers,
  });
  
  const data = await handleApiResponse<AIPortfolioResponse>(response);
  console.log('Backend AI Response:', data); // Debug log
  return data.response || [];
}

// Convert AI portfolio data to PortfolioCard format
export async function convertAIPortfoliosToCards(aiPortfolios: any[]): Promise<import('../portfolio/api').PortfolioCardDto[]> { // eslint-disable-line @typescript-eslint/no-explicit-any
  const { getUserPortfolioInfo } = await import('../portfolio/api');
  
  const portfolioCards = await Promise.all(
    aiPortfolios.map(async (portfolio, index) => {
      try {
        // Handle both structured data and raw JSON elements
        const portfolioId = portfolio.id || portfolio.Id || `ai-portfolio-${index}`;
        const portfolioUserId = portfolio.userId || portfolio.UserId || 'unknown-user';
        const portfolioBio = portfolio.bio || portfolio.Bio || '';
        const updatedAt = portfolio.updatedAt || portfolio.UpdatedAt || new Date().toISOString();
        const viewCount = portfolio.viewCount || portfolio.ViewCount || 0;
        const likeCount = portfolio.likeCount || portfolio.LikeCount || 0;
        
        let userInfo;
        try {
          userInfo = await getUserPortfolioInfo(portfolioUserId);
        } catch (userError) {
          console.warn(`Failed to fetch user info for ${portfolioUserId}:`, userError);
          userInfo = null;
        }
        
        // Format the date
        const formattedDate = new Date(updatedAt).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          year: 'numeric'
        });
        
        return {
          id: portfolioId,
          userId: portfolioUserId,
          name: userInfo?.name || userInfo?.username || 'Portfolio Creator',
          role: userInfo?.professionalTitle || 'Professional',
          location: userInfo?.location || 'Location not specified',
          description: portfolioBio || 'AI-generated portfolio with curated content',
          skills: [], // Will be populated from the actual portfolio data if needed
          views: viewCount,
          likes: likeCount,
          comments: 0, // Not available in AI response
          bookmarks: 0, // Not available in AI response
          date: formattedDate,
          avatar: userInfo?.avatarUrl,
          featured: false,
          templateName: undefined
        } as import('../portfolio/api').PortfolioCardDto;
      } catch (error) {
        console.error(`Error processing portfolio at index ${index}:`, error, portfolio);
        
        // Return card with fallback values
        return {
          id: `ai-error-portfolio-${index}`,
          userId: 'unknown',
          name: 'Portfolio Creator',
          role: 'Professional', 
          location: 'Location not specified',
          description: 'AI-generated portfolio (error processing)',
          skills: [],
          views: 0,
          likes: 0,
          comments: 0,
          bookmarks: 0,
          date: new Date().toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric', 
            year: 'numeric'
          }),
          avatar: undefined,
          featured: false,
          templateName: undefined
        } as import('../portfolio/api').PortfolioCardDto;
      }
    })
  );
  
  return portfolioCards;
} 