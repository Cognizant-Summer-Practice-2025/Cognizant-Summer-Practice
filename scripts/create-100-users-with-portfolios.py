#!/usr/bin/env python3
"""
Mass User and Portfolio Creation Script
Creates N users and immediately creates a test portfolio for each one.
Adds Bearer token support and realistic random data.
"""

import argparse
import json
import os
import random
import subprocess
import sys
import time
from typing import Dict, Any, Tuple, Optional
import requests

USER_API_BASE = "http://localhost:5200/api/Users"
# Resolve portfolio script path relative to this file
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PORTFOLIO_SCRIPT_PATH = os.path.join(SCRIPT_DIR, "generate-portfolio-test-data.py")
DATASET_PATH_DEFAULT = os.path.join(SCRIPT_DIR, "random_data.json")

# Test data arrays
FIRST_NAMES = [
    "Alex", "Sarah", "Michael", "Emma", "James", "Olivia", "William", "Ava", 
    "Benjamin", "Isabella", "Lucas", "Sophia", "Henry", "Charlotte", "Alexander", 
    "Mia", "Sebastian", "Amelia", "Jack", "Harper", "Owen", "Evelyn", "Theodore", 
    "Abigail", "Jacob", "Emily", "Leo", "Elizabeth", "Mason", "Sofia", "Ethan", 
    "Avery", "Noah", "Ella", "Logan", "Scarlett", "Elijah", "Grace", "Oliver", 
    "Chloe", "Aiden", "Victoria", "Gabriel", "Riley", "Samuel", "Aria", "David", 
    "Lily", "Carter", "Aubrey", "Wyatt", "Zoey", "Jayden", "Penelope", "John", 
    "Lillian", "Hunter", "Addison", "Luke", "Layla", "Daniel", "Natalie", "Ryan", 
    "Camila", "Matthew", "Hannah", "Caleb", "Brooklyn", "Isaac", "Samantha"
]

LAST_NAMES = [
    "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", 
    "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", 
    "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", 
    "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", 
    "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", 
    "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", 
    "Carter", "Roberts", "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker", 
    "Cruz", "Edwards", "Collins", "Reyes", "Stewart", "Morris", "Morales", "Murphy"
]

DOMAINS = ["gmail.com", "outlook.com", "yahoo.com", "hotmail.com", "icloud.com", "protonmail.com"]

# Additional randomized fields for realism
PROFESSIONAL_TITLES = [
    "Software Engineer", "Full Stack Developer", "Backend Developer", "Frontend Engineer",
    "Data Engineer", "DevOps Engineer", "Mobile Developer", "Cloud Engineer",
    "Machine Learning Engineer", "Site Reliability Engineer"
]

LOCATIONS = [
    "New York, NY", "San Francisco, CA", "Austin, TX", "Seattle, WA", "Boston, MA",
    "Chicago, IL", "Denver, CO", "Toronto, ON", "London, UK", "Berlin, DE"
]

USER_BIOS = [
    "Passionate engineer building scalable systems and delightful user experiences.",
    "Developer focused on clean architecture, testing, and performance.",
    "Curious technologist who loves solving complex problems with simple solutions.",
    "Builder of robust APIs and intuitive UIs. Coffee-powered and detail-oriented.",
    "Engineer with a knack for automation, CI/CD, and cloud-native development.",
]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Create users and portfolios")
    parser.add_argument("--token", help="Bearer token for API authentication", default=os.getenv("AUTH_TOKEN"))
    parser.add_argument("--users", type=int, default=100, help="Number of users to create (default: 100)")
    parser.add_argument("--concurrency", type=int, default=10, help="Parallel workers (default: 10)")
    parser.add_argument("--dataset", type=str, default=DATASET_PATH_DEFAULT, help="Path to random data JSON (default: scripts/random_data.json)")
    return parser.parse_args()


def build_headers(token: Optional[str]) -> Dict[str, str]:
    headers: Dict[str, str] = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    return headers


def load_dataset(path: Optional[str]) -> Dict[str, Any]:
    dataset_path = path or DATASET_PATH_DEFAULT
    try:
        if dataset_path and os.path.isfile(dataset_path):
            with open(dataset_path, "r", encoding="utf-8") as f:
                data = json.load(f)
                # Basic validation
                return data if isinstance(data, dict) else {}
    except Exception:
        pass
    return {}


