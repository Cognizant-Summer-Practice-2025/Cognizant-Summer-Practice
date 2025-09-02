# GoalKeeper - Professional Portfolio Management Platform

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-15-black?logo=next.js&logoColor=white)](https://nextjs.org/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-24-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

> **A sophisticated, enterprise-grade portfolio management ecosystem built with modern microservices architecture, featuring AI-powered portfolio ranking, real-time messaging, and comprehensive administrative oversight.**

---



## Table of Contents

### [1. Overview](#1-overview)
### [2. System Architecture](#2-system-architecture)
### [3. Key Features](#3-key-features)
### [4. Technology Stack](#4-technology-stack)
### [5. Project Structure](#5-project-structure)
### [6. Services Overview](#6-services-overview)
	- [6.1 Backend Services](#61-backend-services)
	- [6.2 Frontend Services](#62-frontend-services)
### [7. Quick Start](#7-quick-start)
### [9. Documentation](#9-documentation)
### [10. Security Features](#10-security-features)
### [11. Mobile Responsiveness](#11-mobile-responsiveness)
### [12. AI Integration](#12-ai-integration)
### [13. Performance & Scalability](#13-performance--scalability)
### [14. Testing Strategy](#14-testing-strategy)
### [15. Deployment](#15-deployment)
### [16. Contributing](#16-contributing)
### [17. License](#17-license)

---

## 1. Overview

**GoalKeeper** is a comprehensive, enterprise-grade portfolio management platform that enables professionals to showcase their skills, projects, and achievements through customizable portfolios. Built with a modern microservices architecture, the platform provides a seamless experience for portfolio creation, discovery, and professional networking.

### **🎯 Core Mission**
Transform how professionals present themselves online by providing a sophisticated, AI-powered platform that combines beautiful design, intelligent content management, and real-time communication capabilities.

### **🌟 Key Value Propositions**
- **Professional Showcase**: Create stunning, customizable portfolios that stand out
- **AI-Powered Discovery**: Intelligent portfolio ranking and recommendation system
- **Real-Time Communication**: Built-in messaging system for professional networking
- **Enterprise-Grade Security**: OAuth2 authentication with client-side encryption
- **Mobile-First Design**: Responsive design that works perfectly on all devices
- **Comprehensive Admin Tools**: Advanced administrative oversight and analytics

---

## 🎥 Live Presentation

View the live presentation here: https://s.go.ro/anrbg2aj

---

## 2. System Architecture

### **🏛️ Microservices Architecture**
The platform is built using a sophisticated microservices architecture that provides:
- **Scalability**: Independent scaling of services based on demand
- **Maintainability**: Clear separation of concerns and modular design
- **Flexibility**: Technology-agnostic service implementation
- **Reliability**: Fault isolation and independent deployment

### **🔄 Service Communication Patterns**
- **Synchronous**: HTTP-based API calls for direct service-to-service communication
- **Asynchronous**: Event-driven patterns for non-blocking operations
- **Real-time**: SignalR integration for live communication features
- **Cross-Service**: Seamless integration between frontend and backend services

### **🔐 Security Architecture**
- **Unified Authentication**: Consistent JWT token validation across all services
- **OAuth2 Integration**: Multi-provider authentication (Google, GitHub, Facebook, LinkedIn)
- **Client-Side Encryption**: AES encryption with PBKDF2 key derivation
- **Role-Based Access Control**: Granular permissions and administrative oversight

---

## 3. Key Features

### **📝 Portfolio Management**
- ✅ **Personal Blog-Portfolio Creation**: Each user can create and manage their own personal blog to showcase projects, skills, and achievements
- ✅ **Customizable Portfolio Templates**: Let users choose or customize templates for their portfolios
- ✅ **Advanced Content Management**: Comprehensive portfolio, project, skill, and blog post management
- ✅ **Image Management**: Advanced image upload, processing, and optimization capabilities

### **🏠 Portfolio Discovery**
- ✅ **Home Page with All Portfolios**: Main page displays a list of all user portfolios for easy browsing and discovery
- ✅ **Advanced Search and Filtering**: Search and filter portfolios by skills, technologies, location, or tags
- ✅ **AI-Powered Ranking**: AI automatically selects and highlights the top 10 portfolios based on quality and engagement
- ✅ **Bookmark/Favorite Portfolios**: Users can bookmark portfolios they like for quick access later

### **🔐 Authentication & Security**
- ✅ **OAuth Login for Publishing**: Users can securely log in using OAuth (Google, Facebook, GitHub, LinkedIn) to publish and update portfolios
- ✅ **Multi-Provider Support**: Comprehensive OAuth2 implementation with automatic token refresh
- ✅ **Client-Side Encryption**: Secure data transmission with AES encryption
- ✅ **Middleware Protection**: Route protection and authentication validation

### **💬 Communication & Networking**
- ✅ **Messaging System**: Allow users to send and receive private messages within the platform
- ✅ **Real-Time Communication**: Instant message delivery using SignalR WebSocket connections
- ✅ **Conversation Management**: Comprehensive conversation creation, management, and organization
- ✅ **User Search & Discovery**: Advanced user search capabilities for finding and starting conversations

### **📊 Administration & Management**
- ✅ **Admin Panel for Managing Users/Blogs**: Dedicated panel to manage user accounts and oversee all blog content
- ✅ **Comprehensive Dashboard**: Real-time statistics, analytics charts, and platform overview
- ✅ **User Management**: Complete user lifecycle management including creation, modification, and deletion
- ✅ **Reports Management**: Comprehensive handling of user and message reports with resolution workflows

### **📱 User Experience & Design**
- ✅ **Mobile Responsiveness**: Ensure the app works well on mobile devices with responsive design
- ✅ **Customizable Templates**: Flexible portfolio templates with customizable layouts and components
- ✅ **Export/Download Portfolio**: Let users export their portfolio as PDF or shareable links
- ✅ **Integration with External Services**: Connect with GitHub, LinkedIn to import projects or display badges

---

## 4. Technology Stack

### **🔧 Backend Technologies**
- **Framework**: .NET 9.0 with ASP.NET Core
- **Database**: PostgreSQL with Entity Framework Core
- **Caching**: Redis for performance optimization
- **Real-time**: SignalR for WebSocket communication
- **Authentication**: JWT tokens with OAuth2 integration
- **Testing**: xUnit with comprehensive test coverage

### **🎨 Frontend Technologies**
- **Framework**: Next.js 15 with React 19
- **Language**: TypeScript 5 for type safety
- **UI Components**: shadcn/ui built on Radix UI primitives
- **Styling**: Tailwind CSS 4 with Framer Motion animations
- **Authentication**: NextAuth.js 4 with OAuth2 support
- **State Management**: React Context with custom hooks

### **🚀 Infrastructure & DevOps**
- **Containerization**: Docker with docker-compose
- **Cloud Platform**: Azure Container Apps
- **Database**: Azure Database for PostgreSQL
- **Monitoring**: Built-in health checks and logging
- **CI/CD**: Automated deployment pipelines

### **💳 Payments & Subscriptions (Stripe)**
- **Service Ownership**: `backend/backend-user` manages premium subscriptions via `StripeService`
- **Checkout Flow**: Authenticated client calls `POST /api/PremiumSubscription/create-checkout-session` → receives `CheckoutUrl` → redirects to Stripe Checkout
- **Webhooks**: Stripe sends events to `POST /api/PremiumSubscription/webhook` to activate, update, or cancel subscriptions
- **Subscription Status**: Clients query `GET /api/PremiumSubscription/status` for current premium state
- **Cancellation**: `POST /api/PremiumSubscription/cancel` schedules cancellation at period end
- **Data Model**: `PremiumSubscription` persisted with fields for `StripeSubscriptionId`, `StripeCustomerId`, `Status`, `CurrentPeriodStart`, `CurrentPeriodEnd`, `CancelAtPeriodEnd`
- **Env Vars**: `STRIPE_SECRET_KEY`, `STRIPE_PRICE_ID`, `STRIPE_WEBHOOK_SECRET`

---

## 5. Project Structure

```
Cognizant-Summer-Practice/
├── 📁 backend/                    # Backend microservices
│   ├── 📁 backend-user/          # User management & authentication
│   ├── 📁 backend-portfolio/     # Portfolio & content management
│   ├── 📁 backend-messages/      # Real-time messaging system
│   ├── 📁 backend-AI/            # AI-powered features & ranking
│   └── 📄 README.md              # Backend services overview
├── 📁 frontend/                   # Frontend applications
│   ├── 📁 auth-user-service/     # Authentication & user management
│   ├── 📁 home-portfolio-service/ # Portfolio discovery & showcase
│   ├── 📁 messages-service/      # Real-time messaging interface
│   ├── 📁 admin-service/         # Administrative dashboard
│   └── 📄 README.md              # Frontend services overview
├── 📁 database/                   # Database schemas & migrations
├── 📁 documentation/              # System documentation
├── 📁 scripts/                    # Automation & utility scripts
└── 📄 README.md                   # This file
```

---

## 6. Services Overview

### 6.1 Backend Services

#### **🔐 Backend User Service**
- **Purpose**: Comprehensive user management, authentication, and OAuth2 integration
- **Key Features**: Multi-provider OAuth2, JWT token management, user lifecycle management
- **Documentation**: [backend/backend-user/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/backend/backend-user/README.md)

#### **📁 Backend Portfolio Service**
- **Purpose**: Portfolio management, content creation, and showcase functionality
- **Key Features**: CQRS architecture, template system, image management, Redis caching
- **Documentation**: [backend/backend-portfolio/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/backend/backend-portfolio/README.md)

#### **💬 Backend Messages Service**
- **Purpose**: Real-time messaging, conversation management, and communication features
- **Key Features**: SignalR integration, conversation threading, message reporting, soft delete pattern
- **Documentation**: [backend/backend-messages/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/backend/backend-messages/README.md)

#### **🤖 Backend AI Service**
- **Purpose**: AI-powered features, portfolio ranking, and intelligent content analysis
- **Key Features**: OpenRouter integration, portfolio ranking algorithms, strategy patterns
- **Documentation**: [backend/backend-AI/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/backend/backend-AI/README.md)

### 6.2 Frontend Services

#### **🔐 Frontend Auth User Service**
- **Purpose**: Comprehensive user authentication, OAuth2 integration, and user management
- **Key Features**: NextAuth.js integration, client-side encryption, cross-service authentication
- **Documentation**: [frontend/auth-user-service/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/frontend/auth-user-service/README.md)

#### **🏠 Frontend Home Portfolio Service**
- **Purpose**: Portfolio discovery, filtering, and showcase functionality
- **Key Features**: Advanced filtering, intelligent caching, performance optimization, responsive design
- **Documentation**: [frontend/home-portfolio-service/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/frontend/home-portfolio-service/README.md)

#### **💬 Frontend Messages Service**
- **Purpose**: Real-time messaging, conversation management, and communication features
- **Key Features**: SignalR WebSocket support, message encryption, conversation management
- **Documentation**: [frontend/messages-service/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/frontend/messages-service/README.md)

#### **👨‍💼 Frontend Admin Service**
- **Purpose**: Comprehensive administrative dashboard and platform management
- **Key Features**: Real-time analytics, user management, portfolio oversight, reporting workflows
- **Documentation**: [frontend/admin-service/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/frontend/admin-service/README.md)

---

## 7. Quick Start

### **Prerequisites**
- Docker and Docker Compose
- .NET 9.0 SDK
- Node.js 18+ and npm/yarn
- PostgreSQL (or use Docker)

### **Backend Setup**
```bash
# Navigate to backend directory
cd backend

# Start database services
docker-compose up -d

# Set up environment variables
cp .env.example .env
# Edit .env with your configuration

# Run individual services
cd backend-user && dotnet run
cd backend-portfolio && dotnet run
cd backend-messages && dotnet run
cd backend-AI && dotnet run
```

### **Frontend Setup**
```bash
# Navigate to frontend directory
cd frontend

# Set up environment variables
cp .env.example .env
# Edit .env with your configuration

# Install dependencies and start services
cd auth-user-service && npm install && npm run dev
cd home-portfolio-service && npm install && npm run dev
cd messages-service && npm install && npm run dev
cd admin-service && npm install && npm run dev
```

### **Environment Configuration**
The platform uses comprehensive environment configuration for:
- **Service URLs**: Azure Container Apps endpoints
- **Database Connections**: PostgreSQL connection strings
- **OAuth2 Providers**: Google, GitHub, Facebook, LinkedIn credentials
- **AI Services**: OpenRouter API configuration
- **Security**: JWT secrets and encryption keys

---

 

## 9. Documentation

### **Comprehensive Service Documentation**
Each service includes detailed documentation covering:
- **Architecture Overview**: Service design and component structure
- **API Reference**: Complete endpoint documentation with examples
- **Implementation Patterns**: Code examples and best practices
- **Deployment Guide**: Step-by-step deployment instructions
- **Testing Strategy**: Comprehensive testing approach and examples

### **Documentation Links**
- **Backend Overview**: [backend/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/backend/README.md)
- **Frontend Overview**: [frontend/README.md](https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice/blob/master/frontend/README.md)
- **Individual Service READMEs**: Available in each service directory

---

## 10. Security Features

### **Authentication & Authorization**
- **OAuth2 Integration**: Multi-provider authentication with automatic token refresh
- **JWT Management**: Secure token handling with rotation and validation
- **Role-Based Access Control**: Granular permissions for different user types
- **Admin Guard System**: Comprehensive authentication validation for administrative functions

### **Data Protection**
- **Client-Side Encryption**: AES encryption with PBKDF2 key derivation
- **Input Validation**: Comprehensive validation and sanitization across all services
- **CORS Configuration**: Secure cross-origin resource sharing
- **Route Protection**: Middleware-based authentication and authorization

### **Security Best Practices**
- **HTTPS Only**: All production services use secure connections
- **Environment Variables**: Sensitive configuration stored securely
- **Audit Logging**: Comprehensive logging for security monitoring
- **Rate Limiting**: Protection against abuse and attacks

---

## 11. Mobile Responsiveness

### **Mobile-First Design**
- **Responsive Layout**: All services designed with mobile-first approach
- **Touch Optimization**: Optimized touch interactions for mobile devices
- **Performance**: Optimized loading and rendering for mobile networks
- **Cross-Device**: Seamless experience across all device types

### **Progressive Web App Features**
- **Offline Support**: Basic functionality available offline
- **App-Like Experience**: Native app feel on mobile devices
- **Push Notifications**: Real-time updates and alerts
- **Installation**: Add to home screen functionality

---

## 12. AI Integration

### **AI-Powered Features**
- **Portfolio Ranking**: Intelligent algorithms for portfolio evaluation and ranking
- **Content Analysis**: AI-powered analysis of portfolio content and quality
- **Recommendations**: Personalized portfolio recommendations for users
- **Top 10 Selection**: Automatic selection of best portfolios based on multiple criteria

### **AI Service Architecture**
- **OpenRouter Integration**: Access to multiple AI models and providers
- **Strategy Patterns**: Flexible AI service abstraction and integration
- **Performance Optimization**: Intelligent caching and response optimization
- **Scalable Design**: AI services designed for high-volume processing

---

## 13. Performance & Scalability

### **Performance Optimization**
- **Multi-Level Caching**: Redis integration with intelligent cache invalidation
- **Lazy Loading**: On-demand loading of components and data
- **Image Optimization**: Automatic image optimization and thumbnail generation
- **Bundle Optimization**: Code splitting and dynamic imports

### **Scalability Features**
- **Microservices Architecture**: Independent scaling of services
- **Horizontal Scaling**: Support for multiple service instances
- **Load Balancing**: Intelligent request distribution
- **Database Optimization**: Strategic indexing and query optimization

### **Monitoring & Analytics**
- **Real-Time Metrics**: Live performance monitoring
- **User Experience Tracking**: Comprehensive UX analytics
- **Performance Profiling**: Detailed performance analysis
- **Error Tracking**: Sophisticated error handling and reporting

---

## 14. Testing Strategy

### **Comprehensive Testing Coverage**
- **Unit Testing**: Individual component and service testing
- **Integration Testing**: Service-to-service communication testing
- **Component Testing**: Frontend component behavior testing
- **End-to-End Testing**: Complete user workflow testing

### **Testing Tools & Practices**
- **xUnit**: Backend testing framework with comprehensive coverage
- **Jest & React Testing Library**: Frontend testing tools
- **Test Data Factories**: Consistent test data generation
- **Mocking Strategy**: Effective isolation of dependencies

---

## 15. Deployment

### **Containerization**
- **Docker Support**: All services include Dockerfile and container configuration
- **Azure Container Apps**: Production deployment on Azure cloud platform
- **Environment Management**: Comprehensive environment variable configuration
- **Health Checks**: Built-in health monitoring and status endpoints

### **CI/CD Pipeline**
- **Automated Testing**: Comprehensive test suite execution
- **Quality Gates**: Code quality and security checks
- **Automated Deployment**: Seamless deployment to production
- **Rollback Capability**: Quick rollback in case of issues

---

## 16. Contributing

### **How to Contribute**
1. **Fork the repository** and create a feature branch
2. **Follow coding standards** and architectural patterns
3. **Write comprehensive tests** for new features
4. **Update documentation** for any changes
5. **Submit a pull request** with detailed description

### **Development Guidelines**
- **Code Organization**: Follow established architectural patterns
- **Testing**: Maintain high test coverage
- **Documentation**: Keep documentation up-to-date
- **Performance**: Consider performance implications of changes

### **Code Review Process**
- **Automated Checks**: CI/CD pipeline validation
- **Peer Review**: Code review by team members
- **Quality Gates**: Automated quality and security checks
- **Documentation Review**: Ensure documentation is updated

---

## 17. License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🌟 **Why Choose GoalKeeper?**

### **🚀 Enterprise-Grade Architecture**
- **Microservices Design**: Scalable, maintainable, and flexible architecture
- **Modern Technology Stack**: Latest .NET 9.0, Next.js 15, and React 19
- **Comprehensive Security**: OAuth2, JWT, encryption, and role-based access control

### **🎨 Professional User Experience**
- **Beautiful Design**: Modern UI with shadcn/ui components and Tailwind CSS
- **Mobile-First**: Responsive design that works perfectly on all devices
- **Performance Optimized**: Fast loading, intelligent caching, and smooth interactions

### **🤖 AI-Powered Intelligence**
- **Smart Portfolio Ranking**: AI algorithms for portfolio evaluation and discovery
- **Intelligent Recommendations**: Personalized content and portfolio suggestions
- **Content Analysis**: AI-powered quality assessment and optimization

### **🔒 Security & Reliability**
- **Multi-Provider Authentication**: OAuth2 support for Google, GitHub, Facebook, LinkedIn
- **Client-Side Encryption**: Secure data transmission and storage
- **Comprehensive Admin Tools**: Advanced oversight and management capabilities

### **📱 Modern Communication**
- **Real-Time Messaging**: SignalR-powered instant communication
- **Professional Networking**: Built-in messaging for collaboration and networking
- **Notification System**: Email notifications for important updates

---

## 📞 **Support & Contact**

- **Documentation**: Comprehensive guides available in each service directory
- **Issues**: Report bugs and request features through GitHub issues
- **Contributing**: Join the development team and contribute to the project
- **Community**: Connect with other developers and users

---

## 🎯 **Roadmap & Future Enhancements**

### **Planned Features**
- **Advanced Analytics**: Enhanced portfolio performance metrics
- **Social Features**: Enhanced networking and collaboration tools
- **API Marketplace**: Third-party integrations and extensions
- **Mobile Apps**: Native iOS and Android applications

### **Performance Improvements**
- **Edge Computing**: Global CDN and edge optimization
- **Advanced Caching**: Intelligent cache strategies and optimization
- **Database Optimization**: Enhanced query performance and scaling

---

*GoalKeeper - Empowering professionals to showcase their best work with intelligent, beautiful, and secure portfolio management.*

---

<div align="center">

**Made with ❤️ by the GoalKeeper Team**

