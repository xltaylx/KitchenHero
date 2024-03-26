using DataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Access.Repositories
{
    public interface IIngredientRepository
    {
        Task<Ingredient> AddIngredientAsync(Ingredient ingredient);
        Task<IEnumerable<Ingredient>> GetIngredientsAsync();
        Task<Ingredient> GetIngredientByIdAsync(int id);
        Task UpdateIngredientAsync(Ingredient ingredient);
        Task DeleteIngredientAsync(int id);
    }

}
