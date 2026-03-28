using Microsoft.EntityFrameworkCore;
using MemoryGame.Domain.Cards;
using MemoryGame.Domain.Matches;
using MemoryGame.Domain.Penalties;
using MemoryGame.Domain.Social;
using MemoryGame.Domain.Users;

namespace MemoryGame.Infrastructure.Persistence;

public class MemoryGameDbContext : DbContext
{
    public MemoryGameDbContext(DbContextOptions<MemoryGameDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<PendingRegistration> PendingRegistrations => Set<PendingRegistration>();

    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<SocialNetwork> SocialNetworks => Set<SocialNetwork>();

    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchParticipation> MatchParticipations => Set<MatchParticipation>();

    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Deck> Decks => Set<Deck>();

    public DbSet<Penalty> Penalties => Set<Penalty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.Name)
                .HasMaxLength(50);

            entity.Property(e => e.LastName)
                .HasMaxLength(50);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasConversion(
                    v => v.Value,
                    v => Domain.Users.ValueObjects.Email.Create(v));

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.Avatar);

            entity.Property(e => e.IsGuest)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.VerifiedEmail)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.RegistrationDate)
                .IsRequired();

            entity.HasIndex(e => e.Username)
                .IsUnique();

            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        // UserSessions
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Token)
                .IsUnique();

            entity.HasIndex(e => e.ExpiresAt);
        });

        // PendingRegistrations
        modelBuilder.Entity<PendingRegistration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasConversion(
                    v => v.Value,
                    v => Domain.Users.ValueObjects.Email.Create(v));

            entity.Property(e => e.Pin)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.HashedPassword);

            entity.Property(e => e.ExpirationTime)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.HasIndex(e => e.ExpirationTime);
        });

        // FriendRequests
        modelBuilder.Entity<FriendRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.SenderId)
                .IsRequired();

            entity.Property(e => e.ReceiverId)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired();

            entity.Property(e => e.SentAt)
                .IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SenderId, e.ReceiverId })
                .IsUnique();
        });

        // Friendships
        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.FriendId)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.FriendId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.FriendId })
                .IsUnique();
        });

        // SocialNetworks
        modelBuilder.Entity<SocialNetwork>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Account)
                .HasMaxLength(50);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Matches
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.StartDateTime)
                .IsRequired();

            entity.Property(e => e.EndDateTime);

            entity.Property(e => e.Status)
                .IsRequired();

            entity.HasMany<MatchParticipation>()
                .WithOne()
                .HasForeignKey(e => e.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Status);
        });

        // MatchParticipations
        modelBuilder.Entity<MatchParticipation>(entity =>
        {
            entity.HasKey(e => new { e.MatchId, e.UserId });

            entity.Property(e => e.Score)
                .HasConversion(
                    v => v.Value,
                    v => Domain.Matches.ValueObjects.Score.Create(v));

            entity.Property(e => e.WinnerId);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Decks
        modelBuilder.Entity<Deck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.MatchId)
                .IsRequired();

            entity.HasMany<Card>()
                .WithOne()
                .HasForeignKey(e => e.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Match>()
                .WithMany()
                .HasForeignKey(e => e.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Cards
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.Description)
                .HasMaxLength(80);

            entity.Property(e => e.DeckId)
                .IsRequired();
        });

        // Penalties
        modelBuilder.Entity<Penalty>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Type)
                .IsRequired();

            entity.Property(e => e.Duration)
                .IsRequired();

            entity.Property(e => e.MatchId)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Match>()
                .WithMany()
                .HasForeignKey(e => e.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Duration);
        });
    }
}
