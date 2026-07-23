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

        /// <summary>
        /// Unified lifecycle-event endpoint called by the ellwaa backend.
        /// POST /api/affiliate/event/{slug} with body { event, userId, requestId, amount, currency }.
        /// Switches on the event and increments the matching counter, with
        /// idempotency by external key (requestId for payments, userId for signup)
        /// so duplicate/retried webhooks do not double-count.
        /// </summary>
        [HttpPost("api/affiliate/event/{slug}")]
        public async Task<IActionResult> TrackEventBySlug(string slug, [FromBody] AffiliateEventDto? dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Event))
            {
                return BadRequest(new { success = false, message = "Missing 'event' in body." });
            }

            var cleanSlug = slug?.ToLower().Trim();
            var affiliate = await _context.AffiliateRequests
                .FirstOrDefaultAsync(a => a.Slug == cleanSlug);

            if (affiliate == null)
            {
                _logger.LogWarning("TrackEventBySlug failed: Affiliate with slug '{Slug}' not found.", slug);
                return NotFound(new { success = false, message = "Affiliate not found" });
            }

            var evt = dto.Event.ToLower().Trim();
            // External dedup key: requestId for payment events, userId for signup.
            var externalKey = !string.IsNullOrWhiteSpace(dto.RequestId)
                ? dto.RequestId.Trim()
                : !string.IsNullOrWhiteSpace(dto.UserId)
                    ? dto.UserId.Trim()
                    : null;

            // Idempotency: if we already logged this (affiliate, event, key),
            // return success without re-incrementing.
            if (externalKey != null)
            {
                var alreadyLogged = await _context.AffiliateEventLogs
                    .AnyAsync(e => e.AffiliateRequestId == affiliate.Id
                        && e.Event == evt
                        && e.ExternalKey == externalKey);
                if (alreadyLogged)
                {
                    _logger.LogInformation("TrackEventBySlug duplicate ignored: event '{Event}' key '{Key}' for Affiliate #{Id} ({Slug}).",
                        evt, externalKey, affiliate.Id, slug);
                    return Ok(new
                    {
                        success = true,
                        message = "Event already recorded (duplicate ignored).",
                        duplicate = true,
                        signupCount = affiliate.SignupCount,
                        pendingCount = affiliate.PendingCount,
                        count = affiliate.Count
                    });
                }
            }

            switch (evt)
            {
                case "signup":
                    affiliate.SignupCount += 1;
                    break;

                case "payment-pending":
                    affiliate.PendingCount += 1;
                    break;

                case "payment-success":
                    // Record an Approved payment row and bump the approved Count,
                    // unless one already exists for this external key (guarded
                    // by the AffiliateEventLog check above).
                    var payment = new AffiliatePayment
                    {
                        AffiliateRequestId = affiliate.Id,
                        Status = AffiliateStatus.Approved,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AffiliatePayments.Add(payment);
                    affiliate.Count += 1;
                    break;

                default:
                    return BadRequest(new { success = false, message = $"Unknown event '{evt}'. Use signup, payment-pending, or payment-success." });
            }

            // Log the event for idempotency on future retries.
            _context.AffiliateEventLogs.Add(new AffiliateEventLog
            {
                AffiliateRequestId = affiliate.Id,
                Event = evt,
                ExternalKey = externalKey,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("TrackEventBySlug recorded event '{Event}' key '{Key}' for Affiliate #{Id} ({Slug}). Signup={Signup} Pending={Pending} Approved={Count}",
                evt, externalKey, affiliate.Id, slug, affiliate.SignupCount, affiliate.PendingCount, affiliate.Count);

            return Ok(new
            {
                success = true,
                message = $"Event '{evt}' recorded successfully.",
                event_ = evt,
                signupCount = affiliate.SignupCount,
                pendingCount = affiliate.PendingCount,
                count = affiliate.Count,
                visitsCount = affiliate.VisitsCount
            });
        }
    }

    public class PaymentRecordDto
    {
        public string Status { get; set; } = "Pending";
    }

    /// <summary>
    /// Event payload sent by the ellwaa backend's affiliate notifier.
    /// POST {AFFILIATE_WEBHOOK_URL}/{slug} with this JSON body.
    /// </summary>
    public class AffiliateEventDto
    {
        // "signup" | "payment-pending" | "payment-success"
        public string? Event { get; set; }
        public string? UserId { get; set; }
        public string? RequestId { get; set; }
        public int? Amount { get; set; }
        public string? Currency { get; set; }
    }
}
