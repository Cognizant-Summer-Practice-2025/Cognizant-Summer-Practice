// Portfolio API functions
// This file contains API calls for portfolio-related operations

import { 
  PortfolioDataFromDB, 
  UserPortfolio,
  UserPortfolioComprehensive,
  Portfolio,
  Project,
  Experience,
  Skill,
  BlogPost,
  Bookmark,
  PortfolioTemplate
} from './interfaces';

// Do not throw at module load time to allow static builds without envs.
const API_BASE_URL = process.env.NEXT_PUBLIC_PORTFOLIO_API_URL ?? '';
const USER_API_BASE_URL = process.env.NEXT_PUBLIC_USER_API_URL ?? '';

// User info for portfolio cards
export interface UserPortfolioInfo {
  userId: string;
  username: string;
  name: string;
  professionalTitle: string;
  location: string;
  avatarUrl?: string;
  email: string;
}

// Portfolio Card DTO for home page display
export interface PortfolioCardDto {
  id: string;
  userId: string;
  name: string;
  role: string;
  location: string;
  description: string;
  skills: string[];
  views: number;
  likes: number;
  comments: number;
  bookmarks: number;
  date: string;
  avatar?: string;
  featured: boolean;
  templateName?: string;
}

// Portfolio API Response Types (matching backend DTOs)
interface PortfolioResponseDto {
  id: string;
  userId: string;
  templateId: string;
  title: string;
  bio?: string;
  viewCount: number;
  likeCount: number;
  visibility: 0 | 1 | 2; // 0=public, 1=private, 2=unlisted
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
  components?: string;
  template?: {
    id: string;
    name: string;
    description: string;
    previewImageUrl: string;
    isActive: boolean;
  };
  projects: ProjectSummaryDto[];
  experience: ExperienceSummaryDto[];
  skills: SkillSummaryDto[];
  blogPosts: BlogPostSummaryDto[];
}