def build_dataset(min_items: int = 200) -> Dict[str, Any]:
    seniorities = [
        "Intern", "Junior", "Mid-level", "Senior", "Lead", "Staff", "Principal",
        "Head", "Director of", "VP of"
    ]
    roles = [
        "Software Engineer", "Backend Developer", "Frontend Engineer", "Full Stack Developer",
        "DevOps Engineer", "Site Reliability Engineer", "Data Engineer", "Machine Learning Engineer",
        "Cloud Engineer", "Security Engineer", "Mobile Developer", "QA Engineer",
        "Platform Engineer", "Systems Engineer"
    ]
    job_titles: list[str] = [f"{s} {r}" for s in seniorities for r in roles]
    job_titles += [
        "Software Architect", "Solutions Architect", "Engineering Manager",
        "Product Engineer", "Infrastructure Engineer", "AI Engineer"
    ]

    adjectives = [
        "Tech", "Data", "Cloud", "Nova", "Quantum", "Nexus", "Hyper", "Vertex",
        "Bright", "Blue", "Green", "Red", "Prime", "Pioneer", "Next", "Omni",
        "Apex", "Fusion", "Alpha", "Digital", "Secure", "Agile", "Rapid", "Urban",
        "Metro", "Global", "Dynamic", "First", "Future"
    ]
    nouns = [
        "Labs", "Systems", "Solutions", "Ventures", "Dynamics", "Works", "Group",
        "Analytics", "Networks", "Studios", "Industries", "Holdings", "Partners",
        "Platforms", "Software", "Technologies"
    ]
    companies: list[str] = [f"{a}{n}" for a in adjectives for n in nouns]
    companies = companies[: max(min_items, 250)]

    cities = [
        "New York", "San Francisco", "Austin", "Seattle", "Boston", "Chicago", "Denver",
        "Toronto", "Vancouver", "London", "Berlin", "Paris", "Amsterdam", "Stockholm",
        "Zurich", "Dublin", "Madrid", "Lisbon", "Sydney", "Melbourne", "Auckland",
        "Singapore", "Tokyo", "Seoul", "Hong Kong", "Munich", "Frankfurt", "Hamburg",
        "Cologne", "Copenhagen", "Oslo", "Helsinki", "Warsaw", "Prague", "Budapest",
        "Milan", "Rome", "Barcelona", "Valencia", "Porto"
    ]
    regions = [
        "NY", "CA", "TX", "WA", "MA", "IL", "CO", "ON", "BC", "UK", "DE", "FR",
        "NL", "SE", "CH", "IE", "ES", "PT", "NSW", "VIC", "NZ", "SG", "JP", "KR",
        "HK"
    ]
    locations: list[str] = []
    for c in cities:
        for r in regions:
            locations.append(f"{c}, {r}")
            if len(locations) >= max(min_items, 300):
                break
        if len(locations) >= max(min_items, 300):
            break

    domains = [
        "gmail.com", "outlook.com", "yahoo.com", "hotmail.com", "icloud.com",
        "protonmail.com", "mail.com", "pm.me", "gmx.com", "fastmail.com",
        "zoho.com", "hey.com", "aol.com", "yandex.com", "hushmail.com",
        "inbox.com", "live.com", "me.com", "tutanota.com", "duck.com"
    ]

    tech_stacks = [
        ["React", "TypeScript", "Node.js", "MongoDB"],
        ["Vue.js", "JavaScript", "Express", "PostgreSQL"],
        ["Angular", "TypeScript", "Spring Boot", "MySQL"],
        ["Python", "Django", "Redis", "Docker"],
        ["Java", "Spring", "Hibernate", "Oracle"],
        ["C#", ".NET", "Entity Framework", "SQL Server"],
        ["Go", "Gin", "GORM", "PostgreSQL"],
        ["Rust", "Actix", "Diesel", "PostgreSQL"],
        ["Ruby", "Rails", "Sidekiq", "PostgreSQL"],
        ["PHP", "Laravel", "MySQL", "Redis"],
        ["Svelte", "TypeScript", "Node.js", "PostgreSQL"],
        ["Next.js", "TypeScript", "Prisma", "PostgreSQL"],
        ["Nuxt", "TypeScript", "NestJS", "MongoDB"],
        ["Flutter", "Dart", "Firebase", "Cloud Functions"],
        ["React Native", "TypeScript", "GraphQL", "Hasura"]
    ]

    project_types = [
        "E-commerce Platform", "Social Media App", "Project Management Tool", "Analytics Dashboard",
        "Learning Management System", "API Gateway", "Microservice", "Mobile App", "Web Application",
        "Desktop Application", "Chatbot Platform", "Recommendation Engine", "Streaming Service",
        "IoT Management Console", "DevOps Pipeline", "Monitoring System", "CRM System", "ERP System",
        "Payment Processor", "Content Management System", "Booking Platform", "Marketplace",
        "Issue Tracker", "Blog Platform", "Knowledge Base", "Portfolio Website", "News Aggregator",
        "Trading Platform", "Healthcare Portal", "Fitness Tracker", "Food Delivery",
        "Ride Sharing", "Event Management", "Survey Tool", "Email Marketing", "A/B Testing Platform",
        "CI/CD Orchestrator", "Serverless API"
    ]

    project_snippets = [
        "Implemented OAuth2/OIDC and RBAC.",
        "Reduced P95 latency by 45%.",
        "Added OpenTelemetry and structured logs.",
        "Event-driven architecture with DLQs.",
        "CI/CD with automated tests and scans.",
        "Caching, pagination, and rate limiting.",
        "Blue/green deployments and canaries.",
        "Multi-tenant architecture with sharding.",
        "Autoscaling and cost optimizations.",
        "S3-backed asset pipeline and CDN."
    ]

    blog_topics = [
        "Advanced React Patterns", "Microservices with Node.js", "PostgreSQL Performance", "Kubernetes the Hard Way",
        "Modern CSS Layouts", "REST vs GraphQL", "DevOps CI/CD Pipelines", "ML in Production",
        "Web App Security Essentials", "Observability and Monitoring", "Caching Strategies",
        "API Design Best Practices", "System Design Fundamentals", "Event Sourcing and CQRS",
        "Testing Pyramid in Practice", "Deploying Serverless Apps", "Infrastructure as Code",
        "TypeScript for Large Codebases", "Clean Architecture", "Hexagonal Architecture"
    ]

    blog_tags = [
        ["react", "javascript", "frontend", "performance"],
        ["nodejs", "microservices", "backend", "architecture"],
        ["postgresql", "database", "sql", "optimization"],
        ["kubernetes", "devops", "cloud", "containers"],
        ["css", "design", "responsive", "ui"],
        ["api", "rest", "backend", "design"],
        ["cicd", "devops", "automation", "deployment"],
        ["ml", "ai", "python", "data"],
        ["security", "authentication", "best-practices"],
        ["performance", "monitoring", "analytics"]
    ]

    portfolio_titles = [
        "Full Stack Developer Portfolio", "Software Engineer Portfolio", "Cloud and DevOps Portfolio",
        "Data Engineer Portfolio", "Mobile Developer Portfolio", "Platform Engineer Portfolio"
    ]
    portfolio_bios = [
        "Engineer crafting scalable systems and elegant user experiences.",
        "Pragmatic developer with a passion for performance and reliability.",
        "Lifelong learner focused on clean code and resilient architectures.",
        "Builder of resilient distributed systems and internal platforms."
    ]

    return {
        "firstNames": FIRST_NAMES,
        "lastNames": LAST_NAMES,
        "domains": domains,
        "jobTitles": job_titles[: max(min_items, len(job_titles))],
        "companies": companies,
        "locations": locations,
        "bios": USER_BIOS,
        "techStacks": tech_stacks,
        "projectTypes": project_types,
        "projectSnippets": project_snippets,
        "blogTopics": blog_topics,
        "blogTags": blog_tags,
        "portfolioTitles": portfolio_titles,
        "portfolioBios": portfolio_bios,
    }


