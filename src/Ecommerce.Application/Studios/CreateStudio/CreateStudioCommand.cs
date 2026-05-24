using MediatR;

namespace Ecommerce.Application.Studios.CreateStudio;

public record CreateStudioCommand(string Name, string Slug, bool IsActive) : IRequest<CreateStudioResponse>;
