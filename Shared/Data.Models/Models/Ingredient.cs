using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; } // Unit of measurement (e.g., cup, gram)
                                         // You can add other properties as needed (e.g., optional description)

          public ICollection<RecipeIngredient> Recipes { get; set; } // Now using RecipeIngredient


        public Ingredient()
        {
            Recipes = new HashSet<RecipeIngredient>(); // Initialize the collection
        }
    }
}
