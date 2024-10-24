using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        public string? ImagePath { get; set; }
    }
}
