namespace Imago;

/// <summary>
/// Defines a contract for layers that are told when anything is mounted on or unmounted from their stage.
/// </summary>
/// <remarks>
/// A listener added to a stage is first told about everything already mounted on it. A listener removed from a
/// stage is told that everything still mounted was unmounted.
/// </remarks>
public interface IMountListener : ILayer
{
    /// <summary>
    /// Called when an item is mounted on the stage.
    /// </summary>
    /// <param name="item">The item that was mounted.</param>
    public void NotifyItemMounted(IMountable item);

    /// <summary>
    /// Called when an item is unmounted from the stage.
    /// </summary>
    /// <param name="item">The item that was unmounted.</param>
    public void NotifyItemUnmounted(IMountable item);
}
