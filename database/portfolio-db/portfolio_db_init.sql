-- Enum values stored as integers for Entity Framework compatibility
-- visibility: 0=Public, 1=Private, 2=Unlisted

-- Simplified Portfolio Templates table
CREATE TABLE portfolio_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    preview_image_url TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Simplified Portfolios table
CREATE TABLE portfolios (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    template_id UUID NOT NULL REFERENCES portfolio_templates(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    bio TEXT,
    view_count INTEGER NOT NULL DEFAULT 0,
    like_count INTEGER NOT NULL DEFAULT 0,
    visibility INTEGER NOT NULL DEFAULT 0,
    is_published BOOLEAN NOT NULL DEFAULT FALSE,
    components TEXT, -- JSON array of component configurations
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Projects table
CREATE TABLE projects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    portfolio_id UUID NOT NULL REFERENCES portfolios(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    image_url TEXT,
    demo_url TEXT,
    github_url TEXT,
    technologies TEXT[],
    featured BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Experience table
CREATE TABLE experience (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    portfolio_id UUID NOT NULL REFERENCES portfolios(id) ON DELETE CASCADE,
    job_title VARCHAR(255) NOT NULL,
    company_name VARCHAR(255) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE,
    is_current BOOLEAN NOT NULL DEFAULT FALSE,
    description TEXT,
    skills_used TEXT[],
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Skills table with hierarchical categories
CREATE TABLE skills (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    portfolio_id UUID NOT NULL REFERENCES portfolios(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    category_type VARCHAR(50), -- 'hard_skills' or 'soft_skills'
    subcategory VARCHAR(100), -- 'frontend', 'backend', 'communication', etc.
    category VARCHAR(255), -- Full category path for display (deprecated but kept for backward compatibility)
    proficiency_level INTEGER,
    display_order INTEGER,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Blog Posts table
CREATE TABLE blog_posts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    portfolio_id UUID NOT NULL REFERENCES portfolios(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    excerpt TEXT,
    content TEXT,
    featured_image_url TEXT,
    tags TEXT[],
    is_published BOOLEAN NOT NULL DEFAULT FALSE,
    published_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Bookmarks table
CREATE TABLE bookmarks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    portfolio_id UUID NOT NULL REFERENCES portfolios(id) ON DELETE CASCADE,
    collection_name VARCHAR(100) DEFAULT 'General',
    notes TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, portfolio_id)
);

-- Create indexes for performance
CREATE INDEX idx_portfolios_user_id ON portfolios(user_id);
CREATE INDEX idx_portfolios_template_id ON portfolios(template_id);
CREATE INDEX idx_portfolios_visibility ON portfolios(visibility);
CREATE INDEX idx_portfolios_is_published ON portfolios(is_published);
CREATE INDEX idx_projects_portfolio_id ON projects(portfolio_id);
CREATE INDEX idx_experience_portfolio_id ON experience(portfolio_id);
CREATE INDEX idx_skills_portfolio_id ON skills(portfolio_id);
CREATE INDEX idx_skills_category_type ON skills(category_type);
CREATE INDEX idx_skills_subcategory ON skills(subcategory);
CREATE INDEX idx_blog_posts_portfolio_id ON blog_posts(portfolio_id);
CREATE INDEX idx_blog_posts_is_published ON blog_posts(is_published);
CREATE INDEX idx_bookmarks_user_id ON bookmarks(user_id);
CREATE INDEX idx_bookmarks_portfolio_id ON bookmarks(portfolio_id);

-- Insert default portfolio templates
INSERT INTO portfolio_templates (name, description, preview_image_url) VALUES
('Gabriel Bârzu', 'Modern minimalist design with clean typography and structured layout', '/templates/gabriel-barzu/preview.jpg'),
('Modern', 'Clean and minimal design', '/templates/modern/preview.jpg'),
('Creative', 'Bold and artistic layout', '/templates/creative/preview.jpg'),
('Professional', 'Corporate and structured', '/templates/professional/preview.jpg');
