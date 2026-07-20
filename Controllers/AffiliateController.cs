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

        public AffiliateController(IAffiliateService affiliateService, ApplicationDbContext context)
        {
            _affiliateService = affiliateService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Redirect to the new Admin Affiliate flow in HomeController
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
                return Redirect(response.AffiliateUrl);
            }

            ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Failed to generate affiliate link.");
            return View("Index", request);
        }

        [HttpPost("api/affiliate/payment-success/{slug}")]
        public async Task<IActionResult> PaymentSuccessBySlug(string slug)
        {
            // 1. البحث عن المسوق بالـ Slug
            var affiliate = await _context.AffiliateRequests
                .FirstOrDefaultAsync(a => a.Slug == slug);

            if (affiliate == null)
            {
                return NotFound(new { success = false, message = "Affiliate not found" });
            }

            // 2. البحث عن سجل الدفع الخاص به
            var payment = await _context.AffiliatePayments
                .FirstOrDefaultAsync(p => p.AffiliateRequestId == affiliate.Id);

            // 3. إذا لم يكن له سجل، ننشئ واحداً
            if (payment == null)
            {
                payment = new AffiliatePayment
                {
                    AffiliateRequestId = affiliate.Id,
                    Status = AffiliateStatus.Approved,
                    Count = 1,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AffiliatePayments.Add(payment);
            }
            else
            {
                // إذا كان له سجل، نزيد العداد ونغير الحالة
                payment.Count += 1; // يمكن جعلها = 1 حسب المنطق المطلوب
                payment.Status = AffiliateStatus.Approved;
            }
            
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Payment updated successfully for Affiliate" });
        }
    }
}
