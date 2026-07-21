using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Elwala.Models
{
    public class AffiliatePayment
    {
        [Key]
        public int Id { get; set; }

        public int AffiliateRequestId { get; set; }

        [JsonIgnore]
        [ForeignKey("AffiliateRequestId")]
        public AffiliateRequest? AffiliateRequest { get; set; }

        public AffiliateStatus Status { get; set; } = AffiliateStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
