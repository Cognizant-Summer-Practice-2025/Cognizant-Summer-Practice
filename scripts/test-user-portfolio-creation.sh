#!/bin/bash

# Test Script - Create 2 users with portfolios (token-enabled, relative paths)

USER_API_BASE="http://localhost:5200/api/Users"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PORTFOLIO_SCRIPT_PATH="$SCRIPT_DIR/generate-portfolio-test-data.sh"

TOKEN="${AUTH_TOKEN}"

while [ $# -gt 0 ]; do
  case "$1" in
    --token) TOKEN="$2"; shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

if [ -n "$TOKEN" ]; then AUTH_HEADER=( -H "Authorization: Bearer $TOKEN" ); else AUTH_HEADER=(); fi

echo "🧪 Testing User and Portfolio Creation (2 users)"
echo "==============================================="

FIRST_NAMES=("Alex" "Sarah" "Michael" "Emma" "James" "Olivia" "William" "Ava" "Benjamin" "Isabella" "Lucas" "Sophia" "Henry" "Charlotte" "Alexander" "Mia" "Sebastian" "Amelia" "Jack" "Harper" "Owen" "Evelyn" "Theodore" "Abigail" "Jacob" "Emily" "Leo" "Elizabeth" "Mason" "Sofia" "Ethan" "Avery" "Noah" "Ella" "Logan" "Scarlett" "Elijah" "Grace" "Oliver" "Chloe" "Aiden" "Victoria" "Gabriel" "Riley" "Samuel" "Aria" "David" "Lily" "Carter" "Aubrey" "Wyatt" "Zoey" "Jayden" "Penelope" "John" "Lillian" "Hunter" "Addison" "Luke" "Layla" "Daniel" "Natalie" "Ryan" "Camila" "Matthew" "Hannah" "Caleb" "Brooklyn" "Isaac" "Samantha")
LAST_NAMES=("Smith" "Johnson" "Williams" "Brown" "Jones" "Garcia" "Miller" "Davis" "Rodriguez" "Martinez" "Hernandez" "Lopez" "Gonzalez" "Wilson" "Anderson" "Thomas" "Taylor" "Moore" "Jackson" "Martin" "Lee" "Perez" "Thompson" "White" "Harris" "Sanchez" "Clark" "Ramirez" "Lewis" "Robinson" "Walker" "Young" "Allen" "King" "Wright" "Scott" "Torres" "Nguyen" "Hill" "Flores" "Green" "Adams" "Nelson" "Baker" "Hall" "Rivera" "Campbell" "Mitchell" "Carter" "Roberts" "Gomez" "Phillips" "Evans" "Turner" "Diaz" "Parker" "Cruz" "Edwards" "Collins" "Reyes" "Stewart" "Morris" "Morales" "Murphy")
DOMAINS=("gmail.com" "outlook.com" "yahoo.com" "hotmail.com" "icloud.com" "protonmail.com")

CREATED_COUNT=0
FAILED_COUNT=0

for i in {1..2}; do
  FIRST_NAME=${FIRST_NAMES[$((RANDOM % ${#FIRST_NAMES[@]}))]}
  LAST_NAME=${LAST_NAMES[$((RANDOM % ${#LAST_NAMES[@]}))]}
  DOMAIN=${DOMAINS[$((RANDOM % ${#DOMAINS[@]}))]}
  EMAIL="$(echo "$FIRST_NAME" | tr '[:upper:]' '[:lower:]').$(echo "$LAST_NAME" | tr '[:upper:]' '[:lower:]')$((RANDOM % 9999))@${DOMAIN}"
  FULL_NAME="$FIRST_NAME $LAST_NAME"
  AVATAR_SEED=$((RANDOM % 10000))
  echo "👤 Creating user $i/2: $FULL_NAME..."

  USER_RESPONSE=$(curl -s -X POST "$USER_API_BASE/register" \
    -H "Content-Type: application/json" \
    "${AUTH_HEADER[@]}" \
    -d "{\"email\":\"$EMAIL\",\"firstName\":\"$FIRST_NAME\",\"lastName\":\"$LAST_NAME\",\"professionalTitle\":\"Software Developer\",\"bio\":\"Test user created for portfolio testing\",\"location\":\"Test City, TC\",\"profileImage\":\"https://picsum.photos/150/150?random=$AVATAR_SEED\"}")

  echo "Response: $USER_RESPONSE"

  if [[ $USER_RESPONSE == *"id"* ]] && [[ $USER_RESPONSE != *"error"* ]]; then
    USER_ID=$(echo "$USER_RESPONSE" | grep -o '"id":"[^"]*"' | cut -d '"' -f4)
    if [ -n "$USER_ID" ]; then
      echo "✅ User created successfully (ID: $USER_ID)"
      echo "📁 Creating portfolio for user $USER_ID..."
      bash "$PORTFOLIO_SCRIPT_PATH" "$USER_ID" --token "$TOKEN"
      if [ $? -eq 0 ]; then
        echo "✅ Portfolio created successfully!"
        ((CREATED_COUNT++))
      else
        echo "❌ Portfolio creation failed"
        ((FAILED_COUNT++))
      fi
    else
      echo "❌ Failed to extract user ID"
      ((FAILED_COUNT++))
    fi
  else
    echo "❌ Failed to create user"
    echo "   Response: $USER_RESPONSE"
    ((FAILED_COUNT++))
  fi
  echo "---"
done

echo ""
echo "🎯 Test Results:"
echo "==============="
echo "✅ Successful: $CREATED_COUNT"
echo "❌ Failed: $FAILED_COUNT"

if [ $CREATED_COUNT -gt 0 ]; then
  echo ""
  echo "🚀 Test successful! You can now run the full script:"
  echo "   ./create-100-users-with-portfolios.sh --token $AUTH_TOKEN"
  echo "   OR"
  echo "   python3 create-100-users-with-portfolios.py --token $AUTH_TOKEN"
fi
