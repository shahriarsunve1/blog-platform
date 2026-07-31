using BlogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Data.DbContexts;

/// <summary>
/// Blog database context
/// </summary>
public class BlogContext : DbContext
{
    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<MediaFile> MediaFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasMany(u => u.Posts)
            .WithOne(p => p.Author)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<User>()
            .Property(u => u.EmailOnComment)
            .HasDefaultValue(true);
        modelBuilder.Entity<User>()
            .Property(u => u.EmailOnFollow)
            .HasDefaultValue(true);

        // Post configuration
        modelBuilder.Entity<Post>()
            .HasKey(p => p.Id);
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Categories)
            .WithMany(c => c.Posts);
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts);
        // Full-text search over title/excerpt/content, stored as a generated
        // column (shadow property - not part of the domain model) with a GIN
        // index so search queries can use ts_rank instead of a substring scan.
        modelBuilder.Entity<Post>()
            .HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "english",
                p => new { p.Title, p.Excerpt, p.Content });
        modelBuilder.Entity<Post>()
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN");

        // Category configuration
        modelBuilder.Entity<Category>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        // Tag configuration
        modelBuilder.Entity<Tag>()
            .HasKey(t => t.Id);
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        // Comment configuration
        modelBuilder.Entity<Comment>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Like configuration
        modelBuilder.Entity<Like>()
            .HasKey(l => l.Id);
        modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.PostId, l.UserId })
            .IsUnique();
        modelBuilder.Entity<Like>()
            .HasOne(l => l.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Like>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Follow configuration
        modelBuilder.Entity<Follow>()
            .HasKey(f => f.Id);
        modelBuilder.Entity<Follow>()
            .HasIndex(f => new { f.FollowerId, f.FollowingId })
            .IsUnique();
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.FollowerUser)
            .WithMany()
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.FollowingUser)
            .WithMany()
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        // MediaFile configuration
        modelBuilder.Entity<MediaFile>()
            .HasKey(m => m.Id);
        modelBuilder.Entity<MediaFile>()
            .HasOne(m => m.UploadedBy)
            .WithMany()
            .HasForeignKey(m => m.UploadedByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Starter categories/tags so posts have something to attach to out of the box
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = new Guid("11111111-1111-1111-1111-111111111101"), Name = "Technology", Slug = "technology" },
            new Category { Id = new Guid("11111111-1111-1111-1111-111111111102"), Name = "Lifestyle", Slug = "lifestyle" },
            new Category { Id = new Guid("11111111-1111-1111-1111-111111111103"), Name = "Business", Slug = "business" },
            new Category { Id = new Guid("11111111-1111-1111-1111-111111111104"), Name = "Travel", Slug = "travel" },
            new Category { Id = new Guid("11111111-1111-1111-1111-111111111105"), Name = "Health", Slug = "health" }
        );

        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = new Guid("22222222-2222-2222-2222-222222222201"), Name = "Tutorial", Slug = "tutorial" },
            new Tag { Id = new Guid("22222222-2222-2222-2222-222222222202"), Name = "News", Slug = "news" },
            new Tag { Id = new Guid("22222222-2222-2222-2222-222222222203"), Name = "Opinion", Slug = "opinion" },
            new Tag { Id = new Guid("22222222-2222-2222-2222-222222222204"), Name = "Guide", Slug = "guide" },
            new Tag { Id = new Guid("22222222-2222-2222-2222-222222222205"), Name = "Announcement", Slug = "announcement" }
        );
    }
}
