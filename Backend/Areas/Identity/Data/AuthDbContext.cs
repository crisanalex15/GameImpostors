using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Models.Questions;

namespace Backend.Areas.Identity.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<GameObject> Games { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Round> Rounds { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Vote> Votes { get; set; }

    // Questions and Words Tables
    public DbSet<Question> Questions { get; set; }
    public DbSet<WordHidden> WordHiddens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);



        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");

            // Configurăm coloanele personalizate
            b.Property(u => u.UserName).HasColumnType("TEXT");
            b.Property(u => u.CreatedAt).HasColumnType("TEXT");
            b.Property(u => u.LastModifiedAt).HasColumnType("TEXT");
            b.Property(u => u.LastLoginAt).HasColumnType("TEXT");
            b.Property(u => u.IsEmailVerified).HasColumnType("INTEGER");
            b.Property(u => u.EmailVerifiedAt).HasColumnType("TEXT");
            b.Property(u => u.RefreshToken).HasColumnType("TEXT");
            b.Property(u => u.RefreshTokenExpiryTime).HasColumnType("TEXT");
            b.Property(u => u.VerificationCode).HasColumnType("TEXT");
            b.Property(u => u.VerificationCodeExpiryTime).HasColumnType("TEXT");
            b.Property(u => u.ResetPasswordCode).HasColumnType("TEXT");
            b.Property(u => u.VerificationCodePasswordExpiryTime).HasColumnType("TEXT");

        });

        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        builder.Entity<IdentityRole>(b =>
        {
            b.ToTable("Roles");
        });
        builder.Entity<IdentityUserRole<string>>(b =>
        {
            b.ToTable("UserRoles");
        });
        builder.Entity<IdentityUserClaim<string>>(b =>
        {
            b.ToTable("UserClaims");
        });
        builder.Entity<IdentityUserLogin<string>>(b =>
        {
            b.ToTable("UserLogins");
        });

        builder.Entity<GameObject>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.LobbyCode).IsRequired().HasMaxLength(6);
                entity.Property(g => g.LobbyPrivateCode).HasMaxLength(6);
                entity.Property(g => g.MaxPlayers).HasDefaultValue(6);
                entity.Property(g => g.CurrentPlayers).HasDefaultValue(0);
                entity.Property(g => g.TimerDuration).HasDefaultValue(120);
                entity.Property(g => g.CreatedAt).HasDefaultValue(DateTime.UtcNow);
                entity.Property(g => g.UpdatedAt).HasDefaultValue(DateTime.UtcNow);

                // Relationships
                entity.HasMany(g => g.Players)
                      .WithOne(p => p.Game)
                      .HasForeignKey(p => p.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(g => g.Rounds)
                      .WithOne(r => r.Game)
                      .HasForeignKey(r => r.GameId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        // Configure Player relationships
        builder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Score).HasDefaultValue(0);
            entity.Property(p => p.IsReady).HasDefaultValue(false);
            entity.Property(p => p.IsEliminated).HasDefaultValue(false);
            entity.Property(p => p.JoinedAt).HasDefaultValue(DateTime.UtcNow);

            // Relationships
            entity.HasMany(p => p.Answers)
                  .WithOne(a => a.Player)
                  .HasForeignKey(a => a.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Votes)
                  .WithOne(v => v.Voter)
                  .HasForeignKey(v => v.VoterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.VotesReceived)
                  .WithOne(v => v.Target)
                  .HasForeignKey(v => v.TargetId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Round relationships
        builder.Entity<Round>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TimeLimit).HasDefaultValue(120);
            entity.Property(r => r.StartedAt).HasDefaultValue(DateTime.UtcNow);

            // Relationships
            entity.HasMany(r => r.Answers)
                  .WithOne(a => a.Round)
                  .HasForeignKey(a => a.RoundId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Votes)
                  .WithOne(v => v.Round)
                  .HasForeignKey(v => v.RoundId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Answer
        builder.Entity<Answer>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Value).IsRequired().HasMaxLength(500);
            entity.Property(a => a.CreatedAt).HasDefaultValue(DateTime.UtcNow);
            entity.Property(a => a.IsEdited).HasDefaultValue(false);
        });

        // Configure Vote
        builder.Entity<Vote>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Reason).HasMaxLength(200);
            entity.Property(v => v.CreatedAt).HasDefaultValue(DateTime.UtcNow);
        });

        // Configure Question
        builder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.QuestionText).IsRequired().HasMaxLength(500);
            entity.Property(q => q.FakeQuestionText).IsRequired().HasMaxLength(500);
            entity.Property(q => q.Category).IsRequired().HasMaxLength(50);
            entity.Property(q => q.Difficulty).HasDefaultValue(1);
            entity.Property(q => q.PlayerReview).HasDefaultValue(0);
            entity.Property(q => q.ReviewCount).HasDefaultValue(0);
            entity.Property(q => q.IsActive).HasDefaultValue(true);
            entity.Property(q => q.CreatedAt).HasDefaultValue(DateTime.UtcNow);

            // Index for better performance
            entity.HasIndex(q => q.Category);
            entity.HasIndex(q => q.Difficulty);
            entity.HasIndex(q => q.PlayerReview);
            entity.HasIndex(q => q.IsActive);
        });

        // Configure WordHidden
        builder.Entity<WordHidden>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Word).IsRequired().HasMaxLength(100);
            entity.Property(w => w.FakeWord).IsRequired().HasMaxLength(100);
            entity.Property(w => w.Category).IsRequired().HasMaxLength(50);
            entity.Property(w => w.Difficulty).HasDefaultValue(1);
            entity.Property(w => w.PlayerReview).HasDefaultValue(0);
            entity.Property(w => w.ReviewCount).HasDefaultValue(0);
            entity.Property(w => w.IsActive).HasDefaultValue(true);
            entity.Property(w => w.CreatedAt).HasDefaultValue(DateTime.UtcNow);

            // Index for better performance
            entity.HasIndex(w => w.Category);
            entity.HasIndex(w => w.Difficulty);
            entity.HasIndex(w => w.PlayerReview);
            entity.HasIndex(w => w.IsActive);
        });

    }
}
