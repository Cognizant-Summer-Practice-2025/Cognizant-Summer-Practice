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

USER_API_BASE = "http://localhost:5200/api/Users"
PORTFOLIO_SCRIPT_PATH = "./generate-portfolio-test-data.py"

# Test user data
USERS = [
    {"full_name": "TestUser1", "email": "test1@example.com"},
    {"full_name": "TestUser2", "email": "test2@example.com"}
]

def create_user(first_name, last_name, email):
    data = {
        "email": email,
        "firstName": first_name,
        "lastName": last_name,
        "professionalTitle": "Software Developer",
        "bio": "Test user created for portfolio testing",
        "location": "Test City, TC"
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
    for i, user in enumerate(USERS):
        full_name = user["full_name"]
        email = user["email"]
        first_name = full_name.split()[0]
        last_name = "User"
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
