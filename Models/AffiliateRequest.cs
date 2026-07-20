using System.ComponentModel.DataAnnotations;

namespace Elwala.Models
{
    public class AffiliateRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; } 



        [Required(ErrorMessage = "Phone Number is required")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; } 

        [Required(ErrorMessage = "Slug is required")]
        [RegularExpression(@"^[a-z0-9_]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and underscores")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "Partner Type is required")]
        [Display(Name = "Partner Type")]
        public PartnerType? Type { get; set; }

        public int Count { get; set; } = 0; // Number of payments/actions

        [Display(Name = "Language")]
        public string LanguageCode { get; set; } = "ar"; // Default Arabic

        [Display(Name = "Registration Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Stores the Key-Value JSON in the database
        public string? PlatformUrlsJson { get; set; }

        // Used by the UI to bind key-value pairs
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Dictionary<string, string> PlatformUrls { get; set; } = new Dictionary<string, string>();
    }

    public enum PartnerType
    {
        YouTuber,
        Influencer,
        Blogger,
        Marketer,
        Agency,
        Other
    }
}
