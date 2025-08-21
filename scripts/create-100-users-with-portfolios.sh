#!/bin/bash

# Mass User and Portfolio Creation Script (token-enabled, randomized user data)

USER_API_BASE="https://backend-user.kindmoss-e060904c.westeurope.azurecontainerapps.io/api/Users"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PORTFOLIO_SCRIPT_PATH="$SCRIPT_DIR/generate-portfolio-test-data.sh"

USERS=100
MAX_JOBS=10
TOKEN="${AUTH_TOKEN}"
DATASET="$SCRIPT_DIR/random_data.json"

while [ $# -gt 0 ]; do
  case "$1" in
    --users) USERS="$2"; shift 2 ;;
    --concurrency) MAX_JOBS="$2"; shift 2 ;;
    --token) TOKEN="$2"; shift 2 ;;
    --dataset) DATASET="$2"; shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

if [ -n "$TOKEN" ]; then AUTH_HEADER=( -H "Authorization: Bearer $TOKEN" ); else AUTH_HEADER=(); fi

# Ensure dataset JSON exists with >=200 entries for randomness
ensure_dataset_json() {
  local path="$1"
  [ -z "$path" ] && path="$SCRIPT_DIR/random_data.json"
  if [ -f "$path" ]; then
    return 0
  fi
  mkdir -p "$(dirname "$path")"
  local PYTHON=python3
  command -v python3 >/dev/null 2>&1 || PYTHON=python
  if ! command -v "$PYTHON" >/dev/null 2>&1; then
    echo "⚠️  Python not found. Skipping dataset generation."
    return 0
  fi
  "$PYTHON" - <<'PY' > "$path"
import json

first_names = [
    "Alex","Sarah","Michael","Emma","James","Olivia","William","Ava","Benjamin","Isabella","Lucas","Sophia","Henry","Charlotte","Alexander","Mia","Sebastian","Amelia","Jack","Harper","Owen","Evelyn","Theodore","Abigail","Jacob","Emily","Leo","Elizabeth","Mason","Sofia","Ethan","Avery","Noah","Ella","Logan","Scarlett","Elijah","Grace","Oliver","Chloe","Aiden","Victoria","Gabriel","Riley","Samuel","Aria","David","Lily","Carter","Aubrey","Wyatt","Zoey","Jayden","Penelope","John","Lillian","Hunter","Addison","Luke","Layla","Daniel","Natalie","Ryan","Camila","Matthew","Hannah","Caleb","Brooklyn","Isaac","Samantha"
]
last_names = [
    "Smith","Johnson","Williams","Brown","Jones","Garcia","Miller","Davis","Rodriguez","Martinez","Hernandez","Lopez","Gonzalez","Wilson","Anderson","Thomas","Taylor","Moore","Jackson","Martin","Lee","Perez","Thompson","White","Harris","Sanchez","Clark","Ramirez","Lewis","Robinson","Walker","Young","Allen","King","Wright","Scott","Torres","Nguyen","Hill","Flores","Green","Adams","Nelson","Baker","Hall","Rivera","Campbell","Mitchell","Carter","Roberts","Gomez","Phillips","Evans","Turner","Diaz","Parker","Cruz","Edwards","Collins","Reyes","Stewart","Morris","Morales","Murphy"
]

seniorities = ["Intern","Junior","Mid-level","Senior","Lead","Staff","Principal","Head","Director of","VP of"]
roles = [
    "Software Engineer","Backend Developer","Frontend Engineer","Full Stack Developer",
    "DevOps Engineer","Site Reliability Engineer","Data Engineer","Machine Learning Engineer",
    "Cloud Engineer","Security Engineer","Mobile Developer","QA Engineer",
    "Platform Engineer","Systems Engineer"
]
job_titles = [f"{s} {r}" for s in seniorities for r in roles]
job_titles += ["Software Architect","Solutions Architect","Engineering Manager","Product Engineer","Infrastructure Engineer","AI Engineer"]

adjectives = [
  "Tech","Data","Cloud","Nova","Quantum","Nexus","Hyper","Vertex","Bright","Blue","Green","Red","Prime","Pioneer","Next","Omni","Apex","Fusion","Alpha","Digital","Secure","Agile","Rapid","Urban","Metro","Global","Dynamic","First","Future"
]
nouns = [
  "Labs","Systems","Solutions","Ventures","Dynamics","Works","Group","Analytics","Networks","Studios","Industries","Holdings","Partners","Platforms","Software","Technologies"
]
companies = [f"{a}{n}" for a in adjectives for n in nouns][:250]

cities = [
 "New York","San Francisco","Austin","Seattle","Boston","Chicago","Denver","Toronto","Vancouver","London","Berlin","Paris","Amsterdam","Stockholm","Zurich","Dublin","Madrid","Lisbon","Sydney","Melbourne","Auckland","Singapore","Tokyo","Seoul","Hong Kong","Munich","Frankfurt","Hamburg","Cologne","Copenhagen","Oslo","Helsinki","Warsaw","Prague","Budapest","Milan","Rome","Barcelona","Valencia","Porto"
]
regions = ["NY","CA","TX","WA","MA","IL","CO","ON","BC","UK","DE","FR","NL","SE","CH","IE","ES","PT","NSW","VIC","NZ","SG","JP","KR","HK"]
locations = []
for c in cities:
  for r in regions:
    locations.append(f"{c}, {r}")
    if len(locations) >= 300:
      break
  if len(locations) >= 300:
    break

domains = [
  "gmail.com","outlook.com","yahoo.com","hotmail.com","icloud.com","protonmail.com","mail.com","pm.me","gmx.com","fastmail.com","zoho.com","hey.com","aol.com","yandex.com","hushmail.com","inbox.com","live.com","me.com","tutanota.com","duck.com"
]

tech_stacks = [
  ["React","TypeScript","Node.js","MongoDB"],
  ["Vue.js","JavaScript","Express","PostgreSQL"],
  ["Angular","TypeScript","Spring Boot","MySQL"],
  ["Python","Django","Redis","Docker"],
  ["Java","Spring","Hibernate","Oracle"],
  ["C#",".NET","Entity Framework","SQL Server"],
  ["Go","Gin","GORM","PostgreSQL"],
  ["Rust","Actix","Diesel","PostgreSQL"],
  ["Ruby","Rails","Sidekiq","PostgreSQL"],
  ["PHP","Laravel","MySQL","Redis"],
  ["Svelte","TypeScript","Node.js","PostgreSQL"],
  ["Next.js","TypeScript","Prisma","PostgreSQL"],
  ["Nuxt","TypeScript","NestJS","MongoDB"],
  ["Flutter","Dart","Firebase","Cloud Functions"],
  ["React Native","TypeScript","GraphQL","Hasura"]
]

project_types = [
  "E-commerce Platform","Social Media App","Project Management Tool","Analytics Dashboard","Learning Management System","API Gateway","Microservice","Mobile App","Web Application","Desktop Application","Chatbot Platform","Recommendation Engine","Streaming Service","IoT Management Console","DevOps Pipeline","Monitoring System","CRM System","ERP System","Payment Processor","CMS","Booking Platform","Marketplace","Issue Tracker","Blog Platform","Knowledge Base","Portfolio Website","News Aggregator","Trading Platform","Healthcare Portal","Fitness Tracker","Food Delivery","Ride Sharing","Event Management","Survey Tool","Email Marketing","A/B Testing Platform","CI/CD Orchestrator","Serverless API"
]

project_snippets = [
  "Implemented OAuth2/OIDC and RBAC.","Reduced P95 latency by 45%.","Added OpenTelemetry and structured logs.","Event-driven architecture with DLQs.","CI/CD with automated tests and scans.","Caching, pagination, and rate limiting.","Blue/green deployments and canaries.","Multi-tenant architecture with sharding.","Autoscaling and cost optimizations.","S3-backed asset pipeline and CDN."
]

blog_topics = [
  "Advanced React Patterns","Microservices with Node.js","PostgreSQL Performance","Kubernetes the Hard Way","Modern CSS Layouts","REST vs GraphQL","DevOps CI/CD Pipelines","ML in Production","Web App Security Essentials","Observability and Monitoring","Caching Strategies","API Design Best Practices","System Design Fundamentals","Event Sourcing and CQRS","Testing Pyramid in Practice","Deploying Serverless Apps","Infrastructure as Code","TypeScript for Large Codebases","Clean Architecture","Hexagonal Architecture"
]
blog_tags = [
  ["react","javascript","frontend","performance"],
  ["nodejs","microservices","backend","architecture"],
  ["postgresql","database","sql","optimization"],
  ["kubernetes","devops","cloud","containers"],
  ["css","design","responsive","ui"],
  ["api","rest","backend","design"],
  ["cicd","devops","automation","deployment"],
  ["ml","ai","python","data"],
  ["security","authentication","best-practices"],
  ["performance","monitoring","analytics"]
]

portfolio_titles = [
  "Full Stack Developer Portfolio","Software Engineer Portfolio","Cloud and DevOps Portfolio","Data Engineer Portfolio","Mobile Developer Portfolio","Platform Engineer Portfolio"
]
portfolio_bios = [
  "Engineer crafting scalable systems and elegant user experiences.",
  "Pragmatic developer with a passion for performance and reliability.",
  "Lifelong learner focused on clean code and resilient architectures.",
  "Builder of resilient distributed systems and internal platforms."
]

bios = [
  "Passionate engineer building scalable systems and delightful user experiences.",
  "Developer focused on clean architecture, testing, and performance.",
  "Curious technologist who loves solving complex problems with simple solutions.",
  "Builder of robust APIs and intuitive UIs. Coffee-powered and detail-oriented.",
  "Engineer with a knack for automation, CI/CD, and cloud-native development."
]

data = {
  "firstNames": first_names,
  "lastNames": last_names,
  "domains": domains,
  "jobTitles": job_titles,
  "companies": companies,
  "locations": locations,
  "bios": bios,
  "techStacks": tech_stacks,
  "projectTypes": project_types,
  "projectSnippets": project_snippets,
  "blogTopics": blog_topics,
  "blogTags": blog_tags,
  "portfolioTitles": portfolio_titles,
  "portfolioBios": portfolio_bios,
}

json.dump(data, open(0, 'w'), indent=2)
PY
}

