using backend_portfolio.Services.Abstractions;
using backend_portfolio.DTO.Deployment.Request;
using backend_portfolio.DTO.Deployment.Response;
using backend_portfolio.Config;
using System.Text.Json;
using System.Text;

namespace backend_portfolio.Services
{
    /// <summary>
    /// Service for extracting portfolio template code securely
    /// </summary>
    public class TemplateExtractionService : ITemplateExtractionService
    {
        private readonly ILogger<TemplateExtractionService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _homePortfolioServiceUrl;

        // Available templates mapping
        private readonly Dictionary<string, string> _templateMappings = new()
        {
            { "creative", "creative" },
            { "modern", "modern" },
            { "professional", "professional" },
            { "cyberpunk", "cyberpunk" },
            { "terminal", "terminal" },
            { "retro-gaming", "retro-gaming" },
            { "gabriel-barzu", "gabriel-barzu" }
        };

        public TemplateExtractionService(
            ILogger<TemplateExtractionService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient(HttpClientConfiguration.HomePortfolioServiceClientName);
            _homePortfolioServiceUrl = _configuration["HomePortfolioService:BaseUrl"] ?? "http://localhost:3002";
        }

        public async Task<TemplateExtractionResponse> ExtractTemplateAsync(TemplateExtractionRequest request)
        {
            try
            {
                _logger.LogInformation("Extracting template: {TemplateName}", request.TemplateName);

                // Validate template exists
                if (!await ValidateTemplateAsync(request.TemplateName))
                {
                    throw new ArgumentException($"Template '{request.TemplateName}' not found or not accessible");
                }

                // Extract template from home-portfolio-service API
                var templateApiData = await FetchTemplateFromApiAsync(request.TemplateName);
                var components = templateApiData.Components;
                var styles = templateApiData.Styles;
                var dependencies = templateApiData.Dependencies;

                // Create package.json
                var packageJson = CreatePackageJson(dependencies);
                
                // Create Next.js config
                var nextConfig = CreateNextConfig();

                // Calculate total size
                long totalSize = Encoding.UTF8.GetByteCount(packageJson) + 
                                Encoding.UTF8.GetByteCount(nextConfig);
                
                foreach (var component in components.Values)
                {
                    totalSize += Encoding.UTF8.GetByteCount(component);
                }
                
                foreach (var style in styles.Values)
                {
                    totalSize += Encoding.UTF8.GetByteCount(style);
                }

                return new TemplateExtractionResponse
                {
                    TemplateName = request.TemplateName,
                    MainComponent = components.GetValueOrDefault("index", ""),
                    Components = components,
                    Styles = styles,
                    Dependencies = dependencies,
                    NextConfigJs = nextConfig,
                    PackageJson = packageJson,
                    TotalSizeBytes = totalSize,
                    ExtractedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting template {TemplateName}", request.TemplateName);
                throw;
            }
        }

        public Task<bool> ValidateTemplateAsync(string templateName)
        {
            try
            {
                return Task.FromResult(_templateMappings.ContainsKey(templateName.ToLowerInvariant()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating template {TemplateName}", templateName);
                return Task.FromResult(false);
            }
        }

        public Task<List<string>> GetAvailableTemplatesAsync()
        {
            try
            {
                return Task.FromResult(_templateMappings.Keys.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available templates");
                return Task.FromResult(new List<string>());
            }
        }

        public async Task<Dictionary<string, string>> CreateProjectStructureAsync(
            TemplateExtractionResponse templateData, 
            object portfolioData)
        {
            try
            {
                _logger.LogInformation("Creating project structure for template: {TemplateName}", templateData.TemplateName);

                var projectFiles = new Dictionary<string, string>();

                // Add package.json
                projectFiles["package.json"] = templateData.PackageJson;

                // Add next.config.js
                projectFiles["next.config.js"] = templateData.NextConfigJs;

                // Add app directory structure
                projectFiles["app/layout.tsx"] = CreateAppLayout();
                projectFiles["app/page.tsx"] = CreateMainPage(templateData, portfolioData);
                projectFiles["app/globals.css"] = CreateGlobalStyles();

                // Add components
                foreach (var component in templateData.Components)
                {
                    var filePath = $"components/{component.Key}.tsx";
                    projectFiles[filePath] = ProcessComponentWithData(component.Value, portfolioData);
                }

                // Add styles
                foreach (var style in templateData.Styles)
                {
                    var filePath = $"styles/{style.Key}";
                    projectFiles[filePath] = style.Value;
                }

                // Add lib directory
                projectFiles["lib/utils.ts"] = CreateUtilsFile();
                projectFiles["lib/data.ts"] = CreateDataFile(portfolioData);
                projectFiles["lib/portfolio.ts"] = CreatePortfolioInterfacesFile();
                projectFiles["lib/template-manager.ts"] = CreateTemplateManagerFile();

                // Add TypeScript config
                projectFiles["tsconfig.json"] = CreateTypeScriptConfig();

                // Add Tailwind config if needed
                projectFiles["tailwind.config.js"] = CreateTailwindConfig();
                projectFiles["postcss.config.js"] = CreatePostCSSConfig();

                // Add environment files
                projectFiles[".env.local"] = CreateEnvironmentFile();

                // Add README
                projectFiles["README.md"] = CreateReadme(templateData.TemplateName);

                return projectFiles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project structure");
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<TemplateApiResponse> FetchTemplateFromApiAsync(string templateName)
        {
            try
            {
                _logger.LogInformation("Fetching template {TemplateName} from home-portfolio-service API", templateName);

                var apiUrl = $"{_homePortfolioServiceUrl}/api/templates?template={templateName}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch template from API: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    throw new Exception($"Failed to fetch template from API: {response.StatusCode}");
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var templateData = JsonSerializer.Deserialize<TemplateApiResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (templateData == null)
                {
                    throw new Exception("Failed to deserialize template data from API");
                }

                _logger.LogInformation("Successfully fetched template {TemplateName} with {ComponentCount} components and {StyleCount} styles", 
                    templateName, templateData.Components.Count, templateData.Styles.Count);

                return templateData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching template {TemplateName} from API", templateName);
                throw;
            }
        }



        private string CreatePackageJson(List<string> dependencies)
        {
            // Use the same package.json structure as the frontend home-portfolio-service
            var packageJson = new
            {
                name = "portfolio-deployment",
                version = "1.0.0", 
                @private = true,
                scripts = new
                {
                    dev = "next dev",
                    build = "next build",
                    start = "next start",
                    lint = "next lint"
                },
                dependencies = new Dictionary<string, string>
                {
                    // Core Next.js and React dependencies (matching frontend versions)
                    ["react"] = "^19.0.0",
                    ["react-dom"] = "^19.0.0", 
                    ["next"] = "15.3.5",
                    
                    // UI and styling dependencies
                    ["lucide-react"] = "^0.525.0",
                    ["framer-motion"] = "^12.23.12",
                    ["clsx"] = "^2.1.1",
                    ["tailwind-merge"] = "^3.3.1",
                    ["class-variance-authority"] = "^0.7.1",
                    
                    // Radix UI components (commonly used in templates)
                    ["@radix-ui/react-slot"] = "^1.2.3"
                },
                devDependencies = new Dictionary<string, string>
                {
                    ["typescript"] = "^5",
                    ["@types/node"] = "^20",
                    ["@types/react"] = "^19", 
                    ["@types/react-dom"] = "^19",
                    ["tailwindcss"] = "^4",
                    ["autoprefixer"] = "^10.4.0",
                    ["postcss"] = "^8.4.0",
                    ["eslint"] = "^9",
                    ["eslint-config-next"] = "15.3.5"
                }
            };

            return JsonSerializer.Serialize(packageJson, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        private string CreateNextConfig()
        {
            return @"
/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  images: {
    domains: ['images.unsplash.com', 'api.dicebear.com'],
  },
  eslint: {
    ignoreDuringBuilds: true
  },
  typescript: {
    ignoreBuildErrors: true
  }
}

module.exports = nextConfig";
        }

        private string CreateAppLayout()
        {
            return @"
import './globals.css'
import { Inter } from 'next/font/google'

const inter = Inter({ subsets: ['latin'] })

export const metadata = {
  title: 'Portfolio',
  description: 'Personal portfolio website',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang=""en"">
      <body className={inter.className}>{children}</body>
    </html>
  )
}";
        }

        private string CreateMainPage(TemplateExtractionResponse templateData, object portfolioData)
        {
            // Transform template name to proper React component name
            var cleanTemplateName = templateData.TemplateName.Replace("-", "").Replace("_", "");
            var capitalizedTemplateName = char.ToUpper(cleanTemplateName[0]) + cleanTemplateName.Substring(1);
            
            return $@"'use client';

import React from 'react';
import {capitalizedTemplateName}Template from '../components/index';
import {{ portfolioData }} from '../lib/data';

export default function Home() {{
  return <{capitalizedTemplateName}Template data={{portfolioData}} />;
}}";
        }

        private string CreateGlobalStyles()
        {
            return @"
@tailwind base;
@tailwind components;
@tailwind utilities;

html {
  scroll-behavior: smooth;
}

body {
  margin: 0;
  padding: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
    sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}";
        }

        private string ProcessComponentWithData(string component, object portfolioData)
        {
            // Process component template with actual portfolio data
            // This would involve replacing placeholders with actual data
            return component;
        }

        private string CreateUtilsFile()
        {
            return @"
import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatDate(date: string | Date): string {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}";
        }

        private string CreateDataFile(object portfolioData)
        {
            // Convert portfolio data to TypeScript
            var jsonData = JsonSerializer.Serialize(portfolioData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return $@"
export const portfolioData = {jsonData};";
        }

        private string CreateTypeScriptConfig()
        {
            return @"
{
  ""compilerOptions"": {
    ""target"": ""es5"",
    ""lib"": [""dom"", ""dom.iterable"", ""es6""],
    ""allowJs"": true,
    ""skipLibCheck"": true,
    ""strict"": true,
    ""forceConsistentCasingInFileNames"": true,
    ""noEmit"": true,
    ""esModuleInterop"": true,
    ""module"": ""esnext"",
    ""moduleResolution"": ""bundler"",
    ""resolveJsonModule"": true,
    ""isolatedModules"": true,
    ""jsx"": ""preserve"",
    ""incremental"": true,
    ""plugins"": [
      {
        ""name"": ""next""
      }
    ],
    ""baseUrl"": ""."",
    ""paths"": {
      ""@/*"": [""./*""]
    }
  },
  ""include"": [""next-env.d.ts"", ""**/*.ts"", ""**/*.tsx"", "".next/types/**/*.ts""],
  ""exclude"": [""node_modules""]
}";
        }

        private string CreateTailwindConfig()
        {
            return @"
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      animation: {
        'fade-in': 'fadeIn 0.5s ease-in-out',
      },
    },
  },
  plugins: [],
}";
        }

        private string CreatePostCSSConfig()
        {
            return @"
module.exports = {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
}";
        }

        private string CreateEnvironmentFile()
        {
            return @"
# Portfolio Configuration
NEXT_PUBLIC_SITE_NAME=Portfolio
NEXT_PUBLIC_SITE_URL=https://your-portfolio.vercel.app";
        }

        private string CreateReadme(string templateName)
        {
            return $@"
# Portfolio - {templateName.ToUpper()} Template

This is a portfolio website built with Next.js and deployed via Goalkeeper platform.

## Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Technologies Used

- Next.js 14
- React 18
- TypeScript
- Tailwind CSS
- Framer Motion

## Deployment

This project is deployed on Vercel automatically from Goalkeeper platform.
";
        }

        #endregion

        #region Component Templates

        private string CreateHeaderComponent()
        {
            return @"
'use client';

import React from 'react';

interface HeaderProps {
  data: {
    name: string;
    title: string;
    avatar: string;
  };
}

export default function Header({ data }: HeaderProps) {
  return (
    <header className=""bg-white shadow-sm"">
      <div className=""container mx-auto px-4 py-6"">
        <div className=""flex items-center space-x-4"">
          <img
            src={data.avatar || 'https://api.dicebear.com/7.x/avataaars/svg?seed=' + data.name}
            alt={data.name}
            className=""w-12 h-12 rounded-full""
          />
          <div>
            <h1 className=""text-2xl font-bold text-gray-900"">{data.name}</h1>
            <p className=""text-gray-600"">{data.title}</p>
          </div>
        </div>
      </div>
    </header>
  );
}";
        }

        private string CreateAboutComponent()
        {
            return @"
'use client';

import React from 'react';

interface AboutProps {
  data: {
    name: string;
    bio: string;
    location: string;
  };
}

export default function About({ data }: AboutProps) {
  return (
    <section className=""py-12"">
      <div className=""max-w-3xl mx-auto"">
        <h2 className=""text-3xl font-bold text-gray-900 mb-6"">About Me</h2>
        <p className=""text-lg text-gray-700 mb-4"">{data.bio}</p>
        <p className=""text-gray-600"">📍 {data.location}</p>
      </div>
    </section>
  );
}";
        }

        private string CreateProjectsComponent()
        {
            return @"
'use client';

import React from 'react';

interface ProjectsProps {
  projects: Array<{
    title: string;
    description: string;
    technologies: string;
    link?: string;
  }>;
}

export default function Projects({ projects }: ProjectsProps) {
  return (
    <section className=""py-12"">
      <div className=""max-w-6xl mx-auto"">
        <h2 className=""text-3xl font-bold text-gray-900 mb-8"">Projects</h2>
        <div className=""grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"">
          {projects.map((project, index) => (
            <div key={index} className=""bg-white rounded-lg shadow-md p-6"">
              <h3 className=""text-xl font-semibold text-gray-900 mb-2"">{project.title}</h3>
              <p className=""text-gray-700 mb-4"">{project.description}</p>
              <p className=""text-sm text-gray-600 mb-4"">Tech: {project.technologies}</p>
              {project.link && (
                <a
                  href={project.link}
                  target=""_blank""
                  rel=""noopener noreferrer""
                  className=""text-blue-600 hover:text-blue-800 underline""
                >
                  View Project
                </a>
              )}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}";
        }

        private string CreateExperienceComponent()
        {
            return @"
'use client';

import React from 'react';

interface ExperienceProps {
  experience: Array<{
    position: string;
    company: string;
    description: string;
    startDate: string;
    endDate?: string;
  }>;
}

export default function Experience({ experience }: ExperienceProps) {
  return (
    <section className=""py-12"">
      <div className=""max-w-4xl mx-auto"">
        <h2 className=""text-3xl font-bold text-gray-900 mb-8"">Experience</h2>
        <div className=""space-y-8"">
          {experience.map((exp, index) => (
            <div key={index} className=""bg-white rounded-lg shadow-md p-6"">
              <h3 className=""text-xl font-semibold text-gray-900"">{exp.position}</h3>
              <p className=""text-blue-600 font-medium"">{exp.company}</p>
              <p className=""text-gray-600 text-sm mb-4"">
                {exp.startDate} - {exp.endDate || 'Present'}
              </p>
              <p className=""text-gray-700"">{exp.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}";
        }

        private string CreateSkillsComponent()
        {
            return @"
'use client';

import React from 'react';

interface SkillsProps {
  skills: Array<{
    name: string;
    level: string;
  }>;
}

export default function Skills({ skills }: SkillsProps) {
  return (
    <section className=""py-12"">
      <div className=""max-w-4xl mx-auto"">
        <h2 className=""text-3xl font-bold text-gray-900 mb-8"">Skills</h2>
        <div className=""grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4"">
          {skills.map((skill, index) => (
            <div key={index} className=""bg-white rounded-lg shadow-md p-4 text-center"">
              <h3 className=""font-semibold text-gray-900"">{skill.name}</h3>
              <p className=""text-sm text-gray-600"">{skill.level}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}";
        }

        private string CreateBlogPostsComponent()
        {
            return @"
'use client';

import React from 'react';

interface BlogPostsProps {
  blogPosts: Array<{
    title: string;
    summary: string;
    publishedAt: string;
    link?: string;
  }>;
}

export default function BlogPosts({ blogPosts }: BlogPostsProps) {
  if (!blogPosts || blogPosts.length === 0) {
    return null;
  }

  return (
    <section className=""py-12"">
      <div className=""max-w-4xl mx-auto"">
        <h2 className=""text-3xl font-bold text-gray-900 mb-8"">Blog Posts</h2>
        <div className=""space-y-6"">
          {blogPosts.map((post, index) => (
            <div key={index} className=""bg-white rounded-lg shadow-md p-6"">
              <h3 className=""text-xl font-semibold text-gray-900 mb-2"">{post.title}</h3>
              <p className=""text-gray-700 mb-4"">{post.summary}</p>
              <p className=""text-sm text-gray-600"">{post.publishedAt}</p>
              {post.link && (
                <a
                  href={post.link}
                  target=""_blank""
                  rel=""noopener noreferrer""
                  className=""text-blue-600 hover:text-blue-800 underline mt-2 inline-block""
                >
                  Read More
                </a>
              )}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}";
        }

        private string CreateContactComponent()
        {
            return @"
'use client';

import React from 'react';

interface ContactProps {
  data: {
    name: string;
    // Add more contact fields as needed
  };
}

export default function Contact({ data }: ContactProps) {
  return (
    <section className=""py-12"">
      <div className=""max-w-3xl mx-auto text-center"">
        <h2 className=""text-3xl font-bold text-gray-900 mb-8"">Get In Touch</h2>
        <p className=""text-lg text-gray-700 mb-8"">
          Feel free to reach out if you'd like to connect or discuss opportunities.
        </p>
        <button className=""bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-6 rounded-lg transition-colors"">
          Contact Me
        </button>
      </div>
    </section>
  );
}";
        }

        #endregion

        #region DTOs for Template API Response

        private class TemplateApiResponse
        {
            public string TemplateName { get; set; } = string.Empty;
            public Dictionary<string, string> Components { get; set; } = new();
            public Dictionary<string, string> Styles { get; set; } = new();
            public List<string> Dependencies { get; set; } = new();
            public long TotalSizeBytes { get; set; }
            public string ExtractedAt { get; set; } = string.Empty;
        }

        private string CreatePortfolioInterfacesFile()
        {
            return @"
export interface PortfolioDataFromDB {
  profile: {
    name: string;
    title: string;
    bio: string;
    location: string;
    email: string;
    avatar?: string;
  };
  stats: Array<{
    id: number;
    label: string;
    value: number;
    icon?: string;
  }>;
  projects: Array<{
    id: number;
    title: string;
    description: string;
    technologies?: string;
    github_url?: string;
    live_url?: string;
    image_url?: string;
    start_date?: string;
    end_date?: string;
    status?: string;
  }>;
  experience: Array<{
    id: number;
    jobTitle: string;
    companyName: string;
    description: string;
    startDate: string;
    endDate?: string;
    skillsUsed?: string[];
  }>;
  skills: Array<{
    id: number;
    name: string;
    category?: string;
    proficiencyLevel?: number;
  }>;
  blogPosts: Array<{
    id: number;
    title: string;
    content: string;
    summary?: string;
    published_date: string;
    tags?: string;
    read_time?: number;
    views?: number;
    featured?: boolean;
    external_url?: string;
  }>;
}

export interface StatData {
  id: number;
  label: string;
  value: number;
  icon?: string;
}

export interface Experience {
  id: number;
  jobTitle: string;
  companyName: string;
  description: string;
  startDate: string;
  endDate?: string;
  skillsUsed?: string[];
}

export interface Skill {
  id: number;
  name: string;
  category?: string;
  proficiencyLevel?: number;
}";
        }

        private string CreateTemplateManagerFile()
        {
            return @"
import React from 'react';

export interface ComponentInfo {
  component: React.ComponentType<any>;
  data: any;
}

export interface ComponentMap {
  [key: string]: ComponentInfo;
}

export class TemplateManager {
  private static components: ComponentMap = {};

  static registerComponent(name: string, component: React.ComponentType<any>, data: any) {
    this.components[name] = { component, data };
  }

  static getComponent(name: string): ComponentInfo | undefined {
    return this.components[name];
  }

  static getAllComponents(): ComponentMap {
    return this.components;
  }
}";
        }

        #endregion
    }
}