// Portfolio Template DTOs
interface PortfolioTemplateResponseDto {
  id: string;
  name: string;
  description?: string;
  previewImageUrl?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

interface PortfolioTemplateRequestDto {
  name: string;
  description?: string;
  previewImageUrl?: string;
  isActive: boolean;
}

interface PortfolioTemplateSummaryDto {
  id: string;
  name: string;
  description?: string;
  previewImageUrl?: string;
  isActive: boolean;
}

interface PortfolioTemplateUpdateDto {
  name?: string;
  description?: string;
  previewImageUrl?: string;
  isActive?: boolean;
}

// Individual Entity DTOs
interface ProjectResponseDto {
  id: string;
  portfolioId: string;
  title: string;
  description?: string;
  imageUrl?: string;
  demoUrl?: string;
  githubUrl?: string;
  technologies?: string[];
  featured: boolean;
  createdAt: string;
  updatedAt: string;
}

interface ProjectRequestDto {
  portfolioId: string;
  title: string;
  description?: string;
  imageUrl?: string;
  demoUrl?: string;
  githubUrl?: string;
  technologies?: string[];
  featured: boolean;
}

interface ProjectSummaryDto {
  id: string;
  title: string;
  description?: string;
  imageUrl?: string;
  demoUrl?: string;
  githubUrl?: string;
  technologies?: string[];
  featured: boolean;
}

interface ProjectUpdateDto {
  title?: string;
  description?: string;
  imageUrl?: string;
  demoUrl?: string;
  githubUrl?: string;
  technologies?: string[];
  featured?: boolean;
}

interface ExperienceResponseDto {
  id: string;
  portfolioId: string;
  jobTitle: string;
  companyName: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  description?: string;
  skillsUsed?: string[];
  createdAt: string;
  updatedAt: string;
}

interface ExperienceRequestDto {
  portfolioId: string;
  jobTitle: string;
  companyName: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  description?: string;
  skillsUsed?: string[];
}

interface ExperienceSummaryDto {
  id: string;
  jobTitle: string;
  companyName: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  description?: string;
  skillsUsed?: string[];
}

interface ExperienceUpdateDto {
  jobTitle?: string;
  companyName?: string;
  startDate?: string;
  endDate?: string;
  isCurrent?: boolean;
  description?: string;
  skillsUsed?: string[];
}

interface SkillResponseDto {
  id: string;
  portfolioId: string;
  name: string;
  categoryType?: string;
  subcategory?: string;
  category?: string;
  proficiencyLevel?: number;
  displayOrder?: number;
  createdAt: string;
  updatedAt: string;
}

interface SkillRequestDto {
  portfolioId: string;
  name: string;
  categoryType?: string;
  subcategory?: string;
  category?: string;
  proficiencyLevel?: number;
  displayOrder?: number;
}

interface SkillSummaryDto {
  id: string;
  name: string;
  categoryType?: string;
  subcategory?: string;
  category?: string;
  proficiencyLevel?: number;
  displayOrder?: number;
}

interface SkillUpdateDto {
  name?: string;
  categoryType?: string;
  subcategory?: string;
  category?: string;
  proficiencyLevel?: number;
  displayOrder?: number;
}

interface BlogPostResponseDto {
  id: string;
  portfolioId: string;
  title: string;
  excerpt?: string;
  content?: string;
  featuredImageUrl?: string;
  tags?: string[];
  isPublished: boolean;
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
}

interface BlogPostRequestDto {
  portfolioId: string;
  title: string;
  excerpt?: string;
  content?: string;
  featuredImageUrl?: string;
  tags?: string[];
  isPublished: boolean;
}

interface BlogPostSummaryDto {
  id: string;
  title: string;
  excerpt?: string;
  featuredImageUrl?: string;
  tags?: string[];
  isPublished: boolean;
  publishedAt?: string;
  updatedAt: string;
}

interface BlogPostUpdateDto {
  title?: string;
  excerpt?: string;
  content?: string;
  featuredImageUrl?: string;
  tags?: string[];
  isPublished?: boolean;
}

interface BookmarkResponseDto {
  id: string;
  userId: string;
  portfolioId: string;
  collectionName?: string;
  notes?: string;
  createdAt: string;
  portfolio?: PortfolioSummaryDto;
}

interface BookmarkRequestDto {
  userId: string;
  portfolioId: string;
  collectionName?: string;
  notes?: string;
}

interface BookmarkSummaryDto {
  id: string;
  userId: string;
  portfolioId: string;
  collectionName?: string;
  createdAt: string;
  portfolio?: PortfolioSummaryDto;
}

interface BookmarkUpdateDto {
  collectionName?: string;
  notes?: string;
}

// Bulk Portfolio DTOs
interface BulkPortfolioContentDto {
  portfolioId: string;
  projects?: ProjectRequestDto[];
  experience?: ExperienceRequestDto[];
  skills?: SkillRequestDto[];
  blogPosts?: BlogPostRequestDto[];
  publishPortfolio?: boolean;
}

interface BulkPortfolioResponseDto {
  message: string;
  projectsCreated: number;
  experienceCreated: number;
  skillsCreated: number;
  blogPostsCreated: number;
  portfolioPublished: boolean;
}

interface PortfolioSummaryDto {
  id: string;
  userId: string;
  templateId: string;
  title: string;
  bio?: string;
  viewCount: number;
  likeCount: number;
  visibility: 0 | 1 | 2;
  isPublished: boolean;
  updatedAt: string;
  createdAt: string;
  components?: string;
}

// Request/Update DTOs
interface PortfolioRequestDto {
  userId: string;
  templateName: string; // Changed from templateId to templateName
  title: string;
  bio?: string;
  visibility: 0 | 1 | 2;
  isPublished: boolean;
  components?: string;
}

interface PortfolioUpdateDto {
  title?: string;
  bio?: string;
  visibility?: 0 | 1 | 2;
  isPublished?: boolean;
  components?: string;
  templateName?: string; // Allow updating template by name
}

// Bulk Portfolio Content DTO
interface BulkPortfolioContentDto {
  portfolioId: string;
  projects?: ProjectRequestDto[];
  experience?: ExperienceRequestDto[];
  skills?: SkillRequestDto[];
  blogPosts?: BlogPostRequestDto[];
  publishPortfolio?: boolean;
}

// Bulk Portfolio Response DTO
interface BulkPortfolioResponseDto {
  message: string;
  projectsCreated: number;
  experienceCreated: number;
  skillsCreated: number;
  blogPostsCreated: number;
  portfolioPublished: boolean;
}

// Helper function to handle API responses
async function handleApiResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ message: 'Unknown error' }));
    throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
  }
  return response.json();
}

