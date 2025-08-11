#!/usr/bin/env python3
"""
Mass User and Portfolio Creation Script
Creates 100 users and immediately creates a test portfolio for each one
"""

import json
import requests
import random
import subprocess
import sys
import time
from typing import List, Dict, Any

USER_API_BASE = "http://localhost:5200/api/Users"
PORTFOLIO_SCRIPT_PATH = "./generate-portfolio-test-data.py"

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

def generate_user_data() -> Dict[str, str]:
    """Generate random user registration data"""
    first_name = random.choice(FIRST_NAMES)
    last_name = random.choice(LAST_NAMES)
    domain = random.choice(DOMAINS)
    
    email = f"{first_name.lower()}.{last_name.lower()}{random.randint(1, 999)}@{domain}"
    username = f"{first_name.lower()}{last_name.lower()}{random.randint(1, 999)}"
    
    # Generate random avatar using the same approach as portfolio components
    avatar_seed = random.randint(1, 10000)
    
    return {
        "email": email,
        "firstName": first_name,
        "lastName": last_name,
        "professionalTitle": "Software Developer",
        "bio": "Test user created for portfolio stress testing",
        "location": "Test City, TC",
        "profileImage": f"https://picsum.photos/150/150?random={avatar_seed}"
    }

def create_user(user_data: Dict[str, str]) -> tuple[bool, str]:
    """Create a user and return success status and user ID"""
    try:
        response = requests.post(f"{USER_API_BASE}/register", json=user_data, timeout=10)
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

def create_portfolio_for_user(user_id: str) -> tuple[bool, str]:
    """Create a portfolio for the given user ID"""
    try:
        result = subprocess.run(
            [sys.executable, PORTFOLIO_SCRIPT_PATH, user_id],
            capture_output=True,
            text=True,
            timeout=120  # 2 minute timeout for portfolio creation
        )
        
        if result.returncode == 0 and "Portfolio created successfully" in result.stdout:
            return True, "Portfolio created successfully"
        else:
            error_msg = result.stderr if result.stderr else result.stdout
            return False, f"Portfolio creation failed: {error_msg}"
            
    except subprocess.TimeoutExpired:
        return False, "Portfolio creation timed out (>2 minutes)"
    except Exception as e:
        return False, f"Error running portfolio script: {str(e)}"

def main():
    """Main function to orchestrate mass user and portfolio creation"""
    print("Starting Mass User and Portfolio Creation (100 users)")
    print("=" * 56)
    
    from concurrent.futures import ThreadPoolExecutor, as_completed
    created_count = 0
    failed_count = 0
    created_users = []
    results = []
    print("Progress tracking:")
    print("=" * 20)

    def create_user_and_portfolio(i):
        user_data = generate_user_data()
        print(f"Creating user {i}/100: {user_data['firstName']} {user_data['lastName']}... ", end="")
        user_success, user_result = create_user(user_data)
        if user_success:
            user_id = user_result
            print(f"Created (ID: {user_id[:8]}...)")
            print("  Creating portfolio for user... ", end="")
            portfolio_success, portfolio_result = create_portfolio_for_user(user_id)
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

    with ThreadPoolExecutor(max_workers=10) as executor:
        future_to_index = {executor.submit(create_user_and_portfolio, i): i for i in range(1, 101)}
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
            if idx % 10 == 0:
                print(f"Progress: {idx}/100 users processed, {created_count} successful, {failed_count} failed\n")

    # Final summary
    print("\nMass Creation Complete!")
    print("=" * 26)
    print("Final Summary:")
    print(f"   Total users attempted: 100")
    print(f"   Successful creations: {created_count}")
    print(f"   Failed creations: {failed_count}")
    print(f"   Success rate: {(created_count * 100) // 100}%")
    print()
    print("Each successful user has:")
    print("   User account created")
    print("   Test portfolio with 100 projects, 100 experiences, 100 skills, 100 blog posts")
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
