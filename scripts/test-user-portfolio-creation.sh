#!/bin/bash

# Test Script - Create 2 users with portfolios
# This is a smaller version to test the system before running the full 100

USER_API_BASE="http://localhost:5200/api/Users"
PORTFOLIO_SCRIPT_PATH="/Users/theo/Documents/Cognizant-Summer-Practice/scripts/generate-portfolio-test-data.sh"

echo "🧪 Testing User and Portfolio Creation (2 users)"
echo "==============================================="

# Test user data
USERS=(
    "TestUser1:test1@example.com"
    "TestUser2:test2@example.com"
)

CREATED_COUNT=0
FAILED_COUNT=0

for i in {0..1}; do
    IFS=':' read -r -a user_data <<< "${USERS[$i]}"
    FULL_NAME="${user_data[0]}"
    EMAIL="${user_data[1]}"
    
    # Split first and last name
    FIRST_NAME=$(echo "$FULL_NAME" | cut -d' ' -f1)
    LAST_NAME="User"
    
    echo "👤 Creating user $((i+1))/2: $FULL_NAME..."
    
    # Create user
    USER_RESPONSE=$(curl -s -X POST "$USER_API_BASE/register" \
        -H "Content-Type: application/json" \
        -d "{
            \"email\": \"$EMAIL\",
            \"firstName\": \"$FIRST_NAME\",
            \"lastName\": \"$LAST_NAME\",
            \"professionalTitle\": \"Software Developer\",
            \"bio\": \"Test user created for portfolio testing\",
            \"location\": \"Test City, TC\"
        }")
    
    echo "Response: $USER_RESPONSE"
    
    # Check if user creation was successful
    if [[ $USER_RESPONSE == *"id"* ]] && [[ $USER_RESPONSE != *"error"* ]]; then
        # Extract user ID from response
        USER_ID=$(echo "$USER_RESPONSE" | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
        
        if [ ! -z "$USER_ID" ]; then
            echo "✅ User created successfully (ID: $USER_ID)"
            
            # Create portfolio for this user
            echo "📁 Creating portfolio for user $USER_ID..."
            
            # Run portfolio creation script with user ID
            bash "$PORTFOLIO_SCRIPT_PATH" "$USER_ID"
            
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
    echo "   ./create-100-users-with-portfolios.sh"
    echo "   OR"
    echo "   python3 create-100-users-with-portfolios.py"
fi
