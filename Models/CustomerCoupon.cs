using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantFoodOrderingAdmin.Models
{
    public class CustomerCoupon
    {
        [Key]
        public int CustomerCouponId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int CouponId { get; set; }

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [Required]
        [Range(0, 100, ErrorMessage = "Remaining usage must be between 0 and 100")]
        public int RemainingUsage { get; set; } = 1;

        public DateTime? UsedDate { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        public int? OrderId { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("CouponId")]
        public virtual Coupon? Coupon { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
