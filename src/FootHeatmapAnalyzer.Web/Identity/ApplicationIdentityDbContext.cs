using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FootHeatmapAnalyzer.Web.Identity;

/// <summary>
/// Identity database context for tenant-scoped demo users.
/// </summary>
public sealed class ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options)
    : IdentityDbContext<ApplicationUser>(options);
