#!/usr/bin/env python3
"""
Portfolio Test Data Generation Script
This script creates a comprehensive portfolio for testing with 100 items in each category
"""

import json
import requests
import random
import sys
from datetime import datetime, timedelta
from typing import List, Dict, Any

# Accept USER_ID as command line argument
if len(sys.argv) != 2:
    print("Error: User ID is required as argument")
    print("Usage: python3 generate-portfolio-test-data.py <USER_ID>")
    sys.exit(1)

USER_ID = sys.argv[1]
PORTFOLIO_API_BASE = "http://localhost:5201/api/Portfolio"
PORTFOLIO_TEMPLATE_API_BASE = "http://localhost:5201/api/PortfolioTemplate"

def generate_projects(portfolio_id: str, count: int = 100) -> List[Dict[str, Any]]:
    """Generate project data"""
    
    tech_sets = [
        ["React", "TypeScript", "Node.js", "MongoDB"],
        ["Vue.js", "JavaScript", "Express", "PostgreSQL"],
        ["Angular", "TypeScript", "Spring Boot", "MySQL"],
        ["Python", "Django", "Redis", "Docker"],
        ["Java", "Spring", "Hibernate", "Oracle"],
        ["C#", ".NET Core", "Entity Framework", "SQL Server"],
        ["PHP", "Laravel", "Composer", "MariaDB"],
        ["Ruby", "Rails", "Sidekiq", "PostgreSQL"],
        ["Go", "Gin", "GORM", "SQLite"],
        ["Rust", "Actix", "Diesel", "PostgreSQL"]
    ]
    
    project_types = [
        "E-commerce Platform", "Social Media App", "Project Management Tool",
        "Analytics Dashboard", "Learning Management System", "API Gateway",
        "Microservice", "Mobile App", "Web Application", "Desktop Application"
    ]
    
    projects = []
    for i in range(1, count + 1):
        tech_set = tech_sets[i % len(tech_sets)]
        project_type = project_types[i % len(project_types)]
        
        projects.append({
            "portfolioId": portfolio_id,
            "title": f"{project_type} #{i}",
            "description": f"A comprehensive {project_type} built with modern technologies. Features include user authentication, real-time updates, responsive design, and scalable architecture. This project demonstrates advanced programming concepts and best practices in software development.",
            "imageUrl": f"https://picsum.photos/400/300?random={i}",
            "demoUrl": f"https://demo-project-{i}.example.com",
            "githubUrl": f"https://github.com/testuser/project-{i}",
            "technologies": tech_set,
            "featured": i % 10 == 1  # Every 10th project is featured
        })
    
    return projects

def generate_experiences(portfolio_id: str, count: int = 100) -> List[Dict[str, Any]]:
    """Generate experience data"""
    
    companies = [
        "TechCorp International", "InnovateSoft Solutions", "DataDriven Analytics",
        "CloudFirst Technologies", "AgileDelivery Inc", "ScalableApps Ltd",
        "DigitalTransform Co", "SecureCode Systems", "OpenSource Collective",
        "FutureTech Ventures"
    ]
    
    job_titles = [
        "Senior Software Engineer", "Full Stack Developer", "Backend Developer",
        "Frontend Developer", "DevOps Engineer", "Software Architect",
        "Technical Lead", "Engineering Manager", "Principal Engineer", "Staff Engineer"
    ]
    
    experiences = []
    for i in range(1, count + 1):
        company = companies[i % len(companies)]
        job_title = job_titles[i % len(job_titles)]
        
        # Generate dates - spread over last 25 years
        years_ago = i // 4
        start_year = 2024 - years_ago - 1
        end_year = 2024 - years_ago
        
        experiences.append({
            "portfolioId": portfolio_id,
            "jobTitle": job_title,
            "companyName": company,
            "startDate": f"{start_year}-01-01",
            "endDate": f"{end_year}-12-31",
            "isCurrent": i == 1,  # Only first experience is current
            "description": "Led development of critical business applications serving millions of users. Collaborated with cross-functional teams to deliver high-quality software solutions. Mentored junior developers and contributed to architectural decisions. Implemented best practices for code quality, testing, and deployment.",
            "skillsUsed": ["Leadership", "Code Review", "Architecture Design", "Team Collaboration", "Agile Methodologies"]
        })
    
    return experiences

