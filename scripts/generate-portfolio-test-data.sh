#!/bin/bash

# Portfolio Test Data Generation Script (randomized, with bearer token support)

# Resolve Portfolio API base (prefer deployed backend-portfolio)
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
if [ -f "$SCRIPT_DIR/azure/.state/azure.env" ]; then
  # shellcheck disable=SC1090
  . "$SCRIPT_DIR/azure/.state/azure.env"
fi
BASE_ROOT="${PORTFOLIO_API_BASE_URL:-${NEXT_PUBLIC_PORTFOLIO_API_URL:-}}"
if [ -z "$BASE_ROOT" ]; then
  FQDN="$(az containerapp show -g "${AZ_ENV_RG:-$AZ_RG}" -n backend-portfolio --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || true)"
  [ -n "$FQDN" ] && BASE_ROOT="https://$FQDN"
fi
BASE_ROOT="${BASE_ROOT:-http://localhost:5201}"
BASE_ROOT="${BASE_ROOT%/}"
PORTFOLIO_API_BASE="$BASE_ROOT/api/Portfolio"
PORTFOLIO_TEMPLATE_API_BASE="$BASE_ROOT/api/PortfolioTemplate"

if [ -z "$1" ]; then
    echo "❌ Error: User ID is required as first argument"
    echo "Usage: $0 <USER_ID> [--token TOKEN] [--projects N] [--experiences N] [--skills N] [--blogposts N] [--dataset PATH]"
    exit 1
fi

USER_ID="$1"; shift

TOKEN="${AUTH_TOKEN}"; PROJECTS=0; EXPERIENCES=0; SKILLS=0; BLOGPOSTS=0; DATASET=""

while [ $# -gt 0 ]; do
    case "$1" in
        --token) TOKEN="$2"; shift 2 ;;
        --projects) PROJECTS="$2"; shift 2 ;;
        --experiences) EXPERIENCES="$2"; shift 2 ;;
        --skills) SKILLS="$2"; shift 2 ;;
        --blogposts) BLOGPOSTS="$2"; shift 2 ;;
        --dataset) DATASET="$2"; shift 2 ;; # accepted for parity; not used in this shell generator
        *) echo "Unknown option: $1"; exit 1 ;;
    esac
done

if [ -n "$TOKEN" ]; then AUTH_HEADER=( -H "Authorization: Bearer $TOKEN" ); else AUTH_HEADER=(); fi

echo "🚀 Starting Portfolio Test Data Generation for User: $USER_ID"
echo "==============================================================="

