using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantFoodOrderingAdmin.Models
{
    public class Combo
    {
        [Key]
        public int ComboId { get; set; }

        [Required(ErrorMessage = "Combo name is required")]
        [StringLength(200)]
        [Display(Name = "Combo Name")]
        public string ComboName { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Original price is required")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Original Price")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Combo price is required")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Combo Price")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
        public decimal ComboPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Discount %")]
        public decimal Discount { get; set; }

        [StringLength(500)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Vegetarian")]
        public bool IsVeg { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Valid Until")]
        [DataType(DataType.Date)]
        public DateTime? ValidUntil { get; set; }

        // Navigation property for combo items
        public virtual ICollection<ComboItem>? ComboItems { get; set; }
    }
}
