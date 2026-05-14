namespace FootHeatmapAnalyzer.Web.Tenancy;

/// <summary>
/// Carries user input for creating a tenant-scoped profile.
/// </summary>
public sealed record CreateProfileRequest(ProfileKind Kind, string DisplayName, DateOnly? DateOfBirth, string? Notes);
