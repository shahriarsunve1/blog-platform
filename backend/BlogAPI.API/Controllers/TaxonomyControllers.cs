using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using BlogAPI.Core.Services;
using BlogAPI.Core.DTOs;

namespace BlogAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IValidator<CreateCategoryDto> _createValidator;

    public CategoriesController(ICategoryService categoryService, IValidator<CreateCategoryDto> createValidator)
    {
        _categoryService = categoryService;
        _createValidator = createValidator;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<List<CategoryDto>>.Ok(result));
    }

    /// <summary>
    /// Create a new category (any authenticated user)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(CreateCategoryDto request)
    {
        await _createValidator.ValidateAndThrowAsync(request);

        try
        {
            var result = await _categoryService.CreateAsync(request);
            return Ok(ApiResponse<CategoryDto>.Ok(result, "Category created", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CategoryDto>.Fail(ex.Message, 400));
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly IValidator<CreateTagDto> _createValidator;

    public TagsController(ITagService tagService, IValidator<CreateTagDto> createValidator)
    {
        _tagService = tagService;
        _createValidator = createValidator;
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TagDto>>>> GetAll()
    {
        var result = await _tagService.GetAllAsync();
        return Ok(ApiResponse<List<TagDto>>.Ok(result));
    }

    /// <summary>
    /// Create a new tag (any authenticated user)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TagDto>>> Create(CreateTagDto request)
    {
        await _createValidator.ValidateAndThrowAsync(request);

        try
        {
            var result = await _tagService.CreateAsync(request);
            return Ok(ApiResponse<TagDto>.Ok(result, "Tag created", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TagDto>.Fail(ex.Message, 400));
        }
    }
}
