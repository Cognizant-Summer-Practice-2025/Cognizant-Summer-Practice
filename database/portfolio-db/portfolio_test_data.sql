-- Portfolio Database Test Data Population Script
-- This script populates all tables with sample data for testing controllers

-- Clear existing data (in correct order due to foreign key constraints)
TRUNCATE TABLE bookmarks CASCADE;
TRUNCATE TABLE blog_posts CASCADE;
TRUNCATE TABLE skills CASCADE;
TRUNCATE TABLE experience CASCADE;
TRUNCATE TABLE projects CASCADE;
TRUNCATE TABLE portfolios CASCADE;
TRUNCATE TABLE portfolio_templates CASCADE;

-- Insert Portfolio Templates
INSERT INTO portfolio_templates (id, name, description, component_name, preview_image_url, is_active, created_at, updated_at) VALUES
('11111111-1111-1111-1111-111111111111', 'Gabriel Bârzu', 'Modern minimalist design with clean typography and structured layout', 'GabrielBarzuTemplate', '/templates/gabriel-barzu/preview.jpg', true, NOW(), NOW()),
('22222222-2222-2222-2222-222222222222', 'Modern', 'Clean and minimal design', 'ModernTemplate', '/templates/modern/preview.jpg', true, NOW(), NOW()),
('33333333-3333-3333-3333-333333333333', 'Creative', 'Bold and artistic layout', 'CreativeTemplate', '/templates/creative/preview.jpg', true, NOW(), NOW()),
('44444444-4444-4444-4444-444444444444', 'Professional', 'Corporate and structured', 'ProfessionalTemplate', '/templates/professional/preview.jpg', true, NOW(), NOW());

-- Insert Portfolios
INSERT INTO portfolios (id, user_id, template_id, title, bio, view_count, like_count, visibility, is_published, created_at, updated_at) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'a1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111', 'John Doe - Full Stack Developer', 'Passionate full-stack developer with 5+ years of experience building scalable web applications. Specialized in React, Node.js, and cloud technologies.', 1250, 89, 0, true, NOW() - INTERVAL '30 days', NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'b2222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', 'Jane Smith - UI/UX Designer', 'Creative designer focused on user-centered design and beautiful interfaces. 3 years of experience in digital product design.', 890, 67, 0, true, NOW() - INTERVAL '20 days', NOW()),
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'c3333333-3333-3333-3333-333333333333', '33333333-3333-3333-3333-333333333333', 'Mike Johnson - Business Consultant', 'Strategic business consultant helping companies optimize their operations and growth strategies.', 567, 34, 0, true, NOW() - INTERVAL '15 days', NOW()),
('dddddddd-dddd-dddd-dddd-dddddddddddd', 'd4444444-4444-4444-4444-444444444444', '44444444-4444-4444-4444-444444444444', 'Sarah Wilson - Content Writer', 'Professional content writer and blogger with expertise in technical writing and content strategy.', 234, 12, 0, true, NOW() - INTERVAL '10 days', NOW()),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'e5555555-5555-5555-5555-555555555555', '11111111-1111-1111-1111-111111111111', 'Alex Brown - Mobile Developer', 'iOS and Android developer specializing in cross-platform mobile applications using React Native and Flutter.', 456, 28, 2, false, NOW() - INTERVAL '5 days', NOW());

