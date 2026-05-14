using System.Security.Claims;

namespace FootHeatmapAnalyzer.Web.Tenancy;

/// <summary>
/// Reads tenant information from Identity claims and headers.
/// </summary>
public sealed class HttpTenantContextAccessor(IHttpContextAccessor httpContextAccessor) : ITenantContextAccessor
{
    /// <inheritdoc />
    public TenantContext GetCurrent()
    {
        var context = httpContextAccessor.HttpContext;
        var tenantId = context?.User.FindFirst("tenant_id")?.Value
            ?? context?.Request.Headers["X-Tenant-Id"].FirstOrDefault()
            ?? "demo";
        var userId = context?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context?.Request.Headers["X-User-Id"].FirstOrDefault()
            ?? "anonymous";

        return new TenantContext(tenantId, userId);
    }
}
