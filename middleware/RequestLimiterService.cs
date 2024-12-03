using System.Collections.Concurrent;

namespace RequestLimiter_Middleware.middleware
{
	public class RequestLimiterService
	{
		// This dictionary helps store user data as key-value pairs.
		// The key is a unique identifier (e.g., user ID or IP address).
		// The value is a tuple containing the request count and the timestamp of the last request.
		private readonly ConcurrentDictionary<string, (int Count, DateTime Timestamp)> _requestCounts = new();

		// This method is invoked in middleware to increase the request count for a given key.
		// It returns true if the request count is below the limit; otherwise, it returns false.
		public bool IncreaseRequestCount(string key, int requestLimit, TimeSpan resetPeriod)
		{
			var currentTime = DateTime.Now;

			// If the user's unique key does not exist, add it with a count of 1 and the current timestamp.
			_requestCounts.AddOrUpdate(key, (1, currentTime), (k, entry) =>
			{
				// If the current time minus the user's first request time is greater than the reset period,
				// reset the request count and update the timestamp to the current time.
				if ((currentTime - entry.Timestamp) > resetPeriod)
				{
					return (1, currentTime);
				}

				// If the user's request count has reached the limit, return the current entry without incrementing.
				if (entry.Count >= requestLimit)
				{
					return entry;
				}

				// Increment the request count if the limit has not been reached and the reset period has not passed.
				return (entry.Count + 1, entry.Timestamp);
			});

			// Return true if the request count is below the limit, indicating the request is allowed.
			return _requestCounts[key].Count < requestLimit;
		}

		// This method retrieves the request count for a given key.
		// If the key does not exist, it returns 0.
		public int GetRequestCount(string key)
		{
			return _requestCounts.TryGetValue(key, out var entry) ? entry.Count : 0;
		}

		// This method checks if the request count for a given key has reached the request limit.
		// It returns true if the limit has been reached; otherwise, it returns false.
		public bool IsLimitedRequest(string key, int requestLimit)
		{
			return _requestCounts.TryGetValue(key, out var entry) && entry.Count >= requestLimit;
		}

		// This method calculates and returns the reset time for a given key.
		// It adds the reset period to the timestamp of the last request.
		// If the key does not exist, it returns null.
		public DateTime? GetResetTime(string key, TimeSpan resetPeriod)
		{
			if (_requestCounts.TryGetValue(key, out var entry))
			{
				return entry.Timestamp.Add(resetPeriod);
			}

			return null;
		}

		// This method removes the entry for a given key from the dictionary, effectively resetting the request count.
		public void ResetLimit(string key)
		{
			_requestCounts.TryRemove(key, out _);
		}

		
	}

}
