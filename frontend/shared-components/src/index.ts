// Main exports for the shared components package

// UI Components
export * from './components/ui/button';
export * from './components/ui/input';
export * from './components/ui/select';
export * from './components/ui/dropdown-menu';
export * from './components/ui/dialog';
export * from './components/ui/alert-dialog';
export * from './components/ui/tabs';
export * from './components/ui/card';
export * from './components/ui/badge';
export * from './components/ui/avatar';
export * from './components/ui/checkbox';
export * from './components/ui/label';
export * from './components/ui/progress';
export * from './components/ui/textarea';
export * from './components/ui/toast';
export * from './components/ui/chart';
export * from './components/ui/form-dialog';
export * from './components/ui/image-upload';
export * from './components/ui/loading-modal';
export * from './components/ui/skill-dropdown';
export * from './components/ui/animated-number';
export * from './components/ui/component-ordering';

// Layout Components
export { default as Header } from './components/header';
export { Providers } from './components/providers';

// Home Page Components
export * from './components/home-page';

// Portfolio Components
export * from './components/portfolio-page';
export * from './components/portfolio-templates';

// Profile Components
export * from './components/profile-page';

// Publish Components
export * from './components/publish-page';

// Message Components
export * from './components/messages-page';

// Admin Components
export * from './components/admin';

// Auth Components
export * from './components/auth';

// Loader Components
export * from './components/loader';

// Hooks
export * from './hooks/useModalAnimation';

// Library Functions
export * from './lib/utils';
export * from './lib/encryption';
export { 
  getPortfolioTemplates, 
  PORTFOLIO_TEMPLATES, 
  getTemplateById, 
  getDefaultTemplate, 
  convertTemplateUuidToId, 
  convertTemplateUuidToIdSync, 
  loadTemplateComponent 
} from './lib/templates';
export * from './lib/template-registry';
export * from './lib/template-manager';

// Contexts
export * from './lib/contexts/user-context';
export * from './lib/contexts/portfolio-context';
export * from './lib/contexts/draft-context';
export * from './lib/contexts/bookmark-context';
export * from './lib/contexts/home-page-cache-context';
export { 
  WebSocketProvider, 
  useWebSocket 
} from './lib/contexts/websocket-context';
export type { WebSocketContextType } from './lib/contexts/websocket-context';
export * from './lib/contexts/use-portfolio-navigation';

// API Functions
export * from './lib/user';
export { 
  type Portfolio, 
  type Project, 
  type Experience, 
  type Skill, 
  type BlogPost,
  type PortfolioDataFromDB,
  type UserPortfolioComprehensive 
} from './lib/portfolio';
export * from './lib/messages';
export * from './lib/image';
export { bookmarkApi, type BookmarkToggleRequest, type BookmarkResponse } from './lib/bookmark';
export * from './lib/admin';

// Utils
export * from './utils/auth';

// Skills Configuration
export { default as skillsConfig } from './lib/skills-config.json';