# Development Guidelines

## Code Style & Conventions

### Naming Conventions

#### C# (.NET Core)

```csharp
// Classes/Interfaces: PascalCase
public class UserService { }
public interface IUserRepository { }

// Methods/Properties: PascalCase
public async Task<UserDto> GetUserByIdAsync(int id) { }
public string FirstName { get; set; }

// Private fields: _camelCase
private readonly IUserRepository _userRepository;

// Constants: UPPER_SNAKE_CASE
private const int MAX_ATTEMPTS = 3;
private const string CACHE_KEY_PREFIX = "user_";

// Async methods: Suffix with Async
public async Task<List<PostDto>> GetPostsAsync() { }
```

#### TypeScript/Angular

```typescript
// Classes/Interfaces: PascalCase
export class PostService { }
export interface IPost { }

// Functions/Methods: camelCase
getPostById(id: number): Observable<Post> { }

// Properties: camelCase
posts$: Observable<Post[]>;
isLoading = false;

// Constants: UPPER_SNAKE_CASE
const MAX_POST_TITLE_LENGTH = 255;
const API_BASE_URL = '/api';

// Private properties: camelCase with # or private keyword
#cache = new Map();
private isLoggedIn = false;

// Enums: PascalCase (singular)
enum UserRole {
  Admin,
  Editor,
  Reader
}
```

### File Naming

#### C# Files
```
UserService.cs          # Class files named after class
IUserRepository.cs      # Interface files named after interface
user.entity.cs          # Entity models
user.dto.cs             # Data Transfer Objects
user-repository.tests.cs # Test files
```

#### TypeScript Files
```
post.service.ts         # Services
post.component.ts       # Components
post.model.ts           # Models/Interfaces
post.guard.ts           # Guards
post.interceptor.ts     # Interceptors
post.pipe.ts            # Pipes
post.directive.ts       # Directives
post.service.spec.ts    # Test files
```

## Code Organization

### C# Project Structure

```
BlogAPI.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── PostsController.cs
│   └── UsersController.cs
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── AuthenticationMiddleware.cs
├── Filters/
│   ├── ValidationFilterAttribute.cs
│   └── AuthorizationFilterAttribute.cs
├── Program.cs          # Startup configuration
└── appsettings.json

BlogAPI.Core/
├── Services/
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   ├── IPostService.cs
│   └── PostService.cs
├── DTOs/
│   ├── CreatePostDto.cs
│   ├── UpdatePostDto.cs
│   └── PostDto.cs
└── Validators/
    └── CreatePostDtoValidator.cs

BlogAPI.Data/
├── Repositories/
│   ├── IGenericRepository.cs
│   ├── GenericRepository.cs
│   ├── IPostRepository.cs
│   └── PostRepository.cs
├── DbContext/
│   └── BlogContext.cs
└── Migrations/

BlogAPI.Domain/
├── Entities/
│   ├── User.cs
│   ├── Post.cs
│   └── Category.cs
├── Enums/
│   ├── UserRole.cs
│   └── PostStatus.cs
└── Exceptions/
    └── InvalidOperationException.cs
```

### Angular Project Structure

```
src/app/
├── core/                           # Singleton services
│   ├── auth/
│   │   ├── auth.service.ts
│   │   ├── jwt.guard.ts
│   │   └── auth.interceptor.ts
│   ├── guards/
│   ├── interceptors/
│   ├── http/
│   └── services/
│       └── api.service.ts
│
├── shared/                         # Reusable items
│   ├── components/
│   │   ├── header/
│   │   ├── footer/
│   │   └── loading-spinner/
│   ├── directives/
│   ├── pipes/
│   └── models/
│       └── interfaces.ts
│
├── features/                       # Feature modules
│   ├── posts/
│   │   ├── components/
│   │   │   ├── post-list/
│   │   │   ├── post-detail/
│   │   │   └── post-form/
│   │   ├── services/
│   │   │   └── post.service.ts
│   │   ├── models/
│   │   ├── posts.module.ts
│   │   └── posts-routing.module.ts
│   │
│   ├── auth/
│   │   ├── components/
│   │   │   ├── login/
│   │   │   └── register/
│   │   ├── auth.module.ts
│   │   └── auth-routing.module.ts
│   │
│   └── user/
│       ├── components/
│       ├── services/
│       └── user.module.ts
│
├── app.component.ts
├── app.component.html
├── app.module.ts
└── app-routing.module.ts
```

## SOLID Principles

### Single Responsibility Principle (SRP)

```csharp
// ❌ Bad: Multiple responsibilities
public class UserService
{
    public void CreateUser(CreateUserDto dto)
    {
        // Validation
        // Save to database
        // Send email
        // Log activity
    }
}

// ✅ Good: Single responsibility
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public async Task CreateUserAsync(CreateUserDto dto)
    {
        var user = new User { Email = dto.Email };
        await _repository.AddAsync(user);
    }
}
```

