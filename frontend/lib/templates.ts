import { TemplateConfig } from './portfolio/interfaces';

// Template Registry
export const PORTFOLIO_TEMPLATES: TemplateConfig[] = [
  {
    id: 'gabriel-barzu',
    name: 'Gabriel Bârzu',
    description: 'Modern minimalist design with clean typography and structured layout',
    previewImage: '/templates/gabriel-barzu/preview.jpg'
  },
  {
    id: 'modern',
    name: 'Modern',
    description: 'Clean and minimal design',
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
  }
];

// Mapping from database template UUIDs to frontend template string IDs
export const TEMPLATE_UUID_MAPPING: Record<string, string> = {
  'f015a1aa-514d-4645-bed4-2523ca5b22a9': 'gabriel-barzu', // Gabriel Bârzu template
  // Add other template UUID mappings here as needed
};

export function getTemplateById(id: string): TemplateConfig | undefined {
  return PORTFOLIO_TEMPLATES.find(template => template.id === id);
}

export function getDefaultTemplate(): TemplateConfig {
  return PORTFOLIO_TEMPLATES[0]; // Gabriel Bârzu template as default
}

// Convert database template UUID to frontend template string ID
export function convertTemplateUuidToId(templateIdOrUuid: string): string {
  // If it's already a valid template string ID, return it as-is
  if (getTemplateById(templateIdOrUuid)) {
    return templateIdOrUuid;
  }
  
  // Try to find UUID mapping
  const mappedId = TEMPLATE_UUID_MAPPING[templateIdOrUuid];
  if (mappedId) {
    return mappedId;
  }
  
  // If no mapping found, log warning and fall back to default
  console.warn(`No template mapping found for "${templateIdOrUuid}", falling back to default template`);
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