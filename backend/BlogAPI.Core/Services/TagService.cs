using BlogAPI.Core.DTOs;
using BlogAPI.Data.Repositories;

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
}
