namespace FootHeatmapAnalyzer.Web.Tenancy;

/// <summary>
/// Identifies whether a managed profile belongs to a patient or athlete.
/// </summary>
public enum ProfileKind
{
    /// <summary>
    /// Clinical patient profile.
    /// </summary>
    Patient,

    /// <summary>
    /// Sports or coaching athlete profile.
    /// </summary>
    Athlete
}
