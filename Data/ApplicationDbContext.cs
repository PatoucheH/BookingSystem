using Microsoft.EntityFrameworkCore;
using BookingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BookingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public virtual DbSet<Properties> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
