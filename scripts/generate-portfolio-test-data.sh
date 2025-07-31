#!/bin/bash

# Portfolio Test Data Generation Script
# This script creates a comprehensive portfolio for testing with 100 items in each category

# Accept USER_ID as command line argument
if [ -z "$1" ]; then
    echo "❌ Error: User ID is required as argument"
    echo "Usage: $0 <USER_ID>"
    exit 1
fi

USER_ID="$1"
PORTFOLIO_API_BASE="http://localhost:5201/api/Portfolio"
PORTFOLIO_TEMPLATE_API_BASE="http://localhost:5201/api/PortfolioTemplate"

echo "🚀 Starting Portfolio Test Data Generation for User: $USER_ID"
echo "==============================================================="

# Function to get available templates and select one randomly
get_random_template() {
    echo "🎨 Fetching available templates..." >&2
    
    TEMPLATES_RESPONSE=$(curl -s "$PORTFOLIO_TEMPLATE_API_BASE/active")
    
    if [ $? -ne 0 ] || [ -z "$TEMPLATES_RESPONSE" ]; then
        echo "⚠️  Warning: Could not fetch templates, using default" >&2
        echo "Gabriel Bârzu"
        return
    fi
    
    # Extract template names from JSON response - handle multi-word names properly
    TEMPLATE_NAMES=$(echo "$TEMPLATES_RESPONSE" | grep -o '"name":"[^"]*"' | cut -d'"' -f4)
    
    if [ -z "$TEMPLATE_NAMES" ]; then
        echo "⚠️  Warning: No active templates found, using default" >&2
        echo "Gabriel Bârzu"
        return
    fi
    
    # Convert to array using newlines as delimiter to handle multi-word names
    IFS=$'\n' TEMPLATES_ARRAY=($TEMPLATE_NAMES)
    TEMPLATE_COUNT=${#TEMPLATES_ARRAY[@]}
    RANDOM_INDEX=$((RANDOM % TEMPLATE_COUNT))
    SELECTED_TEMPLATE="${TEMPLATES_ARRAY[$RANDOM_INDEX]}"
    
    echo "📋 Found $TEMPLATE_COUNT available templates:" >&2
    for template in "${TEMPLATES_ARRAY[@]}"; do
        echo "   - $template" >&2
    done
    
    echo "🎯 Selected template: $SELECTED_TEMPLATE" >&2
    echo "$SELECTED_TEMPLATE"
}

# Get random template
SELECTED_TEMPLATE=$(get_random_template)

# Step 1: Create a portfolio first
echo "📝 Step 1: Creating a new portfolio..."

# Create JSON payload using a here document to avoid escaping issues
PORTFOLIO_JSON=$(cat <<EOF
{
  "userId": "$USER_ID",
  "templateName": "$SELECTED_TEMPLATE",
  "title": "Comprehensive Test Portfolio - Full Stack Developer",
  "bio": "This is a comprehensive test portfolio designed to stress-test the template system with maximum data. Contains 100 projects, 100 experiences, 100 skills, and 100 blog posts to validate performance and layout scalability.",
  "visibility": 0,
  "isPublished": false,
  "components": "[{\"id\":\"experience-1\",\"type\":\"experience\",\"order\":1,\"isVisible\":true,\"settings\":{}},{\"id\":\"projects-1\",\"type\":\"projects\",\"order\":2,\"isVisible\":true,\"settings\":{}},{\"id\":\"skills-1\",\"type\":\"skills\",\"order\":3,\"isVisible\":true,\"settings\":{}},{\"id\":\"blog_posts-1\",\"type\":\"blog_posts\",\"order\":4,\"isVisible\":true,\"settings\":{}}]"
}
EOF
)

CREATE_PORTFOLIO_RESPONSE=$(curl -s -X POST "$PORTFOLIO_API_BASE" \
  -H "Content-Type: application/json" \
  -d "$PORTFOLIO_JSON")

# Extract portfolio ID from response
PORTFOLIO_ID=$(echo $CREATE_PORTFOLIO_RESPONSE | grep -o '"id":"[^"]*"' | cut -d'"' -f4)

if [ -z "$PORTFOLIO_ID" ]; then
    echo "❌ Failed to create portfolio. Response: $CREATE_PORTFOLIO_RESPONSE"
    exit 1
fi

echo "✅ Portfolio created successfully with ID: $PORTFOLIO_ID"

# Step 2: Generate and save bulk content
echo "📊 Step 2: Generating and saving bulk content (100 items each)..."

# Create the bulk content request
BULK_CONTENT_JSON=$(cat << 'EOF'
{
  "portfolioId": "PORTFOLIO_ID_PLACEHOLDER",
  "projects": [
EOF
)

# Replace placeholder with actual portfolio ID
BULK_CONTENT_JSON=${BULK_CONTENT_JSON//PORTFOLIO_ID_PLACEHOLDER/$PORTFOLIO_ID}

# Generate 100 projects
echo "  📁 Generating 100 projects..."
for i in {1..100}; do
    # Add comma if not first item
    if [ $i -gt 1 ]; then
        BULK_CONTENT_JSON+=","
    fi
    
    # Vary project types and technologies
    TECH_SETS=(
        '["React", "TypeScript", "Node.js", "MongoDB"]'
        '["Vue.js", "JavaScript", "Express", "PostgreSQL"]'
        '["Angular", "TypeScript", "Spring Boot", "MySQL"]'
        '["Python", "Django", "Redis", "Docker"]'
        '["Java", "Spring", "Hibernate", "Oracle"]'
        '["C#", ".NET Core", "Entity Framework", "SQL Server"]'
        '["PHP", "Laravel", "Composer", "MariaDB"]'
        '["Ruby", "Rails", "Sidekiq", "PostgreSQL"]'
        '["Go", "Gin", "GORM", "SQLite"]'
        '["Rust", "Actix", "Diesel", "PostgreSQL"]'
    )
    
    PROJECT_TYPES=(
        "E-commerce Platform"
        "Social Media App"
        "Project Management Tool"
        "Analytics Dashboard"
        "Learning Management System"
        "API Gateway"
        "Microservice"
        "Mobile App"
        "Web Application"
        "Desktop Application"
    )
    
    # Select random tech stack and project type (using 0-based indexing)
    TECH_INDEX=$(((i - 1) % ${#TECH_SETS[@]}))
    TYPE_INDEX=$(((i - 1) % ${#PROJECT_TYPES[@]}))
    
    BULK_CONTENT_JSON+="
    {
      \"portfolioId\": \"$PORTFOLIO_ID\",
      \"title\": \"${PROJECT_TYPES[$TYPE_INDEX]} #$i\",
      \"description\": \"A comprehensive ${PROJECT_TYPES[$TYPE_INDEX]} built with modern technologies. Features include user authentication, real-time updates, responsive design, and scalable architecture. This project demonstrates advanced programming concepts and best practices in software development.\",
      \"imageUrl\": \"https://picsum.photos/400/300?random=$i\",
      \"demoUrl\": \"https://demo-project-$i.example.com\",
      \"githubUrl\": \"https://github.com/testuser/project-$i\",
      \"technologies\": ${TECH_SETS[$TECH_INDEX]},
      \"featured\": $([ $((i % 10)) -eq 1 ] && echo "true" || echo "false")
    }"
done

BULK_CONTENT_JSON+="],
  \"experience\": ["

# Generate 100 experiences
echo "  💼 Generating 100 experiences..."
for i in {1..100}; do
    if [ $i -gt 1 ]; then
        BULK_CONTENT_JSON+=","
    fi
    
    COMPANIES=(
        "TechCorp International"
        "InnovateSoft Solutions"
        "DataDriven Analytics"
        "CloudFirst Technologies"
        "AgileDelivery Inc"
        "ScalableApps Ltd"
        "DigitalTransform Co"
        "SecureCode Systems"
        "OpenSource Collective"
        "FutureTech Ventures"
    )
    
    JOB_TITLES=(
        "Senior Software Engineer"
        "Full Stack Developer"
        "Backend Developer"
        "Frontend Developer"
        "DevOps Engineer"
        "Software Architect"
        "Technical Lead"
        "Engineering Manager"
        "Principal Engineer"
        "Staff Engineer"
    )
    
    COMPANY_INDEX=$(((i - 1) % ${#COMPANIES[@]}))
    TITLE_INDEX=$(((i - 1) % ${#JOB_TITLES[@]}))
    
    # Generate dates - spread over last 25 years (using 0-based indexing)
    YEARS_AGO=$(((i - 1) / 4))
    START_YEAR=$((2024 - YEARS_AGO - 1))
    END_YEAR=$((2024 - YEARS_AGO))
    
    BULK_CONTENT_JSON+="
    {
      \"portfolioId\": \"$PORTFOLIO_ID\",
      \"jobTitle\": \"${JOB_TITLES[$TITLE_INDEX]}\",
      \"companyName\": \"${COMPANIES[$COMPANY_INDEX]}\",
      \"startDate\": \"$START_YEAR-01-01\",
      \"endDate\": \"$END_YEAR-12-31\",
      \"isCurrent\": $([ $i -eq 1 ] && echo "true" || echo "false"),
      \"description\": \"Led development of critical business applications serving millions of users. Collaborated with cross-functional teams to deliver high-quality software solutions. Mentored junior developers and contributed to architectural decisions. Implemented best practices for code quality, testing, and deployment.\",
      \"skillsUsed\": [\"Leadership\", \"Code Review\", \"Architecture Design\", \"Team Collaboration\", \"Agile Methodologies\"]
    }"
done

BULK_CONTENT_JSON+="],
  \"skills\": ["

# Generate 100 skills
echo "  🛠️ Generating 100 skills..."
for i in {1..100}; do
    if [ $i -gt 1 ]; then
        BULK_CONTENT_JSON+=","
    fi
    
    SKILL_CATEGORIES=(
        "hard_skills:frontend"
        "hard_skills:backend"
        "hard_skills:database"
        "hard_skills:devops"
        "hard_skills:mobile"
        "hard_skills:testing"
        "hard_skills:cloud"
        "soft_skills:communication"
        "soft_skills:leadership"
        "soft_skills:problem_solving"
    )
    
    FRONTEND_SKILLS=("React" "Vue.js" "Angular" "HTML5" "CSS3" "JavaScript" "TypeScript" "Sass" "Bootstrap" "Tailwind CSS")
    BACKEND_SKILLS=("Node.js" "Python" "Java" "C#" "PHP" "Ruby" "Go" "Rust" "Express.js" "Django")
    DATABASE_SKILLS=("PostgreSQL" "MySQL" "MongoDB" "Redis" "Elasticsearch" "SQLite" "Oracle" "DynamoDB" "Cassandra" "Neo4j")
    DEVOPS_SKILLS=("Docker" "Kubernetes" "Jenkins" "GitLab CI" "AWS" "Azure" "Terraform" "Ansible" "Prometheus" "Grafana")
    MOBILE_SKILLS=("React Native" "Flutter" "Swift" "Kotlin" "Ionic" "Xamarin" "Cordova" "Native Android" "Native iOS" "Unity")
    TESTING_SKILLS=("Jest" "Cypress" "Selenium" "JUnit" "pytest" "Mocha" "Chai" "TestNG" "Cucumber" "Postman")
    CLOUD_SKILLS=("AWS Lambda" "Azure Functions" "Google Cloud" "CloudFormation" "ARM Templates" "Cloud Storage" "CDN" "Load Balancing" "Auto Scaling" "Serverless")
    COMMUNICATION_SKILLS=("Public Speaking" "Technical Writing" "Documentation" "Presentation Skills" "Cross-functional Collaboration" "Client Communication" "Stakeholder Management" "Conflict Resolution" "Active Listening" "Mentoring")
    LEADERSHIP_SKILLS=("Team Leadership" "Project Management" "Strategic Planning" "Decision Making" "Delegation" "Performance Management" "Change Management" "Innovation" "Vision Setting" "Coaching")
    PROBLEM_SOLVING_SKILLS=("Analytical Thinking" "Critical Thinking" "Creative Problem Solving" "Root Cause Analysis" "Troubleshooting" "Algorithm Design" "System Design" "Performance Optimization" "Debugging" "Research")
    
    CATEGORY_INDEX=$(((i - 1) % ${#SKILL_CATEGORIES[@]}))
    CATEGORY=${SKILL_CATEGORIES[$CATEGORY_INDEX]}
    CATEGORY_TYPE=$(echo $CATEGORY | cut -d':' -f1)
    SUBCATEGORY=$(echo $CATEGORY | cut -d':' -f2)
    
    # Select skill based on category (using 0-based indexing)
    case $SUBCATEGORY in
        "frontend") SKILL=${FRONTEND_SKILLS[$(((i - 1) % ${#FRONTEND_SKILLS[@]}))]} ;;
        "backend") SKILL=${BACKEND_SKILLS[$(((i - 1) % ${#BACKEND_SKILLS[@]}))]} ;;
        "database") SKILL=${DATABASE_SKILLS[$(((i - 1) % ${#DATABASE_SKILLS[@]}))]} ;;
        "devops") SKILL=${DEVOPS_SKILLS[$(((i - 1) % ${#DEVOPS_SKILLS[@]}))]} ;;
        "mobile") SKILL=${MOBILE_SKILLS[$(((i - 1) % ${#MOBILE_SKILLS[@]}))]} ;;
        "testing") SKILL=${TESTING_SKILLS[$(((i - 1) % ${#TESTING_SKILLS[@]}))]} ;;
        "cloud") SKILL=${CLOUD_SKILLS[$(((i - 1) % ${#CLOUD_SKILLS[@]}))]} ;;
        "communication") SKILL=${COMMUNICATION_SKILLS[$(((i - 1) % ${#COMMUNICATION_SKILLS[@]}))]} ;;
        "leadership") SKILL=${LEADERSHIP_SKILLS[$(((i - 1) % ${#LEADERSHIP_SKILLS[@]}))]} ;;
        "problem_solving") SKILL=${PROBLEM_SOLVING_SKILLS[$(((i - 1) % ${#PROBLEM_SOLVING_SKILLS[@]}))]} ;;
    esac
    
    BULK_CONTENT_JSON+="
    {
      \"portfolioId\": \"$PORTFOLIO_ID\",
      \"name\": \"$SKILL\",
      \"categoryType\": \"$CATEGORY_TYPE\",
      \"subcategory\": \"$SUBCATEGORY\",
      \"category\": \"$CATEGORY_TYPE/$SUBCATEGORY\",
      \"proficiencyLevel\": $(((i - 1) % 5 + 1)),
      \"displayOrder\": $i
    }"
done

BULK_CONTENT_JSON+="],
  \"blogPosts\": ["

# Generate 100 blog posts
echo "  📝 Generating 100 blog posts..."
for i in {1..100}; do
    if [ $i -gt 1 ]; then
        BULK_CONTENT_JSON+=","
    fi
    
    BLOG_TOPICS=(
        "Advanced React Patterns and Performance Optimization"
        "Building Scalable Microservices with Node.js"
        "Database Design Best Practices for High-Traffic Applications"
        "Cloud-Native Development with Kubernetes"
        "Modern CSS Techniques and Layout Systems"
        "API Design and RESTful Architecture"
        "DevOps Automation and CI/CD Pipelines"
        "Machine Learning Integration in Web Applications"
        "Security Best Practices for Modern Web Apps"
        "Performance Monitoring and Optimization Strategies"
    )
    
    TAGS_SETS=(
        '["react", "javascript", "performance", "frontend"]'
        '["nodejs", "microservices", "backend", "architecture"]'
        '["database", "sql", "postgresql", "optimization"]'
        '["kubernetes", "devops", "cloud", "containers"]'
        '["css", "frontend", "design", "responsive"]'
        '["api", "rest", "design", "backend"]'
        '["devops", "cicd", "automation", "deployment"]'
        '["ml", "ai", "integration", "python"]'
        '["security", "authentication", "best-practices"]'
        '["performance", "monitoring", "optimization", "analytics"]'
    )
    
    TOPIC_INDEX=$(((i - 1) % ${#BLOG_TOPICS[@]}))
    TAGS_INDEX=$(((i - 1) % ${#TAGS_SETS[@]}))
    
    BULK_CONTENT_JSON+="
    {
      \"portfolioId\": \"$PORTFOLIO_ID\",
      \"title\": \"${BLOG_TOPICS[$TOPIC_INDEX]} - Part $i\",
      \"excerpt\": \"Comprehensive guide covering advanced concepts, practical examples, and real-world applications. Learn industry best practices and cutting-edge techniques used by top technology companies.\",
      \"content\": \"# ${BLOG_TOPICS[$TOPIC_INDEX]} - Part $i\n\nThis comprehensive article explores advanced concepts and practical implementations in modern software development. We'll cover key principles, best practices, and real-world examples that demonstrate how to build robust, scalable applications.\n\n## Introduction\n\nIn today's rapidly evolving technology landscape, staying current with best practices is crucial for delivering high-quality software solutions.\n\n## Key Concepts\n\n1. **Architecture Design**: Understanding system design principles\n2. **Performance Optimization**: Techniques for improving application speed\n3. **Scalability**: Building systems that grow with demand\n4. **Security**: Implementing robust security measures\n5. **Maintainability**: Writing clean, sustainable code\n\n## Implementation Examples\n\n\`\`\`javascript\n// Example code demonstrating best practices\nconst optimizedFunction = async (data) => {\n  try {\n    const result = await processData(data);\n    return result;\n  } catch (error) {\n    console.error('Processing failed:', error);\n    throw error;\n  }\n};\n\`\`\`\n\n## Conclusion\n\nImplementing these practices will significantly improve your application's performance, security, and maintainability. Continue learning and adapting these concepts to your specific use cases.\",
      \"featuredImageUrl\": \"https://picsum.photos/800/400?random=$((i + 1000))\",
      \"tags\": ${TAGS_SETS[$TAGS_INDEX]},
      \"isPublished\": $([ $((i % 3)) -eq 0 ] && echo "true" || echo "false")
    }"
done

BULK_CONTENT_JSON+="],
  \"publishPortfolio\": true
}"

# Save the bulk content
echo "💾 Step 3: Saving bulk content to portfolio..."

SAVE_RESPONSE=$(curl -s -X POST "$PORTFOLIO_API_BASE/$PORTFOLIO_ID/save-content" \
  -H "Content-Type: application/json" \
  -d "$BULK_CONTENT_JSON")

# Check if save was successful
if echo "$SAVE_RESPONSE" | grep -q '"message":\s*"Portfolio content saved successfully"\|"projectsCreated":\s*[0-9]\+'; then
    echo "✅ Bulk content saved successfully!"
    echo "📊 Portfolio Summary:"
    echo "   📁 Projects: 100 (10 featured)"
    echo "   💼 Experience: 100 entries (spanning 25 years)"
    echo "   🛠️ Skills: 100 (across 10 categories)"
    echo "   📝 Blog Posts: 100 (33% published)"
    echo ""
    echo "🌐 Portfolio URL: http://localhost:3000/portfolio/$PORTFOLIO_ID"
    echo "🔧 Admin Panel: http://localhost:3000/admin/portfolio/$PORTFOLIO_ID"
else
    echo "❌ Failed to save bulk content. Response: $SAVE_RESPONSE"
    exit 1
fi

echo ""
echo "🎉 Portfolio test data generation completed successfully!"
echo "📋 Portfolio ID: $PORTFOLIO_ID"
echo "👤 User ID: $USER_ID"
echo "==============================================================="
