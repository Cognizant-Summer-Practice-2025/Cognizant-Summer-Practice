import { PortfolioDataFromDB } from './interfaces';

// Mock portfolio data for testing templates
export const MOCK_PORTFOLIO_DATA: PortfolioDataFromDB = {
  portfolio: {
    id: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
    userId: 'user-123',
    templateId: 'modern',
    title: 'John Doe - Full Stack Developer',
    bio: 'Passionate full-stack developer with 5+ years of experience building scalable web applications and mobile solutions.',
    isPublished: true,
    visibility: 0,
    viewCount: 1250,
    likeCount: 89,
    components: [
      { id: 'about', type: 'about', order: 1, isVisible: true },
      { id: 'experience', type: 'experience', order: 2, isVisible: true },
      { id: 'projects', type: 'projects', order: 3, isVisible: true },
      { id: 'skills', type: 'skills', order: 4, isVisible: true },
      { id: 'blog_posts', type: 'blog_posts', order: 5, isVisible: true },
      { id: 'contact', type: 'contact', order: 6, isVisible: true }
    ],
    createdAt: '2024-01-15T10:30:00Z',
    updatedAt: '2024-01-20T15:45:00Z'
  },
  profile: {
    id: 'user-123',
    name: 'John Doe',
    title: 'Full Stack Developer',
    bio: 'Passionate full-stack developer with 5+ years of experience building scalable web applications and mobile solutions. I love working with modern technologies and creating user-friendly interfaces that solve real-world problems.',
    profileImage: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=400&fit=crop&crop=face',
    location: 'San Francisco, CA',
    email: 'john.doe@example.com'
  },
  stats: [
    { id: 'stat-1', label: 'Years Experience', value: '5+', icon: 'calendar' },
    { id: 'stat-2', label: 'Projects Completed', value: '50+', icon: 'briefcase' },
    { id: 'stat-3', label: 'Happy Clients', value: '25+', icon: 'users' },
    { id: 'stat-4', label: 'Code Commits', value: '2.5K+', icon: 'git-branch' }
  ],
  contacts: {
    email: 'john.doe@example.com',
    location: 'San Francisco, CA'
  },
  quotes: [
    {
      id: 'quote-1',
      text: 'John delivered exceptional work on our e-commerce platform. His attention to detail and technical expertise helped us increase our conversion rate by 40%.',
      author: 'Sarah Johnson',
      position: 'CTO, TechStart Inc.'
    },
    {
      id: 'quote-2',
      text: 'Working with John was a pleasure. He not only built exactly what we needed but also provided valuable insights that improved our overall product strategy.',
      author: 'Michael Chen',
      position: 'Product Manager, Innovation Labs'
    },
    {
      id: 'quote-3',
      text: 'John\'s full-stack expertise and problem-solving skills made him an invaluable team member. He consistently delivered high-quality code on time.',
      author: 'Emily Rodriguez',
      position: 'Lead Developer, Digital Solutions'
    }
  ],
  experience: [
    {
      id: 'exp-1',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      jobTitle: 'Senior Full Stack Developer',
      companyName: 'TechCorp Solutions',
      startDate: '2022-01-15',
      endDate: null,
      isCurrent: true,
      description: 'Lead development of microservices architecture serving 1M+ users. Built and maintained React/Node.js applications with 99.9% uptime. Mentored junior developers and established coding standards for the team.',
      skillsUsed: ['React', 'Node.js', 'TypeScript', 'PostgreSQL', 'AWS', 'Docker', 'Kubernetes'],
      createdAt: '2024-01-15T10:30:00Z',
      updatedAt: '2024-01-20T15:45:00Z'
    },
    {
      id: 'exp-2',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      jobTitle: 'Full Stack Developer',
      companyName: 'StartupXYZ',
      startDate: '2020-03-01',
      endDate: '2021-12-31',
      isCurrent: false,
      description: 'Developed MVP for B2B SaaS platform from scratch. Implemented real-time features using WebSockets and built responsive frontend with React. Integrated third-party APIs and payment processing.',
      skillsUsed: ['React', 'Express.js', 'MongoDB', 'Socket.io', 'Stripe API', 'Material-UI'],
      createdAt: '2024-01-15T10:30:00Z',
      updatedAt: '2024-01-20T15:45:00Z'
    },
    {
      id: 'exp-3',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      jobTitle: 'Frontend Developer',
      companyName: 'Digital Agency Pro',
      startDate: '2019-06-01',
      endDate: '2020-02-28',
      isCurrent: false,
      description: 'Created responsive websites and web applications for various clients. Collaborated with designers to implement pixel-perfect UIs. Optimized performance and ensured cross-browser compatibility.',
      skillsUsed: ['JavaScript', 'Vue.js', 'Sass', 'Webpack', 'Adobe Creative Suite'],
      createdAt: '2024-01-15T10:30:00Z',
      updatedAt: '2024-01-20T15:45:00Z'
    }
  ],
  projects: [
    {
      id: 'proj-1',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      title: 'E-Commerce Platform',
      description: 'Full-featured e-commerce platform with real-time inventory management, payment processing, and admin dashboard. Built with React, Node.js, and MongoDB.',
      imageUrl: 'https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=600&h=400&fit=crop',
      demoUrl: 'https://ecommerce-demo.example.com',
      githubUrl: 'https://github.com/johndoe/ecommerce-platform',
      technologies: ['React', 'Node.js', 'MongoDB', 'Stripe', 'Redux', 'Express'],
      featured: true,
      createdAt: '2024-01-15T10:30:00Z',
      updatedAt: '2024-01-20T15:45:00Z'
    },
    {
      id: 'proj-2',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      title: 'Task Management App',
      description: 'Collaborative task management application with real-time updates, file sharing, and team communication features. Mobile-responsive design.',
      imageUrl: 'https://images.unsplash.com/photo-1611224923853-80b023f02d71?w=600&h=400&fit=crop',
      demoUrl: 'https://taskapp-demo.example.com',
      githubUrl: 'https://github.com/johndoe/task-manager',
      technologies: ['Vue.js', 'Firebase', 'Vuetify', 'Socket.io', 'PWA'],
      featured: true,
      createdAt: '2024-01-15T10:30:00Z',
      updatedAt: '2024-01-20T15:45:00Z'
    },
    {
      id: 'proj-3',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      title: 'Weather Dashboard',
      description: 'Interactive weather dashboard with geolocation, weather maps, and 7-day forecasts. Built using weather APIs with beautiful data visualizations.',
      imageUrl: 'https://images.unsplash.com/photo-1504608524841-42fe6f032b4b?w=600&h=400&fit=crop',
      demoUrl: 'https://weather-dashboard.example.com',
      githubUrl: 'https://github.com/johndoe/weather-dashboard',
      technologies: ['React', 'Chart.js', 'OpenWeather API', 'Tailwind CSS'],
      featured: false,
      createdAt: '2024-01-15T10:30:00Z',
      updatedAt: '2024-01-20T15:45:00Z'
    },
    {
      id: 'proj-4',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      title: 'Portfolio Website',
      description: 'Modern portfolio website with glassmorphism design, dark mode support, and smooth animations. Built with Next.js and Tailwind CSS.',
      imageUrl: 'https://images.unsplash.com/photo-1467232004584-a241de8bcf5d?w=600&h=400&fit=crop',
      demoUrl: 'https://johndoe-portfolio.example.com',
      githubUrl: 'https://github.com/johndoe/portfolio',
      technologies: ['Next.js', 'TypeScript', 'Tailwind CSS', 'Framer Motion'],
      featured: false,
      createdAt: '2024-01-15T10:30:00Z',
      updatedAt: '2024-01-20T15:45:00Z'
    }
  ],
  skills: [
    // Frontend Skills
    { id: 'skill-1', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'React', categoryType: 'hard_skills', subcategory: 'frontend', proficiencyLevel: 95, displayOrder: 1, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-2', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'TypeScript', categoryType: 'hard_skills', subcategory: 'frontend', proficiencyLevel: 90, displayOrder: 2, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-3', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Next.js', categoryType: 'hard_skills', subcategory: 'frontend', proficiencyLevel: 88, displayOrder: 3, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-4', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Vue.js', categoryType: 'hard_skills', subcategory: 'frontend', proficiencyLevel: 85, displayOrder: 4, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-5', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Tailwind CSS', categoryType: 'hard_skills', subcategory: 'frontend', proficiencyLevel: 92, displayOrder: 5, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    
    // Backend Skills
    { id: 'skill-6', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Node.js', categoryType: 'hard_skills', subcategory: 'backend', proficiencyLevel: 90, displayOrder: 6, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-7', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Express.js', categoryType: 'hard_skills', subcategory: 'backend', proficiencyLevel: 88, displayOrder: 7, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-8', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'PostgreSQL', categoryType: 'hard_skills', subcategory: 'backend', proficiencyLevel: 85, displayOrder: 8, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-9', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'MongoDB', categoryType: 'hard_skills', subcategory: 'backend', proficiencyLevel: 83, displayOrder: 9, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    
    // DevOps Skills
    { id: 'skill-10', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'AWS', categoryType: 'hard_skills', subcategory: 'devops', proficiencyLevel: 80, displayOrder: 10, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-11', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Docker', categoryType: 'hard_skills', subcategory: 'devops', proficiencyLevel: 82, displayOrder: 11, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-12', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Kubernetes', categoryType: 'hard_skills', subcategory: 'devops', proficiencyLevel: 75, displayOrder: 12, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    
    // Soft Skills
    { id: 'skill-13', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Team Leadership', categoryType: 'soft_skills', subcategory: 'communication', proficiencyLevel: 90, displayOrder: 13, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-14', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Problem Solving', categoryType: 'soft_skills', subcategory: 'analytical', proficiencyLevel: 95, displayOrder: 14, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' },
    { id: 'skill-15', portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259', name: 'Project Management', categoryType: 'soft_skills', subcategory: 'organizational', proficiencyLevel: 85, displayOrder: 15, createdAt: '2024-01-15T10:30:00Z', updatedAt: '2024-01-20T15:45:00Z' }
  ],
  socialLinks: [
    { id: 'social-1', platform: 'GitHub', url: 'https://github.com/johndoe', icon: 'github' },
    { id: 'social-2', platform: 'LinkedIn', url: 'https://linkedin.com/in/johndoe', icon: 'linkedin' },
    { id: 'social-3', platform: 'Twitter', url: 'https://twitter.com/johndoe', icon: 'twitter' },
    { id: 'social-4', platform: 'Website', url: 'https://johndoe.dev', icon: 'globe' }
  ],
  blogPosts: [
    {
      id: 'blog-1',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      title: 'Building Scalable React Applications with TypeScript',
      excerpt: 'Learn how to structure large React applications using TypeScript, implementing best practices for maintainability and type safety.',
      content: 'Full article content would go here...',
      featuredImageUrl: 'https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=600&h=400&fit=crop',
      tags: ['React', 'TypeScript', 'Architecture', 'Best Practices'],
      isPublished: true,
      publishedAt: '2024-01-10T09:00:00Z',
      createdAt: '2024-01-08T10:30:00Z',
      updatedAt: '2024-01-10T09:00:00Z'
    },
    {
      id: 'blog-2',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      title: 'Microservices Architecture with Node.js',
      excerpt: 'A comprehensive guide to building and deploying microservices using Node.js, Docker, and Kubernetes.',
      content: 'Full article content would go here...',
      featuredImageUrl: 'https://images.unsplash.com/photo-1667372393119-3d4c48d07fc9?w=600&h=400&fit=crop',
      tags: ['Node.js', 'Microservices', 'Docker', 'Kubernetes', 'DevOps'],
      isPublished: true,
      publishedAt: '2024-01-05T14:30:00Z',
      createdAt: '2024-01-03T10:30:00Z',
      updatedAt: '2024-01-05T14:30:00Z'
    },
    {
      id: 'blog-3',
      portfolioId: '00eb7f6a-2e48-4ee6-ab16-c8fabc41b259',
      title: 'Modern CSS Techniques: Glassmorphism and Beyond',
      excerpt: 'Explore the latest CSS trends including glassmorphism effects, custom properties, and advanced layout techniques.',
      content: 'Full article content would go here...',
      featuredImageUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=600&h=400&fit=crop',
      tags: ['CSS', 'Design', 'Glassmorphism', 'Frontend'],
      isPublished: true,
      publishedAt: '2023-12-28T11:15:00Z',
      createdAt: '2023-12-26T10:30:00Z',
      updatedAt: '2023-12-28T11:15:00Z'
    }
  ]
};

// Additional mock data for different scenarios
export const MOCK_PORTFOLIO_MINIMAL: PortfolioDataFromDB = {
  portfolio: {
    id: 'minimal-portfolio-id',
    userId: 'user-minimal',
    templateId: 'modern',
    title: 'Jane Smith - Designer',
    bio: 'Creative designer focused on user experience.',
    isPublished: true,
    visibility: 0,
    viewCount: 250,
    likeCount: 15,
    components: [],
    createdAt: '2024-01-15T10:30:00Z',
    updatedAt: '2024-01-20T15:45:00Z'
  },
  profile: {
    id: 'user-minimal',
    name: 'Jane Smith',
    title: 'UX/UI Designer',
    bio: 'Creative designer focused on user experience.',
    profileImage: 'https://images.unsplash.com/photo-1494790108755-2616b612b29c?w=400&h=400&fit=crop&crop=face',
    location: 'New York, NY',
    email: 'jane.smith@example.com'
  },
  stats: [
    { id: 'stat-1', label: 'Projects', value: '12', icon: 'briefcase' }
  ],
  contacts: {
    email: 'jane.smith@example.com',
    location: 'New York, NY'
  },
  quotes: [],
  experience: [],
  projects: [],
  skills: [],
  socialLinks: [],
  blogPosts: []
};

export function getMockPortfolioData(portfolioId?: string): PortfolioDataFromDB {
  // Return different mock data based on the portfolio ID
  switch (portfolioId) {
    case 'minimal':
      return MOCK_PORTFOLIO_MINIMAL;
    default:
      return MOCK_PORTFOLIO_DATA;
  }
} 