using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantFoodOrderingAdmin.Models
{
    public class ComboItem
    {
        [Key]
        public int ComboItemId { get; set; }

        [Required]
        [Display(Name = "Combo")]
        public int ComboId { get; set; }

        [ForeignKey("ComboId")]
        public virtual Combo? Combo { get; set; }

        [Required]
        [Display(Name = "Food Item")]
        public int FoodId { get; set; }

        [ForeignKey("FoodId")]
        public virtual FoodItem? FoodItem { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; } = 1;
    }
}
