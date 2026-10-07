using HueTour.WebApp.Components;
using HueTour.UseCases.AiAssistant;
using HueTour.UseCases.Auth;
using HueTour.UseCases.Booking;
using HueTour.UseCases.Departures;
using HueTour.UseCases.PluginInterfaces.DataStore;
using HueTour.UseCases.PluginInterfaces.State;
using HueTour.UseCases.Tours;
using HueTour.Plugins.DataStore.InMemory;
using HueTour.Plugins.DataStore.SQL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
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
}
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
builder.Services.AddScoped<IAiTourAdvisorUseCase, AiTourAdvisorUseCase>();

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

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