# Function to get available templates and select one randomly
get_random_template() {
    echo "🎨 Fetching available templates..." >&2
    TEMPLATES_RESPONSE=$(curl -s "${AUTH_HEADER[@]}" "$PORTFOLIO_TEMPLATE_API_BASE/active")
    if [ $? -ne 0 ] || [ -z "$TEMPLATES_RESPONSE" ]; then
        echo "⚠️  Warning: Could not fetch templates, using default" >&2
        echo "Gabriel Bârzu"
        return
    fi
    TEMPLATE_NAMES=$(echo "$TEMPLATES_RESPONSE" | grep -o '"name":"[^"]*"' | cut -d '"' -f4)
    if [ -z "$TEMPLATE_NAMES" ]; then
        echo "⚠️  Warning: No active templates found, using default" >&2
        echo "Gabriel Bârzu"
        return
    fi
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

SELECTED_TEMPLATE=$(get_random_template)

echo "📝 Step 1: Creating a new portfolio..."

# Randomize title and bio
TITLES=(
  "Full Stack Developer Portfolio"
  "Software Engineer Portfolio"
  "Cloud and DevOps Engineer Portfolio"
  "Data Engineer Portfolio"
)
BIOS=(
  "Engineer crafting scalable systems and elegant user experiences."
  "Pragmatic developer with a passion for performance and reliability."
  "Lifelong learner focused on clean code and resilient architectures."
)

PORTFOLIO_JSON=$(cat <<EOF
{
  "userId": "$USER_ID",
  "templateName": "$SELECTED_TEMPLATE",
  "title": "${TITLES[$((RANDOM % ${#TITLES[@]}))]}",
  "bio": "${BIOS[$((RANDOM % ${#BIOS[@]}))]}",
  "visibility": 0,
  "isPublished": false,
  "components": "[{\"id\":\"experience-1\",\"type\":\"experience\",\"order\":1,\"isVisible\":true,\"settings\":{}},{\"id\":\"projects-1\",\"type\":\"projects\",\"order\":2,\"isVisible\":true,\"settings\":{}},{\"id\":\"skills-1\",\"type\":\"skills\",\"order\":3,\"isVisible\":true,\"settings\":{}},{\"id\":\"blog_posts-1\",\"type\":\"blog_posts\",\"order\":4,\"isVisible\":true,\"settings\":{}}]"
}
EOF
)

CREATE_PORTFOLIO_RESPONSE=$(curl -s -X POST "$PORTFOLIO_API_BASE" \
  -H "Content-Type: application/json" \
  "${AUTH_HEADER[@]}" \
  -d "$PORTFOLIO_JSON")

PORTFOLIO_ID=$(echo "$CREATE_PORTFOLIO_RESPONSE" | grep -o '"id":"[^"]*"' | cut -d '"' -f4)
if [ -z "$PORTFOLIO_ID" ]; then
    echo "❌ Failed to create portfolio. Response: $CREATE_PORTFOLIO_RESPONSE"
    exit 1
fi

echo "✅ Portfolio created successfully with ID: $PORTFOLIO_ID"

echo "📊 Step 2: Generating and saving bulk content (randomized counts)..."

[ "$PROJECTS" -eq 0 ] && PROJECTS=$((25 + RANDOM % 96))
[ "$EXPERIENCES" -eq 0 ] && EXPERIENCES=$((3 + RANDOM % 10))
[ "$SKILLS" -eq 0 ] && SKILLS=$((20 + RANDOM % 41))
[ "$BLOGPOSTS" -eq 0 ] && BLOGPOSTS=$((5 + RANDOM % 21))

BULK_CONTENT_JSON=$(cat << 'EOF'
{
  "portfolioId": "PORTFOLIO_ID_PLACEHOLDER",
  "projects": [
EOF
)
BULK_CONTENT_JSON=${BULK_CONTENT_JSON//PORTFOLIO_ID_PLACEHOLDER/$PORTFOLIO_ID}

echo "  📁 Generating $PROJECTS projects..."
for ((i=1; i<=PROJECTS; i++)); do
    if [ $i -gt 1 ]; then BULK_CONTENT_JSON+=","; fi
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
        "E-commerce Platform" "Social Media App" "Project Management Tool" "Analytics Dashboard"
        "Learning Management System" "API Gateway" "Microservice" "Mobile App" "Web Application" "Desktop Application"
    )
    DESCR_SNIPPETS=(
        "Implemented OAuth2/OIDC and RBAC" "Reduced P95 latency by 45%" "Added OpenTelemetry and structured logs"
        "Event-driven architecture with DLQs" "CI/CD with automated tests and scans" "Caching and rate limiting"
        "Migrated to cloud-native services"
    )
    TECH_INDEX=$((RANDOM % ${#TECH_SETS[@]}))
    TYPE_INDEX=$((RANDOM % ${#PROJECT_TYPES[@]}))
    FEATURED=$([ $((RANDOM % 100)) -lt 12 ] && echo true || echo false)
    SNIPPET_A=${DESCR_SNIPPETS[$((RANDOM % ${#DESCR_SNIPPETS[@]}))]}
    SNIPPET_B=${DESCR_SNIPPETS[$((RANDOM % ${#DESCR_SNIPPETS[@]}))]}
    SNIPPET_C=${DESCR_SNIPPETS[$((RANDOM % ${#DESCR_SNIPPETS[@]}))]}
    IMG=$((RANDOM % 100000))
    DEMO=$((RANDOM % 99999))
    GH=$((RANDOM % 99999))
    BULK_CONTENT_JSON+="
    {
      \"portfolioId\": \"$PORTFOLIO_ID\",
      \"title\": \"${PROJECT_TYPES[$TYPE_INDEX]} #$i\",
      \"description\": \"${PROJECT_TYPES[$TYPE_INDEX]} with modern stack. $SNIPPET_A, $SNIPPET_B, $SNIPPET_C.\",
      \"imageUrl\": \"https://picsum.photos/400/300?random=$IMG\",
      \"demoUrl\": \"https://demo-project-$DEMO.example.com\",
      \"githubUrl\": \"https://github.com/example/project-$GH\",
      \"technologies\": ${TECH_SETS[$TECH_INDEX]},
      \"featured\": $FEATURED
    }"
done

BULK_CONTENT_JSON+="],
  \"experience\": ["

echo "  💼 Generating $EXPERIENCES experiences..."
for ((i=1; i<=EXPERIENCES; i++)); do
    if [ $i -gt 1 ]; then BULK_CONTENT_JSON+=","; fi
    COMPANIES=(
        "TechCorp International" "InnovateSoft Solutions" "DataDriven Analytics" "CloudFirst Technologies"
        "AgileDelivery Inc" "ScalableApps Ltd" "DigitalTransform Co" "SecureCode Systems" "OpenSource Collective" "FutureTech Ventures"
    )
    JOB_TITLES=(
        "Senior Software Engineer" "Full Stack Developer" "Backend Developer" "Frontend Developer" "DevOps Engineer"
        "Software Architect" "Technical Lead" "Engineering Manager" "Principal Engineer" "Staff Engineer"
    )
    RESP=(
        "Led cross-functional team to deliver features" "Architected microservices and events"
        "Mentored junior engineers and set standards" "Implemented CI/CD with automated tests"
        "Collaborated with product on roadmap" "Improved reliability with SLOs and runbooks"
    )
    COMPANY=${COMPANIES[$((RANDOM % ${#COMPANIES[@]}))]}
    TITLE=${JOB_TITLES[$((RANDOM % ${#JOB_TITLES[@]}))]}
    CURRENT=$([ $((RANDOM % 100)) -lt 20 ] && echo true || echo false)
    if $CURRENT; then
        YEAR=$((2022 + RANDOM % 3))
        START_DATE="$YEAR-0$((1 + RANDOM % 9))-01"
        END_DATE=null
    else
        START_YEAR=$((2010 + RANDOM % 14))
        END_YEAR=$((START_YEAR + RANDOM % 3))
        START_DATE="$START_YEAR-0$((1 + RANDOM % 9))-01"
        END_DATE="\"$END_YEAR-12-31\""
    fi
    R1=${RESP[$((RANDOM % ${#RESP[@]}))]}
    R2=${RESP[$((RANDOM % ${#RESP[@]}))]}
    R3=${RESP[$((RANDOM % ${#RESP[@]}))]}
    SKILLS_POOL=("Leadership" "Code Review" "Architecture Design" "Team Collaboration" "Agile" "Kubernetes" "AWS" "TypeScript" "C#" "PostgreSQL" "Redis" "Docker" "Terraform" "React" "GraphQL")
    # Build random skills array (3-6)
    NUM_SKILLS=$((3 + RANDOM % 4))
    SKILLS_ARR=""
    for ((s=0; s<NUM_SKILLS; s++)); do
        [ -n "$SKILLS_ARR" ] && SKILLS_ARR+=", "
        SKILLS_ARR+=\"${SKILLS_POOL[$((RANDOM % ${#SKILLS_POOL[@]}))]}\"
    done
    BULK_CONTENT_JSON+="
    {
      \"portfolioId\": \"$PORTFOLIO_ID\",
      \"jobTitle\": \"$TITLE\",
      \"companyName\": \"$COMPANY\",
      \"startDate\": \"$START_DATE\",
      \"endDate\": $END_DATE,
      \"isCurrent\": $CURRENT,
      \"description\": \"$R1. $R2. $R3.\",
      \"skillsUsed\": [$SKILLS_ARR]
    }"
done

BULK_CONTENT_JSON+="],
  \"skills\": ["

echo "  🛠️ Generating $SKILLS skills..."
for ((i=1; i<=SKILLS; i++)); do
    if [ $i -gt 1 ]; then BULK_CONTENT_JSON+=","; fi
    SKILL_CATEGORIES=("hard_skills:frontend" "hard_skills:backend" "hard_skills:database" "hard_skills:devops" "hard_skills:mobile" "hard_skills:testing" "hard_skills:cloud" "soft_skills:communication" "soft_skills:leadership" "soft_skills:problem_solving")
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
    CATEGORY=${SKILL_CATEGORIES[$((RANDOM % ${#SKILL_CATEGORIES[@]}))]}
    CATEGORY_TYPE=$(echo $CATEGORY | cut -d':' -f1)
    SUBCATEGORY=$(echo $CATEGORY | cut -d':' -f2)
    case $SUBCATEGORY in
        frontend) SKILL=${FRONTEND_SKILLS[$((RANDOM % ${#FRONTEND_SKILLS[@]}))]} ;;
        backend) SKILL=${BACKEND_SKILLS[$((RANDOM % ${#BACKEND_SKILLS[@]}))]} ;;
        database) SKILL=${DATABASE_SKILLS[$((RANDOM % ${#DATABASE_SKILLS[@]}))]} ;;
        devops) SKILL=${DEVOPS_SKILLS[$((RANDOM % ${#DEVOPS_SKILLS[@]}))]} ;;
        mobile) SKILL=${MOBILE_SKILLS[$((RANDOM % ${#MOBILE_SKILLS[@]}))]} ;;
        testing) SKILL=${TESTING_SKILLS[$((RANDOM % ${#TESTING_SKILLS[@]}))]} ;;
        cloud) SKILL=${CLOUD_SKILLS[$((RANDOM % ${#CLOUD_SKILLS[@]}))]} ;;
        communication) SKILL=${COMMUNICATION_SKILLS[$((RANDOM % ${#COMMUNICATION_SKILLS[@]}))]} ;;
        leadership) SKILL=${LEADERSHIP_SKILLS[$((RANDOM % ${#LEADERSHIP_SKILLS[@]}))]} ;;
        problem_solving) SKILL=${PROBLEM_SOLVING_SKILLS[$((RANDOM % ${#PROBLEM_SOLVING_SKILLS[@]}))]} ;;
    esac
    PROF_LEVEL=$((1 + RANDOM % 5))
    BULK_CONTENT_JSON+="
    {
      \"portfolioId\": \"$PORTFOLIO_ID\",
      \"name\": \"$SKILL\",
      \"categoryType\": \"$CATEGORY_TYPE\",
      \"subcategory\": \"$SUBCATEGORY\",
      \"category\": \"$CATEGORY_TYPE/$SUBCATEGORY\",
      \"proficiencyLevel\": $PROF_LEVEL,
      \"displayOrder\": $i
    }"
done

BULK_CONTENT_JSON+="],
  \"blogPosts\": ["

echo "  📝 Generating $BLOGPOSTS blog posts..."
for ((i=1; i<=BLOGPOSTS; i++)); do
    if [ $i -gt 1 ]; then BULK_CONTENT_JSON+=","; fi
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
    TOPIC_INDEX=$((RANDOM % ${#BLOG_TOPICS[@]}))
    TAGS_INDEX=$((RANDOM % ${#TAGS_SETS[@]}))
    PUBLISHED=$([ $((RANDOM % 100)) -lt 50 ] && echo true || echo false)
    IMG=$((RANDOM % 100000))
    BULK_CONTENT_JSON+="
    {
      \"portfolioId\": \"$PORTFOLIO_ID\",
      \"title\": \"${BLOG_TOPICS[$TOPIC_INDEX]}\",
      \"excerpt\": \"A practical exploration with real-world patterns, trade-offs, and examples.\",
      \"content\": \"# ${BLOG_TOPICS[$TOPIC_INDEX]}\\n\\nThis article dives into the topic with practical insights and examples.\",
      \"featuredImageUrl\": \"https://picsum.photos/800/400?random=$IMG\",
      \"tags\": ${TAGS_SETS[$TAGS_INDEX]},
      \"isPublished\": $PUBLISHED
    }"
done

BULK_CONTENT_JSON+="],
  \"publishPortfolio\": true
}"

echo "💾 Step 3: Saving bulk content to portfolio..."

SAVE_RESPONSE=$(curl -s -X POST "$PORTFOLIO_API_BASE/$PORTFOLIO_ID/save-content" \
  -H "Content-Type: application/json" \
  "${AUTH_HEADER[@]}" \
  -d "$BULK_CONTENT_JSON")

if echo "$SAVE_RESPONSE" | grep -q '"message":\s*"Portfolio content saved successfully"\|"projectsCreated":\s*[0-9]\+'; then
    echo "✅ Bulk content saved successfully!"
    echo "📊 Portfolio Summary:"
    echo "   📁 Projects: $PROJECTS"
    echo "   💼 Experience: $EXPERIENCES"
    echo "   🛠️ Skills: $SKILLS"
    echo "   📝 Blog Posts: $BLOGPOSTS"
    echo ""
    FRONTEND_BASE="${NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE:-}"
    if [ -z "$FRONTEND_BASE" ]; then
      HFQDN="$(az containerapp show -g "${AZ_ENV_RG:-$AZ_RG}" -n home-portfolio-service --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || true)"
      [ -n "$HFQDN" ] && FRONTEND_BASE="https://$HFQDN"
    fi
    FRONTEND_BASE="${FRONTEND_BASE:-http://localhost:3001}"
    FRONTEND_BASE="${FRONTEND_BASE%/}"
    echo "🌐 Portfolio URL: $FRONTEND_BASE/portfolio/$PORTFOLIO_ID"
    echo "🔧 Admin Panel: $FRONTEND_BASE/admin/portfolio/$PORTFOLIO_ID"
else
    echo "❌ Failed to save bulk content. Response: $SAVE_RESPONSE"
    exit 1
fi

echo ""
echo "🎉 Portfolio test data generation completed successfully!"
echo "📋 Portfolio ID: $PORTFOLIO_ID"
echo "👤 User ID: $USER_ID"
echo "==============================================================="
