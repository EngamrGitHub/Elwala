using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Elwala.Models;
using Elwala.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Elwala.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Affiliate()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Affiliate(AffiliateRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Serialize platform URLs
            if (model.PlatformUrls != null && model.PlatformUrls.Any())
            {
                model.PlatformUrlsJson = System.Text.Json.JsonSerializer.Serialize(model.PlatformUrls);
            }

            // Ensure the Slug is clean
            var cleanSlug = model.Slug?.ToLower().Trim();
            model.Slug = cleanSlug;

            // 1. Save to database directly
            _dbContext.AffiliateRequests.Add(model);
            await _dbContext.SaveChangesAsync();

            // Default to 'ar' if somehow not provided
            var lang = string.IsNullOrWhiteSpace(model.LanguageCode) ? "ar" : model.LanguageCode;
            var uniqueUrl = $"https://assis.ellwaa.com/{lang}/create-request?ref={Uri.EscapeDataString(cleanSlug ?? string.Empty)}";
            
            var result = new AffiliateResponse 
            {
                Success = true,
                AffiliateUrl = uniqueUrl
            };
            
            return View("AffiliateResult", result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            int pageSize = 5;
            var query = _dbContext.AffiliateRequests.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt.Date <= toDate.Value.Date);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Fetch affiliates descending
            var affiliates = await query.OrderByDescending(a => a.Id)
                                          .Skip((page - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(affiliates);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var affiliate = await _dbContext.AffiliateRequests.FindAsync(id);
            if (affiliate == null)
            {
                return NotFound();
            }
            
            // Deserialize platforms if present
            if (!string.IsNullOrEmpty(affiliate.PlatformUrlsJson))
            {
                try
                {
                    affiliate.PlatformUrls = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(affiliate.PlatformUrlsJson) 
                                             ?? new Dictionary<string, string>();
                }
                catch
                {
                    affiliate.PlatformUrls = new Dictionary<string, string>();
                }
            }
            return View(affiliate);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, AffiliateRequest model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var affiliate = await _dbContext.AffiliateRequests.FindAsync(id);
            if (affiliate == null)
            {
                return NotFound();
            }

            // Update simple fields
            affiliate.FullName = model.FullName;
            affiliate.PhoneNumber = model.PhoneNumber;
            affiliate.Type = model.Type;
            affiliate.Count = model.Count;
            affiliate.LanguageCode = model.LanguageCode;

            // Handle Slug uniquely
            var cleanSlug = model.Slug?.ToLower().Trim();
            affiliate.Slug = cleanSlug;

            // Update JSON
            if (model.PlatformUrls != null && model.PlatformUrls.Any())
            {
                affiliate.PlatformUrlsJson = System.Text.Json.JsonSerializer.Serialize(model.PlatformUrls);
            }
            else
            {
                affiliate.PlatformUrlsJson = null;
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
