# Setup & Installation Guide

## Prerequisites

### Required Software

- **Node.js**: 18.x or higher (for Angular)
- **.NET SDK**: 8.0 or higher
- **Git**: 2.x or higher
- **VS Code** or **Visual Studio 2022+**
- **SQL Server** or **PostgreSQL**

### Optional Tools

- **Postman** or **Thunder Client**: API testing
- **Git Extensions**: Version control UI
- **SQL Server Management Studio** or **pgAdmin**: Database management

## Backend Setup (.NET Core)

### 1. Create Solution Structure

```powershell
# Create solution directory
mkdir BlogPlatform
cd BlogPlatform

# Create solution file
dotnet new sln -n BlogAPI

# Create projects
dotnet new webapi -n BlogAPI.API -f net8.0
dotnet new classlib -n BlogAPI.Core -f net8.0
dotnet new classlib -n BlogAPI.Data -f net8.0
dotnet new classlib -n BlogAPI.Domain -f net8.0
dotnet new xunit -n BlogAPI.Tests -f net8.0

# Add projects to solution
dotnet sln BlogAPI.sln add BlogAPI.API/BlogAPI.API.csproj
dotnet sln BlogAPI.sln add BlogAPI.Core/BlogAPI.Core.csproj
dotnet sln BlogAPI.sln add BlogAPI.Data/BlogAPI.Data.csproj
dotnet sln BlogAPI.sln add BlogAPI.Domain/BlogAPI.Domain.csproj
dotnet sln BlogAPI.sln add BlogAPI.Tests/BlogAPI.Tests.csproj
```

### 2. Add Project References

```powershell
# From BlogAPI.API folder
dotnet add reference ../BlogAPI.Core/BlogAPI.Core.csproj
dotnet add reference ../BlogAPI.Domain/BlogAPI.Domain.csproj

# From BlogAPI.Core folder
cd ../BlogAPI.Core
dotnet add reference ../BlogAPI.Domain/BlogAPI.Domain.csproj
dotnet add reference ../BlogAPI.Data/BlogAPI.Data.csproj

# From BlogAPI.Data folder
cd ../BlogAPI.Data
dotnet add reference ../BlogAPI.Domain/BlogAPI.Domain.csproj

# From BlogAPI.Tests folder
cd ../BlogAPI.Tests
dotnet add reference ../BlogAPI.API/BlogAPI.API.csproj
dotnet add reference ../BlogAPI.Core/BlogAPI.Core.csproj
```

### 3. Install Required NuGet Packages

```powershell
# In BlogAPI.API project
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore
dotnet add package FluentValidation
dotnet add package FluentValidation.AspNetCore
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package Serilog.AspNetCore

# In BlogAPI.Data project
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# In BlogAPI.Tests project
dotnet add package Moq
dotnet add package xunit
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package coverlet.collector
```

### 4. Create Database

```powershell
# Install EF Core CLI globally
dotnet tool install --global dotnet-ef

# Create initial migration (run from BlogAPI.API folder)
dotnet ef migrations add InitialMigration -p ../BlogAPI.Data -s .

# Apply migration to database
dotnet ef database update -p ../BlogAPI.Data -s .
```

### 5. Configure appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BlogDb;User Id=sa;Password=YourPassword;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-characters-long!!!",
    "Issuer": "BlogAPI",
    "Audience": "BlogClient",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200", "https://yourdomain.com"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### 6. Run Backend

```powershell
cd BlogAPI.API
dotnet run
# API will be available at http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```

## Frontend Setup (Angular)

### 1. Install Node.js & npm

```powershell
# Check if Node.js is installed
node --version
npm --version

# If not, download from https://nodejs.org/
```

### 2. Install Angular CLI

```powershell
npm install -g @angular/cli@16
ng version
```

### 3. Create Angular Project

```powershell
# Create new Angular project
ng new blog-frontend --routing --style=scss --skip-git

cd blog-frontend
```

### 4. Install Dependencies

```powershell
# Core dependencies
npm install

# Additional packages
npm install ngx-toastr --save
npm install date-fns --save
npm install lodash --save
npm install uuid --save
npm install --save-dev @types/node
```

