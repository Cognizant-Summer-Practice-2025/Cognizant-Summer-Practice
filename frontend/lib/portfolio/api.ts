// Portfolio API functions
// This file contains API calls for portfolio-related operations

import { 
  PortfolioDataFromDB, 
  UserPortfolio,
  UserPortfolioComprehensive
} from './interfaces';

const API_BASE_URL = 'http://localhost:5201'; // Portfolio service URL

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
  template?: {
    id: string;
    name: string;
    description: string;
    componentName: string;
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
  componentName: string;
  previewImageUrl?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

interface PortfolioTemplateRequestDto {
  name: string;
  description?: string;
  componentName: string;
  previewImageUrl?: string;
  isActive: boolean;
}

interface PortfolioTemplateSummaryDto {
  id: string;
  name: string;
  description?: string;
  componentName: string;
  previewImageUrl?: string;
  isActive: boolean;
}

interface PortfolioTemplateUpdateDto {
  name?: string;
  description?: string;
  componentName?: string;
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
  category?: string;
  proficiencyLevel?: number;
  displayOrder?: number;
  createdAt: string;
  updatedAt: string;
}

interface SkillRequestDto {
  portfolioId: string;
  name: string;
  category?: string;
  proficiencyLevel?: number;
  displayOrder?: number;
}

interface SkillSummaryDto {
  id: string;
  name: string;
  category?: string;
  proficiencyLevel?: number;
  displayOrder?: number;
}

interface SkillUpdateDto {
  name?: string;
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
}

// Request/Update DTOs
interface PortfolioRequestDto {
  userId: string;
  templateId: string;
  title: string;
  bio?: string;
  visibility: 0 | 1 | 2;
  isPublished: boolean;
}

interface PortfolioUpdateDto {
  title?: string;
  bio?: string;
  visibility?: 0 | 1 | 2;
  isPublished?: boolean;
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
  return {
    portfolio: {
      id: dto.id,
      userId: dto.userId,
      templateId: dto.templateId,
      title: dto.title,
      bio: dto.bio,
      isPublished: dto.isPublished,
      visibility: dto.visibility,
      viewCount: dto.viewCount,
      likeCount: dto.likeCount,
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
}

// ============= PORTFOLIO API FUNCTIONS =============

export async function getPortfolioById(portfolioId: string): Promise<PortfolioDataFromDB> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/${portfolioId}`);
  const data = await handleApiResponse<PortfolioResponseDto>(response);
  return convertPortfolioResponse(data);
}

export async function getPortfoliosByUserId(userId: string): Promise<UserPortfolio[]> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/user/${userId}`);
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
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt
  }));
}

export async function getAllPortfolios(): Promise<UserPortfolio[]> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio`);
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
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt
  }));
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
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt
  }));
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
    components: [], // Will be populated with default components
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
    components: [], // Will be populated with default components
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

export async function getAllTemplates(): Promise<PortfolioTemplateResponseDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate`);
  return handleApiResponse<PortfolioTemplateResponseDto[]>(response);
}

export async function getTemplateById(templateId: string): Promise<PortfolioTemplateResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate/${templateId}`);
  return handleApiResponse<PortfolioTemplateResponseDto>(response);
}

export async function getActiveTemplates(): Promise<PortfolioTemplateSummaryDto[]> {
  const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate/active`);
  return handleApiResponse<PortfolioTemplateSummaryDto[]>(response);
}

export async function createTemplate(templateData: PortfolioTemplateRequestDto): Promise<PortfolioTemplateResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(templateData),
  });
  
  return handleApiResponse<PortfolioTemplateResponseDto>(response);
}

export async function updateTemplate(templateId: string, templateData: PortfolioTemplateUpdateDto): Promise<PortfolioTemplateResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate/${templateId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(templateData),
  });
  
  return handleApiResponse<PortfolioTemplateResponseDto>(response);
}

export async function deleteTemplate(templateId: string): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/api/PortfolioTemplate/${templateId}`, {
    method: 'DELETE',
  });
  
  if (response.status === 204) {
    return true;
  }
  
  await handleApiResponse(response);
  return false;
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
  const response = await fetch(`${API_BASE_URL}/api/Project`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(projectData),
  });
  
  return handleApiResponse<ProjectResponseDto>(response);
}

export async function updateProject(projectId: string, projectData: ProjectUpdateDto): Promise<ProjectResponseDto> {
  const response = await fetch(`${API_BASE_URL}/api/Project/${projectId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(projectData),
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



// Split Bulk Portfolio Creation - First create portfolio, then add content
export async function createPortfolioAndGetId(portfolioData: PortfolioRequestDto): Promise<string> {
  const response = await fetch(`${API_BASE_URL}/api/Portfolio/create-and-get-id`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(portfolioData),
  });
  
  const data = await handleApiResponse<{ portfolioId: string }>(response);
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
