using DataModels.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Access.Repositories
{
    public class IngredientRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<IngredientRepository> _logger;

        public IngredientRepository(DbContext context, ILogger<IngredientRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Ingredient> AddIngredientAsync(Ingredient ingredient)
        {
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Ingredient added: {Name}", ingredient.Name);
            return ingredient;
        }

        public async Task<IEnumerable<Ingredient>> GetIngredientsAsync()
        {
            return await _context.Ingredients.ToListAsync();
        }

        public async Task<Ingredient> GetIngredientByIdAsync(int id)
        {
            return await _context.Ingredients.FindAsync(id);
        }

        public async Task UpdateIngredientAsync(Ingredient ingredient)
        {
            _context.Ingredients.Update(ingredient);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Ingredient updated: {Name}", ingredient.Name);
        }

        public async Task DeleteIngredientAsync(int id)
        {
            var ingredient = await GetIngredientByIdAsync(id);
            if (ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Ingredient deleted: {Name}", ingredient.Name);
            }
            else
            {
                _logger.LogWarning("Ingredient not found for deletion: {Id}", id);
            }
        }
    }
}
