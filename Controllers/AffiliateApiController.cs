using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Elwala.Data;
using Elwala.Models;

namespace Elwala.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AffiliateApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AffiliateApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/affiliateapi (For browser testing)
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "The Affiliate API is running successfully!" });
        }

        // POST: api/affiliateapi
        [HttpPost]
        public async Task<IActionResult> CreateAffiliate([FromBody] AffiliateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            request.CreatedAt = DateTime.UtcNow;

            _context.AffiliateRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Affiliate created successfully.", data = request });
        }

        // POST: api/affiliateapi/visitor/{slug} or api/affiliate/visitor/{slug}
        [HttpPost("visitor/{slug}")]
        [HttpPost("/api/affiliate/visitor/{slug}")]
        [HttpPost("/api/affiliate/visit/{slug}")]
        public async Task<IActionResult> TrackVisitorBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return BadRequest(new { success = false, message = "Slug is required" });
            }

            var cleanSlug = slug.ToLower().Trim();
            var affiliate = await _context.AffiliateRequests
                .FirstOrDefaultAsync(a => a.Slug == cleanSlug);

            if (affiliate == null)
            {
                return NotFound(new { success = false, message = "Affiliate not found" });
            }

            affiliate.VisitsCount += 1;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Visitor count updated successfully",
                slug = affiliate.Slug,
                visitsCount = affiliate.VisitsCount
            });
        }

        // GET: api/affiliateapi/visitor/{slug} or api/affiliate/visitor/{slug}
        [HttpGet("visitor/{slug}")]
        [HttpGet("/api/affiliate/visitor/{slug}")]
        [HttpGet("/api/affiliate/visit/{slug}")]
        public async Task<IActionResult> GetVisitorBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return BadRequest(new { success = false, message = "Slug is required" });
            }

            var cleanSlug = slug.ToLower().Trim();
            var affiliate = await _context.AffiliateRequests
                .FirstOrDefaultAsync(a => a.Slug == cleanSlug);

            if (affiliate == null)
            {
                return NotFound(new { success = false, message = "Affiliate not found" });
            }

            // INCREMENT EVEN ON GET REQUEST, just in case frontend tracks via GET
            affiliate.VisitsCount += 1;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                id = affiliate.Id,
                fullName = affiliate.FullName,
                slug = affiliate.Slug,
                visitsCount = affiliate.VisitsCount,
                successCount = affiliate.Count
            });
        }

        // PUT: api/affiliateapi/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto statusDto)
        {
            if (!Enum.TryParse<AffiliateStatus>(statusDto.Status, true, out var newStatus))
            {
                return BadRequest(new { message = "Invalid status value." });
            }

            var affiliateRequest = await _context.AffiliateRequests.FindAsync(id);
            if (affiliateRequest == null)
            {
                return NotFound(new { message = "Affiliate not found." });
            }

            var payment = await _context.AffiliatePayments
                                        .Where(p => p.AffiliateRequestId == id)
                                        .OrderByDescending(p => p.CreatedAt)
                                        .FirstOrDefaultAsync();

            if (payment == null)
            {
                payment = new AffiliatePayment
                {
                    AffiliateRequestId = id,
                    Status = newStatus,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AffiliatePayments.Add(payment);

                if (newStatus == AffiliateStatus.Approved)
                {
                    affiliateRequest.Count += 1;
                }
            }
            else
            {
                if (newStatus == AffiliateStatus.Approved && payment.Status != AffiliateStatus.Approved)
                {
                    affiliateRequest.Count += 1;
                }
                payment.Status = newStatus;
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Status updated successfully.", 
                data = payment, 
                currentCount = affiliateRequest.Count 
            });
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; }
    }
}
