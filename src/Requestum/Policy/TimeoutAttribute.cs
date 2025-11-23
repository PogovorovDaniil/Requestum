namespace Requestum.Policy;

/// <summary>
/// Timeout policy for handler
/// </summary>
/// <param name="timeout">Timeout in milliseconds</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TimeoutAttribute(int timeout) : Attribute
{
    public int Timeout { get; } = timeout;
}
