using BlogAPI.Core.DTOs;
using BlogAPI.Core.Services;
using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using Moq;
using Xunit;

namespace BlogAPI.Tests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_categoryRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_NewName_CreatesWithSlug()
    {
        _categoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());
        _categoryRepository.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

        var result = await _sut.CreateAsync(new CreateCategoryDto { Name = "Machine Learning" });

        Assert.Equal("Machine Learning", result.Name);
        Assert.Equal("machine-learning", result.Slug);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSlug_Throws()
    {
        _categoryRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Category> { new() { Id = Guid.NewGuid(), Name = "Technology", Slug = "technology" } });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(new CreateCategoryDto { Name = "Technology" }));
    }
}

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _tagRepository = new();
    private readonly TagService _sut;

    public TagServiceTests()
    {
        _sut = new TagService(_tagRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_NewName_CreatesWithSlug()
    {
        _tagRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Tag>());
        _tagRepository.Setup(r => r.AddAsync(It.IsAny<Tag>())).ReturnsAsync((Tag t) => t);

        var result = await _sut.CreateAsync(new CreateTagDto { Name = "Deep Dive" });

        Assert.Equal("Deep Dive", result.Name);
        Assert.Equal("deep-dive", result.Slug);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSlug_Throws()
    {
        _tagRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Tag> { new() { Id = Guid.NewGuid(), Name = "Guide", Slug = "guide" } });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(new CreateTagDto { Name = "guide" }));
    }
}
