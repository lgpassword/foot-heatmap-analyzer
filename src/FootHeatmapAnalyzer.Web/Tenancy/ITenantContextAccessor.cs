namespace FootHeatmapAnalyzer.Web.Tenancy;

/// <summary>
/// Resolves request-level tenant information for authorization boundaries.
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    /// Gets the current tenant and user identifiers.
    /// </summary>
    TenantContext GetCurrent();
}
