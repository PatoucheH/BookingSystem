using Microsoft.EntityFrameworkCore;
using BookingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BookingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public virtual DbSet<User> Users { get;set;}
        public virtual DbSet<Properties> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Username)
                    .IsRequired()
                    .HasMaxLength(25);
                entity.Property(h => h.Password)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(h => h.Email)
                    .HasMaxLength(150);
                entity.HasMany(h => h.Properties)
                    .WithOne(p => p.Owner)
                    .HasForeignKey(p => p.OwnerId)
                    ;
            });

            modelBuilder.Entity<Properties>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Town)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(p => p.Country)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(p => p.Type)
                    .IsRequired();
                entity.Property(p => p.Description)
                    .HasMaxLength(250);
                entity.Property(p => p.Title)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(p => p.Price)
                    .IsRequired();
                entity.Property(p => p.Photo)
                    .HasMaxLength(250);
                entity.Property(p => p.GuestNbr)
                    .IsRequired();
                entity.HasOne(p => p.Owner)
                    .WithMany(h => h.Properties)
                    .HasForeignKey(p => p .OwnerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


        }

    }
}
