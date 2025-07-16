import { TemplateConfig } from './interfaces';

// Template Registry
export const PORTFOLIO_TEMPLATES: TemplateConfig[] = [
  {
    id: 'gabriel-barzu',
    name: 'Gabriel Bârzu',
    description: 'Modern minimalist design with clean typography and structured layout',
    previewImage: '/templates/gabriel-barzu/preview.jpg',
    componentName: 'GabrielBarzuTemplate'
  },
  {
    id: 'modern',
    name: 'Modern',
    description: 'Clean and minimal design',
    previewImage: '/templates/modern/preview.jpg',
    componentName: 'ModernTemplate'
  },
  {
    id: 'creative',
    name: 'Creative',
    description: 'Bold and artistic layout',
    previewImage: '/templates/creative/preview.jpg',
    componentName: 'CreativeTemplate'
  },
  {
    id: 'professional',
    name: 'Professional',
    description: 'Corporate and structured',
    previewImage: '/templates/professional/preview.jpg',
    componentName: 'ProfessionalTemplate'
  }
];

export function getTemplateById(id: string): TemplateConfig | undefined {
  return PORTFOLIO_TEMPLATES.find(template => template.id === id);
}

export function getDefaultTemplate(): TemplateConfig {
  return PORTFOLIO_TEMPLATES[0]; // Gabriel Bârzu template as default
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