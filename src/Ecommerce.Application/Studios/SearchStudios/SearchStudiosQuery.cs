using MediatR;

namespace Ecommerce.Application.Studios.SearchStudios;

public record SearchStudiosQuery(string? Search) : IRequest<SearchStudiosResponse>;
