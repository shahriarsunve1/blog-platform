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
    }
}
