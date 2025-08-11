#!/usr/bin/env python3
"""
Portfolio Test Data Generation Script
Creates a portfolio with randomized realistic data and counts.
Adds Bearer token support.
"""

import argparse
import json
import os
import random
import sys
from datetime import datetime, timedelta
from typing import List, Dict, Any

import requests

PORTFOLIO_API_BASE = "http://localhost:5201/api/Portfolio"
PORTFOLIO_TEMPLATE_API_BASE = "http://localhost:5201/api/PortfolioTemplate"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate randomized portfolio data")
    parser.add_argument("user_id", help="User ID for which to create the portfolio")
    parser.add_argument("--token", default=os.getenv("AUTH_TOKEN"), help="Bearer token for API authentication")
    parser.add_argument("--projects", type=int, default=0, help="Override number of projects (default: random)")
    parser.add_argument("--experiences", type=int, default=0, help="Override number of experiences (default: random)")
    parser.add_argument("--skills", type=int, default=0, help="Override number of skills (default: random)")
    parser.add_argument("--blogposts", type=int, default=0, help="Override number of blog posts (default: random)")
    parser.add_argument("--dataset", type=str, default="", help="Optional path to random data JSON to enrich generation")
    return parser.parse_args()


def build_headers(token: str | None) -> Dict[str, str]:
    headers: Dict[str, str] = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    return headers

def generate_projects(portfolio_id: str, count: int, dataset: Dict[str, Any]) -> List[Dict[str, Any]]:
    """Generate project data"""
    
    tech_sets = dataset.get("techStacks", [
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
    ])
    
    project_types = dataset.get("projectTypes", [
        "E-commerce Platform", "Social Media App", "Project Management Tool",
        "Analytics Dashboard", "Learning Management System", "API Gateway",
        "Microservice", "Mobile App", "Web Application", "Desktop Application"
    ])
    
    description_snippets = dataset.get("projectSnippets", [
        "Implemented OAuth2/OIDC and role-based access control.",
        "Optimized critical paths, reducing P95 latency by 45%.",
        "Introduced observability with OpenTelemetry and structured logging.",
        "Designed resilient event-driven architecture with retries and DLQs.",
        "Built CI/CD pipelines with automated tests and security scans.",
        "Migrated legacy components to cloud-native managed services.",
        "Implemented caching, pagination, and rate limiting.",
    ])

    projects: List[Dict[str, Any]] = []
    for i in range(1, count + 1):
        tech_set = random.choice(tech_sets)
        project_type = random.choice(project_types)
        featured = random.random() < 0.12
        snippet_sample = ", ".join(random.sample(description_snippets, k=random.randint(2, 4)))
        projects.append({
            "portfolioId": portfolio_id,
            "title": f"{project_type} #{i}",
            "description": f"{project_type} with modern stack. {snippet_sample}",
            "imageUrl": f"https://picsum.photos/400/300?random={random.randint(1, 100000)}",
            "demoUrl": f"https://demo-project-{random.randint(1, 99999)}.example.com",
            "githubUrl": f"https://github.com/example/project-{random.randint(1, 99999)}",
            "technologies": tech_set,
            "featured": featured,
        })

    return projects

def generate_experiences(portfolio_id: str, count: int, dataset: Dict[str, Any]) -> List[Dict[str, Any]]:
    """Generate experience data"""
    
    companies = dataset.get("companies", [
        "TechCorp International", "InnovateSoft Solutions", "DataDriven Analytics",
        "CloudFirst Technologies", "AgileDelivery Inc", "ScalableApps Ltd",
        "DigitalTransform Co", "SecureCode Systems", "OpenSource Collective",
        "FutureTech Ventures"
    ])
    
    job_titles = dataset.get("jobTitles", [
        "Senior Software Engineer", "Full Stack Developer", "Backend Developer",
        "Frontend Developer", "DevOps Engineer", "Software Architect",
        "Technical Lead", "Engineering Manager", "Principal Engineer", "Staff Engineer"
    ])
    
    responsibilities = dataset.get("responsibilities", [
        "Led cross-functional team to deliver key features on time.",
        "Architected microservices and event-driven workflows.",
        "Mentored junior engineers and established coding standards.",
        "Implemented CI/CD with automated testing and canary releases.",
        "Collaborated with product to refine requirements and roadmap.",
        "Improved reliability with SLOs, alerting, and runbooks.",
    ])

    experiences: List[Dict[str, Any]] = []
    current_assigned = False
    for _ in range(count):
        company = random.choice(companies)
        job_title = random.choice(job_titles)

        end_date_current = random.random() < 0.15 and not current_assigned
        if end_date_current:
            # Current role: started between 6 and 36 months ago
            months_ago = random.randint(6, 36)
            start = datetime.utcnow() - timedelta(days=30 * months_ago)
            start_date = start.date().isoformat()
            end_date = None
            current_assigned = True
        else:
            # Past role of 6-36 months, ended 1-84 months ago
            duration_months = random.randint(6, 36)
            end_offset_months = random.randint(1, 84)
            end = datetime.utcnow() - timedelta(days=30 * end_offset_months)
            start = end - timedelta(days=30 * duration_months)
            start_date = start.date().isoformat()
            end_date = end.date().isoformat()

        desc = " ".join(random.sample(responsibilities, k=random.randint(2, 4)))
        skills_used = random.sample([
            "Leadership", "Code Review", "Architecture Design", "Team Collaboration",
            "Agile", "Kubernetes", "AWS", "TypeScript", "C#", "PostgreSQL",
            "Redis", "Docker", "Terraform", "React", "GraphQL"
        ], k=random.randint(3, 6))

        experiences.append({
            "portfolioId": portfolio_id,
            "jobTitle": job_title,
            "companyName": company,
            "startDate": start_date,
            "endDate": end_date,
            "isCurrent": end_date is None,
            "description": desc,
            "skillsUsed": skills_used,
        })

    return experiences

