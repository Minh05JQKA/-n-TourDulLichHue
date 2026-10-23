using HueTour.WebApp.Components;
using HueTour.WebApp.Configuration;
using HueTour.WebApp.Services;
using HueTour.UseCases.AiAssistant;
using HueTour.UseCases.Auth;
using HueTour.UseCases.Booking;
using HueTour.UseCases.Departures;
using HueTour.UseCases.FoodPlaces;
using HueTour.UseCases.MapExplorer;
using HueTour.UseCases.PluginInterfaces.DataStore;
using HueTour.UseCases.PluginInterfaces.State;
using HueTour.UseCases.Reports;
using HueTour.UseCases.Tours;
using HueTour.Plugins.DataStore.InMemory;
using HueTour.Plugins.DataStore.SQL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.AI;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

var dataProtectionKeyPath = builder.Configuration["DataProtection:KeyPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeyPath))
{
    Directory.CreateDirectory(dataProtectionKeyPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyPath));
}

var useSqlDatabase = builder.Configuration.GetValue<bool>("UseSqlDatabase");
if (useSqlDatabase)
{
    var connectionString = builder.Configuration.GetConnectionString("HueTourDb")
        ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:HueTourDb.");
    builder.Services.AddSingleton<ISqlDbConnectionFactory>(_ => new SqlDbConnectionFactory(connectionString));
    builder.Services.AddScoped<ITourRepository, TourRepository>();
    builder.Services.AddScoped<IDepartureRepository, DepartureRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IPriceRuleRepository, PriceRuleRepository>();
    builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
    builder.Services.AddScoped<ITourBookingRepository, TourBookingRepository>();
    builder.Services.AddScoped<IFoodPlaceRepository, FoodPlaceRepository>();
}
else
{
    // In-memory sample data lets the app start before SQL Server is configured.
    builder.Services.AddSingleton<ITourRepository, TourInMemoryRepository>();
    builder.Services.AddSingleton<IDepartureRepository, DepartureInMemoryRepository>();
    builder.Services.AddSingleton<ICustomerRepository, CustomerInMemoryRepository>();
    builder.Services.AddSingleton<IPriceRuleRepository, PriceRuleInMemoryRepository>();
    builder.Services.AddSingleton<IUserAccountRepository, UserAccountInMemoryRepository>();
    builder.Services.AddSingleton<ITourBookingRepository, TourBookingInMemoryRepository>();
    builder.Services.AddSingleton<IFoodPlaceRepository, FoodPlaceInMemoryRepository>();
}
builder.Services.Configure<VietQrPaymentOptions>(builder.Configuration.GetSection(VietQrPaymentOptions.SectionName));
builder.Services.PostConfigure<VietQrPaymentOptions>(options =>
{
    var bankCode = Environment.GetEnvironmentVariable("HUETOUR_BANK_CODE");
    var bankName = Environment.GetEnvironmentVariable("HUETOUR_BANK_NAME");
    var accountNumber = Environment.GetEnvironmentVariable("HUETOUR_BANK_ACCOUNT_NUMBER");
    var accountHolder = Environment.GetEnvironmentVariable("HUETOUR_BANK_ACCOUNT_HOLDER");
    var qrImagePath = Environment.GetEnvironmentVariable("HUETOUR_BANK_QR_IMAGE_PATH");
    var transferTemplate = Environment.GetEnvironmentVariable("HUETOUR_BANK_TRANSFER_CONTENT_TEMPLATE");

    if (!string.IsNullOrWhiteSpace(bankCode)) options.BankCode = bankCode;
    if (!string.IsNullOrWhiteSpace(bankName)) options.BankName = bankName;
    if (!string.IsNullOrWhiteSpace(accountNumber)) options.AccountNumber = accountNumber;
    if (!string.IsNullOrWhiteSpace(accountHolder)) options.AccountHolder = accountHolder;
    if (!string.IsNullOrWhiteSpace(qrImagePath)) options.QrImagePath = qrImagePath;
    if (!string.IsNullOrWhiteSpace(transferTemplate)) options.TransferContentTemplate = transferTemplate;
});
builder.Services.AddScoped<IBookingCartStateService, BookingCartStateService>();

