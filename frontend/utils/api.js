const API_URL_USER = "http://localhost:5200/api/users";

// Get user by email
export const getUserByEmail = async (email) => {
    try {
        const response = await fetch(`${API_URL_USER}/email/${email}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching user by email:', error);
        throw error;
    }
};

// Create a new user
export const createUser = async (userData) => {
    try {
        const response = await fetch(API_URL_USER, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData),
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Error creating user:', error);
        throw error;
    }
};
