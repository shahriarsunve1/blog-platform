# Blog Platform - Agentic Development Project

A modern blog platform built with **Angular** frontend and **.NET Core** backend, following industry best practices and clean architecture principles.

## Project Overview

This blog platform enables:
- **Readers**: Browse and read blog posts without authentication
- **Authors**: Create, edit, and manage posts (requires authentication)
- **Administrators**: Manage users, moderate content, and system configuration

## Tech Stack

### Frontend
- **Framework**: Angular 16+
- **Language**: TypeScript
- **Styling**: SCSS/CSS
- **State Management**: NgRx (recommended)
- **HTTP Client**: Angular HttpClient with Interceptors
- **Testing**: Jasmine & Karma

### Backend
- **Runtime**: .NET 8+
- **Framework**: ASP.NET Core
- **Database**: SQL Server / PostgreSQL
- **ORM**: Entity Framework Core
- **API**: RESTful with OpenAPI/Swagger
- **Testing**: xUnit & Moq

## Key Features

- ✅ Public post browsing without login
- ✅ User authentication and authorization
- ✅ Create/Edit/Delete posts (authenticated users)
- ✅ Comments system (future phase)
- ✅ Tag/Category system
- ✅ Search functionality
- ✅ Responsive design

## Project Structure

```
blog-platform/
├── frontend/                 # Angular application
├── backend/                  # .NET Core API
├── docs/                     # Documentation
│   ├── ARCHITECTURE.md
│   ├── API_SPECS.md
│   ├── DEVELOPMENT_GUIDELINES.md
│   ├── FRONTEND_STRUCTURE.md
│   └── SETUP.md
├── README.md
├── VISION.md
└── CONTRIBUTING.md
```

## Quick Start

See [SETUP.md](./docs/SETUP.md) for detailed setup instructions.

## Development Standards

- Code follows [DEVELOPMENT_GUIDELINES.md](./docs/DEVELOPMENT_GUIDELINES.md)
- API design adheres to [API_SPECS.md](./docs/API_SPECS.md)
- Architecture follows [ARCHITECTURE.md](./docs/ARCHITECTURE.md)

## Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for contribution guidelines.

## License

MIT

---

**Last Updated**: July 2026