-- Insert Projects
INSERT INTO projects (id, portfolio_id, title, description, image_url, demo_url, github_url, technologies, featured, created_at, updated_at) VALUES
('f1111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'E-Commerce Platform', 'Full-featured e-commerce platform with payment integration, inventory management, and admin dashboard.', 'https://example.com/projects/ecommerce.jpg', 'https://demo.ecommerce.com', 'https://github.com/johndoe/ecommerce', ARRAY['React', 'Node.js', 'MongoDB', 'Stripe', 'Redux'], true, NOW() - INTERVAL '25 days', NOW()),
('f2222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Task Management App', 'Collaborative task management application with real-time updates and team collaboration features.', 'https://example.com/projects/taskapp.jpg', 'https://demo.taskapp.com', 'https://github.com/johndoe/taskapp', ARRAY['Vue.js', 'Express.js', 'PostgreSQL', 'Socket.io'], true, NOW() - INTERVAL '20 days', NOW()),
('f3333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Weather Dashboard', 'Real-time weather dashboard with forecasts, maps, and weather alerts.', 'https://example.com/projects/weather.jpg', 'https://demo.weather.com', 'https://github.com/johndoe/weather', ARRAY['React', 'TypeScript', 'OpenWeather API', 'Chart.js'], false, NOW() - INTERVAL '15 days', NOW()),
('f4444444-4444-4444-4444-444444444444', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Mobile Banking App Design', 'Complete UI/UX design for a modern mobile banking application with user research and prototyping.', 'https://example.com/projects/banking-design.jpg', 'https://figma.com/banking-app', NULL, ARRAY['Figma', 'Adobe XD', 'Principle', 'InVision'], true, NOW() - INTERVAL '18 days', NOW()),
('f5555555-5555-5555-5555-555555555555', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Restaurant Website Redesign', 'Complete redesign of a restaurant website focusing on user experience and mobile optimization.', 'https://example.com/projects/restaurant.jpg', 'https://restaurant-redesign.com', NULL, ARRAY['Figma', 'HTML/CSS', 'JavaScript', 'GSAP'], true, NOW() - INTERVAL '12 days', NOW()),
('f6666666-6666-6666-6666-666666666666', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Digital Transformation Project', 'Led digital transformation initiative for a Fortune 500 company, improving efficiency by 40%.', 'https://example.com/projects/transformation.jpg', NULL, NULL, ARRAY['Strategy', 'Process Improvement', 'Change Management', 'Analytics'], true, NOW() - INTERVAL '30 days', NOW()),
('f7777777-7777-7777-7777-777777777777', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Technical Documentation Portal', 'Comprehensive technical documentation portal for a SaaS platform with search and analytics.', 'https://example.com/projects/docs.jpg', 'https://docs.example.com', 'https://github.com/sarahwilson/docs', ARRAY['Gatsby', 'MDX', 'Algolia', 'Analytics'], true, NOW() - INTERVAL '8 days', NOW());

-- Insert Experience
INSERT INTO experience (id, portfolio_id, job_title, company_name, start_date, end_date, is_current, description, skills_used, created_at, updated_at) VALUES
('e1111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Senior Full Stack Developer', 'TechCorp Inc.', '2022-01-15', NULL, true, 'Leading development of scalable web applications serving 100K+ users. Mentoring junior developers and architecting cloud-native solutions.', ARRAY['React', 'Node.js', 'AWS', 'PostgreSQL', 'Docker', 'Kubernetes'], NOW() - INTERVAL '25 days', NOW()),
('e2222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Full Stack Developer', 'StartupXYZ', '2020-06-01', '2021-12-31', false, 'Developed and maintained multiple client-facing applications. Implemented CI/CD pipelines and improved deployment efficiency by 60%.', ARRAY['Vue.js', 'Express.js', 'MongoDB', 'Jenkins', 'Git'], NOW() - INTERVAL '25 days', NOW()),
('e3333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Junior Web Developer', 'WebAgency Pro', '2019-03-01', '2020-05-31', false, 'Built responsive websites and web applications for various clients. Collaborated with design team to implement pixel-perfect UIs.', ARRAY['HTML/CSS', 'JavaScript', 'jQuery', 'Bootstrap', 'PHP'], NOW() - INTERVAL '25 days', NOW()),
('e4444444-4444-4444-4444-444444444444', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Senior UI/UX Designer', 'DesignStudio', '2023-02-01', NULL, true, 'Leading design initiatives for mobile and web applications. Conducting user research and creating design systems.', ARRAY['Figma', 'Adobe Creative Suite', 'Principle', 'User Research', 'Design Systems'], NOW() - INTERVAL '18 days', NOW()),
('e5555555-5555-5555-5555-555555555555', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'UI/UX Designer', 'DigitalCorp', '2021-08-01', '2023-01-31', false, 'Designed user interfaces for web and mobile applications. Collaborated with development teams to ensure design implementation.', ARRAY['Sketch', 'InVision', 'Zeplin', 'Prototyping', 'User Testing'], NOW() - INTERVAL '18 days', NOW()),
('e6666666-6666-6666-6666-666666666666', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Senior Business Consultant', 'ConsultingPro', '2021-09-01', NULL, true, 'Providing strategic consulting services to Fortune 500 companies. Specializing in digital transformation and process optimization.', ARRAY['Strategy', 'Process Improvement', 'Change Management', 'Data Analysis', 'Project Management'], NOW() - INTERVAL '15 days', NOW()),
('e7777777-7777-7777-7777-777777777777', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Content Marketing Manager', 'ContentCorp', '2022-03-01', NULL, true, 'Managing content strategy and creation for B2B SaaS company. Increased organic traffic by 150% through SEO and content optimization.', ARRAY['Content Strategy', 'SEO', 'Google Analytics', 'WordPress', 'Social Media'], NOW() - INTERVAL '8 days', NOW());

-- Insert Skills
INSERT INTO skills (id, portfolio_id, name, category, proficiency_level, display_order, created_at, updated_at) VALUES
-- John Doe's Skills
('a1111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'JavaScript', 'Frontend', 5, 1, NOW() - INTERVAL '25 days', NOW()),
('a2222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'React', 'Frontend', 5, 2, NOW() - INTERVAL '25 days', NOW()),
('a3333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Node.js', 'Backend', 4, 3, NOW() - INTERVAL '25 days', NOW()),
('a4444444-4444-4444-4444-444444444444', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'PostgreSQL', 'Database', 4, 4, NOW() - INTERVAL '25 days', NOW()),
('a5555555-5555-5555-5555-555555555555', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'AWS', 'Cloud', 3, 5, NOW() - INTERVAL '25 days', NOW()),
('a6666666-6666-6666-6666-666666666666', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Docker', 'DevOps', 3, 6, NOW() - INTERVAL '25 days', NOW()),
-- Jane Smith's Skills
('a7777777-7777-7777-7777-777777777777', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'UI Design', 'Design', 5, 1, NOW() - INTERVAL '18 days', NOW()),
('a8888888-8888-8888-8888-888888888888', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'UX Research', 'Design', 4, 2, NOW() - INTERVAL '18 days', NOW()),
('a9999999-9999-9999-9999-999999999999', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Figma', 'Tools', 5, 3, NOW() - INTERVAL '18 days', NOW()),
('aa111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Adobe Creative Suite', 'Tools', 4, 4, NOW() - INTERVAL '18 days', NOW()),
('ab222222-2222-2222-2222-222222222222', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Prototyping', 'Design', 4, 5, NOW() - INTERVAL '18 days', NOW()),
-- Mike Johnson's Skills
('ac333333-3333-3333-3333-333333333333', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Strategic Planning', 'Business', 5, 1, NOW() - INTERVAL '15 days', NOW()),
('ad444444-4444-4444-4444-444444444444', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Process Optimization', 'Business', 4, 2, NOW() - INTERVAL '15 days', NOW()),
('ae555555-5555-5555-5555-555555555555', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Data Analysis', 'Analytics', 4, 3, NOW() - INTERVAL '15 days', NOW()),
('af666666-6666-6666-6666-666666666666', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Project Management', 'Management', 5, 4, NOW() - INTERVAL '15 days', NOW()),
-- Sarah Wilson's Skills
('a0777777-7777-7777-7777-777777777777', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Content Writing', 'Writing', 5, 1, NOW() - INTERVAL '8 days', NOW()),
('a1888888-8888-8888-8888-888888888888', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'SEO', 'Marketing', 4, 2, NOW() - INTERVAL '8 days', NOW()),
('a2999999-9999-9999-9999-999999999999', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Technical Writing', 'Writing', 4, 3, NOW() - INTERVAL '8 days', NOW()),
('a3111111-1111-1111-1111-111111111111', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Content Strategy', 'Strategy', 4, 4, NOW() - INTERVAL '8 days', NOW());

-- Insert Blog Posts
INSERT INTO blog_posts (id, portfolio_id, title, excerpt, content, featured_image_url, tags, is_published, published_at, created_at, updated_at) VALUES
('b1111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Building Scalable React Applications', 'Learn how to build React applications that can handle thousands of users with proper architecture and optimization techniques.', 'In this comprehensive guide, we''ll explore the key principles of building scalable React applications. From component architecture to state management, performance optimization, and deployment strategies...', 'https://example.com/blog/react-scalable.jpg', ARRAY['React', 'JavaScript', 'Performance', 'Architecture'], true, NOW() - INTERVAL '10 days', NOW() - INTERVAL '12 days', NOW() - INTERVAL '10 days'),
('b2222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Microservices with Node.js', 'A deep dive into building microservices architecture using Node.js and Docker containers.', 'Microservices architecture has become increasingly popular for building scalable and maintainable applications. In this article, we''ll explore how to implement microservices using Node.js...', 'https://example.com/blog/microservices.jpg', ARRAY['Node.js', 'Microservices', 'Docker', 'Architecture'], true, NOW() - INTERVAL '5 days', NOW() - INTERVAL '7 days', NOW() - INTERVAL '5 days'),
('b3333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'AWS Deployment Best Practices', 'Learn the best practices for deploying applications to AWS with security, scalability, and cost optimization in mind.', 'Deploying applications to AWS can be complex, but following best practices can help ensure your applications are secure, scalable, and cost-effective...', 'https://example.com/blog/aws-deployment.jpg', ARRAY['AWS', 'DevOps', 'Deployment', 'Cloud'], false, NULL, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days'),
('b4444444-4444-4444-4444-444444444444', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Design Systems for Scalable Teams', 'How to create and maintain design systems that enable design and development teams to work efficiently.', 'Design systems are crucial for maintaining consistency and efficiency in product development. This article covers the essential components of a successful design system...', 'https://example.com/blog/design-systems.jpg', ARRAY['Design Systems', 'UI/UX', 'Team Collaboration', 'Efficiency'], true, NOW() - INTERVAL '8 days', NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),
('b5555555-5555-5555-5555-555555555555', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'User Research Methods for Better UX', 'Explore different user research methods and how to apply them to improve user experience.', 'User research is the foundation of great user experience design. In this post, we''ll explore various research methods and when to use them...', 'https://example.com/blog/user-research.jpg', ARRAY['User Research', 'UX', 'Methods', 'Design Process'], true, NOW() - INTERVAL '15 days', NOW() - INTERVAL '17 days', NOW() - INTERVAL '15 days'),
('b6666666-6666-6666-6666-666666666666', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Content Marketing Strategy for SaaS', 'A comprehensive guide to building an effective content marketing strategy for SaaS companies.', 'Content marketing is essential for SaaS companies to attract and retain customers. This guide covers everything from content planning to distribution...', 'https://example.com/blog/content-marketing.jpg', ARRAY['Content Marketing', 'SaaS', 'Strategy', 'Growth'], true, NOW() - INTERVAL '6 days', NOW() - INTERVAL '8 days', NOW() - INTERVAL '6 days'),
('b7777777-7777-7777-7777-777777777777', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Technical Writing Best Practices', 'Learn how to write clear, concise, and effective technical documentation.', 'Technical writing is a crucial skill for developers and technical professionals. This article covers best practices for writing documentation that actually helps users...', 'https://example.com/blog/technical-writing.jpg', ARRAY['Technical Writing', 'Documentation', 'Communication', 'Best Practices'], true, NOW() - INTERVAL '12 days', NOW() - INTERVAL '14 days', NOW() - INTERVAL '12 days');

-- Insert Bookmarks
INSERT INTO bookmarks (id, user_id, portfolio_id, collection_name, notes, created_at) VALUES
('c1111111-1111-1111-1111-111111111111', 'b2222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Inspiration', 'Great full-stack developer portfolio with excellent project showcases', NOW() - INTERVAL '5 days'),
('c2222222-2222-2222-2222-222222222222', 'c3333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Developer Profiles', 'Impressive technical skills and project variety', NOW() - INTERVAL '8 days'),
('c3333333-3333-3333-3333-333333333333', 'd4444444-4444-4444-4444-444444444444', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Tech Inspiration', 'Love the clean design and technical blog posts', NOW() - INTERVAL '12 days'),
('c4444444-4444-4444-4444-444444444444', 'a1111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Design References', 'Excellent UI/UX work and design process documentation', NOW() - INTERVAL '3 days'),
('c5555555-5555-5555-5555-555555555555', 'c3333333-3333-3333-3333-333333333333', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Creative Work', 'Beautiful design portfolio with great case studies', NOW() - INTERVAL '7 days'),
('c6666666-6666-6666-6666-666666666666', 'e5555555-5555-5555-5555-555555555555', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Design Inspiration', 'Great example of designer portfolio structure', NOW() - INTERVAL '10 days'),
('c7777777-7777-7777-7777-777777777777', 'a1111111-1111-1111-1111-111111111111', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Business Profiles', 'Professional consultant portfolio with good case studies', NOW() - INTERVAL '6 days'),
('c8888888-8888-8888-8888-888888888888', 'b2222222-2222-2222-2222-222222222222', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Writing Samples', 'Excellent technical writing and content strategy examples', NOW() - INTERVAL '4 days'),
('c9999999-9999-9999-9999-999999999999', 'd4444444-4444-4444-4444-444444444444', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'Consulting', 'Good business consulting portfolio structure', NOW() - INTERVAL '9 days'),
('c0000000-0000-0000-0000-000000000000', 'e5555555-5555-5555-5555-555555555555', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'Content Strategy', 'Great content marketing insights and case studies', NOW() - INTERVAL '11 days');