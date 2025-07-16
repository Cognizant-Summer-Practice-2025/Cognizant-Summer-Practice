import { ComponentConfig, PortfolioDataFromDB } from './interfaces';

export interface ComponentMap {
  [key: string]: React.ComponentType<{ data: unknown }>;
}

export class TemplateManager {
  private componentMap: ComponentMap;

  constructor(componentMap: ComponentMap) {
    this.componentMap = componentMap;
  }

  // Get visible components sorted by order
  getVisibleComponents(components: ComponentConfig[]): ComponentConfig[] {
    return components
      .filter(component => component.isVisible)
      .sort((a, b) => a.order - b.order);
  }

  // Get component by type
  getComponent(type: string) {
    return this.componentMap[type];
  }

  // Render components dynamically
  renderComponents(portfolioData: PortfolioDataFromDB) {
    const visibleComponents = this.getVisibleComponents(portfolioData.portfolio.components);
    
    return visibleComponents.map((componentConfig) => {
      const Component = this.getComponent(componentConfig.type);
      
      if (!Component) {
        console.warn(`Component type "${componentConfig.type}" not found in component map`);
        return null;
      }

      // Get the appropriate data for each component type
      const componentData = this.getComponentData(componentConfig.type, portfolioData);
      
      return {
        id: componentConfig.id,
        type: componentConfig.type,
        order: componentConfig.order,
        component: Component,
        data: componentData,
        settings: componentConfig.settings || {}
      };
    }).filter(Boolean);
  }

  // Get data for specific component type
  private getComponentData(type: string, portfolioData: PortfolioDataFromDB) {
    switch (type) {
      case 'experience':
        return portfolioData.experience;
      case 'projects':
        return portfolioData.projects;
      case 'skills':
        return portfolioData.skills;
      case 'blog_posts':
        return portfolioData.blogPosts;
      case 'contact':
        return portfolioData.contacts;
      case 'about':
        return portfolioData.quotes;
      default:
        return null;
    }
  }

  // Create default component configuration
  static createDefaultComponentConfig(): ComponentConfig[] {
    return [
      {
        id: 'experience-1',
        type: 'experience',
        order: 1,
        isVisible: true,
        settings: {}
      },
      {
        id: 'projects-1',
        type: 'projects',
        order: 2,
        isVisible: true,
        settings: {}
      },
      {
        id: 'skills-1',
        type: 'skills',
        order: 3,
        isVisible: true,
        settings: {}
      },
      {
        id: 'blog_posts-1',
        type: 'blog_posts',
        order: 4,
        isVisible: false, // Hidden by default
        settings: {}
      }
    ];
  }
}