def generate_skills(portfolio_id: str, count: int, dataset: Dict[str, Any]) -> List[Dict[str, Any]]:
    """Generate skills data"""
    
    skill_data = dataset.get("skills", {
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
    })
    
    skills = []
    categories = []
    for category_type, subcategories in skill_data.items():
        for subcategory, skill_names in subcategories.items():
            categories.append((category_type, subcategory, skill_names))
    
    for i in range(1, count + 1):
        category_type, subcategory, skill_names = random.choice(categories)
        skill_name = random.choice(skill_names)
        skills.append({
            "portfolioId": portfolio_id,
            "name": skill_name,
            "categoryType": category_type,
            "subcategory": subcategory,
            "category": f"{category_type}/{subcategory}",
            "proficiencyLevel": random.randint(1, 5),
            "displayOrder": i,
        })
    
    return skills

def generate_blog_posts(portfolio_id: str, count: int, dataset: Dict[str, Any]) -> List[Dict[str, Any]]:
    """Generate blog post data"""
    
    blog_topics = dataset.get("blogTopics", [
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
    ])
    
    tags_sets = dataset.get("blogTags", [
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
    ])
    
    blog_posts: List[Dict[str, Any]] = []
    for i in range(1, count + 1):
        topic = random.choice(blog_topics)
        tags = json.loads(random.choice(tags_sets).replace("'", '"')) if isinstance(tags_sets[0], str) else random.choice(tags_sets)
        content = f"""# {topic}

This article dives into {topic.lower()} with practical insights and examples.

## Highlights
- Real-world patterns and trade-offs
- Common pitfalls and how to avoid them
- Performance, reliability, and security considerations

## Example
```ts
export async function handler() {{
  return '{{topic}}';
}}
```
"""
        blog_posts.append({
            "portfolioId": portfolio_id,
            "title": f"{topic}",
            "excerpt": "A practical exploration with real-world patterns, trade-offs, and examples.",
            "content": content,
            "featuredImageUrl": f"https://picsum.photos/800/400?random={random.randint(1, 100000)}",
            "tags": tags,
            "isPublished": random.random() < 0.5,
        })

    return blog_posts

def get_available_templates(headers: Dict[str, str]) -> List[Dict[str, Any]]:
    """Get available portfolio templates from the API"""
    print("[TEMPLATES] Fetching available templates...")
    try:
        response = requests.get(f"{PORTFOLIO_TEMPLATE_API_BASE}/active", headers=headers)
        response.raise_for_status()
        templates = response.json()
        
        if not templates:
            print("[WARNING] No active templates found, using default")
            return [{"name": "Gabriel Bârzu"}]
        
        print(f"[TEMPLATES] Found {len(templates)} available templates:")
        for template in templates:
            print(f"   - {template['name']}")
        
        return templates
    except Exception as e:
        print(f"[WARNING] Error fetching templates: {e}")
        print("Using default template: Gabriel Bârzu")
        return [{"name": "Gabriel Bârzu"}]

def select_random_template(templates: List[Dict[str, Any]]) -> str:
    """Select a random template from the available templates"""
    selected_template = random.choice(templates)
    template_name = selected_template['name']
    print(f"[TARGET] Selected template: {template_name}")
    return template_name

