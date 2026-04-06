using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;
using FlashcardsPlatformFull.Models;
using FlashcardsPlatformFull.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// EF Core + PostgreSQL
builder.Services.AddDbContext<FlashcardsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    options.SignIn.RequireConfirmedAccount =
        builder.Configuration.GetValue<bool>("Identity:RequireConfirmedAccount");

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<FlashcardsDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReadOnlyAccess", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.Identity?.IsAuthenticated == true &&
            (ctx.User.IsInRole("Admin") ||
             ctx.User.IsInRole("User") ||
             ctx.User.IsInRole("Guest"))
        ));

    options.AddPolicy("WriteAccess", policy =>
        policy.RequireRole("Admin", "User"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// OpenAI generator
builder.Services.AddHttpClient<IOpenAiFlashcardGenerator, OpenAiFlashcardGenerator>();

// Email sender
var emailProvider = builder.Configuration["Email:Provider"] ?? "Console";
if (string.Equals(emailProvider, "Smtp", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
else
    builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlashcardsDbContext>();
    db.Database.Migrate();

    await Seed.SeedRolesAndUsersAsync(scope.ServiceProvider, builder.Configuration);
    await Seed.SeedSampleDataAsync(scope.ServiceProvider, builder.Configuration);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Identity/Account/Login");
    return Task.CompletedTask;
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Decks}/{action=Index}/{id?}");

app.Run();