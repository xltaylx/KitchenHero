using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataModels.Models.DbContext
{
    public class KitchenHeroDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public KitchenHeroDbContext(DbContextOptions<KitchenHeroDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } // Users table
        public DbSet<Recipe> Recipes { get; set; } // Recipes table
        public DbSet<Ingredient> Ingredients { get; set; } // Ingredients table
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; } // Associative table for recipe and ingredients

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Username).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.Property(r => r.Name).IsRequired();
                // Add other recipe model configuration (e.g., instructions)
            });

            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.Property(i => i.Name).IsRequired();
                entity.Property(i => i.Unit).IsRequired();
            });

            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId }); // Define composite primary key

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.Ingredients) // Now using ICollection<RecipeIngredient>
                .HasForeignKey(ri => ri.RecipeId);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.Recipes) // Keep this line only
                .HasForeignKey(ri => ri.IngredientId);
        }
    }
}
