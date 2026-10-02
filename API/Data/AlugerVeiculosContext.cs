using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace API.Data
{
    public class AlugerVeiculosContext : DbContext
    {
        public AlugerVeiculosContext(DbContextOptions<AlugerVeiculosContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; } = null!;

        public DbSet<Contract> Contracts { get; set; } = null!;

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Vehicle> Vehicles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Contract -> Client
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            // Contract -> Vehicle
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Vehicle)
                .WithMany(v => v.Contracts)
                .HasForeignKey(c => c.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);

            // Contract -> User
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // RefreshToken -> User
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.Property(v => v.Brand).HasMaxLength(50);
                entity.Property(v => v.Model).HasMaxLength(50);
                entity.Property(v => v.LicensePlate).HasMaxLength(8);

                entity.Property(v => v.FuelType)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(v => v.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(c => c.FullName).HasMaxLength(150);
                entity.Property(c => c.Email).HasMaxLength(254);
                entity.Property(c => c.Phone).HasMaxLength(9);
                entity.Property(c => c.DriverLicenseNumber).HasMaxLength(20);

                entity.Property(c => c.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Name).HasMaxLength(150);
                entity.Property(u => u.Email).HasMaxLength(254);
                entity.Property(u => u.PasswordHash).HasMaxLength(60);

                entity.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(u => u.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.Property(rt => rt.TokenHash).HasMaxLength(128);
                entity.Property(rt => rt.ReplacedByTokenHash).HasMaxLength(128);
            });

            // Unique
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.DriverLicenseNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash)
                .IsUnique();

            // Availability lookups
            modelBuilder.Entity<Contract>()
                .HasIndex(c => new { c.VehicleId, c.StartDate, c.EndDate });

            //Generate date on add and ignore on update
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.CreatedAt)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            modelBuilder.Entity<Client>()
                .Property(c => c.CreatedAt)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            modelBuilder.Entity<Contract>()
                .Property(c => c.CreatedAt)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.CreatedAt)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            base.OnModelCreating(modelBuilder);
        }
    }
}
