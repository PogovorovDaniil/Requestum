namespace Requestum.Contract;

/// <summary>
/// Represents a request that can be tagged for selecting specific handlers and middlewares.
/// <para>
/// For handlers (commands and queries): Only ONE handler with a matching tag will be selected.
/// If no tagged handler matches, an untagged handler will be used.
/// </para>
/// <para>
/// For event receivers: ALL receivers with a matching tag will execute.
/// </para>
/// <para>
/// For middlewares: ALL middlewares with a matching tag AND all untagged middlewares will execute.
/// </para>
/// <para>
/// Handlers and middlewares can be registered with a specific tag using <see cref="HandlerTagAttribute"/>.
/// </para>
/// </summary>
public interface ITaggedRequest
{
    /// <summary>
    /// Gets the tag associated with this request.
    /// <para>
    /// For handlers (commands and queries): Selects a single handler with this tag (if available).
    /// </para>
    /// <para>
    /// For event receivers: Includes all receivers with this tag.
    /// </para>
    /// <para>
    /// For middlewares: Includes all middlewares with this tag in addition to untagged middlewares.
    /// </para>
    /// </summary>
    string Tag { get; }
}
