using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elwala.Models
{
    public class AffiliatePayment
    {
        [Key]
        public int Id { get; set; }

        public int AffiliateRequestId { get; set; }

        [ForeignKey("AffiliateRequestId")]
        public AffiliateRequest AffiliateRequest { get; set; }

        public AffiliateStatus Status { get; set; } = AffiliateStatus.Pending;
        public int Count { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