def generate_skills(portfolio_id: str, count: int = 100) -> List[Dict[str, Any]]:
    """Generate skills data"""
    
    skill_data = {
        "hard_skills": {
            "frontend": ["React", "Vue.js", "Angular", "HTML5", "CSS3", "JavaScript", "TypeScript", "Sass", "Bootstrap", "Tailwind CSS"],
            "backend": ["Node.js", "Python", "Java", "C#", "PHP", "Ruby", "Go", "Rust", "Express.js", "Django"],
            "database": ["PostgreSQL", "MySQL", "MongoDB", "Redis", "Elasticsearch", "SQLite", "Oracle", "DynamoDB", "Cassandra", "Neo4j"],
            "devops": ["Docker", "Kubernetes", "Jenkins", "GitLab CI", "AWS", "Azure", "Terraform", "Ansible", "Prometheus", "Grafana"],
            "mobile": ["React Native", "Flutter", "Swift", "Kotlin", "Ionic", "Xamarin", "Cordova", "Native Android", "Native iOS", "Unity"],
            "testing": ["Jest", "Cypress", "Selenium", "JUnit", "pytest", "Mocha", "Chai", "TestNG", "Cucumber", "Postman"],
            "cloud": ["AWS Lambda", "Azure Functions", "Google Cloud", "CloudFormation", "ARM Templates", "Cloud Storage", "CDN", "Load Balancing", "Auto Scaling", "Serverless"]
        },
        "soft_skills": {
            "communication": ["Public Speaking", "Technical Writing", "Documentation", "Presentation Skills", "Cross-functional Collaboration", "Client Communication", "Stakeholder Management", "Conflict Resolution", "Active Listening", "Mentoring"],
            "leadership": ["Team Leadership", "Project Management", "Strategic Planning", "Decision Making", "Delegation", "Performance Management", "Change Management", "Innovation", "Vision Setting", "Coaching"],
            "problem_solving": ["Analytical Thinking", "Critical Thinking", "Creative Problem Solving", "Root Cause Analysis", "Troubleshooting", "Algorithm Design", "System Design", "Performance Optimization", "Debugging", "Research"]
        }
    }
    
    skills = []
    categories = []
    for category_type, subcategories in skill_data.items():
        for subcategory, skill_names in subcategories.items():
            categories.append((category_type, subcategory, skill_names))
    
    for i in range(1, count + 1):
        category_type, subcategory, skill_names = categories[i % len(categories)]
        skill_name = skill_names[i % len(skill_names)]
        
        skills.append({
            "portfolioId": portfolio_id,
            "name": f"{skill_name} #{i}",
            "categoryType": category_type,
            "subcategory": subcategory,
            "category": f"{category_type}/{subcategory}",
            "proficiencyLevel": (i % 5) + 1,  # 1-5 scale
            "displayOrder": i
        })
    
    return skills

def generate_blog_posts(portfolio_id: str, count: int = 100) -> List[Dict[str, Any]]:
    """Generate blog post data"""
    
    blog_topics = [
        "Advanced React Patterns and Performance Optimization",
        "Building Scalable Microservices with Node.js",
        "Database Design Best Practices for High-Traffic Applications",
        "Cloud-Native Development with Kubernetes",
        "Modern CSS Techniques and Layout Systems",
        "API Design and RESTful Architecture",
        "DevOps Automation and CI/CD Pipelines",
        "Machine Learning Integration in Web Applications",
        "Security Best Practices for Modern Web Apps",
        "Performance Monitoring and Optimization Strategies"
    ]
    
    tags_sets = [
        ["react", "javascript", "performance", "frontend"],
        ["nodejs", "microservices", "backend", "architecture"],
        ["database", "sql", "postgresql", "optimization"],
        ["kubernetes", "devops", "cloud", "containers"],
        ["css", "frontend", "design", "responsive"],
        ["api", "rest", "design", "backend"],
        ["devops", "cicd", "automation", "deployment"],
        ["ml", "ai", "integration", "python"],
        ["security", "authentication", "best-practices"],
        ["performance", "monitoring", "optimization", "analytics"]
    ]
    
    blog_posts = []
    for i in range(1, count + 1):
        topic = blog_topics[i % len(blog_topics)]
        tags = tags_sets[i % len(tags_sets)]
        
        content = f"""# {topic} - Part {i}

This comprehensive article explores advanced concepts and practical implementations in modern software development. We'll cover key principles, best practices, and real-world examples that demonstrate how to build robust, scalable applications.

## Introduction

In today's rapidly evolving technology landscape, staying current with best practices is crucial for delivering high-quality software solutions.

## Key Concepts

1. **Architecture Design**: Understanding system design principles
2. **Performance Optimization**: Techniques for improving application speed
3. **Scalability**: Building systems that grow with demand
4. **Security**: Implementing robust security measures
5. **Maintainability**: Writing clean, sustainable code

## Implementation Examples

```javascript
// Example code demonstrating best practices
const optimizedFunction = async (data) => {{
  try {{
    const result = await processData(data);
    return result;
  }} catch (error) {{
    console.error('Processing failed:', error);
    throw error;
  }}
}};
```

## Conclusion

Implementing these practices will significantly improve your application's performance, security, and maintainability. Continue learning and adapting these concepts to your specific use cases."""

        blog_posts.append({
            "portfolioId": portfolio_id,
            "title": f"{topic} - Part {i}",
            "excerpt": "Comprehensive guide covering advanced concepts, practical examples, and real-world applications. Learn industry best practices and cutting-edge techniques used by top technology companies.",
            "content": content,
            "featuredImageUrl": f"https://picsum.photos/800/400?random={i + 1000}",
            "tags": tags,
            "isPublished": i % 3 == 0  # Every 3rd post is published
        })
    
    return blog_posts

