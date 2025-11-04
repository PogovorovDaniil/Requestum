namespace Requestum.Contract;

/// <summary>
/// Marks a middleware with a specific tag to control which requests it processes.
/// <para>
/// The middleware will execute for requests that implement <see cref="ITaggedRequest"/>
/// and have a matching tag, in addition to all untagged middlewares.
/// </para>
/// <para>
/// Middlewares without this attribute will process all requests regardless of tags.
/// </para>
/// </summary>
/// <param name="tag">The tag identifier for filtering requests.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class MiddlewareTagAttribute(string tag) : Attribute
{
    /// <summary>
    /// Gets the tag identifier used for filtering requests.
    /// </summary>
    public string Tag { get; init; } = tag;
}
