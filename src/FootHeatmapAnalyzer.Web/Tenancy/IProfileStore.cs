namespace FootHeatmapAnalyzer.Web.Tenancy;

/// <summary>
/// Stores and retrieves tenant-scoped patient or athlete profiles.
/// </summary>
public interface IProfileStore
{
    /// <summary>
    /// Creates a profile in the current tenant.
    /// </summary>
    ManagedProfile Create(TenantContext tenant, CreateProfileRequest request);

    /// <summary>
    /// Lists profiles visible to the current tenant.
    /// </summary>
    IReadOnlyList<ManagedProfile> List(TenantContext tenant);

    /// <summary>
    /// Finds one profile visible to the current tenant.
    /// </summary>
    ManagedProfile? Find(TenantContext tenant, string profileId);
}
