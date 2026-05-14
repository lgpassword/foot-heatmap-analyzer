namespace FootHeatmapAnalyzer.Web.Tenancy;

/// <summary>
/// Stores a tenant-isolated patient or athlete profile.
/// </summary>
public sealed record ManagedProfile(string Id, string TenantId, ProfileKind Kind, string DisplayName, DateOnly? DateOfBirth, string Notes);
