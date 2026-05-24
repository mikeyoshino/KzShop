using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Studios.CreateStudio;

public class CreateStudioHandler : IRequestHandler<CreateStudioCommand, Result<CreateStudioResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateStudioHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateStudioResponse>> Handle(CreateStudioCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var normalizedName = request.Name.Trim();

        var hasDuplicateSlug = await _context.Studios
            .AsNoTracking()
            .AnyAsync(x => x.Slug == normalizedSlug, cancellationToken);
        if (hasDuplicateSlug)
        {
            return Result.Failure<CreateStudioResponse>(BusinessErrorCode.DuplicateSlug, "Studio slug already exists.");
        }

        var hasDuplicateName = await _context.Studios
            .AsNoTracking()
            .AnyAsync(x => x.Name == normalizedName, cancellationToken);
        if (hasDuplicateName)
        {
            return Result.Failure<CreateStudioResponse>(BusinessErrorCode.DuplicateName, "Studio name already exists.");
        }

        var studio = new Studio(normalizedName, normalizedSlug, request.IsActive);
        _context.Studios.Add(studio);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex, "Slug"))
        {
            return Result.Failure<CreateStudioResponse>(BusinessErrorCode.DuplicateSlug, "Studio slug already exists.");
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex, "Name"))
        {
            return Result.Failure<CreateStudioResponse>(BusinessErrorCode.DuplicateName, "Studio name already exists.");
        }

        return Result.Success(new CreateStudioResponse(studio.Id, studio.Name, studio.Slug, studio.IsActive));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception, string propertyName)
    {
        var message = $"{exception.Message} {exception.InnerException?.Message}";
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("unique", StringComparison.OrdinalIgnoreCase)
            && (message.Contains($"IX_Studios_{propertyName}", StringComparison.OrdinalIgnoreCase)
                || message.Contains(propertyName, StringComparison.OrdinalIgnoreCase));
    }
}