def ensure_dataset_file(path: str) -> Dict[str, Any]:
    try:
        os.makedirs(os.path.dirname(path), exist_ok=True)
    except Exception:
        pass
    if not os.path.isfile(path):
        data = build_dataset(200)
        with open(path, "w", encoding="utf-8") as f:
            json.dump(data, f, indent=2)
        return data
    return load_dataset(path)

def generate_user_data(dataset: Dict[str, Any]) -> Dict[str, str]:
    """Generate random user registration data with realistic fields"""
    first_name = random.choice(dataset.get("firstNames", FIRST_NAMES))
    last_name = random.choice(dataset.get("lastNames", LAST_NAMES))
    domain = random.choice(dataset.get("domains", DOMAINS))
    
    email = f"{first_name.lower()}.{last_name.lower()}{random.randint(1, 9999)}@{domain}"
    
    # Generate random avatar using the same approach as portfolio components
    avatar_seed = random.randint(1, 10000)
    
    return {
        "email": email,
        "firstName": first_name,
        "lastName": last_name,
        "professionalTitle": random.choice(dataset.get("jobTitles", PROFESSIONAL_TITLES)),
        "bio": random.choice(dataset.get("bios", USER_BIOS)),
        "location": random.choice(dataset.get("locations", LOCATIONS)),
        "profileImage": f"https://picsum.photos/150/150?random={avatar_seed}"
    }