// Helper function to convert backend DTO to frontend interface
function convertPortfolioResponse(dto: PortfolioResponseDto): PortfolioDataFromDB {
  
  const result = {
    portfolio: {
      id: dto.id,
      userId: dto.userId,
      templateId: dto.templateId,
      templateName: dto.template?.name, // Include template name for easier access
      title: dto.title,
      bio: dto.bio,
      isPublished: dto.isPublished,
      visibility: dto.visibility,
      viewCount: dto.viewCount,
      likeCount: dto.likeCount,
      components: dto.components ? JSON.parse(dto.components) : undefined,
      createdAt: dto.createdAt,
      updatedAt: dto.updatedAt
    },
    profile: {
      id: dto.userId,
      name: '', // Will need to fetch from user service
      title: '',
      bio: dto.bio || '',
      profileImage: '',
      location: '',
      email: ''
    },
    stats: [], // Will be populated with calculated stats
    contacts: {
      email: '',
      location: ''
    },
    quotes: [], // Will need to be stored in customSections or fetched separately
    experience: dto.experience.map(exp => ({
      id: exp.id,
      portfolioId: dto.id,
      jobTitle: exp.jobTitle,
      companyName: exp.companyName,
      startDate: exp.startDate,
      endDate: exp.endDate,
      isCurrent: exp.isCurrent,
      description: exp.description,
      skillsUsed: exp.skillsUsed,
      createdAt: dto.createdAt, // Backend doesn't return these for summary
      updatedAt: dto.updatedAt
    })),
    projects: dto.projects.map(proj => ({
      id: proj.id,
      portfolioId: dto.id,
      title: proj.title,
      description: proj.description,
      imageUrl: proj.imageUrl,
      demoUrl: proj.demoUrl,
      githubUrl: proj.githubUrl,
      technologies: proj.technologies,
      featured: proj.featured,
      createdAt: dto.createdAt, // Backend doesn't return these for summary
      updatedAt: dto.updatedAt
    })),
    skills: dto.skills.map(skill => ({
      id: skill.id,
      portfolioId: dto.id,
      name: skill.name,
      categoryType: skill.categoryType,
      subcategory: skill.subcategory,
      category: skill.category,
      proficiencyLevel: skill.proficiencyLevel,
      displayOrder: skill.displayOrder,
      createdAt: dto.createdAt, // Backend doesn't return these for summary
      updatedAt: dto.updatedAt
    })),
    socialLinks: [], // Will need to be stored in customSections or fetched separately
    blogPosts: dto.blogPosts.map(post => ({
      id: post.id,
      portfolioId: dto.id,
      title: post.title,
      excerpt: post.excerpt,
      content: '', // Summary doesn't include full content
      featuredImageUrl: post.featuredImageUrl,
      tags: post.tags,
      isPublished: post.isPublished,
      publishedAt: post.publishedAt,
      createdAt: dto.createdAt, // Backend doesn't return these for summary
      updatedAt: post.updatedAt
    }))
  };
  
  return result;
}

// ============= PORTFOLIO API FUNCTIONS =============

export async function getPortfolioById(portfolioId: string): Promise<PortfolioDataFromDB> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/${portfolioId}`);
  const data = await handleApiResponse<PortfolioResponseDto>(response);
  return convertPortfolioResponse(data);
}

// Get comprehensive portfolio data for a specific portfolio ID (similar to getUserPortfolioComprehensive but for a single portfolio)
export async function getPortfolioComprehensive(portfolioId: string): Promise<{
  portfolio: Portfolio;
  projects: Project[];
  experience: Experience[];
  skills: Skill[];
  blogPosts: BlogPost[];
  bookmarks: Bookmark[];
}> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/${portfolioId}`);
  const data = await handleApiResponse<PortfolioResponseDto>(response);
  
  const portfolioData = convertPortfolioResponse(data);
  
  // Convert to the format expected by the portfolio context
  const portfolio: Portfolio = {
    id: portfolioData.portfolio.id,
    userId: portfolioData.portfolio.userId,
    templateId: portfolioData.portfolio.templateId,
    title: portfolioData.portfolio.title || '',
    bio: portfolioData.portfolio.bio,
    visibility: portfolioData.portfolio.visibility,
    isPublished: portfolioData.portfolio.isPublished,
    viewCount: portfolioData.portfolio.viewCount,
    likeCount: portfolioData.portfolio.likeCount,
    updatedAt: portfolioData.portfolio.updatedAt,
    components: portfolioData.portfolio.components,
  };

  const result = {
    portfolio,
    projects: portfolioData.projects || [],
    experience: portfolioData.experience || [],
    skills: portfolioData.skills || [],
    blogPosts: portfolioData.blogPosts || [],
    bookmarks: [], // Bookmarks would need to be fetched separately if available
  };

  return result;
}

