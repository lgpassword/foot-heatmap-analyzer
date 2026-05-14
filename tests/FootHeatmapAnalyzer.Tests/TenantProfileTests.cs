using FootHeatmapAnalyzer.Web.Tenancy;

namespace FootHeatmapAnalyzer.Tests;

/// <summary>
/// Covers tenant isolation for managed patient and athlete profiles.
/// </summary>
public sealed class TenantProfileTests
{
    [Fact]
    public void ProfileStore_ListsOnlyCurrentTenantProfiles()
    {
        var store = new InMemoryProfileStore();
        var tenantA = new TenantContext("tenant-a", "user-a");
        var tenantB = new TenantContext("tenant-b", "user-b");

        var visible = store.Create(tenantA, new CreateProfileRequest(ProfileKind.Patient, "Alice", null, null));
        store.Create(tenantB, new CreateProfileRequest(ProfileKind.Athlete, "Bob", null, null));

        var profiles = store.List(tenantA);

        Assert.Single(profiles);
        Assert.Equal(visible.Id, profiles[0].Id);
        Assert.Null(store.Find(tenantA, profiles[0].Id + "-missing"));
    }
}
