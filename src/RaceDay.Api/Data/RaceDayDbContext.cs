using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public sealed class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options)
{
    public DbSet<RaceRole> Roles => Set<RaceRole>();
    public DbSet<RaceDayUser> Users => Set<RaceDayUser>();
    public DbSet<RaceEvent> Events => Set<RaceEvent>();
    public DbSet<RaceCategory> Categories => Set<RaceCategory>();
    public DbSet<RaceEnrolment> Enrolments => Set<RaceEnrolment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RaceRole>(entity =>
        {
            entity.ToTable("Role");
            entity.HasKey(role => role.Id);
            entity.Property(role => role.Name).HasMaxLength(30).IsRequired();
            entity.HasIndex(role => role.Name).IsUnique();
        });

        modelBuilder.Entity<RaceDayUser>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.FullName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(150).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(user => user.Phone).HasMaxLength(25).IsRequired();
            entity.Property(user => user.City).HasMaxLength(80).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasOne(user => user.Role).WithMany(role => role.Users)
                .HasForeignKey(user => user.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RaceEvent>(entity =>
        {
            entity.ToTable("Event");
            entity.HasKey(raceEvent => raceEvent.Id);
            entity.Property(raceEvent => raceEvent.Name).HasMaxLength(150).IsRequired();
            entity.Property(raceEvent => raceEvent.Description).HasMaxLength(500).IsRequired();
            entity.Property(raceEvent => raceEvent.Location).HasMaxLength(150).IsRequired();
            entity.Property(raceEvent => raceEvent.EventType).HasMaxLength(20).IsRequired();
            entity.Property(raceEvent => raceEvent.Status).HasMaxLength(20).IsRequired();
            entity.Property(raceEvent => raceEvent.DistanceKm).HasPrecision(5, 2);
            entity.HasOne<RaceDayUser>().WithMany().HasForeignKey(raceEvent => raceEvent.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RaceCategory>(entity =>
        {
            entity.ToTable("Category");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(50).IsRequired();
            entity.Property(category => category.Fee).HasPrecision(8, 2);
            entity.HasOne<RaceEvent>().WithMany().HasForeignKey(category => category.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RaceEnrolment>(entity =>
        {
            entity.ToTable("Enrolment");
            entity.HasKey(enrolment => enrolment.Id);
            entity.Property(enrolment => enrolment.Status).HasMaxLength(20).IsRequired();
            entity.HasIndex(enrolment => new { enrolment.ParticipantId, enrolment.EventId }).IsUnique();
            entity.HasOne<RaceDayUser>().WithMany().HasForeignKey(enrolment => enrolment.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<RaceEvent>().WithMany().HasForeignKey(enrolment => enrolment.EventId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<RaceCategory>().WithMany().HasForeignKey(enrolment => enrolment.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RaceResult>(entity =>
        {
            entity.ToTable("Result");
            entity.HasKey(result => result.Id);
            entity.HasIndex(result => result.EnrolmentId).IsUnique();
            entity.HasOne<RaceEnrolment>().WithMany().HasForeignKey(result => result.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RaceDayUser>().WithMany().HasForeignKey(result => result.CapturedByOrganiserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