// Template cache for efficient lookups
const templateCache = new Map<string, string>(); // templateId -> templateName

// Helper function to get template name by ID with caching
async function getTemplateNameById(templateId: string): Promise<string | undefined> {
  // Check cache first
  if (templateCache.has(templateId)) {
    return templateCache.get(templateId);
  }
  
  try {
    const template = await getTemplateById(templateId);
    if (template) {
      templateCache.set(templateId, template.name);
      return template.name;
    }
  } catch (error) {
    console.error('Error fetching template name:', error);
  }
  
  return undefined;
}

export async function getPortfoliosByUserId(userId: string): Promise<UserPortfolio[]> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/user/${userId}`);
  const data = await handleApiResponse<PortfolioSummaryDto[]>(response);
  
  // Resolve template names efficiently
  const portfolios = await Promise.all(data.map(async (dto) => {
    const templateName = await getTemplateNameById(dto.templateId);
    
    return {
      id: dto.id,
      userId: dto.userId,
      templateId: dto.templateId,
      templateName: templateName, // Resolved template name
      title: dto.title,
      bio: dto.bio,
      isPublished: dto.isPublished,
      visibility: dto.visibility,
      viewCount: dto.viewCount,
      likeCount: dto.likeCount,
      components: dto.components ? JSON.parse(dto.components) : undefined,
      createdAt: dto.createdAt,
      updatedAt: dto.updatedAt
    };
  }));
  
  return portfolios;
}

export async function getAllPortfolios(): Promise<UserPortfolio[]> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio`);
  const data = await handleApiResponse<PortfolioSummaryDto[]>(response);
  
  // Resolve template names efficiently
  const portfolios = await Promise.all(data.map(async (dto) => {
    const templateName = await getTemplateNameById(dto.templateId);
    
    return {
      id: dto.id,
      userId: dto.userId,
      templateId: dto.templateId,
      templateName: templateName, // Resolved template name
      title: dto.title,
      bio: dto.bio,
      isPublished: dto.isPublished,
      visibility: dto.visibility,
      viewCount: dto.viewCount,
      likeCount: dto.likeCount,
      components: dto.components ? JSON.parse(dto.components) : undefined,
      createdAt: dto.createdAt,
      updatedAt: dto.updatedAt
    };
  }));
  
  return portfolios;
}

export async function getPublishedPortfolios(): Promise<UserPortfolio[]> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/published`);
  const data = await handleApiResponse<PortfolioSummaryDto[]>(response);
  
  return data.map(dto => ({
    id: dto.id,
    userId: dto.userId,
    templateId: dto.templateId,
    title: dto.title,
    bio: dto.bio,
    isPublished: dto.isPublished,
    visibility: dto.visibility,
    viewCount: dto.viewCount,
    likeCount: dto.likeCount,
    components: dto.components ? JSON.parse(dto.components) : undefined,
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt
  }));
}

// Get user info for portfolio cards
export async function getUserPortfolioInfo(userId: string): Promise<UserPortfolioInfo> {
  const response = await fetch(`${USER_API_BASE_URL}/api/Users/${userId}/portfolio-info`);
  
  const result = await handleApiResponse<UserPortfolioInfo>(response);
  
  return result;
}

export async function getPortfolioCardsForHomePage(): Promise<PortfolioCardDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/home-page-cards`);
  const portfolioCards = await handleApiResponse<PortfolioCardDto[]>(response);
  
  // Fetch user info for each portfolio card
  const enrichedCards = await Promise.all(
    portfolioCards.map(async (card) => {
      try {
        const userInfo = await getUserPortfolioInfo(card.userId);
        return {
          ...card,
          name: userInfo.name || userInfo.username,
          role: userInfo.professionalTitle,
          location: userInfo.location,
          avatar: userInfo.avatarUrl
        };
      } catch (error) {
        console.error(`❌ Error fetching user info for ${card.userId}:`, error);
        // Return card with fallback values
        return {
          ...card,
          name: 'Unknown User',
          role: 'Portfolio Creator', 
          location: 'Location not specified',
          avatar: undefined
        };
      }
    })
  );
  
  return enrichedCards;
}

