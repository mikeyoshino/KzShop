using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Studios.CreateStudio;

public class CreateStudioHandler : IRequestHandler<CreateStudioCommand, CreateStudioResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateStudioHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateStudioResponse> Handle(CreateStudioCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var normalizedName = request.Name.Trim();

        var hasDuplicateSlug = await _context.Studios
            .AsNoTracking()
            .AnyAsync(x => x.Slug == normalizedSlug, cancellationToken);
        if (hasDuplicateSlug)
        {
            throw new InvalidOperationException("Studio slug already exists.");
        }

        var hasDuplicateName = await _context.Studios
            .AsNoTracking()
            .AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower(), cancellationToken);
        if (hasDuplicateName)
        {
            throw new InvalidOperationException("Studio name already exists.");
        }

        var studio = new Studio(normalizedName, normalizedSlug, request.IsActive);
        _context.Studios.Add(studio);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateStudioResponse(studio.Id, studio.Name, studio.Slug, studio.IsActive);
    }
}