ensure_dataset_json "$DATASET"

echo "🚀 Starting Mass User and Portfolio Creation ($USERS users)"
echo "========================================================"

# Arrays for generating realistic test data
FIRST_NAMES=("Alex" "Sarah" "Michael" "Emma" "James" "Olivia" "William" "Ava" "Benjamin" "Isabella" "Lucas" "Sophia" "Henry" "Charlotte" "Alexander" "Mia" "Sebastian" "Amelia" "Jack" "Harper" "Owen" "Evelyn" "Theodore" "Abigail" "Jacob" "Emily" "Leo" "Elizabeth" "Mason" "Sofia" "Ethan" "Avery" "Noah" "Ella" "Logan" "Scarlett" "Elijah" "Grace" "Oliver" "Chloe" "Aiden" "Victoria" "Gabriel" "Riley" "Samuel" "Aria" "David" "Lily" "Carter" "Aubrey" "Wyatt" "Zoey" "Jayden" "Penelope" "John" "Lillian" "Hunter" "Addison" "Luke" "Layla" "Daniel" "Natalie" "Ryan" "Camila" "Matthew" "Hannah" "Caleb" "Brooklyn" "Isaac" "Samantha")
LAST_NAMES=("Smith" "Johnson" "Williams" "Brown" "Jones" "Garcia" "Miller" "Davis" "Rodriguez" "Martinez" "Hernandez" "Lopez" "Gonzalez" "Wilson" "Anderson" "Thomas" "Taylor" "Moore" "Jackson" "Martin" "Lee" "Perez" "Thompson" "White" "Harris" "Sanchez" "Clark" "Ramirez" "Lewis" "Robinson" "Walker" "Young" "Allen" "King" "Wright" "Scott" "Torres" "Nguyen" "Hill" "Flores" "Green" "Adams" "Nelson" "Baker" "Hall" "Rivera" "Campbell" "Mitchell" "Carter" "Roberts" "Gomez" "Phillips" "Evans" "Turner" "Diaz" "Parker" "Cruz" "Edwards" "Collins" "Reyes" "Stewart" "Morris" "Morales" "Murphy")
DOMAINS=("gmail.com" "outlook.com" "yahoo.com" "hotmail.com" "icloud.com" "protonmail.com")
TITLES=("Software Engineer" "Full Stack Developer" "Backend Developer" "Frontend Engineer" "Data Engineer" "DevOps Engineer" "Mobile Developer" "Cloud Engineer" "ML Engineer" "SRE")
LOCATIONS=("New York, NY" "San Francisco, CA" "Austin, TX" "Seattle, WA" "Boston, MA" "Chicago, IL" "Denver, CO" "Toronto, ON" "London, UK" "Berlin, DE")
BIOS=(
  "Passionate engineer building scalable systems and delightful user experiences."
  "Developer focused on clean architecture, testing, and performance."
  "Curious technologist who loves solving complex problems with simple solutions."
  "Builder of robust APIs and intuitive UIs. Coffee-powered and detail-oriented."
  "Engineer with a knack for automation, CI/CD, and cloud-native development."
)

