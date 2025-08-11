// Portfolio Template Interfaces
export interface TemplateConfig {
  id: string;
  name: string;
  description: string;
  previewImage: string;
}

export interface PortfolioData {
  id?: string;
  userId?: string;
  templateId?: string;
  basicInfo: BasicInfo;
  stats: StatData[];
  contacts: ContactInfo;
  quotes: Quote[];
  experience: Experience[];
  projects: Project[];
  skills: Skill[];
  socialLinks: SocialLink[];
  blogPosts?: BlogPost[];
}

export interface BasicInfo {
  name: string;
  title: string;
  bio: string;
  profileImage: string;
  location?: string;
}

export interface StatData {
  id: string;
  label: string;
  value: string;
  icon?: string;
}

// Alias for StatData to match component usage
export type Stat = StatData;

export interface ContactInfo {
  email: string;
  location?: string;
}

export interface Quote {
  id: string;
  text: string;
  author?: string;
  position?: string;
}

export interface Experience {
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

export interface Project {
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

export interface Skill {
  id: string;
  portfolioId: string;
  name: string;
  categoryType?: string; // 'hard_skills' or 'soft_skills'
  subcategory?: string; // 'frontend', 'backend', 'communication', etc.
  category?: string; // Full category path for display (deprecated but kept for backward compatibility)
  proficiencyLevel?: number; // 1-100
  displayOrder?: number;
  createdAt: string;
  updatedAt: string;
}

export interface SocialLink {
  id: string;
  platform: string;
  url: string;
  icon: string;
}

export interface BlogPost {
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

// Database-driven template system interfaces
export interface ComponentConfig {
  id: string;
  type: 'experience' | 'projects' | 'skills' | 'blog_posts' | 'contact' | 'about';
  order: number;
  isVisible: boolean;
  settings?: Record<string, unknown>;
}

export interface UserPortfolio {
  id: string;
  userId: string;
  templateId: string;
  templateName?: string; // Add template name for easier access
  title?: string;
  bio?: string;
  isPublished: boolean;
  visibility: 0 | 1 | 2; // 0=public, 1=private, 2=unlisted
  viewCount: number;
  likeCount: number;
  components?: ComponentConfig[];
  createdAt: string;
  updatedAt: string;
}

export interface UserProfile {
  id: string;
  name: string;
  title: string;
  bio: string;
  profileImage: string;
  location?: string;
  email: string;
}

export interface PortfolioDataFromDB {
  portfolio: UserPortfolio;
  profile: UserProfile;
  stats: StatData[];
  contacts: ContactInfo;
  quotes: Quote[];
  experience: Experience[];
  projects: Project[];
  skills: Skill[];
  socialLinks: SocialLink[];
  blogPosts: BlogPost[];
}

export interface Bookmark {
  id: string;
  portfolioId: string;
  title: string;
  url: string;
  description?: string;
  tags?: string[];
  createdAt: string;
  updatedAt: string;
}

export interface PortfolioTemplate {
  id: string;
  name: string;
  description?: string;
  previewImageUrl?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Portfolio {
  id: string;
  userId: string;
  templateId: string;
  title: string;
  bio?: string;
  viewCount: number;
  likeCount: number;
  visibility: 0 | 1 | 2; // 0=public, 1=private, 2=unlisted
  isPublished: boolean;
  updatedAt: string;
  components?: ComponentConfig[];
  template?: PortfolioTemplate;
}

export interface UserPortfolioComprehensive {
  userId: string;
  portfolios: Portfolio[];
  projects: Project[];
  experience: Experience[];
  skills: Skill[];
  blogPosts: BlogPost[];
  bookmarks: Bookmark[];
  templates: PortfolioTemplate[];
}
