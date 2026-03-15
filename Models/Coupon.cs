using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantFoodOrderingAdmin.Models
{
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }

        [Required(ErrorMessage = "Coupon code is required")]
        [StringLength(20, ErrorMessage = "Coupon code cannot exceed 20 characters")]
        public string CouponCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount type is required")]
        [StringLength(20)]
        public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "Fixed"

        [Required(ErrorMessage = "Discount value is required")]
        [Range(0, 100000, ErrorMessage = "Discount value must be between 0 and 100000")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MinimumOrderAmount { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        [Range(1, 100, ErrorMessage = "Usage limit must be between 1 and 100")]
        public int UsageLimit { get; set; } = 1;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<CustomerCoupon>? CustomerCoupons { get; set; }
    }
}
