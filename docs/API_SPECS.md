# API Specifications

## API Overview

### Base URL
- Development: `http://localhost:5000/api`
- Production: `https://api.yourdomain.com/api`

### Authentication
- Method: JWT (JSON Web Token)
- Header: `Authorization: Bearer {token}`
- Token Format: JWT with HS256 signature

### Content Type
- All requests: `Content-Type: application/json`
- All responses: `application/json`

### Versioning
- Current Version: v1
- Format: Include version in header or URL
- Future: Support multiple versions simultaneously

## Authentication Endpoints

### Register User
```
POST /auth/register
Content-Type: application/json

Request:
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}

Response (201 Created):
{
  "success": true,
  "statusCode": 201,
  "message": "User registered successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "User",
    "createdAt": "2026-07-28T10:30:00Z"
  }
}

Error Response (400 Bad Request):
{
  "success": false,
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Email already registered"
    },
    {
      "field": "password",
      "message": "Password must be at least 8 characters"
    }
  ]
}
```

### Login
```
POST /auth/login
Content-Type: application/json

Request:
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "email": "john@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "User"
    }
  }
}

Error Response (401 Unauthorized):
{
  "success": false,
  "statusCode": 401,
  "message": "Invalid credentials"
}
```

### Refresh Token
```
POST /auth/refresh
Content-Type: application/json

Request:
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

### Logout
```
POST /auth/logout
Authorization: Bearer {accessToken}

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "message": "Logged out successfully"
}
```

## Posts Endpoints

### Get All Published Posts (Public)
```
GET /posts?page=1&pageSize=10&sortBy=createdAt&sortOrder=desc

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "data": {
    "items": [
      {
        "id": "post-123",
        "title": "Getting Started with Angular",
        "excerpt": "A beginner's guide to Angular framework",
        "content": "Full content here...",
        "author": {
          "id": "user-123",
          "username": "john_doe",
          "firstName": "John"
        },
        "status": "Published",
        "categories": ["Technology", "Angular"],
        "tags": ["angular", "frontend", "typescript"],
        "viewCount": 1520,
        "createdAt": "2026-07-20T10:00:00Z",
        "updatedAt": "2026-07-25T14:30:00Z",
        "publishedAt": "2026-07-20T10:00:00Z"
      }
    ],
    "totalCount": 45,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

### Get Single Post (Public)
```
GET /posts/{postId}

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "post-123",
    "title": "Getting Started with Angular",
    "excerpt": "A beginner's guide to Angular framework",
    "content": "Full content here...",
    "author": {
      "id": "user-123",
      "username": "john_doe",
      "firstName": "John",
      "avatar": "https://cdn.example.com/user-123.jpg"
    },
    "status": "Published",
    "categories": ["Technology", "Angular"],
    "tags": ["angular", "frontend", "typescript"],
    "viewCount": 1521,
    "createdAt": "2026-07-20T10:00:00Z",
    "updatedAt": "2026-07-25T14:30:00Z",
    "publishedAt": "2026-07-20T10:00:00Z"
  }
}

Error Response (404 Not Found):
{
  "success": false,
  "statusCode": 404,
  "message": "Post not found"
}
```

### Create Post (Authenticated)
```
POST /posts
Authorization: Bearer {accessToken}
Content-Type: application/json

Request:
{
  "title": "New Blog Post",
  "excerpt": "A short excerpt",
  "content": "Full post content with markdown support",
  "status": "Draft",
  "categoryIds": ["cat-1", "cat-2"],
  "tagIds": ["tag-1", "tag-2"]
}

Response (201 Created):
{
  "success": true,
  "statusCode": 201,
  "message": "Post created successfully",
  "data": {
    "id": "post-456",
    "title": "New Blog Post",
    "excerpt": "A short excerpt",
    "content": "Full post content",
    "author": {
      "id": "user-123",
      "username": "john_doe"
    },
    "status": "Draft",
    "categories": ["Technology"],
    "tags": ["new", "blog"],
    "viewCount": 0,
    "createdAt": "2026-07-28T10:30:00Z"
  }
}
```

### Update Post (Author/Admin)
```
PUT /posts/{postId}
Authorization: Bearer {accessToken}
Content-Type: application/json

Request:
{
  "title": "Updated Title",
  "excerpt": "Updated excerpt",
  "content": "Updated content",
  "status": "Published",
  "categoryIds": ["cat-1"],
  "tagIds": ["tag-1", "tag-3"]
}

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "message": "Post updated successfully",
  "data": { ... }
}

Error Response (403 Forbidden):
{
  "success": false,
  "statusCode": 403,
  "message": "You don't have permission to update this post"
}
```

### Delete Post (Author/Admin)
```
DELETE /posts/{postId}
Authorization: Bearer {accessToken}

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "message": "Post deleted successfully"
}
```

### Get User's Posts (Author)
```
GET /posts/user/{userId}?status=all

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "data": {
    "items": [ ... ],
    "totalCount": 12,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

## User Endpoints

### Get User Profile (Public)
```
GET /users/{userId}

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "user-123",
    "username": "john_doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "bio": "Passionate about web development",
    "avatar": "https://cdn.example.com/user-123.jpg",
    "postCount": 12,
    "createdAt": "2026-01-15T10:00:00Z"
  }
}
```

### Update User Profile (Authenticated)
```
PUT /users/{userId}
Authorization: Bearer {accessToken}
Content-Type: application/json

