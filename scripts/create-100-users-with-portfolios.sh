#!/bin/bash

# Mass User and Portfolio Creation Script
# Creates 100 users and immediately creates a test portfolio for each one

USER_API_BASE="http://localhost:5200/api/Users"
PORTFOLIO_SCRIPT_PATH="/Users/theo/Documents/Cognizant-Summer-Practice/scripts/generate-portfolio-test-data.sh"

echo "🚀 Starting Mass User and Portfolio Creation (100 users)"
echo "========================================================"

# Arrays for generating realistic test data
FIRST_NAMES=("Alex" "Sarah" "Michael" "Emma" "James" "Olivia" "William" "Ava" "Benjamin" "Isabella" "Lucas" "Sophia" "Henry" "Charlotte" "Alexander" "Mia" "Sebastian" "Amelia" "Jack" "Harper" "Owen" "Evelyn" "Theodore" "Abigail" "Jacob" "Emily" "Leo" "Elizabeth" "Mason" "Sofia" "Ethan" "Avery" "Noah" "Ella" "Logan" "Scarlett" "Elijah" "Grace" "Oliver" "Chloe" "Aiden" "Victoria" "Gabriel" "Riley" "Samuel" "Aria" "David" "Lily" "Carter" "Aubrey" "Wyatt" "Zoey" "Jayden" "Penelope" "John" "Lillian" "Hunter" "Addison" "Luke" "Layla" "Daniel" "Natalie" "Ryan" "Camila" "Matthew" "Hannah" "Caleb" "Brooklyn" "Isaac" "Samantha" "Nathan" "Anna" "Andrew" "Leah" "Joshua" "Audrey" "Christopher" "Allison" "Anthony" "Savannah" "Dylan" "Gabriella" "Thomas" "Claire" "Charles" "Aaliyah" "Isaiah" "Josephine" "Landon" "Maya" "Connor" "Madelyn" "Adrian" "Kaylee" "Jonathan" "Mackenzie" "Nolan" "Paisley" "Jeremiah" "Brielle" "Easton" "Madison" "Elias" "Kinsley")

LAST_NAMES=("Smith" "Johnson" "Williams" "Brown" "Jones" "Garcia" "Miller" "Davis" "Rodriguez" "Martinez" "Hernandez" "Lopez" "Gonzalez" "Wilson" "Anderson" "Thomas" "Taylor" "Moore" "Jackson" "Martin" "Lee" "Perez" "Thompson" "White" "Harris" "Sanchez" "Clark" "Ramirez" "Lewis" "Robinson" "Walker" "Young" "Allen" "King" "Wright" "Scott" "Torres" "Nguyen" "Hill" "Flores" "Green" "Adams" "Nelson" "Baker" "Hall" "Rivera" "Campbell" "Mitchell" "Carter" "Roberts" "Gomez" "Phillips" "Evans" "Turner" "Diaz" "Parker" "Cruz" "Edwards" "Collins" "Reyes" "Stewart" "Morris" "Morales" "Murphy" "Cook" "Rogers" "Gutierrez" "Ortiz" "Morgan" "Cooper" "Peterson" "Bailey" "Reed" "Kelly" "Howard" "Ramos" "Kim" "Cox" "Ward" "Richardson" "Watson" "Brooks" "Chavez" "Wood" "James" "Bennett" "Gray" "Mendoza" "Ruiz" "Hughes" "Price" "Alvarez" "Castillo" "Sanders" "Patel" "Myers" "Long" "Ross" "Foster" "Jimenez")

DOMAINS=("gmail.com" "outlook.com" "yahoo.com" "hotmail.com" "icloud.com" "protonmail.com")

# Counter for progress tracking
CREATED_COUNT=0
FAILED_COUNT=0

echo "📊 Progress tracking:"
echo "===================="

for i in {1..100}; do
    # Generate random user data
    FIRST_NAME=${FIRST_NAMES[$((RANDOM % ${#FIRST_NAMES[@]}))]}
    LAST_NAME=${LAST_NAMES[$((RANDOM % ${#LAST_NAMES[@]}))]}
    DOMAIN=${DOMAINS[$((RANDOM % ${#DOMAINS[@]}))]}
    
    EMAIL="$(echo "$FIRST_NAME" | tr '[:upper:]' '[:lower:]').$(echo "$LAST_NAME" | tr '[:upper:]' '[:lower:]')$((RANDOM % 999))@${DOMAIN}"
    
    # Generate random avatar using the same approach as portfolio components
    AVATAR_SEED=$((RANDOM % 10000))
    
    echo -n "👤 Creating user $i/100: $FIRST_NAME $LAST_NAME... "
    
    # Create user
    USER_RESPONSE=$(curl -s -X POST "$USER_API_BASE/register" \
        -H "Content-Type: application/json" \
        -d "{
            \"email\": \"$EMAIL\",
            \"firstName\": \"$FIRST_NAME\",
            \"lastName\": \"$LAST_NAME\",
            \"professionalTitle\": \"Software Developer\",
            \"bio\": \"Test user created for portfolio stress testing\",
            \"location\": \"Test City, TC\",
            \"profileImage\": \"https://picsum.photos/150/150?random=$AVATAR_SEED\"
        }")
    
    # Check if user creation was successful
    if [[ $USER_RESPONSE == *"id"* ]] && [[ $USER_RESPONSE != *"error"* ]]; then
        # Extract user ID from response
        USER_ID=$(echo "$USER_RESPONSE" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
        
        if [ ! -z "$USER_ID" ]; then
            echo "✅ Created (ID: ${USER_ID:0:8}...)"
            
            # Immediately create portfolio for this user
            echo "  📁 Creating portfolio for user..."
            
            # Run portfolio creation script with user ID
            PORTFOLIO_OUTPUT=$(bash "$PORTFOLIO_SCRIPT_PATH" "$USER_ID" 2>&1)
            
            if [[ $PORTFOLIO_OUTPUT == *"Portfolio created successfully"* ]]; then
                echo "  ✅ Portfolio created successfully!"
                ((CREATED_COUNT++))
            else
                echo "  ❌ Portfolio creation failed"
                echo "     Error: $PORTFOLIO_OUTPUT"
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
    
    # Small delay to avoid overwhelming the server
    sleep 0.1
    
    # Progress update every 10 users
    if [ $((i % 10)) -eq 0 ]; then
        echo "📈 Progress: $i/100 users processed, $CREATED_COUNT successful, $FAILED_COUNT failed"
        echo ""
    fi
done

echo ""
echo "🎉 Mass Creation Complete!"
echo "=========================="
echo "📊 Final Summary:"
echo "   👥 Total users attempted: 100"
echo "   ✅ Successful creations: $CREATED_COUNT"
echo "   ❌ Failed creations: $FAILED_COUNT"
echo "   📈 Success rate: $(( (CREATED_COUNT * 100) / 100 ))%"
echo ""
echo "🔍 Each successful user has:"
echo "   👤 User account created"
echo "   📁 Test portfolio with 100 projects, 100 experiences, 100 skills, 100 blog posts"
echo ""
echo "🌐 Access portfolios via: http://localhost:3000/portfolio/[PORTFOLIO_ID]"
echo "🔧 Backend APIs running on: User (5200), Portfolio (5201)"
