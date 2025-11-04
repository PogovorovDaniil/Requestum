namespace Requestum.DependencyInjection;

internal readonly struct RequestumServiceKey : IEquatable<RequestumServiceKey>
{
    private readonly string[] _tagArray;
    private readonly bool isRequestSide;

    public RequestumServiceKey(bool isRequestSide, params string[] keys)
    {
        _tagArray = keys.Where(k => !string.IsNullOrWhiteSpace(k))
                       .OrderBy(k => k, StringComparer.Ordinal)
                       .ToArray();
        
        this.isRequestSide = isRequestSide;
    }

    public static RequestumServiceKey operator +(RequestumServiceKey serviceKey, string[] tags)
    {
        if (tags == null || tags.Length == 0) 
            return serviceKey;

        var combined = new string[serviceKey._tagArray.Length + tags.Length];
        serviceKey._tagArray.CopyTo(combined, 0);
        tags.CopyTo(combined, serviceKey._tagArray.Length);

        return new RequestumServiceKey(serviceKey.isRequestSide, combined);
    }

    public bool Equals(RequestumServiceKey other)
    {
        string[] requestTags;
        string[] handlerTags;
        if (isRequestSide)
        {
            requestTags = _tagArray;
            handlerTags = other._tagArray;
        }
        else
        {
            requestTags = other._tagArray;
            handlerTags = _tagArray;
        }

        foreach (var handlerTag in handlerTags)
        {
            if (Array.IndexOf(requestTags, handlerTag) < 0) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => 
        obj is RequestumServiceKey key && Equals(key);

    public override int GetHashCode() => isRequestSide.GetHashCode();

    public static bool operator ==(RequestumServiceKey left, RequestumServiceKey right) => 
        left.Equals(right);

    public static bool operator !=(RequestumServiceKey left, RequestumServiceKey right) => 
        !left.Equals(right);

    public const string Command = "Command";
    public const string Query = "Query";

    public static readonly RequestumServiceKey Request = new(true);
    public static readonly RequestumServiceKey RequestCommand = new(true, Command);
    public static readonly RequestumServiceKey RequestQuery = new(true, Query);

    public static readonly RequestumServiceKey MiddlewareCommand = new(false, Command);
    public static readonly RequestumServiceKey MiddlewareQuery = new(false, Query);
}
