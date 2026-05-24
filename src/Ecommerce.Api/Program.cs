using Ecommerce.Application;
using Ecommerce.Infrastructure.Auth;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var supabase = builder.Configuration.GetSection(SupabaseJwtOptions.SectionName).Get<SupabaseJwtOptions>();
        if (supabase is null || string.IsNullOrWhiteSpace(supabase.Url))
        {
            return;
        }

        options.Authority = $"{supabase.Url.TrimEnd('/')}/auth/v1";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(supabase.JwtIssuer),
            ValidIssuer = string.IsNullOrWhiteSpace(supabase.JwtIssuer) ? null : supabase.JwtIssuer,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.Claims.Any(c =>
                (c.Type == "role" || c.Type == "app_role" || c.Type == "user_role") &&
                string.Equals(c.Value, "admin", StringComparison.OrdinalIgnoreCase)));
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await CatalogSeed.SeedDevelopmentCatalogAsync(dbContext);

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
