using Elwala.Models;

namespace Elwala.Services
{
    public interface IAffiliateService
    {
        Task<AffiliateResponse> GenerateAffiliateLinkAsync(AffiliateRequest request);
    }
}
