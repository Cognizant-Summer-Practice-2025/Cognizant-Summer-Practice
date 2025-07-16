export interface User {
  id: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  avatarUrl?: string;
  isActive: boolean;
  isAdmin: boolean;
  lastLoginAt?: string;
}

export interface CheckEmailResponse {
  exists: boolean;
  user: User | null;
}

export interface RegisterUserRequest {
  email: string;
  firstName?: string;
  lastName?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
}

export interface OAuthProvider {
  id: string;
  userId: string;
  provider: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn';
  providerId: string;
  providerEmail: string;
  tokenExpiresAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface OAuthProviderSummary {
  id: string;
  provider: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn';
  providerEmail: string;
  createdAt: string;
}

export interface RegisterOAuthUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
  provider: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn';
  providerId: string;
  providerEmail: string;
  accessToken: string;
  refreshToken?: string;
  tokenExpiresAt?: string;
}

export interface CheckOAuthProviderResponse {
  exists: boolean;
  provider: OAuthProvider | null;
}

// Portfolio Template Interfaces
export interface TemplateConfig {
  id: string;
  name: string;
  description: string;
  previewImage: string;
  componentName: string;
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
  description: string;
  skillsUsed: string[];
  createdAt: string;
  updatedAt: string;
}

export interface Project {
  id: string;
  portfolioId: string;
  title: string;
  description: string;
  imageUrl: string;
  demoUrl?: string;
  githubUrl?: string;
  technologies: string[];
  featured: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Skill {
  id: string;
  portfolioId: string;
  name: string;
  category: string;
  proficiencyLevel: number; // 1-100
  displayOrder: number;
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
  excerpt: string;
  content: string;
  featuredImageUrl?: string;
  tags: string[];
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
  title?: string;
  bio?: string;
  isPublished: boolean;
  visibility: 'public' | 'private' | 'unlisted';
  viewCount: number;
  likeCount: number;
  customConfig?: Record<string, unknown>;
  components: ComponentConfig[];
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