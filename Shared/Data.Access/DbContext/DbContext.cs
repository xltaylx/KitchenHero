using Data.Models;
using DataModels.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Data.Access
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext, Access.IDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }

        public DbContext() { } // Parameterless constructor


        public DbContext(DbContextOptions<DbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false); // Optional: Set to false if emails are case-insensitive

                entity.HasIndex(e => e.Email).IsUnique(); // Unique constraint for email


                // Add other model configuration for indexes, relationships, etc.
            });

            modelBuilder.Entity<Ingredient>()
    .Property(i => i.ImageData)
    .IsRequired(false); // Mark ImageData as optional

            modelBuilder.Entity<Ingredient>()
                .Property(i => i.Unit)
                .HasMaxLength(10); // Set maximum length for Unit (e.g., "pcs", "kg")
            modelBuilder.Entity<Ingredient>()
                .Property(i => i.Category)
                .IsRequired() // Make Category a required field
                .HasMaxLength(50); // Set maximum length for Category

            modelBuilder.Entity<Ingredient>()
                .Property(i => i.ScannedFlag)
                .HasDefaultValue(false); // Set default value for ScannedFlag to false


            base.OnModelCreating(modelBuilder);
        }

        async Task<int> IDbContext.SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        EntityEntry<TEntity> IDbContext.Entry<TEntity>(TEntity entity)
        {
            return base.Entry(entity);
        }
    }
}
