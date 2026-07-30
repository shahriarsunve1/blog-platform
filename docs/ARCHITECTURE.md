# Architecture - Blog Platform

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Layer (Angular)                   │
├─────────────────────────────────────────────────────────────┤
│  Components │ Services │ Guards │ Interceptors │ Resolvers  │
├─────────────────────────────────────────────────────────────┤
│                    HTTP/REST API Layer                       │
├─────────────────────────────────────────────────────────────┤
│           Server Layer (.NET Core / ASP.NET)                 │
│  Controllers │ Services │ Middleware │ Filters │ Validators │
├─────────────────────────────────────────────────────────────┤
│              Data Access Layer (EF Core)                     │
│    Repositories │ DbContext │ Entities │ Migrations         │
├─────────────────────────────────────────────────────────────┤
│              Database Layer (SQL Server)                     │
│    Users │ Posts │ Comments │ Tags │ Categories             │
└─────────────────────────────────────────────────────────────┘
```

## Backend Architecture (.NET Core)

### Layer Structure

```
BlogAPI/
├── BlogAPI.API/           # Presentation Layer (Controllers, DTOs)
├── BlogAPI.Core/          # Business Logic Layer (Services, Interfaces)
├── BlogAPI.Data/          # Data Access Layer (Repositories, DbContext)
├── BlogAPI.Domain/        # Domain Layer (Entities, Enums, Exceptions)
└── BlogAPI.Tests/         # Unit & Integration Tests
```

### Components

#### 1. **Domain Layer** (`BlogAPI.Domain`)
- Entities: User, Post, Comment, Category, Tag
- Enums: UserRole, PostStatus
- Exceptions: Custom application exceptions
- Value Objects: Email, Username
- **No external dependencies**

#### 2. **Data Layer** (`BlogAPI.Data`)
- DbContext configuration
- Repository implementations
- Database migrations
- Seed data
- **Dependencies**: Domain, EF Core

#### 3. **Core/Business Layer** (`BlogAPI.Core`)
- Services: UserService, PostService, AuthService
- Validation logic
- Business rules
- Mapping/DTOs
- **Dependencies**: Domain

#### 4. **API Layer** (`BlogAPI.API`)
- Controllers: UsersController, PostsController, AuthController
- Middleware: Authentication, Error handling
- Filters: Validation, Authorization
- Dependency Injection setup
- **Dependencies**: All layers

### Design Patterns

- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Loose coupling
- **SOLID Principles**:
  - Single Responsibility: Each class has one reason to change
  - Open/Closed: Open for extension, closed for modification
  - Liskov Substitution: Interfaces substitutable
  - Interface Segregation: Specific interfaces
  - Dependency Inversion: Depend on abstractions

### Database Schema

```
Users
├── Id (PK)
├── Username (unique)
├── Email (unique)
├── PasswordHash
├── FirstName
├── LastName
├── Role (Enum)
├── CreatedAt
├── UpdatedAt
└── IsActive

Posts
├── Id (PK)
├── UserId (FK → Users)
├── Title
├── Content
├── Excerpt
├── Status (Draft/Published/Archived)
├── CreatedAt
├── UpdatedAt
├── PublishedAt
└── ViewCount

PostCategories
├── PostId (FK)
├── CategoryId (FK)

PostTags
├── PostId (FK)
├── TagId (FK)

Categories
├── Id (PK)
├── Name
├── Slug

