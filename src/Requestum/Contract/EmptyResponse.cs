namespace Requestum.Contract;

/// <summary>
/// Placeholder response for commands (<see cref="ICommand"/>), 
/// used when a command does not need to return any result.
/// </summary>
public class EmptyResponse
{
    public static readonly EmptyResponse Instance = new();
}