### 5. Create Folder Structure

Use the structure provided in [FRONTEND_STRUCTURE.md](./FRONTEND_STRUCTURE.md)

### 6. Configure Environment

```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};

// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://api.yourdomain.com/api'
};
```

### 7. Run Frontend Development Server

```powershell
ng serve
# Application will be available at http://localhost:4200
```

## Database Setup

### Using SQL Server

```sql
-- Create database
CREATE DATABASE BlogDb;

-- Create tables (EF Core will do this via migrations)
-- Use Entity Framework migrations for schema management
```

### Using PostgreSQL

```sql
-- Create database
CREATE DATABASE blog_db;

-- Install EF Core provider
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

## Running Tests

### Backend Tests

```powershell
cd BlogAPI.Tests

# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test
dotnet test --filter "Name~AuthServiceTests"
```

### Frontend Tests

```powershell
cd blog-frontend

# Run tests once
ng test --watch=false

# Run tests with coverage
ng test --code-coverage --watch=false

# Run e2e tests
ng e2e
```

## Git Setup

### Initialize Repository

```powershell
git init
git add .
git commit -m "Initial commit: Project setup"
```

### Create .gitignore

```
# Backend
BlogAPI.*/bin/
BlogAPI.*/obj/
*.user
*.suo
.vs/

# Frontend
node_modules/
dist/
.angular/
coverage/

# Environment files
*.local.ts
.env

# OS files
.DS_Store
Thumbs.db
```

## Environment Configuration

### Development Environment

```powershell
# Backend development
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run

# Frontend development
ng serve --open
```

### Production Build

```powershell
# Backend
dotnet publish -c Release -o ./publish

# Frontend
ng build --configuration production
```

## Initial Data Seeding

### Seed Users and Posts

Create a seeding service in `BlogAPI.Data`:

```csharp
public class DataSeeder
{
    public static void Seed(BlogContext context)
    {
        if (context.Users.Any()) return;

        var users = new List<User>
        {
            new User 
            { 
                Username = "admin",
                Email = "admin@blog.com",
                FirstName = "Admin",
                LastName = "User",
                Role = UserRole.Admin
            },
            new User 
            { 
                Username = "john_doe",
                Email = "john@blog.com",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Editor
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();
    }
}
```

Call in `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BlogContext>();
    DataSeeder.Seed(context);
}
```

## Verification Checklist

- [ ] .NET SDK installed (`dotnet --version`)
- [ ] SQL Server/PostgreSQL running
- [ ] Node.js installed (`node --version`)
- [ ] Angular CLI installed (`ng version`)
- [ ] Backend project created and builds successfully
- [ ] Frontend project created and builds successfully
- [ ] Database migrations applied
- [ ] Backend API running on localhost:5000
- [ ] Frontend dev server running on localhost:4200
- [ ] API endpoints accessible via Swagger
- [ ] Tests running successfully

## Troubleshooting

### Port Already in Use

```powershell
# Find process using port 5000
Get-NetTCPConnection -LocalPort 5000

# Kill process
Stop-Process -Id <PID> -Force

# For Angular (port 4200)
ng serve --port 4300
```

### Database Connection Issues

```powershell
# Test connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"

# Check secrets
dotnet user-secrets list
```

### Angular Build Errors

```powershell
# Clear cache and reinstall
rm node_modules
npm install

# Clear Angular cache
ng cache clean
```

### EF Core Migration Issues

```powershell
# Remove last migration
dotnet ef migrations remove

# Create fresh migration
dotnet ef migrations add InitialMigration

# Update database
dotnet ef database update
```

## Next Steps

1. Read [ARCHITECTURE.md](./ARCHITECTURE.md) to understand project structure
2. Review [DEVELOPMENT_GUIDELINES.md](./DEVELOPMENT_GUIDELINES.md) for coding standards
3. Check [API_SPECS.md](./API_SPECS.md) for API endpoints
4. Start implementing features according to [VISION.md](./VISION.md)

---

**Last Updated**: July 2026
**Maintainer**: Development Team
