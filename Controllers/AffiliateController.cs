using Microsoft.AspNetCore.Mvc;
using Elwala.Models;
using Elwala.Services;
using Elwala.Data;
using Microsoft.EntityFrameworkCore;

namespace Elwala.Controllers
{
    public class AffiliateController : Controller
    {
        private readonly IAffiliateService _affiliateService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AffiliateController> _logger;

        public AffiliateController(
            IAffiliateService affiliateService, 
            ApplicationDbContext context,
            ILogger<AffiliateController> logger)
        {
            _affiliateService = affiliateService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Affiliate", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Submit(AffiliateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", request);
            }

            var response = await _affiliateService.GenerateAffiliateLinkAsync(request);

            if (response.Success && !string.IsNullOrEmpty(response.AffiliateUrl))
            {
                _logger.LogInformation("Generated affiliate link for partner {FullName} ({Slug})", request.FullName, request.Slug);
                return Redirect(response.AffiliateUrl);
            }

            ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Failed to generate affiliate link.");
            return View("Index", request);
        }

        [HttpPost("api/affiliate/payment-success/{slug}")]
        public async Task<IActionResult> PaymentSuccessBySlug(string slug)
        {
            var affiliate = await _context.AffiliateRequests
                .FirstOrDefaultAsync(a => a.Slug == slug);

            if (affiliate == null)
            {
                _logger.LogWarning("PaymentSuccessBySlug failed: Affiliate with slug '{Slug}' not found.", slug);
                return NotFound(new { success = false, message = "Affiliate not found" });
            }

            // Create a new Approved payment transaction entry
            var payment = new AffiliatePayment
            {
                AffiliateRequestId = affiliate.Id,
                Status = AffiliateStatus.Approved,
                CreatedAt = DateTime.UtcNow
            };

            _context.AffiliatePayments.Add(payment);
            affiliate.Count += 1;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Backend recorded Approved Payment #{PaymentId} for Affiliate #{AffiliateId} ({Slug}). New Count: {Count}", 
                payment.Id, affiliate.Id, slug, affiliate.Count);

            return Ok(new { 
                success = true, 
                message = "Payment recorded successfully",
                paymentId = payment.Id,
                affiliateId = affiliate.Id,
                count = affiliate.Count
            });
        }

        [HttpPost("api/affiliate/payment-record/{slug}")]
        public async Task<IActionResult> PaymentRecordBySlug(string slug, [FromBody] PaymentRecordDto dto)
        {
            var affiliate = await _context.AffiliateRequests
                .FirstOrDefaultAsync(a => a.Slug == slug);

            if (affiliate == null)
            {
                _logger.LogWarning("PaymentRecordBySlug failed: Affiliate with slug '{Slug}' not found.", slug);
                return NotFound(new { success = false, message = "Affiliate not found" });
            }

            if (!Enum.TryParse<AffiliateStatus>(dto.Status, true, out var status))
            {
                return BadRequest(new { success = false, message = "Invalid status. Use Pending, Approved, or Rejected." });
            }

            var payment = new AffiliatePayment
            {
                AffiliateRequestId = affiliate.Id,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };

            _context.AffiliatePayments.Add(payment);

            if (status == AffiliateStatus.Approved)
            {
                affiliate.Count += 1;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Backend recorded Payment #{PaymentId} with Status '{Status}' for Affiliate #{AffiliateId} ({Slug}). Total Count: {Count}", 
                payment.Id, status, affiliate.Id, slug, affiliate.Count);

            return Ok(new { 
                success = true, 
                message = $"Payment with status {status} recorded successfully",
                paymentId = payment.Id,
                affiliateId = affiliate.Id,
                count = affiliate.Count
            });
        }
    }

    public class PaymentRecordDto
    {
        public string Status { get; set; } = "Pending";
    }
}
