#!/usr/bin/env python3
"""
Test Script - Create 2 users with portfolios (Python version)
Adds Bearer token support and realistic random fields.
"""

import argparse
import json
import os
import random
import subprocess
import sys
import time

import requests

USER_API_BASE = "http://localhost:5200/api/Users"
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PORTFOLIO_SCRIPT_PATH = os.path.join(SCRIPT_DIR, "generate-portfolio-test-data.py")

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
TITLES = ["Software Engineer", "Full Stack Developer", "Backend Developer", "Frontend Engineer", "Data Engineer", "DevOps Engineer"]
LOCATIONS = ["New York, NY", "San Francisco, CA", "Austin, TX", "Seattle, WA", "Boston, MA", "Chicago, IL", "London, UK"]
BIOS = [
    "Passionate engineer building scalable systems and delightful user experiences.",
    "Developer focused on clean architecture, testing, and performance.",
    "Curious technologist solving complex problems with simple solutions.",
]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Test creation of users and portfolios")
    parser.add_argument("--token", default=os.getenv("AUTH_TOKEN"), help="Bearer token for API auth")
    parser.add_argument("--dataset", default=os.path.join(SCRIPT_DIR, "random_data.json"), help="Optional dataset JSON path")
    return parser.parse_args()


def headers(token: str | None):
    h = {"Content-Type": "application/json"}
    if token:
        h["Authorization"] = f"Bearer {token}"
    return h


def generate_random_user_data():
    first_name = random.choice(FIRST_NAMES)
    last_name = random.choice(LAST_NAMES)
    domain = random.choice(DOMAINS)
    email = f"{first_name.lower()}.{last_name.lower()}{random.randint(1, 9999)}@{domain}"
    title = random.choice(TITLES)
    bio = random.choice(BIOS)
    location = random.choice(LOCATIONS)
    avatar_seed = random.randint(1, 10000)
    return {
        "email": email,
        "firstName": first_name,
        "lastName": last_name,
        "professionalTitle": title,
        "bio": bio,
        "location": location,
        "profileImage": f"https://picsum.photos/150/150?random={avatar_seed}",
    }


def create_user(user_data, token: str | None):
    try:
        resp = requests.post(f"{USER_API_BASE}/register", json=user_data, headers=headers(token), timeout=15)
        resp.raise_for_status()
        data = resp.json()
        return data.get("id"), data
    except Exception as e:
        return None, str(e)


def create_portfolio_for_user(user_id: str, token: str | None, dataset_path: str | None):
    try:
        cmd = [sys.executable, PORTFOLIO_SCRIPT_PATH, user_id]
        if token:
            cmd.extend(["--token", token])
        if dataset_path:
            cmd.extend(["--dataset", dataset_path])
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=180)
        return (result.returncode == 0), (result.stdout or result.stderr)
    except Exception as e:
        return False, str(e)


def main():
    args = parse_args()
    print("Testing User and Portfolio Creation (2 users)")
    print("===============================================")
    created_count = 0
    failed_count = 0

    for i in range(2):
        user_data = generate_random_user_data()
        full_name = f"{user_data['firstName']} {user_data['lastName']}"
        print(f"Creating user {i+1}/2: {full_name}...")
        user_id, user_response = create_user(user_data, args.token)
        print(f"Response: {user_response}")
        if user_id:
            print(f"User created successfully (ID: {user_id})")
            print(f"Creating portfolio for user {user_id}...")
            success, output = create_portfolio_for_user(user_id, args.token, args.dataset)
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
        print("   ./create-100-users-with-portfolios.sh --token $AUTH_TOKEN")
        print("   OR")
        print("   python3 create-100-users-with-portfolios.py --token $AUTH_TOKEN")


if __name__ == "__main__":
    main()
