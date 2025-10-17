namespace Requestum.Contract;

/// <summary>
/// Placeholder response for commands (<see cref="ICommand"/>), 
/// used when a command does not need to return any result.
/// </summary>
public class CommandResponse
{
    public static readonly CommandResponse Instance = new();
    public override string ToString() => "Yes, it's definitely a command response!";
}