export async function createPortfolio(portfolioData: PortfolioRequestDto): Promise<UserPortfolio> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(portfolioData),
  });
  
  const data = await handleApiResponse<PortfolioResponseDto>(response);
  
  return {
    id: data.id,
    userId: data.userId,
    templateId: data.templateId,
    title: data.title,
    bio: data.bio,
    isPublished: data.isPublished,
    visibility: data.visibility,
    viewCount: data.viewCount,
    likeCount: data.likeCount,
    components: data.components ? JSON.parse(data.components) : undefined,
    createdAt: data.createdAt,
    updatedAt: data.updatedAt
  };
}

export async function updatePortfolio(portfolioId: string, portfolioData: PortfolioUpdateDto): Promise<UserPortfolio> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/${portfolioId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(portfolioData),
  });
  
  const data = await handleApiResponse<PortfolioResponseDto>(response);
  
  return {
    id: data.id,
    userId: data.userId,
    templateId: data.templateId,
    title: data.title,
    bio: data.bio,
    isPublished: data.isPublished,
    visibility: data.visibility,
    viewCount: data.viewCount,
    likeCount: data.likeCount,
    components: data.components ? JSON.parse(data.components) : undefined,
    createdAt: data.createdAt,
    updatedAt: data.updatedAt
  };
}

export async function deletePortfolio(portfolioId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/${portfolioId}`, {
    method: 'DELETE',
  });
  
  if (response.status === 204) {
    return true;
  }
  
  await handleApiResponse(response);
  return false;
}

export async function incrementViewCount(portfolioId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/${portfolioId}/view`, {
    method: 'POST',
  });
  
  if (response.ok) {
    return true;
  }
  
  await handleApiResponse(response);
  return false;
}

export async function incrementLikeCount(portfolioId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/${portfolioId}/like`, {
    method: 'POST',
  });
  
  if (response.ok) {
    return true;
  }
  
  await handleApiResponse(response);
  return false;
}

export async function decrementLikeCount(portfolioId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/${portfolioId}/unlike`, {
    method: 'POST',
  });
  
  if (response.ok) {
    return true;
  }
  
  await handleApiResponse(response);
  return false;
}



// ============= PORTFOLIO TEMPLATE API FUNCTIONS =============

export async function getActiveTemplates(): Promise<PortfolioTemplate[]> {
  const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate/active`);
  return handleApiResponse<PortfolioTemplate[]>(response);
}

export async function getAllTemplates(): Promise<PortfolioTemplate[]> {
  const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate`);
  return handleApiResponse<PortfolioTemplate[]>(response);
}

export async function getTemplateByName(name: string): Promise<PortfolioTemplate | null> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate/name/${encodeURIComponent(name)}`);
    if (response.status === 404) {
      return null;
    }
    return handleApiResponse<PortfolioTemplate>(response);
  } catch (error) {
    console.error('Error fetching template by name:', error);
    return null;
  }
}

export async function getTemplateById(id: string): Promise<PortfolioTemplate | null> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate/${id}`);
    if (response.status === 404) {
      return null;
    }
    return handleApiResponse<PortfolioTemplate>(response);
  } catch (error) {
    console.error('Error fetching template by id:', error);
    return null;
  }
}