// Mock data generator for development
export function createMockPortfolioData(userId: string): PortfolioDataFromDB {
  return {
    portfolio: {
      id: `portfolio-${userId}`,
      userId: userId,
      templateId: 'gabriel-barzu',
      title: 'My Portfolio',
      bio: 'Welcome to my professional portfolio',
      isPublished: true,
      visibility: 'public',
      viewCount: 1250,
      likeCount: 89,
      customConfig: {},
      components: TemplateManager.createDefaultComponentConfig(),
      createdAt: '2024-01-15T10:00:00Z',
      updatedAt: '2024-01-20T15:30:00Z'
    },
    profile: {
      id: userId,
      name: "Gabriel Cătălin Bârzu",
      title: "Software Developer",
      bio: "Passionate software developer with expertise in modern web technologies. I enjoy creating scalable solutions and building exceptional user experiences.",
      profileImage: "https://placehold.co/120x120",
      location: "Bucharest, Romania",
      email: "gabriel@example.com"
    },
    stats: [
      { id: '1', label: 'Projects Delivered', value: '50+', icon: '🚀' },
      { id: '2', label: 'Lines of Code', value: '100K+', icon: '💻' },
      { id: '3', label: 'Coffees Consumed', value: '2.5K+', icon: '☕' },
      { id: '4', label: 'Years Programming', value: '5+', icon: '🎯' }
    ],
    contacts: {
      email: "gabriel@example.com",
      location: "Bucharest, Romania"
    },
    quotes: [
      {
        id: '1',
        text: "Code is poetry written in logic.",
        author: "Gabriel Bârzu"
      },
      {
        id: '2', 
        text: "Great software is built by great teams.",
        author: "Anonymous"
      }
    ],
    experience: [
      {
        id: '1',
        portfolioId: 'mock-portfolio-id',
        jobTitle: "Senior Software Developer",
        companyName: "Tech Corp",
        startDate: "2022-01-01",
        endDate: undefined,
        isCurrent: true,
        description: "Leading development of enterprise applications using React, Node.js, and cloud technologies.",
        skillsUsed: ["React", "Node.js", "TypeScript", "AWS"],
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-15T10:00:00Z'
      },
      {
        id: '2',
        portfolioId: 'mock-portfolio-id',
        jobTitle: "Full Stack Developer", 
        companyName: "StartupXYZ",
        startDate: "2020-01-01",
        endDate: "2022-01-01",
        isCurrent: false,
        description: "Built and maintained web applications from concept to deployment.",
        skillsUsed: ["Vue.js", "Python", "PostgreSQL"],
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-15T10:00:00Z'
      },
      {
        id: '3',
        portfolioId: 'mock-portfolio-id',
        jobTitle: "Junior Developer", 
        companyName: "DevStudio",
        startDate: "2019-01-01",
        endDate: "2020-01-01",
        isCurrent: false,
        description: "Developed responsive web applications and learned modern development practices.",
        skillsUsed: ["JavaScript", "React", "Express"],
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-15T10:00:00Z'
      }
    ],
    projects: [
      {
        id: '1',
        portfolioId: 'mock-portfolio-id',
        title: "E-Commerce Platform",
        description: "Modern e-commerce solution with advanced features",
        imageUrl: "https://placehold.co/400x250",
        technologies: ["React", "Node.js", "MongoDB"],
        demoUrl: "#",
        githubUrl: "#",
        featured: true,
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-15T10:00:00Z'
      },
      {
        id: '2',
        portfolioId: 'mock-portfolio-id',
        title: "Task Management App",
        description: "Collaborative task management with real-time updates", 
        imageUrl: "https://placehold.co/400x250",
        technologies: ["Vue.js", "Socket.io", "PostgreSQL"],
        demoUrl: "#",
        githubUrl: "#",
        featured: true,
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-15T10:00:00Z'
      },
      {
        id: '3',
        portfolioId: 'mock-portfolio-id',
        title: "Portfolio Website",
        description: "Personal portfolio built with Next.js and modern design", 
        imageUrl: "https://placehold.co/400x250",
        technologies: ["Next.js", "TypeScript", "Tailwind CSS"],
        demoUrl: "#",
        githubUrl: "#",
        featured: false,
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-15T10:00:00Z'
      }
    ],
    skills: [
      { id: '1', portfolioId: 'mock-portfolio-id', name: 'JavaScript', category: 'Frontend', proficiencyLevel: 95, displayOrder: 1, createdAt: '2024-01-15T10:00:00Z', updatedAt: '2024-01-15T10:00:00Z' },
      { id: '2', portfolioId: 'mock-portfolio-id', name: 'React', category: 'Frontend', proficiencyLevel: 90, displayOrder: 2, createdAt: '2024-01-15T10:00:00Z', updatedAt: '2024-01-15T10:00:00Z' },
      { id: '3', portfolioId: 'mock-portfolio-id', name: 'Node.js', category: 'Backend', proficiencyLevel: 85, displayOrder: 3, createdAt: '2024-01-15T10:00:00Z', updatedAt: '2024-01-15T10:00:00Z' },
      { id: '4', portfolioId: 'mock-portfolio-id', name: 'TypeScript', category: 'Frontend', proficiencyLevel: 88, displayOrder: 4, createdAt: '2024-01-15T10:00:00Z', updatedAt: '2024-01-15T10:00:00Z' },
      { id: '5', portfolioId: 'mock-portfolio-id', name: 'Python', category: 'Backend', proficiencyLevel: 80, displayOrder: 5, createdAt: '2024-01-15T10:00:00Z', updatedAt: '2024-01-15T10:00:00Z' },
      { id: '6', portfolioId: 'mock-portfolio-id', name: 'PostgreSQL', category: 'Database', proficiencyLevel: 75, displayOrder: 6, createdAt: '2024-01-15T10:00:00Z', updatedAt: '2024-01-15T10:00:00Z' }
    ],
    socialLinks: [
      { id: '1', platform: 'GitHub', url: 'https://github.com', icon: 'github' },
      { id: '2', platform: 'LinkedIn', url: 'https://linkedin.com', icon: 'linkedin' },
      { id: '3', platform: 'Twitter', url: 'https://twitter.com', icon: 'twitter' }
    ],
    blogPosts: [
      {
        id: '1',
        portfolioId: 'mock-portfolio-id',
        title: 'Building Modern Web Applications',
        excerpt: 'Learn how to build scalable web applications using modern technologies and best practices.',
        content: 'Full content of the blog post about building modern web applications...',
        featuredImageUrl: 'https://placehold.co/400x250',
        tags: ['React', 'TypeScript', 'Web Development'],
        isPublished: true,
        publishedAt: '2024-01-15T10:00:00Z',
        createdAt: '2024-01-15T10:00:00Z',
        updatedAt: '2024-01-15T10:00:00Z'
      },
      {
        id: '2',
        portfolioId: 'mock-portfolio-id',
        title: 'The Future of Software Development',
        excerpt: 'Exploring emerging trends and technologies that will shape the future of software development.',
        content: 'Full content of the blog post about the future of software development...',
        featuredImageUrl: 'https://placehold.co/400x250',
        tags: ['Technology', 'Future', 'AI'],
        isPublished: true,
        publishedAt: '2024-01-10T14:30:00Z',
        createdAt: '2024-01-10T14:30:00Z',
        updatedAt: '2024-01-10T14:30:00Z'
      }
    ]
  };
} 