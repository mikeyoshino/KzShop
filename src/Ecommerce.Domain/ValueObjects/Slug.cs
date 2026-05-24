using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.ValueObjects;

public readonly record struct Slug
{
    public string Value { get; }

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Slug cannot be empty.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        Value = normalized;
    }

    public static implicit operator string(Slug slug) => slug.Value;

    public static explicit operator Slug(string value) => new(value);

    public override string ToString() => Value;
}
