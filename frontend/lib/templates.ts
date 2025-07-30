import { TemplateConfig } from './portfolio/interfaces';
import { templateRegistry } from './template-registry';

// Static fallback templates for when dynamic loading fails
const FALLBACK_TEMPLATES: TemplateConfig[] = [
  {
    id: 'gabriel-barzu',
    name: 'Gabriel Bârzu',
    description: 'Modern minimalist design with clean typography and structured layout',
    previewImage: '/templates/gabriel-barzu/preview.jpg'
  },
  {
    id: 'modern',
    name: 'Modern',
    description: 'Contemporary design with glassmorphism effects, dark mode support, and smooth animations',
    previewImage: '/templates/modern/preview.jpg'
  },
  {
    id: 'creative',
    name: 'Creative',
    description: 'Bold and artistic layout',
    previewImage: '/templates/creative/preview.jpg'
  },
  {
    id: 'professional',
    name: 'Professional',
    description: 'Corporate and structured',
    previewImage: '/templates/professional/preview.jpg'
  },
  {
    id: 'cyberpunk',
    name: 'Cyberpunk',
    description: 'Futuristic neon-themed design with dark backgrounds and matrix-style effects',
    previewImage: '/templates/cyberpunk/preview.jpg'
  },
  {
    id: 'terminal',
    name: 'Terminal',
    description: 'Command-line interface inspired design with monospace fonts and terminal aesthetics',
    previewImage: '/templates/terminal/preview.jpg'
  },
  {
    id: 'retro-gaming',
    name: 'Retro Gaming',
    description: '8-bit inspired pixel art design with retro gaming aesthetics and classic arcade vibes',
    previewImage: '/templates/retro-gaming/preview.jpg'
  }
];

// Dynamic template registry - gets populated from backend
export async function getPortfolioTemplates(): Promise<TemplateConfig[]> {
  await templateRegistry.initialize();
  const backendTemplates = templateRegistry.getTemplates();
  
  if (backendTemplates.length === 0) {
    return FALLBACK_TEMPLATES;
  }

  return backendTemplates.map(template => ({
    id: templateRegistry.convertUuidToId(template.id),
    name: template.name,
    description: template.description || '',
    previewImage: template.previewImageUrl || '/templates/default/preview.jpg'
  }));
}

// Backward compatibility - synchronous version that uses fallback
export const PORTFOLIO_TEMPLATES: TemplateConfig[] = FALLBACK_TEMPLATES;

export function getTemplateById(id: string): TemplateConfig | undefined {
  return PORTFOLIO_TEMPLATES.find(template => template.id === id);
}

export function getDefaultTemplate(): TemplateConfig {
  return PORTFOLIO_TEMPLATES[0]; // Gabriel Bârzu template as default
}

// Convert database template UUID to frontend template string ID (now uses dynamic registry)
export async function convertTemplateUuidToId(templateIdOrUuid: string): Promise<string> {
  // If it's already a valid template string ID, return it as-is
  if (getTemplateById(templateIdOrUuid)) {
    return templateIdOrUuid;
  }
  
  // Ensure template registry is initialized
  await templateRegistry.initialize();
  
  // Try to use dynamic registry
  const mappedId = templateRegistry.convertUuidToId(templateIdOrUuid);
  if (mappedId && getTemplateById(mappedId)) {
    console.log(`🎨 Converted template UUID ${templateIdOrUuid} to frontend ID: ${mappedId}`);
    return mappedId;
  }
  
  // If no mapping found, log warning and fall back to default
  console.warn(`❌ No template mapping found for "${templateIdOrUuid}", falling back to default template`);
  return getDefaultTemplate().id;
}

// Synchronous version for backward compatibility (uses fallback mapping)
export function convertTemplateUuidToIdSync(templateIdOrUuid: string): string {
  // If it's already a valid template string ID, return it as-is
  if (getTemplateById(templateIdOrUuid)) {
    return templateIdOrUuid;
  }
  
  // Try to use dynamic registry (if already initialized)
  const mappedId = templateRegistry.convertUuidToId(templateIdOrUuid);
  if (mappedId && getTemplateById(mappedId)) {
    console.log(`🎨 Converted template UUID ${templateIdOrUuid} to frontend ID: ${mappedId}`);
    return mappedId;
  }
  
  // If no mapping found, log warning and fall back to default
  console.warn(`❌ No template mapping found for "${templateIdOrUuid}", falling back to default template`);
  return getDefaultTemplate().id;
}

// Template component lazy loading
export async function loadTemplateComponent(templateId: string) {
  const template = getTemplateById(templateId);
  if (!template) {
    throw new Error(`Template with id "${templateId}" not found`);
  }

  try {
    switch (templateId) {
      case 'gabriel-barzu':
        return await import('@/components/portfolio-templates/gabriel-barzu');
      case 'modern':
        return await import('@/components/portfolio-templates/modern');
      case 'creative':
        return await import('@/components/portfolio-templates/creative');
      case 'professional':
        return await import('@/components/portfolio-templates/professional');
      default:
        throw new Error(`Component for template "${templateId}" not implemented`);
    }
  } catch (error) {
    console.error(`Failed to load template component for "${templateId}":`, error);
    // Fallback to Gabriel Bârzu template
    return await import('@/components/portfolio-templates/gabriel-barzu');
  }
} 