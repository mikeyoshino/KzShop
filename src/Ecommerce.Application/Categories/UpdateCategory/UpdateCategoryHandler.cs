using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.UpdateCategory;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Result<UpdateCategoryResponse>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateCategoryResponse>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var normalizedName = request.Name.Trim();
        var normalizedDescription = request.Description?.Trim() ?? string.Empty;

        var category = await _context.Categories
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (category is null)
        {
            return Result.Failure<UpdateCategoryResponse>(BusinessErrorCode.CategoryNotFound, "Category was not found.");
        }

        var hasDuplicateSlug = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Id != request.Id && x.Slug == normalizedSlug, cancellationToken);
        if (hasDuplicateSlug)
        {
            return Result.Failure<UpdateCategoryResponse>(BusinessErrorCode.DuplicateSlug, "Category slug already exists.");
        }

        var hasDuplicateName = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Id != request.Id && x.Name == normalizedName, cancellationToken);
        if (hasDuplicateName)
        {
            return Result.Failure<UpdateCategoryResponse>(BusinessErrorCode.DuplicateName, "Category name already exists.");
        }

        category.Update(normalizedName, normalizedSlug, normalizedDescription, request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateCategoryResponse(request.Id, normalizedName, normalizedSlug, normalizedDescription, request.IsActive));
    }
}
