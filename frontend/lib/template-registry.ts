import { PortfolioTemplate } from './portfolio/interfaces';
import { getActiveTemplates, seedDefaultTemplates } from './portfolio/api';

// Template registry to manage dynamic template loading
class TemplateRegistry {
  private templates: PortfolioTemplate[] = [];
  private uuidMapping: Record<string, string> = {};
  private nameToIdMapping: Record<string, string> = {};
  private initialized = false;

  async initialize(): Promise<void> {
    if (this.initialized) return;

    try {
      console.log('🎨 Initializing template registry...');
      
      // Try to fetch templates
      let templates = await getActiveTemplates();
      
      // If no templates found, seed the defaults
      if (templates.length === 0) {
        console.log('📦 No templates found, seeding defaults...');
        await seedDefaultTemplates();
        templates = await getActiveTemplates();
      }

      this.templates = templates;
      
      // Build mappings
      this.buildMappings();
      
      this.initialized = true;
      console.log('✅ Template registry initialized with', templates.length, 'templates');
      console.log('📋 Available templates:', templates.map(t => `${t.name} (${t.id})`));
      
    } catch (error) {
      console.error('❌ Failed to initialize template registry:', error);
      // Fall back to hardcoded defaults
      this.initializeFallback();
    }
  }

  private buildMappings(): void {
    this.uuidMapping = {};
    this.nameToIdMapping = {};

    // Frontend template name to ID mapping
    const frontendTemplateMap: Record<string, string> = {
      'Gabriel Bârzu': 'gabriel-barzu',
      'Modern': 'modern',
      'Creative': 'creative',
      'Professional': 'professional'
    };

    for (const template of this.templates) {
      const frontendId = frontendTemplateMap[template.name];
      if (frontendId) {
        this.uuidMapping[template.id] = frontendId;
        this.nameToIdMapping[template.name.toLowerCase()] = frontendId;
      }
    }

    console.log('🔗 Built UUID mapping:', this.uuidMapping);
    console.log('🏷️ Built name mapping:', this.nameToIdMapping);
  }

  private initializeFallback(): void {
    console.log('🚨 Using fallback template configuration');
    
    // Fallback UUID mapping for when backend is unavailable
    // These UUIDs are from the actual backend response
    this.uuidMapping = {
      'a145ac43-e240-4050-958a-804ce4ba43ec': 'gabriel-barzu',
      'f6bbed21-ef42-4efd-afe0-216abc205579': 'modern',
      '6e3a7576-512e-41f4-ac56-a15a07c880c7': 'creative', 
      'c10132f0-0383-46ea-978b-741c99fc499c': 'professional',
      'f015a1aa-514d-4645-bed4-2523ca5b22a9': 'gabriel-barzu', // legacy
    };

    this.nameToIdMapping = {
      'gabriel bârzu': 'gabriel-barzu',
      'modern': 'modern',
      'creative': 'creative',
      'professional': 'professional'
    };

    this.initialized = true;
  }

  getTemplates(): PortfolioTemplate[] {
    return this.templates;
  }

  getUuidMapping(): Record<string, string> {
    return this.uuidMapping;
  }

  convertUuidToId(uuid: string): string {
    return this.uuidMapping[uuid] || 'gabriel-barzu'; // fallback to default
  }

  convertNameToId(name: string): string {
    return this.nameToIdMapping[name.toLowerCase()] || 'gabriel-barzu'; // fallback to default
  }

  getTemplateByName(name: string): PortfolioTemplate | undefined {
    return this.templates.find(t => t.name.toLowerCase() === name.toLowerCase());
  }

  getTemplateById(uuid: string): PortfolioTemplate | undefined {
    return this.templates.find(t => t.id === uuid);
  }

  // Get template UUID by frontend template ID
  getTemplateUuidByFrontendId(frontendId: string): string | null {
    // Find the template with matching frontend ID
    for (const [uuid, fId] of Object.entries(this.uuidMapping)) {
      if (fId === frontendId) {
        return uuid;
      }
    }
    return null;
  }

  // Get template UUID by template name
  getTemplateUuidByName(name: string): string | null {
    const template = this.getTemplateByName(name);
    return template ? template.id : null;
  }

  async refresh(): Promise<void> {
    this.initialized = false;
    await this.initialize();
  }
}

// Global instance
export const templateRegistry = new TemplateRegistry();

// Initialize on module load
if (typeof window !== 'undefined') {
  templateRegistry.initialize().catch(console.error);
}

export default templateRegistry; 