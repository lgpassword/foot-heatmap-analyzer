namespace FootHeatmapAnalyzer.Web.Tenancy;

/// <summary>
/// Describes the tenant currently associated with a request.
/// </summary>
public sealed record TenantContext(string TenantId, string UserId);
