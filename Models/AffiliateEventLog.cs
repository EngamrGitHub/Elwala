using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Elwala.Models
{
    /// <summary>
    /// Append-only log of lifecycle events received from the ellwaa backend.
    /// The unique index on (AffiliateRequestId, Event, ExternalKey) makes each
    /// event idempotent per external key (e.g. ellwaa requestId / userId), so
    /// duplicate/retried webhook POSTs do not double-count signups, pending
    /// requests, or approved payments.
    /// </summary>
    public class AffiliateEventLog
    {
        [Key]
        public int Id { get; set; }

        public int AffiliateRequestId { get; set; }

        [JsonIgnore]
        [ForeignKey("AffiliateRequestId")]
        public AffiliateRequest? AffiliateRequest { get; set; }

        // "signup" | "payment-pending" | "payment-success"
        [Required]
        [MaxLength(32)]
        public string Event { get; set; } = string.Empty;

        // External dedup key — ellwaa's requestId for payment events, userId
        // for signup. Null events (e.g. bodyless legacy calls) always insert.
        [MaxLength(100)]
        public string? ExternalKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}