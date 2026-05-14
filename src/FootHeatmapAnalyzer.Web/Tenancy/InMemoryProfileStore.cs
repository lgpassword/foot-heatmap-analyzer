using System.Collections.Concurrent;

namespace FootHeatmapAnalyzer.Web.Tenancy;

/// <summary>
/// Keeps tenant profiles in memory for demo and test deployments without runtime database files.
/// </summary>
public sealed class InMemoryProfileStore : IProfileStore
{
    private readonly ConcurrentDictionary<string, ManagedProfile> profiles = new();

    /// <inheritdoc />
    public ManagedProfile Create(TenantContext tenant, CreateProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new ArgumentException("Profile display name is required.", nameof(request));
        }

        var profile = new ManagedProfile(
            Convert.ToHexString(Guid.NewGuid().ToByteArray()),
            tenant.TenantId,
            request.Kind,
            request.DisplayName.Trim(),
            request.DateOfBirth,
            request.Notes?.Trim() ?? string.Empty);
        profiles[profile.Id] = profile;
        return profile;
    }

    /// <inheritdoc />
    public IReadOnlyList<ManagedProfile> List(TenantContext tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        return profiles.Values
            .Where(profile => profile.TenantId == tenant.TenantId)
            .OrderBy(profile => profile.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <inheritdoc />
    public ManagedProfile? Find(TenantContext tenant, string profileId)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        return profiles.TryGetValue(profileId, out var profile) && profile.TenantId == tenant.TenantId
            ? profile
            : null;
    }
}
