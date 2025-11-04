namespace Requestum.Contract;

/// <summary>
/// Marks a handler or middleware with a specific tag to control which requests it processes.
/// <para>
/// For handlers (commands and queries): The handler will ONLY execute for requests that implement
/// <see cref="ITaggedRequest"/> and have a matching <see cref="ITaggedRequest.Tag"/> value.
/// Only one tagged handler can match per request.
/// </para>
/// <para>
/// For event receivers: The receiver will execute for requests that implement <see cref="ITaggedRequest"/>
/// and have a matching tag.
/// Multiple tagged receivers can execute for the same event.
/// </para>
/// <para>
/// For middlewares: The middleware will execute for requests that implement <see cref="ITaggedRequest"/>
/// and have a matching tag, in addition to all untagged middlewares.
/// </para>
/// <para>
/// Handlers and middlewares without this attribute will process all requests regardless of tags.
/// </para>
/// </summary>
/// <param name="tag">The tag identifier for filtering requests.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class HandlerTagAttribute(string tag) : Attribute
{
    /// <summary>
    /// Gets the tag identifier used for filtering requests.
    /// </summary>
    public string Tag { get; init; } = tag;
}
