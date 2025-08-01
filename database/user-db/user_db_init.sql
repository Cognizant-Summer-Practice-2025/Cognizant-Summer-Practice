-- Enum values stored as integers for Entity Framework compatibility
-- oauth_provider_type: 0=Google, 1=GitHub, 2=LinkedIn, 3=Facebook
-- reported_type: 0=User, 1=Portfolio, 2=Message, 3=BlogPost, 4=Comment
-- report_type: 0=Spam, 1=Harassment, 2=InappropriateContent, 3=FakeProfile, 4=Copyright, 5=Other
-- report_status: 0=Pending, 1=UnderReview, 2=Resolved, 3=Dismissed

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    professional_title VARCHAR(200),
    bio TEXT,
    location VARCHAR(100),
    avatar_url TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_admin BOOLEAN NOT NULL DEFAULT FALSE,
    last_login_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- OAuth Providers table
CREATE TABLE oauth_providers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    provider INTEGER NOT NULL,
    provider_id VARCHAR(255) NOT NULL,
    provider_email VARCHAR(255) NOT NULL,
    access_token TEXT NOT NULL,
    refresh_token TEXT,
    token_expires_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(provider, provider_id)
);

-- Newsletters table
CREATE TABLE newsletters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    type VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, type)
);

-- User Analytics table
CREATE TABLE user_analytics (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    session_id VARCHAR(255) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB DEFAULT '{}',
    ip_address INET,
    user_agent TEXT,
    referrer_url TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- User Reports table
CREATE TABLE user_reports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reporter_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    resolved_by UUID REFERENCES users(id) ON DELETE SET NULL,
    reported_service VARCHAR(50) NOT NULL,
    reported_type INTEGER NOT NULL,
    reported_id UUID NOT NULL,
    report_type INTEGER NOT NULL,
    description TEXT NOT NULL,
    status INTEGER NOT NULL DEFAULT 0,
    admin_notes TEXT,
    resolved_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Bookmarks table
CREATE TABLE bookmarks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    portfolio_id VARCHAR(255) NOT NULL,
    portfolio_title VARCHAR(255),
    portfolio_owner_name VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, portfolio_id)
);

-- Create indexes for performance
CREATE INDEX idx_users_created_at ON users(created_at);
CREATE INDEX idx_users_is_active ON users(is_active);
CREATE INDEX idx_oauth_providers_user_id ON oauth_providers(user_id);
CREATE INDEX idx_newsletters_user_id ON newsletters(user_id);
CREATE INDEX idx_user_analytics_user_id ON user_analytics(user_id);
CREATE INDEX idx_user_analytics_session_id ON user_analytics(session_id);
CREATE INDEX idx_user_analytics_created_at ON user_analytics(created_at);
CREATE INDEX idx_user_reports_reporter_id ON user_reports(reporter_id);
CREATE INDEX idx_user_reports_reported_service_id ON user_reports(reported_service, reported_id);
CREATE INDEX idx_user_reports_status ON user_reports(status);
CREATE INDEX idx_bookmarks_user_id ON bookmarks(user_id);
CREATE INDEX idx_bookmarks_created_at ON bookmarks(created_at);

-- Insert sample users for testing
INSERT INTO users (id, email, username, first_name, last_name, professional_title, bio, location, avatar_url, is_active, is_admin) VALUES 
(gen_random_uuid(), 'john.doe@example.com', 'johndoe', 'John', 'Doe', 'Software Engineer', 'Passionate full-stack developer with 5 years of experience.', 'San Francisco, CA', 'https://placehold.co/100x100', true, false),
(gen_random_uuid(), 'jane.smith@example.com', 'janesmith', 'Jane', 'Smith', 'UX Designer', 'Creative designer focused on user experience and interface design.', 'New York, NY', 'https://placehold.co/100x100', true, false),
(gen_random_uuid(), 'bob.wilson@example.com', 'bobwilson', 'Bob', 'Wilson', 'DevOps Engineer', 'Infrastructure and automation specialist.', 'Austin, TX', 'https://placehold.co/100x100', true, false),
(gen_random_uuid(), 'alice.brown@example.com', 'alicebrown', 'Alice', 'Brown', 'Product Manager', 'Product strategy and roadmap planning expert.', 'Seattle, WA', 'https://placehold.co/100x100', true, false),
(gen_random_uuid(), 'mike.davis@example.com', 'mikedavis', 'Mike', 'Davis', 'Data Scientist', 'Machine learning and analytics professional.', 'Boston, MA', 'https://placehold.co/100x100', true, false),
(gen_random_uuid(), 'sarah.johnson@example.com', 'sarahjohnson', 'Sarah', 'Johnson', 'Frontend Developer', 'React and Vue.js specialist with design background.', 'Los Angeles, CA', 'https://placehold.co/100x100', true, false),
(gen_random_uuid(), 'david.lee@example.com', 'davidlee', 'David', 'Lee', 'Backend Developer', 'API design and microservices architecture expert.', 'Chicago, IL', 'https://placehold.co/100x100', true, false),
(gen_random_uuid(), 'emily.taylor@example.com', 'emilytaylor', 'Emily', 'Taylor', 'QA Engineer', 'Test automation and quality assurance specialist.', 'Denver, CO', 'https://placehold.co/100x100', true, false);

-- Success message
\echo 'User service database initialized successfully!'
\echo 'Created tables: users, oauth_providers, newsletters, user_analytics, user_reports, bookmarks' 
\echo 'Inserted 8 sample users for testing' 