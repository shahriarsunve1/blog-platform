using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Core.Services;

/// <summary>
/// Tag service implementation
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<List<TagDto>> GetAllAsync()
    {
        var tags = await _tagRepository.GetAllAsync();
        return tags
            .Select(t => new TagDto { Id = t.Id, Name = t.Name, Slug = t.Slug })
            .OrderBy(t => t.Name)
            .ToList();
    }

    public async Task<TagDto> CreateAsync(CreateTagDto request)
    {
        var slug = SlugHelper.Slugify(request.Name);
        var existing = await _tagRepository.GetAllAsync();
        if (existing.Any(t => t.Slug == slug))
            throw new InvalidOperationException("A tag with that name already exists");

        var tag = new Tag { Name = request.Name.Trim(), Slug = slug };
        var created = await _tagRepository.AddAsync(tag);
        return new TagDto { Id = created.Id, Name = created.Name, Slug = created.Slug };
    }
}