def get_available_templates() -> List[Dict[str, Any]]:
    """Get available portfolio templates from the API"""
    try:
        response = requests.get(f"{PORTFOLIO_TEMPLATE_API_BASE}/active")
        response.raise_for_status()
        templates = response.json()
        
        if not templates:
            print("Warning: No active templates found, using default")
            return [{"name": "Gabriel Bârzu"}]
        
        print(f"Found {len(templates)} available templates:")
        for template in templates:
            print(f"  - {template['name']}")
        
        return templates
    except Exception as e:
        print(f"Error fetching templates: {e}")
        print("Using default template: Gabriel Bârzu")
        return [{"name": "Gabriel Bârzu"}]

def select_random_template(templates: List[Dict[str, Any]]) -> str:
    """Select a random template from the available templates"""
    selected_template = random.choice(templates)
    template_name = selected_template['name']
    print(f"Selected template: {template_name}")
    return template_name

def create_portfolio() -> str:
    """Create a new portfolio and return its ID"""
    
    print("Step 1: Creating a new portfolio...")
    
    # Get available templates and select one randomly
    available_templates = get_available_templates()
    selected_template = select_random_template(available_templates)
    
    portfolio_data = {
        "userId": USER_ID,
        "templateName": selected_template,
        "title": "Comprehensive Test Portfolio - Full Stack Developer",
        "bio": "This is a comprehensive test portfolio designed to stress-test the template system with maximum data. Contains 100 projects, 100 experiences, 100 skills, and 100 blog posts to validate performance and layout scalability.",
        "visibility": 0,  # Public
        "isPublished": False,
        "components": json.dumps([
            {"id": "experience-1", "type": "experience", "order": 1, "isVisible": True, "settings": {}},
            {"id": "projects-1", "type": "projects", "order": 2, "isVisible": True, "settings": {}},
            {"id": "skills-1", "type": "skills", "order": 3, "isVisible": True, "settings": {}},
            {"id": "blog_posts-1", "type": "blog_posts", "order": 4, "isVisible": True, "settings": {}}
        ])
    }
    
    try:
        response = requests.post(PORTFOLIO_API_BASE, json=portfolio_data)
        response.raise_for_status()
        
        portfolio = response.json()
        portfolio_id = portfolio.get("id")
        
        if not portfolio_id:
            raise Exception(f"No portfolio ID in response: {portfolio}")
        
        print(f"Portfolio created successfully with ID: {portfolio_id}")
        return portfolio_id
        
    except requests.exceptions.RequestException as e:
        print(f"Failed to create portfolio: {e}")
        raise

def save_bulk_content(portfolio_id: str):
    """Save bulk content to the portfolio"""
    
    print("Step 2: Generating and saving bulk content (100 items each)...")
    
    # Generate all content
    print("  Generating 100 projects...")
    projects = generate_projects(portfolio_id, 100)
    
    print("  Generating 100 experiences...")
    experiences = generate_experiences(portfolio_id, 100)
    
    print("  Generating 100 skills...")
    skills = generate_skills(portfolio_id, 100)
    
    print("  Generating 100 blog posts...")
    blog_posts = generate_blog_posts(portfolio_id, 100)
    
    # Create bulk content request
    bulk_content = {
        "portfolioId": portfolio_id,
        "projects": projects,
        "experience": experiences,
        "skills": skills,
        "blogPosts": blog_posts,
        "publishPortfolio": True
    }
    
    print("Step 3: Saving bulk content to portfolio...")
    
    try:
        url = f"{PORTFOLIO_API_BASE}/{portfolio_id}/save-content"
        response = requests.post(url, json=bulk_content)
        response.raise_for_status()
        
        result = response.json()
        print("Bulk content saved successfully!")
        
        return result
        
    except requests.exceptions.RequestException as e:
        print(f"Failed to save bulk content: {e}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response: {e.response.text}")
        raise

def main():
    """Main function to orchestrate the portfolio creation"""
    
    print("Starting Portfolio Test Data Generation for User:", USER_ID)
    print("=" * 63)
    
    try:
        # Step 1: Create portfolio
        portfolio_id = create_portfolio()
        
        # Step 2: Generate and save content
        save_bulk_content(portfolio_id)
        
        # Summary
        print("\nPortfolio Summary:")
        print("   Projects: 100 (10 featured)")
        print("   Experience: 100 entries (spanning 25 years)")
        print("   Skills: 100 (across 10 categories)")
        print("   Blog Posts: 100 (33% published)")
        print()
        print(f"Portfolio URL: http://localhost:3000/portfolio/{portfolio_id}")
        print(f"Admin Panel: http://localhost:3000/admin/portfolio/{portfolio_id}")
        print()
        print("Portfolio test data generation completed successfully!")
        print(f"Portfolio ID: {portfolio_id}")
        print(f"User ID: {USER_ID}")
        print("=" * 63)
        
    except Exception as e:
        print(f"\nError during portfolio generation: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())
