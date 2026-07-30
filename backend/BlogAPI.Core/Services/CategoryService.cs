using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;

namespace BlogAPI.Core.Services;

/// <summary>
/// Category service implementation
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Slug = c.Slug })
            .OrderBy(c => c.Name)
            .ToList();
    }
}