def create_portfolio(user_id: str, headers: Dict[str, str], dataset: Dict[str, Any]) -> str:
    """Create a new portfolio and return its ID"""
    
    print("[STEP 1] Creating a new portfolio...")
    
    # Get available templates and select one randomly
    available_templates = get_available_templates(headers)
    selected_template = select_random_template(available_templates)
    
    portfolio_data = {
        "userId": user_id,
        "templateName": selected_template,
        "title": random.choice(dataset.get("portfolioTitles", [
            "Full Stack Developer Portfolio",
            "Software Engineer Portfolio",
            "Cloud and DevOps Engineer Portfolio",
            "Data Engineer Portfolio",
        ])),
        "bio": random.choice(dataset.get("portfolioBios", [
            "Engineer crafting scalable systems and elegant user experiences.",
            "Pragmatic developer with a passion for performance and reliability.",
            "Lifelong learner focused on clean code and resilient architectures.",
        ])),
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
        response = requests.post(PORTFOLIO_API_BASE, json=portfolio_data, headers=headers)
        response.raise_for_status()
        
        portfolio = response.json()
        portfolio_id = portfolio.get("id")
        
        if not portfolio_id:
            raise Exception(f"No portfolio ID in response: {portfolio}")
        
        print(f"[SUCCESS] Portfolio created successfully with ID: {portfolio_id}")
        return portfolio_id
        
    except requests.exceptions.RequestException as e:
        print(f"[ERROR] Failed to create portfolio: {e}")
        raise

def save_bulk_content(portfolio_id: str, headers: Dict[str, str], counts: Dict[str, int], dataset: Dict[str, Any]):
    """Save bulk content to the portfolio"""
    
    print("[STEP 2] Generating and saving bulk content (100 items each)...")
    
    # Generate all content
    print(f"  [PROJECTS] Generating {counts['projects']} projects...")
    projects = generate_projects(portfolio_id, counts['projects'], dataset)
    
    print(f"  [EXPERIENCE] Generating {counts['experiences']} experiences...")
    experiences = generate_experiences(portfolio_id, counts['experiences'], dataset)
    
    print(f"  [SKILLS] Generating {counts['skills']} skills...")
    skills = generate_skills(portfolio_id, counts['skills'], dataset)
    
    print(f"  [BLOG] Generating {counts['blogposts']} blog posts...")
    blog_posts = generate_blog_posts(portfolio_id, counts['blogposts'], dataset)
    
    # Create bulk content request
    bulk_content = {
        "portfolioId": portfolio_id,
        "projects": projects,
        "experience": experiences,
        "skills": skills,
        "blogPosts": blog_posts,
        "publishPortfolio": True
    }
    
    print("[STEP 3] Saving bulk content to portfolio...")
    
    try:
        url = f"{PORTFOLIO_API_BASE}/{portfolio_id}/save-content"
        response = requests.post(url, json=bulk_content, headers=headers)
        response.raise_for_status()
        
        result = response.json()
        print("[SUCCESS] Bulk content saved successfully!")
        
        return result
        
    except requests.exceptions.RequestException as e:
        print(f"[ERROR] Failed to save bulk content: {e}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response: {e.response.text}")
        raise

def main():
    """Main function to orchestrate the portfolio creation"""
    args = parse_args()
    headers = build_headers(args.token)
    user_id = args.user_id
    dataset: Dict[str, Any] = {}
    if args.dataset and os.path.isfile(args.dataset):
        try:
            with open(args.dataset, "r", encoding="utf-8") as f:
                loaded = json.load(f)
                dataset = loaded if isinstance(loaded, dict) else {}
        except Exception:
            dataset = {}
    counts = {
        'projects': args.projects or random.randint(25, 120),
        'experiences': args.experiences or random.randint(3, 12),
        'skills': args.skills or random.randint(20, 60),
        'blogposts': args.blogposts or random.randint(5, 25),
    }

    print("[START] Portfolio Test Data Generation for User:", user_id)
    print("=" * 63)
    
    try:
        # Step 1: Create portfolio
        portfolio_id = create_portfolio(user_id, headers, dataset)
        
        # Step 2: Generate and save content
        save_bulk_content(portfolio_id, headers, counts, dataset)
        
        # Summary
        print("\n[SUMMARY] Portfolio Summary:")
        print(f"   [PROJECTS] Projects: {counts['projects']}")
        print(f"   [EXPERIENCE] Experience: {counts['experiences']}")
        print(f"   [SKILLS] Skills: {counts['skills']}")
        print(f"   [BLOG] Blog Posts: {counts['blogposts']}")
        print()
        print(f"[URL] Portfolio URL: http://localhost:3000/portfolio/{portfolio_id}")
        print(f"[ADMIN] Admin Panel: http://localhost:3000/admin/portfolio/{portfolio_id}")
        print()
        print("[COMPLETE] Portfolio test data generation completed successfully!")
        print(f"[ID] Portfolio ID: {portfolio_id}")
        print(f"[USER] User ID: {user_id}")
        print("=" * 63)
        
    except Exception as e:
        print(f"\n[ERROR] Error during portfolio generation: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())
