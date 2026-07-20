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
                Count = 0,
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

            payment.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully.", data = payment });
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; }
    }
}
