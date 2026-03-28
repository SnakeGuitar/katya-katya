using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MemoryGame.Domain.Cards;
using MemoryGame.Domain.Matches;
using MemoryGame.Domain.Penalties;
using MemoryGame.Domain.Social;
using MemoryGame.Domain.Users;
using MemoryGame.Infrastructure.Persistence;
using MemoryGame.Infrastructure.Repositories;

namespace MemoryGame.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<MemoryGameDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISocialRepository, SocialRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IPenaltyRepository, PenaltyRepository>();

        return services;
    }
}
