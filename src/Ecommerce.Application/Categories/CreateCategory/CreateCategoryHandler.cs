using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Categories.CreateCategory;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var normalizedName = request.Name.Trim();
        var normalizedDescription = request.Description?.Trim() ?? string.Empty;

        var hasDuplicateSlug = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Slug == normalizedSlug, cancellationToken);
        if (hasDuplicateSlug)
        {
            throw new InvalidOperationException("Category slug already exists.");
        }

        var hasDuplicateName = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Name == normalizedName, cancellationToken);
        if (hasDuplicateName)
        {
            throw new InvalidOperationException("Category name already exists.");
        }

        var category = new Category(normalizedName, normalizedSlug, normalizedDescription, request.IsActive);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResponse(category.Id, category.Name, category.Slug, category.Description, category.IsActive);
    }
}
