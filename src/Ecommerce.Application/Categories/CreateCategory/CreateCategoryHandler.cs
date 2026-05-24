using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using Ecommerce.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.CreateCategory;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Result<CreateCategoryResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateCategoryResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var normalizedName = request.Name.Trim();
        var normalizedDescription = request.Description?.Trim() ?? string.Empty;

        var hasDuplicateSlug = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Slug == normalizedSlug, cancellationToken);
        if (hasDuplicateSlug)
        {
            return Result.Failure<CreateCategoryResponse>(BusinessErrorCode.DuplicateSlug, "Category slug already exists.");
        }

        var hasDuplicateName = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Name == normalizedName, cancellationToken);
        if (hasDuplicateName)
        {
            return Result.Failure<CreateCategoryResponse>(BusinessErrorCode.DuplicateName, "Category name already exists.");
        }

        var category = new Category(normalizedName, normalizedSlug, normalizedDescription, request.IsActive);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCategoryResponse(category.Id, category.Name, category.Slug, category.Description, category.IsActive));
    }
}
