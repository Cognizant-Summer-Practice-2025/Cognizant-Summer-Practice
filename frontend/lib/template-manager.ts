import { ComponentConfig, PortfolioDataFromDB } from './portfolio/interfaces';

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
    // Defensive check to ensure components is an array
    if (!Array.isArray(components)) {
      console.warn('getVisibleComponents received non-array value:', components);
      return [];
    }
    
    const visibleComponents = components
      .filter(component => component.isVisible)
      .sort((a, b) => a.order - b.order);

    return visibleComponents;
  }

  // Get component by type
  getComponent(type: string) {
    return this.componentMap[type];
  }

  // Render components dynamically
  renderComponents(portfolioData: PortfolioDataFromDB) {
    const portfolioComponents = portfolioData.portfolio.components || [];
    // Ensure we have an array before proceeding
    if (!Array.isArray(portfolioComponents)) {
      console.warn('renderComponents: portfolio.components is not an array:', portfolioComponents);
      return [];
    }
    
    const visibleComponents = this.getVisibleComponents(portfolioComponents);
    
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
        id: 'about-1',
        type: 'about',
        order: 1,
        isVisible: true,
        settings: {}
      },
      {
        id: 'experience-1',
        type: 'experience',
        order: 2,
        isVisible: true,
        settings: {}
      },
      {
        id: 'projects-1',
        type: 'projects',
        order: 3,
        isVisible: true,
        settings: {}
      },
      {
        id: 'skills-1',
        type: 'skills',
        order: 4,
        isVisible: true,
        settings: {}
      },
      {
        id: 'blog_posts-1',
        type: 'blog_posts',
        order: 5,
        isVisible: false, // Hidden by default
        settings: {}
      },
      {
        id: 'contact-1',
        type: 'contact',
        order: 6,
        isVisible: true,
        settings: {}
      }
    ];
  }

  // Enhanced data validation and transformation
  static validateAndTransformPortfolioData(data: PortfolioDataFromDB): PortfolioDataFromDB {
    // Ensure required arrays exist
    const validatedData: PortfolioDataFromDB = {
      ...data,
      stats: data.stats || [],
      quotes: data.quotes || [],
      experience: data.experience || [],
      projects: data.projects || [],
      skills: data.skills || [],
      socialLinks: data.socialLinks || [],
      blogPosts: data.blogPosts || []
    };

    // Ensure component configuration exists
    if (!validatedData.portfolio.components || validatedData.portfolio.components.length === 0) {
      validatedData.portfolio.components = TemplateManager.createDefaultComponentConfig();
    }

    // Generate default stats if none exist
    if (validatedData.stats.length === 0) {
      validatedData.stats = [
      {
        id: '1',
          label: 'Portfolio Views', 
          value: validatedData.portfolio.viewCount?.toString() || '0', 
          icon: '👁️' 
      },
      {
        id: '2', 
          label: 'Portfolio Likes', 
          value: validatedData.portfolio.likeCount?.toString() || '0', 
          icon: '❤️' 
      },
      {
        id: '3',
          label: 'Projects', 
          value: validatedData.projects.length.toString(), 
          icon: '🚀' 
        },
      {
          id: '4', 
          label: 'Skills', 
          value: validatedData.skills.length.toString(), 
          icon: '🎯' 
        }
      ];
    }

    // Set default profile data if missing
    if (!validatedData.profile.name || validatedData.profile.name.trim() === '') {
      validatedData.profile.name = 'Portfolio Owner';
    }
    if (!validatedData.profile.title || validatedData.profile.title.trim() === '') {
      validatedData.profile.title = 'Professional';
    }
    if (!validatedData.profile.profileImage || validatedData.profile.profileImage.trim() === '') {
      validatedData.profile.profileImage = 'https://placehold.co/120x120';
    }
    if (!validatedData.profile.email || validatedData.profile.email.trim() === '') {
      validatedData.profile.email = validatedData.contacts.email || 'contact@example.com';
    }

    // Set default contact info if missing
    if (!validatedData.contacts.email || validatedData.contacts.email.trim() === '') {
      validatedData.contacts.email = validatedData.profile.email || 'contact@example.com';
    }

    return validatedData;
  }

  // Helper method to get component settings
  static getComponentSettings(components: ComponentConfig[], componentType: string): Record<string, unknown> {
    const component = components.find(c => c.type === componentType);
    return component?.settings || {};
  }

  // Helper method to check if component is visible
  static isComponentVisible(components: ComponentConfig[], componentType: string): boolean {
    const component = components.find(c => c.type === componentType);
    return component?.isVisible ?? false;
  }
} 