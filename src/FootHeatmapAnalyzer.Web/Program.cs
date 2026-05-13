using FootHeatmapAnalyzer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IFootScanParser, FootScanParser>();
builder.Services.AddSingleton<IHeatmapFeatureExtractor, HeatmapFeatureExtractor>();
builder.Services.AddSingleton<FootRiskClassifier>();
builder.Services.AddSingleton<IFootAnalysisService, FootAnalysisService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
