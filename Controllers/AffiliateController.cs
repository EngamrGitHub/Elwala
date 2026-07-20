using Microsoft.AspNetCore.Mvc;
using Elwala.Models;
using Elwala.Services;

namespace Elwala.Controllers
{
    public class AffiliateController : Controller
    {
        private readonly IAffiliateService _affiliateService;

        public AffiliateController(IAffiliateService affiliateService)
        {
            _affiliateService = affiliateService;
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
    }
}
