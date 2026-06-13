using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KatyaKatya.Application.Common.Interfaces;
using KatyaKatya.Domain.Cards;
using KatyaKatya.Domain.Matches;
using KatyaKatya.Domain.Penalties;
using KatyaKatya.Domain.Social;
using KatyaKatya.Domain.Users;
using KatyaKatya.Infrastructure.Persistence;
using KatyaKatya.Infrastructure.Repositories;
using KatyaKatya.Application.Lobbies.Interfaces;
using KatyaKatya.Infrastructure.Lobbies;
using KatyaKatya.Infrastructure.Services;

namespace KatyaKatya.Infrastructure;

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

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPendingRegistrationRepository, PendingRegistrationRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<ISocialRepository, SocialRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IPenaltyRepository, PenaltyRepository>();

        // Lobby (singletons — in-memory state shared across requests)
        services.AddSingleton<ILobbyManager, LobbyManager>();
        services.AddSingleton<IPresenceTracker, PresenceTracker>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
