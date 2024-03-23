using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Instructions { get; set; }
        // You can add other properties as needed (e.g., cooking time, difficulty level)

        public ICollection<RecipeIngredient> Ingredients { get; set; } // Now using RecipeIngredient

        public Recipe()
        {
            Ingredients = new HashSet<RecipeIngredient>(); // Initialize the collection
        }
    }
}
