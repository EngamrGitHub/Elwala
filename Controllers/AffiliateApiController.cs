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

            // Create initial payment/status record
            var initialPayment = new AffiliatePayment
            {
                AffiliateRequestId = request.Id,
                Status = AffiliateStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.AffiliatePayments.Add(initialPayment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Affiliate created successfully.", data = request });
        }

        // PUT: api/affiliateapi/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto statusDto)
        {
            var payment = await _context.AffiliatePayments
                                        .Where(p => p.AffiliateRequestId == id)
                                        .OrderByDescending(p => p.CreatedAt)
                                        .FirstOrDefaultAsync();

            if (payment == null)
            {
                return NotFound(new { message = "Payment record for Affiliate not found." });
            }

            if (!Enum.TryParse<AffiliateStatus>(statusDto.Status, true, out var newStatus))
            {
                return BadRequest(new { message = "Invalid status value." });
            }

            var affiliateRequest = await _context.AffiliateRequests.FindAsync(payment.AffiliateRequestId);
            if (newStatus == AffiliateStatus.Approved && payment.Status != AffiliateStatus.Approved)
            {
                if (affiliateRequest != null)
                {
                    affiliateRequest.Count += 1;
                    _context.AffiliateRequests.Update(affiliateRequest);
                }
            }

            payment.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Status updated successfully.", 
                data = payment, 
                currentCount = affiliateRequest?.Count 
            });
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; }
    }
}