def create_user(user_data: Dict[str, str], headers: Dict[str, str]) -> Tuple[bool, str]:
    """Create a user and return success status and user ID"""
    try:
        response = requests.post(f"{USER_API_BASE}/register", json=user_data, headers=headers, timeout=15)
        response.raise_for_status()
        
        user_response = response.json()
        user_id = user_response.get("id")
        
        if user_id:
            return True, user_id
        else:
            return False, f"No user ID in response: {user_response}"
            
    except requests.exceptions.RequestException as e:
        return False, f"Request failed: {str(e)}"
    except json.JSONDecodeError as e:
        return False, f"Invalid JSON response: {str(e)}"
    except Exception as e:
        return False, f"Unexpected error: {str(e)}"

def create_portfolio_for_user(user_id: str, token: Optional[str], dataset_path: Optional[str]) -> Tuple[bool, str]:
    """Create a portfolio for the given user ID"""
    try:
        cmd = [sys.executable, PORTFOLIO_SCRIPT_PATH, user_id]
        if token:
            cmd.extend(["--token", token])
        if dataset_path and os.path.isfile(dataset_path):
            cmd.extend(["--dataset", dataset_path])
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=180)
        
        if result.returncode == 0 and "Portfolio test data generation completed successfully" in result.stdout:
            return True, "Portfolio created successfully"
        else:
            error_msg = result.stderr if result.stderr else result.stdout
            return False, f"Portfolio creation failed: {error_msg}"
            
    except subprocess.TimeoutExpired:
        return False, "Portfolio creation timed out (>3 minutes)"
    except Exception as e:
        return False, f"Error running portfolio script: {str(e)}"

def main():
    """Main function to orchestrate mass user and portfolio creation"""
    args = parse_args()
    if not args.token:
        print("[WARNING] No token provided. Set --token or AUTH_TOKEN env. Requests may fail if auth is required.")
    headers = build_headers(args.token)
    dataset_path = args.dataset or DATASET_PATH_DEFAULT
    # Create a dataset JSON with >=200 entries if it doesn't exist
    dataset = ensure_dataset_file(dataset_path)

    print(f"Starting Mass User and Portfolio Creation ({args.users} users)")
    print("=" * 56)
    
    from concurrent.futures import ThreadPoolExecutor, as_completed
    created_count = 0
    failed_count = 0
    created_users = []
    results = []
    print("Progress tracking:")
    print("=" * 20)

    def create_user_and_portfolio(i):
        user_data = generate_user_data(dataset)
        print(f"Creating user {i}/{args.users}: {user_data['firstName']} {user_data['lastName']}... ", end="")
        user_success, user_result = create_user(user_data, headers)
        if user_success:
            user_id = user_result
            print(f"Created (ID: {user_id[:8]}...)")
            print("  Creating portfolio for user... ", end="")
            ds_path = args.dataset if args.dataset and os.path.isfile(args.dataset) else None
            portfolio_success, portfolio_result = create_portfolio_for_user(user_id, args.token, ds_path)
            if portfolio_success:
                print("Portfolio created!")
                return (True, user_id, user_data)
            else:
                print("Portfolio failed")
                print(f"     Error: {portfolio_result}")
                return (False, f"Portfolio failed: {portfolio_result}", user_data)
        else:
            print("User creation failed")
            print(f"   Error: {user_result}")
            return (False, f"User creation failed: {user_result}", user_data)

    with ThreadPoolExecutor(max_workers=args.concurrency) as executor:
        future_to_index = {executor.submit(create_user_and_portfolio, i): i for i in range(1, args.users + 1)}
        for idx, future in enumerate(as_completed(future_to_index), 1):
            result = future.result()
            if result[0]:
                created_count += 1
                created_users.append({
                    "userId": result[1],
                    "name": f"{result[2]['firstName']} {result[2]['lastName']}",
                    "email": result[2]['email']
                })
            else:
                failed_count += 1
            if idx % 10 == 0 or idx == args.users:
                print(f"Progress: {idx}/{args.users} users processed, {created_count} successful, {failed_count} failed\n")

    # Final summary
    print("\nMass Creation Complete!")
    print("=" * 26)
    print("Final Summary:")
    print(f"   Total users attempted: {args.users}")
    print(f"   Successful creations: {created_count}")
    print(f"   Failed creations: {failed_count}")
    print(f"   Success rate: {(created_count * 100) // max(1, args.users)}%")
    print()
    print("Each successful user has:")
    print("   User account created")
    print("   Portfolio with randomized counts and realistic data")
    print()
    print("Access portfolios via: http://localhost:3000/portfolio/[PORTFOLIO_ID]")
    print("Backend APIs running on: User (5200), Portfolio (5201)")
    # User list saving removed as requested

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\nProcess interrupted by user")
        sys.exit(1)
    except Exception as e:
        print(f"\nUnexpected error: {str(e)}")
        sys.exit(1)
