using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Enums;

namespace BlogAPI.Data.Repositories;

/// <summary>
/// Generic repository interface
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
    Task<int> CountAsync();
}

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<List<User>> GetRecentAsync(int count);
}

/// <summary>
/// Post repository interface
/// </summary>
public interface IPostRepository : IGenericRepository<Post>
{
    Task<List<Post>> GetPublishedPostsAsync(int pageNumber, int pageSize, Guid? categoryId = null, Guid? tagId = null, string? search = null, Guid? authorId = null);
    Task<List<Post>> GetUserPostsAsync(Guid userId);
    Task<Post?> GetPostWithDetailsAsync(Guid id);
    Task<int> GetPublishedPostsCountAsync(Guid? categoryId = null, Guid? tagId = null, string? search = null, Guid? authorId = null);
    Task<int> CountByStatusAsync(PostStatus status);
    Task<List<Post>> GetRecentAsync(int count);
}

/// <summary>
/// Category repository interface
/// </summary>
public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> ids);
}

/// <summary>
/// Tag repository interface
/// </summary>
public interface ITagRepository : IGenericRepository<Tag>
{
    Task<List<Tag>> GetByIdsAsync(IEnumerable<Guid> ids);
}

/// <summary>
/// Comment repository interface
/// </summary>
public interface ICommentRepository : IGenericRepository<Comment>
{
    Task<List<Comment>> GetByPostIdAsync(Guid postId);
}

/// <summary>
/// Like repository interface
/// </summary>
public interface ILikeRepository : IGenericRepository<Like>
{
    Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId);
}

/// <summary>
/// Follow repository interface
/// </summary>
public interface IFollowRepository : IGenericRepository<Follow>
{
    Task<Follow?> GetAsync(Guid followerId, Guid followingId);
    Task<int> GetFollowerCountAsync(Guid userId);
}
