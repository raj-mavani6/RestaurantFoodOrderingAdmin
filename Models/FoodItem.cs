using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantFoodOrderingAdmin.Models
{
    public class FoodItem
    {
        [Key]
        public int FoodId { get; set; }

        [Required]
        [StringLength(200)]
        public string FoodName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public bool IsAvailable { get; set; } = true;

        public bool IsVeg { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
