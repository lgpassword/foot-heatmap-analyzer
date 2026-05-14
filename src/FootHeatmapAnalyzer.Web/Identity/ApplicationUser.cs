using Microsoft.AspNetCore.Identity;

namespace FootHeatmapAnalyzer.Web.Identity;

/// <summary>
/// Represents an authenticated clinician or coach scoped to one tenant.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Tenant identifier used to isolate patient and athlete data.
    /// </summary>
    public string TenantId { get; set; } = "demo";
}
