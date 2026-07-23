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

            // Check if slug is unique
            if (!string.IsNullOrEmpty(cleanSlug))
            {
                var isSlugTaken = await _dbContext.AffiliateRequests.AnyAsync(a => a.Slug == cleanSlug);
                if (isSlugTaken)
                {
                    ModelState.AddModelError("Slug", "This URL Slug is already taken by another affiliate. Please choose a different one.");
                    return View(model);
                }
            }

            // 1. Save to database directly
            _dbContext.AffiliateRequests.Add(model);
            await _dbContext.SaveChangesAsync();

            // Default to 'ar' if somehow not provided
            var lang = string.IsNullOrWhiteSpace(model.LanguageCode) ? "ar" : model.LanguageCode;
            var uniqueUrl = $"{Request.Scheme}://{Request.Host}/go/{cleanSlug}";
            
            var result = new AffiliateResponse 
            {
                Success = true,
                AffiliateUrl = uniqueUrl
            };
            
            return View("AffiliateResult", result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Dashboard(
            string? search,
            PartnerType? partnerType,
            AffiliateStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            string? sortBy = "date_desc",
            int page = 1)
        {
            int pageSize = 10;
            var query = _dbContext.AffiliateRequests
                                  .Include(a => a.Payments)
                                  .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a => (a.FullName != null && a.FullName.ToLower().Contains(term)) ||
                                         (a.PhoneNumber != null && a.PhoneNumber.ToLower().Contains(term)) ||
                                         (a.Slug != null && a.Slug.ToLower().Contains(term)));
            }

            // Partner Type filter
            if (partnerType.HasValue)
            {
                query = query.Where(a => a.Type == partnerType.Value);
            }

            // Status filter
            if (status.HasValue)
            {
                query = query.Where(a => a.Payments.Any(p => p.Status == status.Value));
            }

            // Date Range filters
            if (fromDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt.Date <= toDate.Value.Date);
            }

            // KPI Counts across database
            int totalAffiliates = await query.CountAsync();
            int totalSuccessCount = await _dbContext.AffiliatePayments.CountAsync(p => p.Status == AffiliateStatus.Approved);
            int totalVisits = await _dbContext.AffiliateRequests.SumAsync(a => a.VisitsCount);

            // Sorting
            switch (sortBy?.ToLower())
            {
                case "date_asc":
                    query = query.OrderBy(a => a.CreatedAt);
                    break;
                case "count_desc":
                    query = query.OrderByDescending(a => a.Count);
                    break;
                case "count_asc":
                    query = query.OrderBy(a => a.Count);
                    break;
                case "visits_desc":
                    query = query.OrderByDescending(a => a.VisitsCount);
                    break;
                case "visits_asc":
                    query = query.OrderBy(a => a.VisitsCount);
                    break;
                case "name_asc":
                    query = query.OrderBy(a => a.FullName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(a => a.FullName);
                    break;
                case "date_desc":
                default:
                    query = query.OrderByDescending(a => a.CreatedAt);
                    break;
            }

            int totalPages = (int)Math.Ceiling(totalAffiliates / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            var affiliates = await query.Skip((page - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

            // Deserialize PlatformUrls for each affiliate
            foreach (var item in affiliates)
            {
                if (!string.IsNullOrEmpty(item.PlatformUrlsJson))
                {
                    try
                    {
                        item.PlatformUrls = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(item.PlatformUrlsJson) 
                                            ?? new Dictionary<string, string>();
                    }
                    catch
                    {
                        item.PlatformUrls = new Dictionary<string, string>();
                    }
                }
            }

            ViewBag.Search = search;
            ViewBag.PartnerType = partnerType;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalAffiliates = totalAffiliates;
            ViewBag.TotalSuccessCount = totalSuccessCount;
            ViewBag.TotalVisits = totalVisits;

            return View(affiliates);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var affiliate = await _dbContext.AffiliateRequests
                                            .Include(a => a.Payments)
                                            .FirstOrDefaultAsync(a => a.Id == id);
            if (affiliate == null)
            {
                return NotFound();
            }

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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var uniqueUrl = $"{Request.Scheme}://{Request.Host}/go/{affiliate.Slug}";
                var approvedPayments = affiliate.Payments.Count(p => p.Status == AffiliateStatus.Approved);
                var successCount = Math.Max(affiliate.Count, approvedPayments);

                return Json(new
                {
                    id = affiliate.Id,
                    fullName = affiliate.FullName,
                    phoneNumber = affiliate.PhoneNumber,
                    slug = affiliate.Slug,
                    type = affiliate.Type?.ToString(),
                    languageCode = affiliate.LanguageCode,
                    createdAt = affiliate.CreatedAt.ToString("yyyy-MMM-dd HH:mm"),
                    count = affiliate.Count,
                    visitsCount = affiliate.VisitsCount,
                    successCount = successCount,
                    uniqueUrl = uniqueUrl,
                    platformUrls = affiliate.PlatformUrls,
                    payments = affiliate.Payments.Select(p => new
                    {
                        id = p.Id,
                        status = p.Status.ToString(),
                        createdAt = p.CreatedAt.ToString("yyyy-MMM-dd HH:mm")
                    })
                });
            }

            return View(affiliate);
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
            affiliate.LanguageCode = model.LanguageCode;

            // Handle Slug uniquely
            var cleanSlug = model.Slug?.ToLower().Trim();
            if (!string.IsNullOrEmpty(cleanSlug))
            {
                var isSlugTaken = await _dbContext.AffiliateRequests.AnyAsync(a => a.Slug == cleanSlug && a.Id != id);
                if (isSlugTaken)
                {
                    ModelState.AddModelError("Slug", "This URL Slug is already taken by another affiliate. Please choose a different one.");
                    return View(model);
                }
            }
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PaymentsDashboard(DateTime? startDate, DateTime? endDate, int page = 1)
        {
            int pageSize = 5;
            var query = _dbContext.AffiliatePayments.Include(p => p.AffiliateRequest).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date <= endDate.Value.Date);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var payments = await query.OrderByDescending(p => p.Id)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.AllAffiliates = await _dbContext.AffiliateRequests.OrderBy(a => a.FullName).ToListAsync();

            return View(payments);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