export async function seedDefaultTemplates(): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate/seed`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
  });
  await handleApiResponse<{ message: string }>(response);
}

// ============= INDIVIDUAL ENTITY API FUNCTIONS =============
// Note: These would need to be implemented as separate controllers in the backend
// For now, they're placeholder functions that could be used if individual entity endpoints are added

// Projects
export async function getProjectsByPortfolioId(portfolioId: string): Promise<ProjectResponseDto[]> {
  // This would require a ProjectController in the backend
  const response = await fetch(`${API_BASE_URL}/api/Project/portfolio/${portfolioId}`);
  return handleApiResponse<ProjectResponseDto[]>(response);
}

export async function createProject(projectData: ProjectRequestDto): Promise<ProjectResponseDto> {
  // Filter out undefined values to avoid sending them as empty strings
  const cleanedData = Object.fromEntries(
    Object.entries(projectData).filter(([, value]) => value !== undefined)
  ) as ProjectRequestDto;

  const response = await fetch(`${API_BASE_URL}/api/Project`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(cleanedData),
  });
  
  return handleApiResponse<ProjectResponseDto>(response);
}

export async function updateProject(projectId: string, projectData: ProjectUpdateDto): Promise<ProjectResponseDto> {
  // Filter out undefined values to avoid sending them as empty strings
  const cleanedData = Object.fromEntries(
    Object.entries(projectData).filter(([, value]) => value !== undefined)
  ) as ProjectUpdateDto;

  console.log('Sending project update data:', cleanedData); // Debug log

  const response = await fetch(`${API_BASE_URL}/api/Project/${projectId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(cleanedData),
  });
  
  return handleApiResponse<ProjectResponseDto>(response);
}

export async function deleteProject(projectId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/Project/${projectId}`, {
    method: 'DELETE',
  });
  
  return response.status === 204;
}

// Experience
export async function getExperienceByPortfolioId(portfolioId: string): Promise<ExperienceResponseDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/Experience/portfolio/${portfolioId}`);
  return handleApiResponse<ExperienceResponseDto[]>(response);
}

export async function createExperience(experienceData: ExperienceRequestDto): Promise<ExperienceResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/Experience`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(experienceData),
  });
  
  return handleApiResponse<ExperienceResponseDto>(response);
}

export async function updateExperience(experienceId: string, experienceData: ExperienceUpdateDto): Promise<ExperienceResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/Experience/${experienceId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(experienceData),
  });
  
  return handleApiResponse<ExperienceResponseDto>(response);
}

export async function deleteExperience(experienceId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/Experience/${experienceId}`, {
    method: 'DELETE',
  });
  
  return response.status === 204;
}

// Skills
export async function getSkillsByPortfolioId(portfolioId: string): Promise<SkillResponseDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/Skill/portfolio/${portfolioId}`);
  return handleApiResponse<SkillResponseDto[]>(response);
}

export async function createSkill(skillData: SkillRequestDto): Promise<SkillResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/Skill`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(skillData),
  });
  
  return handleApiResponse<SkillResponseDto>(response);
}

export async function updateSkill(skillId: string, skillData: SkillUpdateDto): Promise<SkillResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/Skill/${skillId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(skillData),
  });
  
  return handleApiResponse<SkillResponseDto>(response);
}

export async function deleteSkill(skillId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/Skill/${skillId}`, {
    method: 'DELETE',
  });
  
  return response.status === 204;
}

// Blog Posts
export async function getBlogPostsByPortfolioId(portfolioId: string): Promise<BlogPostResponseDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/BlogPost/portfolio/${portfolioId}`);
  return handleApiResponse<BlogPostResponseDto[]>(response);
}

export async function createBlogPost(blogPostData: BlogPostRequestDto): Promise<BlogPostResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/BlogPost`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(blogPostData),
  });
  
  return handleApiResponse<BlogPostResponseDto>(response);
}

export async function updateBlogPost(blogPostId: string, blogPostData: BlogPostUpdateDto): Promise<BlogPostResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/BlogPost/${blogPostId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(blogPostData),
  });
  
  return handleApiResponse<BlogPostResponseDto>(response);
}

export async function deleteBlogPost(blogPostId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/BlogPost/${blogPostId}`, {
    method: 'DELETE',
  });
  
  return response.status === 204;
}

// Bookmarks
export async function getBookmarksByUserId(userId: string): Promise<BookmarkResponseDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/Bookmark/user/${userId}`);
  return handleApiResponse<BookmarkResponseDto[]>(response);
}

export async function createBookmark(bookmarkData: BookmarkRequestDto): Promise<BookmarkResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/Bookmark`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(bookmarkData),
  });
  
  return handleApiResponse<BookmarkResponseDto>(response);
}

