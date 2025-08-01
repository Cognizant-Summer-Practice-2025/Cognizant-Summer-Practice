// Main exports for the shared components package

// UI Components
export { Button, buttonVariants } from './components/ui/button';
export { Input } from './components/ui/input';
export { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './components/ui/select';
export { 
  DropdownMenu, 
  DropdownMenuContent, 
  DropdownMenuItem, 
  DropdownMenuTrigger,
  DropdownMenuSeparator,
  DropdownMenuLabel,
  DropdownMenuRadioItem,
  DropdownMenuSubTrigger
} from './components/ui/dropdown-menu';
export { 
  Dialog, 
  DialogContent, 
  DialogHeader, 
  DialogTitle, 
  DialogTrigger,
  DialogFooter,
  DialogDescription
} from './components/ui/dialog';
export { 
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
  AlertProvider,
  useAlert
} from './components/ui/alert-dialog';
export { Tabs, TabsContent, TabsList, TabsTrigger } from './components/ui/tabs';
export { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from './components/ui/card';
export { Badge, badgeVariants } from './components/ui/badge';
export { Avatar, AvatarFallback, AvatarImage } from './components/ui/avatar';
export { Checkbox } from './components/ui/checkbox';
export { Label } from './components/ui/label';
export { Progress } from './components/ui/progress';
export { Textarea } from './components/ui/textarea';
export { useToast, ToastProvider, message } from './components/ui/toast';
export { 
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
  ChartStyle,
  type ChartConfig
} from './components/ui/chart';
export { FormDialog } from './components/ui/form-dialog';
export { ImageUpload } from './components/ui/image-upload';
export { LoadingModal } from './components/ui/loading-modal';
export { SkillDropdown } from './components/ui/skill-dropdown';
export { AnimatedNumber } from './components/ui/animated-number';
export { ComponentOrdering } from './components/ui/component-ordering';

// Layout Components
export { default as Header } from './components/header';
export { Providers } from './components/providers';

// Home Page Components
export { 
  FilterSidebar, 
  FilterGroup, 
  SelectedFilters, 
  SkillsFilter, 
  MobileFilter, 
  ModernSearch,
  PortfolioCard,
  PortfolioGrid,
  HomeHeader,
  PaginationControls
} from './components/home-page';
export type { 
  FilterOption, 
  SkillCategory, 
  SkillSubcategory,
  FilterSidebarProps,
  FilterGroupProps,
  SelectedFiltersProps,
  SkillsFilterProps,
  MobileFilterProps
} from './components/home-page';

// Portfolio Components
export { 
  ContactSection, 
  ExperienceSection, 
  ProfileSection, 
  QuotesSection, 
  SocialLinksSection, 
  StatsSection, 
  TabsSection 
} from './components/portfolio-page';

// Portfolio Templates (specific exports)
export { default as CreativeTemplate } from './components/portfolio-templates/creative';
export { default as CyberpunkTemplate } from './components/portfolio-templates/cyberpunk';
export { default as GabrielBarzuTemplate } from './components/portfolio-templates/gabriel-barzu';
export { default as ModernTemplate } from './components/portfolio-templates/modern';
export { default as ProfessionalTemplate } from './components/portfolio-templates/professional';
export { default as RetroGamingTemplate } from './components/portfolio-templates/retro-gaming';
export { default as TerminalTemplate } from './components/portfolio-templates/terminal';

// Profile Components
export { 
  BasicInfo as ProfileBasicInfo, 
  Experience as ProfileExperience, 
  ProfileSidebar, 
  Projects as ProfileProjects, 
  Settings as ProfileSettings, 
  Skills as ProfileSkills, 
  Template as ProfileTemplate 
} from './components/profile-page';

// Publish Components
export { 
  AddBlogPost, 
  AddExperience, 
  AddProject, 
  AddSkills, 
  BlogPostsList, 
  ExperienceList, 
  ProjectsList, 
  SkillsList,
  PortfolioInformation,
  PortfolioSettings,
  PublishHeader,
  PublishSidebar,
  PublishTabs,
  SkillTag
} from './components/publish-page';

// Message Components
export { 
  MessageAvatar, 
  MessageButton, 
  Chat, 
  VirtualizedMessageItem, 
  VirtualizedMessagesList, 
  ChatHeader, 
  ConversationMenu, 
  MessageInput, 
  MessageMenu, 
  MessagesHeader, 
  MessagesSidebar, 
  UserSearchModal 
} from './components/messages-page';

// Admin Components
// Temporarily disable chart components to resolve recharts bundling issues
export { 
  // UserGrowthChart,
  // ProjectTypesChart,
  // DailyActivityChart,
  // TrendChart,
  // ChartsSection, 
  AdminHeader, 
  StatsCards, 
  UserDetailsDialog, 
  UserManagement 
} from './components/admin';

// Auth Components
export { RegistrationModal } from './components/auth';

// Loader Components
export { Loading, LoadingOverlay } from './components/loader';

// Hooks
export { useModalAnimation } from './hooks/useModalAnimation';

// Library Functions
export { cn } from './lib/utils';
export { MessageEncryption } from './lib/encryption';
export { 
  getPortfolioTemplates, 
  PORTFOLIO_TEMPLATES, 
  getTemplateById as getPortfolioTemplateById, 
  getDefaultTemplate, 
  convertTemplateUuidToId, 
  convertTemplateUuidToIdSync, 
  loadTemplateComponent 
} from './lib/templates';
export { templateRegistry } from './lib/template-registry';
export { TemplateManager } from './lib/template-manager';

// Contexts
export { useUser, UserProvider } from './lib/contexts/user-context';
export { usePortfolio, PortfolioProvider } from './lib/contexts/portfolio-context';
export { useProfile, ProfileProvider } from './lib/contexts/profile-context';
export { useDraft, DraftProvider } from './lib/contexts/draft-context';
export { useBookmarks, BookmarkProvider } from './lib/contexts/bookmark-context';
export { useHomePageCache, HomePageCacheProvider } from './lib/contexts/home-page-cache-context';
export { 
  WebSocketProvider, 
  useWebSocket 
} from './lib/contexts/websocket-context';
export type { WebSocketContextType } from './lib/contexts/websocket-context';
export { usePortfolioNavigation } from './lib/contexts/use-portfolio-navigation';

// API Functions
export { 
  searchUsers, 
  checkUserExists,
  registerUser,
  getUserByEmail,
  registerOAuthUser,
  checkOAuthProvider,
  checkUserOAuthProvider,
  updateOAuthProvider,
  getUserOAuthProviders,
  addOAuthProvider,
  removeOAuthProvider,
  updateUser,
  type SearchUser,
  type User,
  type CheckEmailResponse,
  type RegisterUserRequest,
  type RegisterOAuthUserRequest,
  type OAuthProvider,
  type CheckOAuthProviderResponse,
  type OAuthProviderSummary
} from './lib/user';
export {
  // Portfolio API functions
  getUserPortfolioComprehensive,
  getPortfolioComprehensive,
  incrementViewCount,
  getPortfolioCardsForHomePage,
  getUserPortfolioInfo,
  updatePortfolio,
  createPortfolioAndGetId,
  savePortfolioContent,
  createPortfolio,
  deletePortfolio,
  incrementLikeCount,
  decrementLikeCount,
  getProjectsByPortfolioId,
  createProject,
  updateProject,
  deleteProject,
  getExperienceByPortfolioId,
  createExperience,
  updateExperience,
  deleteExperience,
  getSkillsByPortfolioId,
  createSkill,
  updateSkill,
  deleteSkill,
  getBlogPostsByPortfolioId,
  createBlogPost,
  updateBlogPost,
  deleteBlogPost,
  getActiveTemplates,
  getAllTemplates,
  getTemplateByName,
  getTemplateById,
  // Portfolio types
  type Portfolio,
  type Project,
  type Experience,
  type Skill,
  type BlogPost,
  type PortfolioDataFromDB,
  type UserPortfolioComprehensive,
  type PortfolioCardDto,
  type ComponentConfig,
  type TemplateConfig,
  type UserProfile,
  type Quote,
  type ContactInfo,
  type StatData,
  type SocialLink,
  type Bookmark,
  type PortfolioTemplate,
  type UserPortfolio,
  type BasicInfo,
  type PortfolioData,
  type UserPortfolioInfo,
  type PortfolioRequestDto,
  type PortfolioUpdateDto,
  type ProjectResponseDto,
  type ProjectRequestDto,
  type ProjectUpdateDto
} from './lib/portfolio';
export { useMessages, type Message, type Conversation } from './lib/messages';
export { 
  uploadImage, 
  deleteImage, 
  getSupportedSubfolders,
  checkImageUploadHealth,
  validateImageFile,
  getFileExtension,
  generateImagePreview,
  type ImageUploadResponse,
  type SupportedSubfoldersResponse
} from './lib/image';
export { bookmarkApi, type BookmarkToggleRequest, type BookmarkResponse } from './lib/bookmark';
export { 
  AdminAPI,
  type AdminUser,
  type AdminPortfolio,
  type AdminStats,
  type UserWithPortfolio,
  type PortfolioWithOwner
} from './lib/admin';

// Utils
export { getSession, authOptions } from './utils/auth';

// Skills Configuration
export { default as skillsConfig } from './lib/skills-config.json';