### Open/Closed Principle (OCP)

```csharp
// ✅ Good: Open for extension, closed for modification
public interface INotificationService
{
    Task SendAsync(string message);
}

public class EmailNotificationService : INotificationService
{
    public async Task SendAsync(string message) { }
}

public class SmsNotificationService : INotificationService
{
    public async Task SendAsync(string message) { }
}
```

### Liskov Substitution Principle (LSP)

```typescript
// ✅ Good: Derived classes properly implement interface
interface DataProvider {
  getData(): Observable<any>;
}

class ApiDataProvider implements DataProvider {
  getData(): Observable<any> { return this.http.get('/api/data'); }
}

class CachedDataProvider implements DataProvider {
  getData(): Observable<any> { return this.cache.get(); }
}
```

### Interface Segregation Principle (ISP)

```csharp
// ❌ Bad: Fat interface
public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
    Task<User> GetUserByIdAsync(int id);
    Task<List<User>> GetAllUsersAsync();
    Task ArchiveUserAsync(int id);
    Task RestoreUserAsync(int id);
}

// ✅ Good: Segregated interfaces
public interface IUserReader
{
    Task<User> GetUserByIdAsync(int id);
    Task<List<User>> GetAllUsersAsync();
}

public interface IUserWriter
{
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}

public interface IUserArchiver
{
    Task ArchiveUserAsync(int id);
    Task RestoreUserAsync(int id);
}
```

### Dependency Inversion Principle (DIP)

```csharp
// ❌ Bad: Depends on concrete class
public class PostService
{
    private readonly SqlPostRepository _repository = new();
}

// ✅ Good: Depends on abstraction
public class PostService
{
    private readonly IPostRepository _repository;

    public PostService(IPostRepository repository)
    {
        _repository = repository;
    }
}
```

## Testing Guidelines

### Testing Pyramid

```
        /\              Unit Tests: 70%
       /  \             Integration: 20%
      /____\            E2E: 10%
     /      \
    /________\
```

### C# Unit Tests

```csharp
[Fact]
public async Task CreatePost_WithValidData_ReturnsCreatedPost()
{
    // Arrange
    var dto = new CreatePostDto { Title = "Test", Content = "Test content" };
    var mockRepository = new Mock<IPostRepository>();
    var service = new PostService(mockRepository.Object);

    // Act
    var result = await service.CreatePostAsync(dto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(dto.Title, result.Title);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
}
```

### Angular Unit Tests

```typescript
describe('PostService', () => {
  let service: PostService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PostService]
    });
    service = TestBed.inject(PostService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should fetch posts', () => {
    const mockPosts = [{ id: 1, title: 'Test' }];

    service.getPosts().subscribe(posts => {
      expect(posts.length).toBe(1);
      expect(posts[0].title).toBe('Test');
    });

    const req = httpMock.expectOne('/api/posts');
    expect(req.request.method).toBe('GET');
    req.flush(mockPosts);
  });
});
```

## Error Handling

### C# Custom Exceptions

```csharp
namespace BlogAPI.Domain.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
```

### C# Global Exception Handler Middleware

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                EntityNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }
}
```

### TypeScript Error Handling

```typescript
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error) => {
        if (error.status === 401) {
          this.router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
}
```

## Documentation Standards

### C# XML Documentation

```csharp
/// <summary>
/// Creates a new blog post for the authenticated user.
/// </summary>
/// <param name="dto">The post data to create</param>
/// <returns>The created post DTO</returns>
/// <exception cref="UnauthorizedException">Thrown when user is not authenticated</exception>
public async Task<PostDto> CreatePostAsync(CreatePostDto dto)
{
}
```

### TypeScript JSDoc

```typescript
/**
 * Retrieves all published blog posts with pagination support
 * @param pageNumber - The page number (1-based)
 * @param pageSize - Number of posts per page
 * @returns Observable of paginated posts
 */
getPosts(pageNumber: number = 1, pageSize: number = 10): Observable<Post[]> {
}
```

## Git Commit Conventions

Use conventional commits:

```
feat: add user authentication
fix: resolve post not showing in list
docs: update README with setup instructions
style: format code according to linter
refactor: simplify service logic
test: add unit tests for AuthService
chore: update dependencies
```

## Code Review Checklist

### C# Code Review
- [ ] SOLID principles followed
- [ ] Proper use of async/await
- [ ] Exception handling implemented
- [ ] Unit tests present (80%+ coverage)
- [ ] XML documentation provided
- [ ] No hardcoded values
- [ ] Naming conventions followed
- [ ] DRY principle applied

### TypeScript/Angular Code Review
- [ ] OnPush change detection used
- [ ] Unsubscribe properly handled
- [ ] Smart/dumb component pattern
- [ ] Unit tests present
- [ ] Type safety maintained
- [ ] No any types
- [ ] Immutability respected
- [ ] Memory leaks prevented

---

**Last Updated**: July 2026
