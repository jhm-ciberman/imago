using System;
using System.Collections.Generic;

namespace LifeSim.Engine.SceneGraph;

/// <summary>
/// This class is responsible for managing a list of pickable objects and assigning them unique IDs.
/// </summary>
public class PickingManger
{
    private readonly Dictionary<uint, IPickable> _pickables = new Dictionary<uint, IPickable>();
    private uint _nextId = 1;

    /// <summary>
    /// Adds a pickable object to the list of pickable objects.
    /// </summary>
    /// <param name="pickable">The pickable object to add.</param>
    public void RegisterPickable(IPickable pickable)
    {
        if (pickable.PickId != 0)
        {
            throw new InvalidOperationException("The pickable object already has an ID assigned.");
        }

        pickable.PickId = this._nextId;
        this._pickables.Add(this._nextId, pickable);
        this._nextId++;
    }

    /// <summary>
    /// Removes a pickable object from the list of pickable objects.
    /// </summary>
    /// <param name="pickable">The pickable object to remove.</param>
    public void UnregisterPickable(IPickable pickable)
    {
        if (pickable.PickId == 0)
        {
            throw new InvalidOperationException("The pickable object does not have an ID assigned.");
        }

        this._pickables.Remove(pickable.PickId);
        pickable.PickId = 0;
    }

    /// <summary>
    /// Gets the pickable object with the specified ID or null if no such object exists.
    /// </summary>
    /// <param name="id">The ID of the pickable object to get.</param>
    /// <returns>The pickable object with the specified ID or null if no such object exists.</returns>
    public IPickable? GetPickable(uint id)
    {
        if (this._pickables.TryGetValue(id, out IPickable? pickable))
        {
            return pickable;
        }

        return null;
    }

    /// <summary>
    /// Gets or sets the currently highlighted pickable object.
    /// </summary>
    public IPickable? HighlightedPickable { get; set; }
}
