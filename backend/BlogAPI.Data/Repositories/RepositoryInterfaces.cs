using BlogAPI.Domain.Entities;

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
}

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}

/// <summary>
/// Post repository interface
/// </summary>
public interface IPostRepository : IGenericRepository<Post>
{
    Task<List<Post>> GetPublishedPostsAsync(int pageNumber, int pageSize);
    Task<List<Post>> GetUserPostsAsync(Guid userId);
    Task<Post?> GetPostWithDetailsAsync(Guid id);
    Task<int> GetPublishedPostsCountAsync();
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
