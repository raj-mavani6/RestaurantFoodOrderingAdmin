using System.ComponentModel.DataAnnotations;

namespace RestaurantFoodOrderingAdmin.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsVeg { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
