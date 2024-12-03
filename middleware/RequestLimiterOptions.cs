namespace RequestLimiter_Middleware.middleware;

public class RequestLimiterOptions
{

	public int RequestLimit { get; set; }

	public TimeSpan ResetPeriod { get; set; }

}