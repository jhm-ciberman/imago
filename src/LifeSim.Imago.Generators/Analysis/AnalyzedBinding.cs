namespace LifeSim.Imago.Generators.Analysis;

/// <summary>
/// A resolved binding ready for code emission.
/// </summary>
internal sealed class AnalyzedBinding
{
    /// <summary>
    /// Gets or sets the C# expression for the source object (e.g., "this._vm").
    /// </summary>
    public string SourceExpression { get; set; } = "";

    /// <summary>
    /// Gets or sets the deduplication key for the source object (e.g., "_vm").
    /// Used to group bindings that share the same source into one handler.
    /// </summary>
    public string SourceKey { get; set; } = "";

    /// <summary>
    /// Gets or sets the property name to watch on the source object (e.g., "Hunger").
    /// </summary>
    public string PropertyName { get; set; } = "";

    /// <summary>
    /// Gets or sets the variable name for the target element (e.g., "__e0" or "this").
    /// </summary>
    public string TargetVariable { get; set; } = "";

    /// <summary>
    /// Gets or sets the property or method name on the target element (e.g., "Value" or "SetValue").
    /// </summary>
    public string TargetProperty { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether the target is a method rather than a property.
    /// When true, the emitter generates a method call instead of a property assignment.
    /// </summary>
    public bool IsMethodTarget { get; set; }

    /// <summary>
    /// Gets or sets the full C# expression for the right-hand side of the assignment
    /// (e.g., "this._vm.Hunger").
    /// </summary>
    public string AssignExpression { get; set; } = "";
}
