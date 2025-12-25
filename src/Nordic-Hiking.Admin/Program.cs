using Microsoft.EntityFrameworkCore;
using NordicHiking.Admin.Components;
using NordicHiking.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Services
builder.Services.AddScoped<NordicHiking.Core.Interfaces.IYouTubeService, NordicHiking.Admin.Services.YouTubeService>();
builder.Services.AddScoped<NordicHiking.Core.Interfaces.IAiAnalysisService, NordicHiking.Admin.Services.ClaudeAnalysisService>();
builder.Services.AddSingleton<NordicHiking.Core.Interfaces.IGeocodingService, NordicHiking.Admin.Services.GeocodingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
