using Microsoft.Extensions.Options;

namespace RequestLimiter_Middleware.middleware
{

	namespace ImenPardaz.WebApplication.Middlewares
	{
		public class RequestLimiterMiddleware
		{
			// Dependencies required by the middleware
			private readonly RequestLimiterService _service;
			private readonly RequestLimiterOptions _limiterOptions;
			private readonly RequestDelegate _next;

			// Constructor to initialize the middleware with its dependencies
			public RequestLimiterMiddleware(RequestLimiterService service, IOptions<RequestLimiterOptions> limiterOptions, RequestDelegate next)
			{
				_service = service;
				_limiterOptions = limiterOptions.Value;
				_next = next;
			}

			// Middleware logic to handle request limiting
			public async Task InvokeAsync(HttpContext context)
			{
				// Create a unique key for the user by combining User-Agent and remote IP address
				// This key is used to store each user's request data in the limiter service
				var userAgent = context.Request.Headers["User-Agent"].ToString();
				var remoteAddress = context.Connection.RemoteIpAddress;
				var key = $"{userAgent}-{remoteAddress}";

				// Check and update the user's request count and timestamp using the request limiter service
				if (_service.IncreaseRequestCount(key, _limiterOptions.RequestLimit, _limiterOptions.ResetPeriod))
				{
					// Add custom headers to the response indicating the current request count and reset time
					context.Response.Headers.Add("X-Request-Count", _service.GetRequestCount(key).ToString());
					context.Response.Headers.Add("X-Reset-Time", _service.GetResetTime(key, _limiterOptions.ResetPeriod)?.ToString("HH:mm:ss "));
				}

				// If the user's request count has reached the limit, prevent further requests
				if (_service.IsLimitedRequest(key, _limiterOptions.RequestLimit))
				{
					// Optionally, you could take additional measures such as using reCAPTCHA to verify human interaction
					await context.Response.WriteAsJsonAsync($"Your requests have reached the limit of {_limiterOptions.RequestLimit}. Try again after  {(_service.GetResetTime(key, _limiterOptions.ResetPeriod) - DateTime.Now)?.TotalSeconds:00000} seconds!");
					return;
				}

				// Call the next middleware in the pipeline
				await _next(context);
			}
		}

	}
}