CREATED_COUNT=0
FAILED_COUNT=0

create_user_and_portfolio() {
  local i="$1"
  local FIRST_NAME=${FIRST_NAMES[$((RANDOM % ${#FIRST_NAMES[@]}))]}
  local LAST_NAME=${LAST_NAMES[$((RANDOM % ${#LAST_NAMES[@]}))]}
  local DOMAIN=${DOMAINS[$((RANDOM % ${#DOMAINS[@]}))]}
  local EMAIL="$(echo "$FIRST_NAME" | tr '[:upper:]' '[:lower:]').$(echo "$LAST_NAME" | tr '[:upper:]' '[:lower:]')$((RANDOM % 9999))@${DOMAIN}"
  local AVATAR_SEED=$((RANDOM % 10000))
  local TITLE=${TITLES[$((RANDOM % ${#TITLES[@]}))]}
  local BIO=${BIOS[$((RANDOM % ${#BIOS[@]}))]}
  local LOCATION=${LOCATIONS[$((RANDOM % ${#LOCATIONS[@]}))]}

  echo -n "Creating user $i/$USERS: $FIRST_NAME $LAST_NAME... "
  USER_RESPONSE=$(curl -s -X POST "$USER_API_BASE/register" \
    -H "Content-Type: application/json" \
    "${AUTH_HEADER[@]}" \
    -d "{\"email\":\"$EMAIL\",\"firstName\":\"$FIRST_NAME\",\"lastName\":\"$LAST_NAME\",\"professionalTitle\":\"$TITLE\",\"bio\":\"$BIO\",\"location\":\"$LOCATION\",\"profileImage\":\"https://picsum.photos/150/150?random=$AVATAR_SEED\"}")

  if [[ $USER_RESPONSE == *"id"* ]] && [[ $USER_RESPONSE != *"error"* ]]; then
    USER_ID=$(echo "$USER_RESPONSE" | grep -o '"id":"[^"]*"' | cut -d '"' -f4)
    if [ -n "$USER_ID" ]; then
      echo "Created (ID: ${USER_ID:0:8}...)"
      echo "  Creating portfolio for user..."
      PORTFOLIO_OUTPUT=$(bash "$PORTFOLIO_SCRIPT_PATH" "$USER_ID" --token "$TOKEN" --dataset "$DATASET" 2>&1)
      if echo "$PORTFOLIO_OUTPUT" | grep -q "Portfolio test data generation completed successfully"; then
        echo "  Portfolio created successfully!"
        return 0
      else
        echo "  Portfolio creation failed"
        echo "     Error: $PORTFOLIO_OUTPUT"
        return 1
      fi
    else
      echo "Failed to extract user ID"
      return 1
    fi
  else
    echo "Failed to create user"
    echo "   Response: $USER_RESPONSE"
    return 1
  fi
}

job_pids=()
job_results_file=$(mktemp)

for ((i=1; i<=USERS; i++)); do
  (
    if create_user_and_portfolio "$i"; then
      echo SUCCESS >> "$job_results_file"
    else
      echo FAIL >> "$job_results_file"
    fi
  ) &
  job_pids+=("$!")
  while [ "$(jobs -r | wc -l | tr -d ' ')" -ge "$MAX_JOBS" ]; do
    sleep 0.2
  done
done

for pid in "${job_pids[@]}"; do
  wait "$pid"
done

CREATED_COUNT=$(grep -c SUCCESS "$job_results_file" 2>/dev/null || echo 0)
FAILED_COUNT=$(grep -c FAIL "$job_results_file" 2>/dev/null || echo 0)
rm -f "$job_results_file"

echo ""
echo "🎉 Mass Creation Complete!"
echo "=========================="
echo "📈 Final Summary:"
echo "   📊 Total users attempted: $USERS"
echo "   ✅ Successful creations: $CREATED_COUNT"
echo "   ❌ Failed creations: $FAILED_COUNT"
if [ "$USERS" -gt 0 ]; then
  echo "   📈 Success rate: $((CREATED_COUNT * 100 / USERS))%"
fi
echo ""
echo "🎯 Each successful user has:"
echo "   👤 User account created"
echo "   📁 Portfolio with randomized projects, experience, skills, and blog posts"
echo ""
echo "🌐 Access portfolios via: http://localhost:3000/portfolio/[PORTFOLIO_ID]"
echo "🚀 Backend APIs running on: User (5200), Portfolio (5201)"
