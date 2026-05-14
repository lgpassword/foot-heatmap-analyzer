namespace FootHeatmapAnalyzer.Web.Api;

/// <summary>
/// Standard REST contract for pressure hardware submitting a scan.
/// </summary>
public sealed record HardwareScanRequest(
    string DeviceId,
    string? TenantId,
    string? ProfileId,
    string PayloadEncoding,
    string Payload,
    DateTimeOffset CapturedAt);
