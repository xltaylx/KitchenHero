using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Models
{
    public class Ingredient
    {
        public int Id { get; set; } // Primary key for the database table
        public string Name { get; set; }
        public int Quantity { get; set; }

        // Optional field to store image data
        public byte[] ImageData { get; set; }

        public string Unit { get; set; } // Unit of measurement (e.g., "pcs", "kg", "oz")
        public string Category { get; set; } // Category of the item (e.g., "Produce", "Bakery")
        public DateTime? ExpiryDate { get; set; } // Optional expiry date for perishable items
        public bool ScannedFlag { get; set; } // Flag to indicate if the item has been scanned
    }
}
