using FluentValidation;
using BlogAPI.Core.DTOs;

namespace BlogAPI.Core.Validators;

public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).MaximumLength(500);
        RuleFor(x => x.Excerpt).NotEmpty().MinimumLength(10).MaximumLength(1000);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(50);
        RuleFor(x => x.Status).NotEmpty().Must(s => s is "Draft" or "Published" or "Archived")
            .WithMessage("Status must be one of: Draft, Published, Archived");
    }
}

public class UpdatePostDtoValidator : AbstractValidator<UpdatePostDto>
{
    public UpdatePostDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).MaximumLength(500);
        RuleFor(x => x.Excerpt).NotEmpty().MinimumLength(10).MaximumLength(1000);
        RuleFor(x => x.Content).NotEmpty().MinimumLength(50);
        RuleFor(x => x.Status).NotEmpty().Must(s => s is "Draft" or "Published" or "Archived")
            .WithMessage("Status must be one of: Draft, Published, Archived");
    }
}

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
