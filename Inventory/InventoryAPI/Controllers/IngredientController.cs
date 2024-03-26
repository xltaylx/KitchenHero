using Data.Access.Repositories;
using DataModels.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace InventoryAPI.Controllers
{
    public class IngredientController : ControllerBase
    {
        private readonly IIngredientRepository _ingredientRepository;
        private readonly ILogger<IngredientController> _logger;
        private readonly IConfiguration _configuration;

        public IngredientController(IIngredientRepository ingredientRepository, ILogger<IngredientController> logger, IConfiguration configuration)
        {
            _ingredientRepository = ingredientRepository;
            _logger = logger;
            _configuration = configuration;
            
        }

        [HttpPost]
        public async Task<IActionResult> AddIngredient([FromBody] Ingredient ingredient)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Generate prompt and image data (replace with placeholder or actual logic)
            string prompt = GeneratePrompt(ingredient.Name); // Replace with actual prompt generation logic
            byte[] imageData = await GenerateImage(prompt, _configuration["OpenAI:SecretKey"]); // Replace with actual image generation

            ingredient.ImageData = imageData;

            var addedIngredient = await _ingredientRepository.AddIngredientAsync(ingredient);

            if (addedIngredient == null)
            {
                return StatusCode(500, "Error adding ingredient.");
            }

            _logger.LogInformation("Ingredient added: {Name}", ingredient.Name);
            return CreatedAtRoute("GetIngredient", new { id = addedIngredient.Id }, addedIngredient);
        }

        [HttpGet]
        public async Task<IActionResult> GetIngredients()
        {
            var ingredients = await _ingredientRepository.GetIngredientsAsync();
            return Ok(ingredients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIngredient(int id)
        {
            var ingredient = await _ingredientRepository.GetIngredientByIdAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            return Ok(ingredient);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] Ingredient ingredient)
        {
            if (id != ingredient.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingIngredient = await _ingredientRepository.GetIngredientByIdAsync(id);
            if (existingIngredient == null)
            {
                return NotFound();
            }

            existingIngredient.Name = ingredient.Name;
            existingIngredient.Quantity = ingredient.Quantity;
            existingIngredient.Unit = ingredient.Unit;
            existingIngredient.Category = ingredient.Category;
            existingIngredient.ExpiryDate = ingredient.ExpiryDate;
            existingIngredient.ScannedFlag = ingredient.ScannedFlag;

            await _ingredientRepository.UpdateIngredientAsync(existingIngredient);

            _logger.LogInformation("Ingredient updated: {Name}", ingredient.Name);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            await _ingredientRepository.DeleteIngredientAsync(id);
            _logger.LogInformation("Ingredient deleted with ID: {Id}", id);
            return NoContent();
        }

        public static string PreprocessText(string text)
        {
            // Regular expression for punctuation characters
            string punctuationRegex = @"[\p{P}]";

            // Convert to lowercase and remove punctuation
            string cleanedText = Regex.Replace(text.ToLower(), punctuationRegex, "");
            return cleanedText;
        }

        public static string GeneratePrompt(string ingredientName)
        {
            return $"A photorealistic image of a {ingredientName} on a white background.";
        }

        public static async Task<byte[]> GenerateImage(string prompt, string secret)
        {
            // Replace with your API access credentials and endpoint URL (specific to your chosen API)

            string apiKey = secret;
            string apiUrl = "https://api.openai.com/v1/images/generate";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var data = new { prompt = prompt, n = 1 }; // Generate 1 image
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();  // Handle non-200 status codes

                // Process the response to extract the generated image data (specific steps depend on the API format)
                var imageData = await response.Content.ReadAsByteArrayAsync();
                return imageData;
            }
        }
    }
}