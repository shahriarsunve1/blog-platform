using BlogAPI.Data.DbContexts;
using BlogAPI.Data.Repositories;
using BlogAPI.Core.Services;
using BlogAPI.Core.Validators;
using BlogAPI.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using System.Text;
using System.Threading.RateLimiting;

namespace BlogAPI.API;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<BlogContext>(options =>
            options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();

        // Services
        services.AddScoped<IAuthService, AuthServiceImpl>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddHttpClient<IEmailService, ResendEmailService>();

        // Validation
        services.AddValidatorsFromAssemblyContaining<RegisterUserDtoValidator>();

        // Authentication
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // CORS
        var corsOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
            {
                builder.WithOrigins(corsOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        // Rate limiting - throttles brute-force-prone (login/register) and
        // cost-heavy (comments, media uploads) endpoints.
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Too many requests. Please try again shortly.",
                    statusCode = StatusCodes.Status429TooManyRequests
                }, cancellationToken);
            };

            options.AddPolicy("auth", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 5,
                        QueueLimit = 0
                    }));

            options.AddPolicy("comments", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.FindFirst("id")?.Value
                        ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 10,
                        QueueLimit = 0
                    }));

            options.AddPolicy("media", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.FindFirst("id")?.Value
                        ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(10),
                        PermitLimit = 20,
                        QueueLimit = 0
                    }));
        });

        // Controllers
        services.AddControllers();

        // Response caching (used for rarely-changing lookups like categories/tags)
        services.AddResponseCaching();

        // Swagger
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Render terminates TLS and forwards plain HTTP internally; trust its
        // X-Forwarded-Proto header so Request.Scheme (used to build absolute media
        // URLs) reports "https" instead of "http", avoiding mixed-content blocking.
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseMiddleware<ErrorHandlingMiddleware>();

        // Swagger stays available in all environments for now (hobby project, no
        // sensitive schema to hide) so the deployed API is easy to explore/test.
        app.UseSwagger();
        app.UseSwaggerUI();

        // Skip HTTPS redirection behind a platform proxy (e.g. Render) that already
        // terminates TLS and forwards plain HTTP internally.
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT")))
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseCors("AllowFrontend");
        app.UseResponseCaching();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
