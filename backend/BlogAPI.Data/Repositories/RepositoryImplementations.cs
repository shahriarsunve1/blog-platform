using BlogAPI.Data.DbContexts;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Data.Repositories;

/// <summary>
/// Generic repository implementation
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly BlogContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(BlogContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(BlogContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
}

/// <summary>
/// Post repository implementation
/// </summary>
public class PostRepository : GenericRepository<Post>, IPostRepository
{
    public PostRepository(BlogContext context) : base(context)
    {
    }

    public async Task<List<Post>> GetPublishedPostsAsync(int pageNumber, int pageSize, Guid? categoryId = null, Guid? tagId = null, string? search = null, Guid? authorId = null)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLower();

        return await _context.Posts
            .Where(p => p.Status == PostStatus.Published)
            .Where(p => categoryId == null || p.Categories.Any(c => c.Id == categoryId))
            .Where(p => tagId == null || p.Tags.Any(t => t.Id == tagId))
            .Where(p => authorId == null || p.UserId == authorId)
            .Where(p => normalizedSearch == null
                || p.Title.ToLower().Contains(normalizedSearch)
                || p.Excerpt.ToLower().Contains(normalizedSearch)
                || p.Content.ToLower().Contains(normalizedSearch))
            .Include(p => p.Author)
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.PublishedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Post>> GetUserPostsAsync(Guid userId)
    {
        return await _context.Posts
            .Where(p => p.UserId == userId)
            .Include(p => p.Author)
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetPostWithDetailsAsync(Guid id)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<int> GetPublishedPostsCountAsync(Guid? categoryId = null, Guid? tagId = null, string? search = null, Guid? authorId = null)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLower();

        return await _context.Posts
            .Where(p => p.Status == PostStatus.Published)
            .Where(p => categoryId == null || p.Categories.Any(c => c.Id == categoryId))
            .Where(p => tagId == null || p.Tags.Any(t => t.Id == tagId))
            .Where(p => authorId == null || p.UserId == authorId)
            .Where(p => normalizedSearch == null
                || p.Title.ToLower().Contains(normalizedSearch)
                || p.Excerpt.ToLower().Contains(normalizedSearch)
                || p.Content.ToLower().Contains(normalizedSearch))
            .CountAsync();
    }
}

/// <summary>
/// Category repository implementation
/// </summary>
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(BlogContext context) : base(context)
    {
    }

    public async Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Categories.Where(c => ids.Contains(c.Id)).ToListAsync();
    }
}

/// <summary>
/// Tag repository implementation
/// </summary>
public class TagRepository : GenericRepository<Tag>, ITagRepository
{
    public TagRepository(BlogContext context) : base(context)
    {
    }

    public async Task<List<Tag>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Tags.Where(t => ids.Contains(t.Id)).ToListAsync();
    }
}

/// <summary>
/// Comment repository implementation
/// </summary>
public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(BlogContext context) : base(context)
    {
    }

    public async Task<List<Comment>> GetByPostIdAsync(Guid postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}

/// <summary>
/// Like repository implementation
/// </summary>
public class LikeRepository : GenericRepository<Like>, ILikeRepository
{
    public LikeRepository(BlogContext context) : base(context)
    {
    }

    public async Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId)
    {
        return await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
    }
}

/// <summary>
/// Follow repository implementation
/// </summary>
public class FollowRepository : GenericRepository<Follow>, IFollowRepository
{
    public FollowRepository(BlogContext context) : base(context)
    {
    }

    public async Task<Follow?> GetAsync(Guid followerId, Guid followingId)
    {
        return await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<int> GetFollowerCountAsync(Guid userId)
    {
        return await _context.Follows.CountAsync(f => f.FollowingId == userId);
    }
}
