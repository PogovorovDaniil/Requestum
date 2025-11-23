namespace Requestum.Policy;

/// <summary>
/// Retry policy for handler
/// </summary>
/// <param name="retryCount">Count of retries</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RetryAttribute(int retryCount) : Attribute
{
    public int RetryCount { get; } = retryCount;
}
