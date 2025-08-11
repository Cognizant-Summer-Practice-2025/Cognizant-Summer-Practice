#!/usr/bin/env python3
"""
Test Script - Create 2 users with portfolios (Python version)
This is a smaller version to test the system before running the full 100
"""

import json
import requests
import subprocess
import sys
import time
import random

USER_API_BASE = "http://localhost:5200/api/Users"
PORTFOLIO_SCRIPT_PATH = "./generate-portfolio-test-data.py"

# Test data arrays for generating realistic random data
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

def generate_random_user_data():
    """Generate random user data"""
    first_name = random.choice(FIRST_NAMES)
    last_name = random.choice(LAST_NAMES)
    domain = random.choice(DOMAINS)
    
    email = f"{first_name.lower()}.{last_name.lower()}{random.randint(1, 999)}@{domain}"
    
    return first_name, last_name, email

def create_user(first_name, last_name, email):
    # Generate random avatar using the same approach as portfolio components
    avatar_seed = random.randint(1, 10000)
    
    data = {
        "email": email,
        "firstName": first_name,
        "lastName": last_name,
        "professionalTitle": "Software Developer",
        "bio": "Test user created for portfolio testing",
        "location": "Test City, TC",
        "profileImage": f"https://picsum.photos/150/150?random={avatar_seed}"
    }
    try:
        response = requests.post(f"{USER_API_BASE}/register", json=data, timeout=10)
        response.raise_for_status()
        user_response = response.json()
        user_id = user_response.get("id")
        return user_id, user_response
    except Exception as e:
        return None, str(e)

def create_portfolio_for_user(user_id):
    try:
        result = subprocess.run([
            sys.executable, PORTFOLIO_SCRIPT_PATH, user_id
        ], capture_output=True, text=True, timeout=120)
        if result.returncode == 0:
            return True, result.stdout
        else:
            return False, result.stderr or result.stdout
    except Exception as e:
        return False, str(e)

def main():
    print("Testing User and Portfolio Creation (2 users)")
    print("===============================================")
    created_count = 0
    failed_count = 0
    
    for i in range(2):
        first_name, last_name, email = generate_random_user_data()
        full_name = f"{first_name} {last_name}"
        
        print(f"Creating user {i+1}/2: {full_name}...")
        user_id, user_response = create_user(first_name, last_name, email)
        print(f"Response: {user_response}")
        if user_id:
            print(f"User created successfully (ID: {user_id})")
            print(f"Creating portfolio for user {user_id}...")
            success, output = create_portfolio_for_user(user_id)
            if success:
                print("Portfolio created successfully!")
                created_count += 1
            else:
                print(f"Portfolio creation failed: {output}")
                failed_count += 1
        else:
            print(f"Failed to create user: {user_response}")
            failed_count += 1
        print("---")
        time.sleep(0.1)
    print("\nTest Results:")
    print("===============")
    print(f"Successful: {created_count}")
    print(f"Failed: {failed_count}")
    if created_count > 0:
        print("\nTest successful! You can now run the full script:")
        print("   ./create-100-users-with-portfolios.sh")
        print("   OR")
        print("   python3 create-100-users-with-portfolios.py")

if __name__ == "__main__":
    main()
