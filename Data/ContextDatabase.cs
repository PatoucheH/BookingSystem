using Microsoft.EntityFrameworkCore;
using BookingSystem.Models;

namespace BookingSystem.Data
{
    public class ContextDatabase : DbContext
    {
        public ContextDatabase(DbContextOptions<ContextDatabase> options) : base(options) { }

        public virtual DbSet<Guest> Guests { get; set; }
        public virtual DbSet<Owner> Owners { get;set;}
        public virtual DbSet<Properties> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guest>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Username)
                    .IsRequired()
                    .HasMaxLength(25);
                entity.Property(g => g.Password)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(g => g.Email)
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<Owner>(entity =>
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
                    .HasForeignKey(p => p.HostId)
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
                entity.HasOne(p => p.Owner)
                    .WithMany(h => h.Properties)
                    .HasForeignKey(p => p .HostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
