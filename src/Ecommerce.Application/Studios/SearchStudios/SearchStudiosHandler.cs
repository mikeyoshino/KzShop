using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Studios.SearchStudios;

public class SearchStudiosHandler : IRequestHandler<SearchStudiosQuery, SearchStudiosResponse>
{
    private readonly IApplicationDbContext _context;

    public SearchStudiosHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SearchStudiosResponse> Handle(SearchStudiosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Studios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.Slug.Contains(search));
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Select(x => new StudioSearchItemResponse(x.Id, x.Name, x.Slug, x.IsActive))
            .ToListAsync(cancellationToken);

        return new SearchStudiosResponse(items);
    }
}
