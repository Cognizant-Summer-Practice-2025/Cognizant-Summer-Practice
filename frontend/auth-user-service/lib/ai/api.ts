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
  response: AIGeneratedPortfolio[];
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
export async function generateBestPortfolios(): Promise<AIGeneratedPortfolio[]> {
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
  return data.response;
}

// Convert AI portfolio data to PortfolioCard format
export async function convertAIPortfoliosToCards(aiPortfolios: AIGeneratedPortfolio[]): Promise<import('../portfolio/api').PortfolioCardDto[]> {
  const { getUserPortfolioInfo } = await import('../portfolio/api');
  
  const portfolioCards = await Promise.all(
    aiPortfolios.map(async (portfolio) => {
      try {
        const userInfo = await getUserPortfolioInfo(portfolio.userId);
        
        // Format the date
        const formattedDate = new Date(portfolio.updatedAt).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          year: 'numeric'
        });
        
        return {
          id: portfolio.id,
          userId: portfolio.userId,
          name: userInfo.name || userInfo.username,
          role: userInfo.professionalTitle,
          location: userInfo.location,
          description: portfolio.bio || 'AI-generated portfolio with curated content',
          skills: [], // Will be populated from the actual portfolio data if needed
          views: portfolio.viewCount,
          likes: portfolio.likeCount,
          comments: 0, // Not available in AI response
          bookmarks: 0, // Not available in AI response
          date: formattedDate,
          avatar: userInfo.avatarUrl,
          featured: false,
          templateName: undefined
        } as import('../portfolio/api').PortfolioCardDto;
      } catch (error) {
        console.error(`Error fetching user info for ${portfolio.userId}:`, error);
        
        // Return card with fallback values
        const formattedDate = new Date(portfolio.updatedAt).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          year: 'numeric'
        });
        
        return {
          id: portfolio.id,
          userId: portfolio.userId,
          name: 'Portfolio Creator',
          role: 'Professional',
          location: 'Location not specified',
          description: portfolio.bio || 'AI-generated portfolio with curated content',
          skills: [],
          views: portfolio.viewCount,
          likes: portfolio.likeCount,
          comments: 0,
          bookmarks: 0,
          date: formattedDate,
          avatar: undefined,
          featured: false,
          templateName: undefined
        } as import('../portfolio/api').PortfolioCardDto;
      }
    })
  );
  
  return portfolioCards;
} 