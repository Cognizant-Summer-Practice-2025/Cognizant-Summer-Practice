-- Create ENUMs for User Service
CREATE TYPE oauth_provider_type AS ENUM ('Google', 'GitHub', 'LinkedIn', 'Facebook');
CREATE TYPE reported_type AS ENUM ('User', 'Portfolio', 'Message', 'BlogPost', 'Comment');
CREATE TYPE report_type AS ENUM ('Spam', 'Harassment', 'InappropriateContent', 'FakeProfile', 'Copyright', 'Other');
CREATE TYPE report_status AS ENUM ('Pending', 'UnderReview', 'Resolved', 'Dismissed');

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
    last_login_at TIMESTAMP
);

-- OAuth Providers table
CREATE TABLE oauth_providers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    provider oauth_provider_type NOT NULL,
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
    reported_type reported_type NOT NULL,
    reported_id UUID NOT NULL,
    report_type report_type NOT NULL,
    description TEXT NOT NULL,
    status report_status NOT NULL DEFAULT 'Pending',
    admin_notes TEXT,
    resolved_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create indexes for performance
CREATE INDEX idx_oauth_providers_user_id ON oauth_providers(user_id);
CREATE INDEX idx_newsletters_user_id ON newsletters(user_id);
CREATE INDEX idx_user_analytics_user_id ON user_analytics(user_id);
CREATE INDEX idx_user_analytics_session_id ON user_analytics(session_id);
CREATE INDEX idx_user_analytics_created_at ON user_analytics(created_at);
CREATE INDEX idx_user_reports_reporter_id ON user_reports(reporter_id);
CREATE INDEX idx_user_reports_reported_service_id ON user_reports(reported_service, reported_id);
CREATE INDEX idx_user_reports_status ON user_reports(status);

-- Success message
\echo 'User service database initialized successfully!'
\echo 'Created tables: users, oauth_providers, newsletters, user_analytics, user_reports' 