export async function updateBookmark(bookmarkId: string, bookmarkData: BookmarkUpdateDto): Promise<BookmarkResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/Bookmark/${bookmarkId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(bookmarkData),
  });
  
  return handleApiResponse<BookmarkResponseDto>(response);
}

export async function deleteBookmark(bookmarkId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/Bookmark/${bookmarkId}`, {
    method: 'DELETE',
  });
  
  return response.status === 204;
}

// Get comprehensive portfolio data for a user (all portfolios with related entities)
export async function getUserPortfolioComprehensive(userId: string): Promise<UserPortfolioComprehensive> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/user/${userId}/comprehensive`);
  
  if (!response.ok) {
    // If comprehensive endpoint doesn't exist, fall back to individual calls
    const portfolios = await getPortfoliosByUserId(userId);
    const templates = await getActiveTemplates();
    
    // For now, return empty arrays for other entities since we don't have direct user-level endpoints
    // In a real implementation, you'd need to aggregate data from all user's portfolios
    return {
      userId,
      portfolios: portfolios.map(p => ({
        id: p.id,
        userId: p.userId,
        templateId: p.templateId,
        title: p.title || '',
        bio: p.bio,
        viewCount: p.viewCount,
        likeCount: p.likeCount,
        visibility: p.visibility,
        isPublished: p.isPublished,
        updatedAt: p.updatedAt,
        components: p.components,
        template: undefined // Will be populated if needed
      })),
      projects: [], // Would need to aggregate from all portfolios
      experience: [], // Would need to aggregate from all portfolios
      skills: [], // Would need to aggregate from all portfolios
      blogPosts: [], // Would need to aggregate from all portfolios
      bookmarks: [], // Would need to fetch from bookmark service
      templates: templates.map(t => ({
        id: t.id,
        name: t.name,
        description: t.description,
        previewImageUrl: t.previewImageUrl,
        isActive: t.isActive,
        createdAt: '', // Not provided in summary
        updatedAt: '' // Not provided in summary
      }))
    };
  }
  
  return handleApiResponse<UserPortfolioComprehensive>(response);
}

// Split Bulk Portfolio Creation - First create portfolio, then add content
export async function createPortfolioAndGetId(portfolioData: PortfolioRequestDto): Promise<string> {
  console.log('📤 API: Creating portfolio with data:', portfolioData);
  
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/create-and-get-id`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(portfolioData),
  });
  
  console.log('📤 API: Portfolio creation response status:', response.status);
  
  const data = await handleApiResponse<{ portfolioId: string }>(response);
  console.log('📤 API: Portfolio created with ID:', data.portfolioId);
  return data.portfolioId;
}

export async function savePortfolioContent(portfolioId: string, contentData: {
  projects?: ProjectRequestDto[];
  experience?: ExperienceRequestDto[];
  skills?: SkillRequestDto[];
  blogPosts?: BlogPostRequestDto[];
  publishPortfolio?: boolean;
}): Promise<BulkPortfolioResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/${portfolioId}/save-content`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(contentData),
  });
  
  return handleApiResponse<BulkPortfolioResponseDto>(response);
}

// Export all API functions and types
export type {
  PortfolioRequestDto,
  PortfolioUpdateDto,
  PortfolioResponseDto,
  PortfolioTemplateResponseDto,
  PortfolioTemplateRequestDto,
  PortfolioTemplateSummaryDto,
  PortfolioTemplateUpdateDto,
  ProjectResponseDto,
  ProjectRequestDto,
  ProjectSummaryDto,
  ProjectUpdateDto,
  ExperienceResponseDto,
  ExperienceRequestDto,
  ExperienceSummaryDto,
  ExperienceUpdateDto,
  SkillResponseDto,
  SkillRequestDto,
  SkillSummaryDto,
  SkillUpdateDto,
  BlogPostResponseDto,
  BlogPostRequestDto,
  BlogPostSummaryDto,
  BlogPostUpdateDto,
  BookmarkResponseDto,
  BookmarkRequestDto,
  BookmarkSummaryDto,
  BookmarkUpdateDto,
  BulkPortfolioContentDto,
  BulkPortfolioResponseDto
};
