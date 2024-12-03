using RequestLimiter_Middleware.middleware;
using RequestLimiter_Middleware.middleware.ImenPardaz.WebApplication.Middlewares;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

// Add RequestLimiterService as a singleton. 
// This service should be initialized once to store users' request counts and timestamps throughout the application's lifetime.
builder.Services.AddSingleton<RequestLimiterService>();

// Configure RequestLimiterOptions with custom settings
builder.Services.Configure<RequestLimiterOptions>(options =>
{

	// Set the request limit for use in middleware as a configuration setting
	options.RequestLimit = 20;
	// Set the reset period for users' request counts
	options.ResetPeriod = TimeSpan.FromMinutes(2);

});

var app = builder.Build();

// Make sure to use the RequestLimiterMiddleware before routing
// so it can process and check all incoming requests
app.UseMiddleware<RequestLimiterMiddleware>();
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
