namespace LifeSim.Utils;

/// <summary>
/// Represents an identifiable object.
/// </summary>
public interface IIdentifiable
{
    /// <summary>
    /// Gets the identifier of this object.
    /// </summary>
    string Identifier { get; }
}
