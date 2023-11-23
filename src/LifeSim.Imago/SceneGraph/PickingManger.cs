using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Input;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.SceneGraph;

public interface IPickableView
{
    void MouseEnter(HitInfo hitInfo);
    void MouseMove(HitInfo hitInfo);
    void MouseLeave();
}

/// <summary>
/// This class is responsible for managing a list of pickable objects and assigning them unique IDs.
/// </summary>
public class PickingManger
{
    private readonly Dictionary<uint, IPickable> _pickables = new Dictionary<uint, IPickable>();
    private uint _nextId = 1;
    private IPickableView? _current = null;

    /// <summary>
    /// Gets or sets the currently highlighted pickable object.
    /// </summary>
    public IPickable? HighlightedPickable { get; set; }

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
    /// Updates the picking.
    /// </summary>
    public void Update()
    {
        var pickableView = this.GetPickableView(out var hitInfo);

        if (this._current == pickableView)
        {
            this._current?.MouseMove(hitInfo);
        }
        else
        {
            this._current?.MouseLeave();
            this._current = pickableView;
            this._current?.MouseEnter(hitInfo);
        }
    }

    /// <summary>
    /// Returns the pickable view that is currently under the mouse cursor.
    /// </summary>
    /// <param name="hitInfo">The hit info that was used to find the pickable.</param>
    /// <returns>The pickable view that is currently under the mouse cursor.</returns>
    private IPickableView? GetPickableView(out HitInfo hitInfo)
    {
        hitInfo = default;
        var camera = Application.Instance.Scene.Camera;
        if (camera == null) return null;

        if (this.HighlightedPickable is not RenderNode3D selectedRenderNode)
            return null;

        Vector2 viewPortPoint = InputManager.Current.MousePosition / camera.Viewport.Size;
        var ray = camera.ViewportRay(viewPortPoint);
        if (!selectedRenderNode.RayCast(ray, out hitInfo))
            return null;

        Node3D? node = selectedRenderNode;
        while (node != null) // Traverse the scene graph to find the selected object.
        {
            if (node is IPickableView pickableView)
                return pickableView;

            node = node.Parent; // Go up in the scene graph.
        }

        return null;
    }
}
