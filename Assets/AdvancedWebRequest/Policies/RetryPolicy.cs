using System;
using UnityEngine;

namespace AdvancedWebRequest.Core
{
    public class RetryPolicy
    {
        public int MaxRetries { get; set; } = 3;
        public float BaseDelaySeconds { get; set; } = 1f;
        public float MaxDelaySeconds { get; set; } = 10f;
        public bool UseJitter { get; set; } = true;

        public static RetryPolicy Default => new RetryPolicy
        {
            MaxRetries = 3,
            BaseDelaySeconds = 1f,
            MaxDelaySeconds = 10f,
            UseJitter = true
        };

        public static RetryPolicy NoRetry => new RetryPolicy
        {
            MaxRetries = 0
        };

        public static RetryPolicy Aggressive => new RetryPolicy
        {
            MaxRetries = 5,
            BaseDelaySeconds = 0.5f,
            MaxDelaySeconds = 5f,
            UseJitter = true
        };

        public bool ShouldRetry(ApiException exception, int attemptNumber)
        {
            if (attemptNumber >= MaxRetries)
                return false;

            return exception.Category switch
            {
                ApiErrorCategory.Timeout => true,
                ApiErrorCategory.NetworkError => true,
                ApiErrorCategory.RateLimited => true,
                ApiErrorCategory.ServerError => true,
                _ => false
            };
        }

        public TimeSpan GetDelay(int attemptNumber)
        {
            var exponentialDelay = BaseDelaySeconds * Mathf.Pow(2, attemptNumber - 1);
            var delay = Mathf.Min(exponentialDelay, MaxDelaySeconds);

            if (UseJitter)
            {
                var jitter = UnityEngine.Random.Range(0f, delay * 0.3f);
                delay += jitter;
            }

            return TimeSpan.FromSeconds(delay);
        }
    }
}
