import { NextRequest, NextResponse } from 'next/server';
import fs from 'fs';
import path from 'path';

export async function GET(request: NextRequest) {
  try {
    const searchParams = request.nextUrl.searchParams;
    const templateName = searchParams.get('template');

    if (!templateName) {
      return NextResponse.json({ error: 'Template name is required' }, { status: 400 });
    }

    // Security check: only allow known templates
    const allowedTemplates = [
      'creative',
      'modern', 
      'professional',
      'cyberpunk',
      'terminal',
      'retro-gaming',
      'gabriel-barzu'
    ];

    if (!allowedTemplates.includes(templateName)) {
      return NextResponse.json({ error: 'Template not found' }, { status: 404 });
    }

    const templatePath = path.join(process.cwd(), 'components', 'portfolio-templates', templateName);
    
    if (!fs.existsSync(templatePath)) {
      return NextResponse.json({ error: 'Template directory not found' }, { status: 404 });
    }

    const templateData = await extractTemplateFiles(templatePath);
    
    return NextResponse.json({
      templateName,
      ...templateData,
      extractedAt: new Date().toISOString()
    });

  } catch (error) {
    console.error('Error extracting template:', error);
    return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
  }
}

async function extractTemplateFiles(templatePath: string) {
  const components: Record<string, string> = {};
  const styles: Record<string, string> = {};
  const dependencies: string[] = [
    'react',
    'react-dom', 
    'next',
    'typescript',
    '@types/node',
    '@types/react',
    '@types/react-dom',
    'tailwindcss',
    'autoprefixer',
    'postcss',
    'lucide-react',
    'framer-motion'
  ];

  try {
    // Read main template file
    const mainFilePath = path.join(templatePath, 'index.tsx');
    if (fs.existsSync(mainFilePath)) {
      components['index'] = fs.readFileSync(mainFilePath, 'utf8');
    }

    // Read component files
    const componentsPath = path.join(templatePath, 'components');
    if (fs.existsSync(componentsPath)) {
      const componentFiles = fs.readdirSync(componentsPath);
      
      for (const file of componentFiles) {
        if (file.endsWith('.tsx')) {
          const componentName = path.basename(file, '.tsx');
          const componentPath = path.join(componentsPath, file);
          components[componentName] = fs.readFileSync(componentPath, 'utf8');
        }
      }
    }

    // Read style files
    const stylesPath = path.join(templatePath, 'styles');
    if (fs.existsSync(stylesPath)) {
      const styleFiles = fs.readdirSync(stylesPath);
      
      for (const file of styleFiles) {
        if (file.endsWith('.css')) {
          const stylePath = path.join(stylesPath, file);
          styles[file] = fs.readFileSync(stylePath, 'utf8');
        }
      }
    }

  } catch (error) {
    console.error('Error reading template files:', error);
    throw error;
  }

  // Calculate total size from all files
  const totalSizeBytes = Object.values(components).reduce((size, content) => size + Buffer.byteLength(content, 'utf8'), 0) +
                        Object.values(styles).reduce((size, content) => size + Buffer.byteLength(content, 'utf8'), 0);

  return {
    components,
    styles,
    dependencies,
    totalSizeBytes
  };
}