Request:
{
  "firstName": "John",
  "lastName": "Doe",
  "bio": "Updated bio",
  "avatar": "base64_image_data"
}

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "message": "Profile updated successfully",
  "data": { ... }
}
```

### Delete User Account (Authenticated)
```
DELETE /users/{userId}
Authorization: Bearer {accessToken}

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "message": "Account deleted successfully"
}
```

## Categories Endpoints

### Get All Categories (Public)
```
GET /categories

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "id": "cat-1",
      "name": "Technology",
      "slug": "technology",
      "postCount": 25
    },
    {
      "id": "cat-2",
      "name": "Lifestyle",
      "slug": "lifestyle",
      "postCount": 18
    }
  ]
}
```

### Get Posts by Category (Public)
```
GET /categories/{categoryId}/posts?page=1&pageSize=10

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "data": {
    "category": { ... },
    "posts": [ ... ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

## Tags Endpoints

### Get All Tags (Public)
```
GET /tags

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "id": "tag-1",
      "name": "angular",
      "slug": "angular",
      "postCount": 8
    }
  ]
}
```

### Get Posts by Tag (Public)
```
GET /tags/{tagId}/posts?page=1&pageSize=10

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "data": {
    "tag": { ... },
    "posts": [ ... ],
    "totalCount": 8,
    "pageNumber": 1
  }
}
```

## Search Endpoint

### Search Posts
```
GET /search?query=angular&type=posts&page=1

Response (200 OK):
{
  "success": true,
  "statusCode": 200,
  "data": {
    "query": "angular",
    "results": [
      {
        "id": "post-123",
        "title": "Getting Started with Angular",
        "excerpt": "...",
        "author": "John Doe",
        "publishedAt": "2026-07-20T10:00:00Z"
      }
    ],
    "totalResults": 5,
    "pageNumber": 1
  }
}
```

## Error Status Codes

| Status Code | Meaning |
|-------------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 400 | Bad Request - Invalid request data |
| 401 | Unauthorized - Missing or invalid token |
| 403 | Forbidden - User lacks permission |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource already exists |
| 422 | Unprocessable Entity - Validation failed |
| 500 | Server Error - Internal server error |

## Rate Limiting

- **Limit**: 100 requests per minute per IP
- **Headers**: X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset

## Pagination

All list endpoints support:
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `sortBy`: Sort field (default: createdAt)
- `sortOrder`: asc or desc (default: desc)

## CORS Configuration

- **Allowed Origins**: Configured per environment
- **Allowed Methods**: GET, POST, PUT, DELETE, OPTIONS
- **Allowed Headers**: Content-Type, Authorization
- **Credentials**: true

---

**Last Updated**: July 2026
**API Version**: 1.0
