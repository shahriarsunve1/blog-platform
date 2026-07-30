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
