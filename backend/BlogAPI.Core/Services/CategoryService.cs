using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;

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

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto request)
    {
        var slug = SlugHelper.Slugify(request.Name);
        var existing = await _categoryRepository.GetAllAsync();
        if (existing.Any(c => c.Slug == slug))
            throw new InvalidOperationException("A category with that name already exists");

        var category = new Category { Name = request.Name.Trim(), Slug = slug };
        var created = await _categoryRepository.AddAsync(category);
        return new CategoryDto { Id = created.Id, Name = created.Name, Slug = created.Slug };
    }
}