Tags
├── Id (PK)
├── Name
└── Slug
```

## Frontend Architecture (Angular)

### Folder Structure

```
src/
├── app/
│   ├── core/                    # Singleton services, guards, interceptors
│   │   ├── auth/
│   │   ├── guards/
│   │   ├── interceptors/
│   │   └── services/
│   ├── shared/                  # Reusable components, directives, pipes
│   │   ├── components/
│   │   ├── directives/
│   │   ├── pipes/
│   │   └── models/
│   ├── features/                # Feature modules
│   │   ├── posts/
│   │   ├── auth/
│   │   └── user/
│   ├── app.module.ts
│   ├── app-routing.module.ts
│   └── app.component.ts
├── assets/
└── environments/
```

### Component Architecture

- **Smart Components** (Containers): Handle state, logic, API calls
- **Dumb Components** (Presentational): Pure input/output, no side effects
- **OnPush Change Detection**: Performance optimization

### State Management

**Option 1: NgRx (Recommended for large apps)**
```
store/
├── posts/
│   ├── posts.actions.ts
│   ├── posts.reducer.ts
│   ├── posts.effects.ts
│   ├── posts.selectors.ts
│   └── posts.state.ts
└── auth/
```

**Option 2: Services with Subjects (Lightweight)**
```
services/
├── post.service.ts      # BehaviorSubject patterns
├── auth.service.ts
└── notification.service.ts
```

### Module Structure

- **Core Module**: Singleton services, guards, interceptors
- **Shared Module**: Common components, pipes, directives
- **Feature Modules**: Lazy-loaded feature areas (Posts, Auth, User)
- **App Module**: Root module

## API Design

### RESTful Endpoints

```
Authentication:
  POST   /api/auth/register          # Register new user
  POST   /api/auth/login             # Login
  POST   /api/auth/refresh           # Refresh token
  POST   /api/auth/logout            # Logout

Posts:
  GET    /api/posts                  # List all published posts (public)
  GET    /api/posts/{id}             # Get single post (public)
  POST   /api/posts                  # Create post (authenticated)
  PUT    /api/posts/{id}             # Update post (author/admin)
  DELETE /api/posts/{id}             # Delete post (author/admin)
  GET    /api/posts/user/{userId}    # Get user's posts

Users:
  GET    /api/users/{id}             # Get user profile (public)
  PUT    /api/users/{id}             # Update profile (authenticated)
  DELETE /api/users/{id}             # Delete account (user/admin)
```

### Response Format

```json
{
  "success": true,
  "statusCode": 200,
  "data": {},
  "message": "Operation successful",
  "errors": []
}
```

### Error Responses

```json
{
  "success": false,
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Invalid email format"
    }
  ]
}
```

## Security Architecture

### Authentication Flow

```
1. User submits credentials
2. Backend validates and generates JWT token
3. Token stored in secure cookie/localStorage
4. Frontend includes token in Authorization header
5. Backend validates token in middleware
6. Request processed if valid, rejected if invalid
```

### JWT Token Structure

```
Header: { "alg": "HS256", "typ": "JWT" }
Payload: { "sub": "userId", "email": "user@example.com", "role": "User", "exp": timestamp }
Signature: HMACSHA256(header.payload, secret)
```

### Authorization

- **Role-based Access Control (RBAC)**
  - Admin: Full access
  - Editor: Can create/edit own posts
  - User: Read-only
  - Guest: Public posts only

## Deployment Architecture

```
Developer Machine
    ↓
Git Repository (GitHub/GitLab)
    ↓
CI/CD Pipeline (GitHub Actions / Azure DevOps)
    ├→ Run Tests
    ├→ Build (Frontend + Backend)
    ├→ Security Scan
    └→ Deploy to Staging/Production
        ├→ Frontend (Azure Static Web Apps / Vercel)
        └→ Backend (.NET API on Azure App Service / AWS EC2)
            ├→ Database (Azure SQL / RDS)
            └→ Storage (Azure Blob / S3)
```

## Data Flow

### Creating a Post

```
1. User fills form in Angular component
2. Form validation on client
3. POST request sent with JWT token
4. API middleware validates token
5. Controller validates DTO
6. Service applies business logic
7. Repository persists to database
8. Response returned to frontend
9. UI updated, notification shown
```

## Performance Considerations

- **Lazy Loading**: Feature modules loaded on demand
- **Caching**: HTTP caching headers, service-level caching
- **Pagination**: Large datasets paginated
- **Compression**: Gzip for responses
- **Code Splitting**: Separate bundles for features
- **Database**: Indexes on frequently queried columns
- **CDN**: Static assets served via CDN

---

**Last Updated**: July 2026
**Next Review**: After initial implementation phase
