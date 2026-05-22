using Authentication.Configuration;
using Authentication.Data;
using Authentication.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
            .GetConnectionString("DefaultConnection")));

// ── All app services ───────────────────────────────────────────────────────
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

// ── Auto migrate + seed on startup ────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();

    await DbSeeder.SeedAsync(context, logger);
}

// ── Middleware pipeline ────────────────────────────────────────────────────
// Order is critical — do not change
app.UseGlobalExceptionHandler();   // 1. Catch all exceptions
app.UseSecurityHeaders();          // 2. Security headers on every response
app.UseRequestLogging();           // 3. Log every request

app.UseHttpsRedirection();         // 4. Force HTTPS
app.UseCors("CropInsurancePolicy"); // 5. CORS before auth
app.UseRateLimiter();              // 6. Rate limiting
app.UseAuthentication();           // 7. Validate JWT
app.UseAuthorization();            // 8. Check roles/policies
app.MapControllers();              // 9. Route to controllers


app.Urls.ToList().ForEach(url => Log.Information("Server running on: {Url}", url));

app.Run();