builder.Services.AddScoped<IViewToursUseCase, ViewToursUseCase>();
builder.Services.AddScoped<ISearchToursUseCase, SearchToursUseCase>();
builder.Services.AddScoped<IManageToursUseCase, ManageToursUseCase>();
builder.Services.AddScoped<IViewTourDetailsUseCase, ViewTourDetailsUseCase>();
builder.Services.AddScoped<IViewDeparturesByTourUseCase, ViewDeparturesByTourUseCase>();
builder.Services.AddScoped<IGetDepartureDetailsUseCase, GetDepartureDetailsUseCase>();
builder.Services.AddScoped<IManageDeparturesUseCase, ManageDeparturesUseCase>();
builder.Services.AddScoped<ICalculateBookingPricingUseCase, CalculateBookingPricingUseCase>();
builder.Services.AddScoped<IValidateTourBookingUseCase, ValidateTourBookingUseCase>();
builder.Services.AddScoped<IPlaceTourBookingUseCase, PlaceTourBookingUseCase>();
builder.Services.AddScoped<IManageBookingsUseCase, ManageBookingsUseCase>();
builder.Services.AddScoped<IViewBookingConfirmationUseCase, ViewBookingConfirmationUseCase>();
builder.Services.AddScoped<IAuthenticateUserUseCase, AuthenticateUserUseCase>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IChatClient, HueTourChatClient>();
builder.Services.AddScoped<IAiTourAdvisorUseCase, AiTourAdvisorUseCase>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<ITravelDiscoveryService, TravelDiscoveryService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(25);
});
builder.Services.AddScoped<ITravelAssistantService, TravelAssistantService>();
builder.Services.AddScoped<IExportBookingReportUseCase, ExportBookingReportUseCase>();
builder.Services.AddScoped<IFoodPlaceCatalogUseCase, FoodPlaceCatalogUseCase>();
builder.Services.AddScoped<IDiscoveryMapDataSource, TravelDiscoveryMapDataSource>();
builder.Services.AddScoped<IMapPoiCatalogUseCase, MapPoiCatalogUseCase>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "HueTour.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.UseAntiforgery();

app.MapPost("/auth/login", async (HttpContext context, IAuthenticateUserUseCase authenticate) =>
{
    var form = await context.Request.ReadFormAsync();
    var user = await authenticate.ExecuteAsync(form["username"].ToString(), form["password"].ToString());
    if (user is null)
    {
        var returnUrl = Uri.EscapeDataString(form["returnUrl"].ToString());
        var error = Uri.EscapeDataString("Tên đăng nhập hoặc mật khẩu không chính xác.");
        return Results.Redirect($"/login?ErrorMessage={error}&ReturnUrl={returnUrl}");
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.FullName),
        new(ClaimTypes.Role, user.Role),
        new("username", user.Username)
    };
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    var destination = form["returnUrl"].ToString();
    return Results.LocalRedirect(!string.IsNullOrWhiteSpace(destination) && Uri.IsWellFormedUriString(destination, UriKind.Relative)
        ? destination : "/");
});

app.MapGet("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

// TIÊU CHÍ K4.3: Endpoint xuất báo cáo Excel/CSV từ dữ liệu thật
app.MapGet("/api/reports/bookings.xlsx", async (IExportBookingReportUseCase reportUseCase) =>
{
    var excelBytes = await reportUseCase.ExportBookingsToExcelAsync();
    string fileName = $"HueTour_BaoCaoDatTour_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
    return Results.File(
        excelBytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        fileName);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/api/reports/bookings-csv", async (IExportBookingReportUseCase reportUseCase) =>
{
    var csvBytes = await reportUseCase.ExportBookingsToCsvAsync();
    string fileName = $"HueTour_BaoCaoDatTour_{DateTime.Now:yyyyMMdd_HHmm}.csv";
    return Results.File(csvBytes, "text/csv; charset=utf-8", fileName);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/api/reports/passengers-csv/{departureId:int}", async (int departureId, IExportBookingReportUseCase reportUseCase) =>
{
    var csvBytes = await reportUseCase.ExportDeparturePassengersToCsvAsync(departureId);
    string fileName = $"HueTour_Chuyen_{departureId}_HanhKhach_{DateTime.Now:yyyyMMdd}.csv";
    return Results.File(csvBytes, "text/csv; charset=utf-8", fileName);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
