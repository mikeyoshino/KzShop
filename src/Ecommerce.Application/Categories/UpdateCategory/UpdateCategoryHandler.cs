using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.UpdateCategory;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var normalizedName = request.Name.Trim();
        var normalizedDescription = request.Description?.Trim() ?? string.Empty;

        var category = await _context.Categories
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (category is null)
        {
            throw new InvalidOperationException("Category was not found.");
        }

        var hasDuplicateSlug = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Id != request.Id && x.Slug == normalizedSlug, cancellationToken);
        if (hasDuplicateSlug)
        {
            throw new InvalidOperationException("Category slug already exists.");
        }

        var hasDuplicateName = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Id != request.Id && x.Name == normalizedName, cancellationToken);
        if (hasDuplicateName)
        {
            throw new InvalidOperationException("Category name already exists.");
        }

        category.Update(normalizedName, normalizedSlug, normalizedDescription, request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryResponse(request.Id, normalizedName, normalizedSlug, normalizedDescription, request.IsActive);
    }
}
