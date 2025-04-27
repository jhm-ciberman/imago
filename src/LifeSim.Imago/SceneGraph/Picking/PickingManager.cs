using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Input;
using LifeSim.Imago.SceneGraph.Cameras;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.SceneGraph.Picking;

/// <summary>
/// This class is responsible for managing a list of pickable objects and assigning them unique IDs.
/// </summary>
public class PickingManager
{
    /// <summary>
    /// Occurs when the currently selected pickable object changes.
    /// </summary>
    public event EventHandler? PickableTargetChanged;

    private readonly Dictionary<uint, IPickable> _pickables = new Dictionary<uint, IPickable>();
    private uint _nextId = 1;

    /// <summary>
    /// Gets or sets the currently highlighted pickable object.
    /// This is the node that is currently under the mouse cursor as reported by the renderer.
    /// </summary>
    public IPickable? HighlightedPickable { get; set; }

    /// <summary>
    /// Gets or sets the camera used for picking.
    /// </summary>
    public Camera? Camera { get; set; } = null;

    private IPickableTarget? _pickableTarget;

    /// <summary>
    /// Gets or sets the currently selected pickable object.
    /// This is the nearst pickable object in the scene graph that is under the mouse cursor. To find this object,
    /// the scene graph is traversed from the currently highlighted pickable object up to the root node and the first
    /// object that implements the <see cref="IPickableTarget"/> interface is selected.
    /// </summary>
    public IPickableTarget? PickableTarget
    {
        get => this._pickableTarget;
        set
        {
            if (this._pickableTarget == value) return;
            this._pickableTarget = value;
            this.PickableTargetChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Adds a pickable object to the list of pickable objects.
    /// </summary>
    /// <param name="pickable">The pickable object to add.</param>
    public void RegisterPickable(IPickable pickable)
    {
        if (pickable.PickId != 0)
            throw new InvalidOperationException("The pickable object already has an ID assigned.");

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
            throw new InvalidOperationException("The pickable object does not have an ID assigned.");

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
            return pickable;

        return null;
    }

    /// <summary>
    /// Gets the HitInfo that was used to find the currently selected pickable object.
    /// </summary>
    public HitInfo HitInfo { get; private set; }

    /// <summary>
    /// Updates the picking manager.
    /// </summary>
    /// <param name="camera">The camera used for picking.</param>
    public void Update(Camera? camera)
    {
        var newTarget = this.GetPickableView(camera, out var hitInfo);

        this.HitInfo = hitInfo;

        if (this.PickableTarget == newTarget)
            this.PickableTarget?.MouseMove(hitInfo);
        else
        {
            this.PickableTarget?.MouseLeave();
            newTarget?.MouseEnter(hitInfo);

            this.PickableTarget = newTarget;
        }
    }

    /// <summary>
    /// Returns the pickable view that is currently under the mouse cursor.
    /// </summary>
    /// <param name="camera">The camera used for picking.</param>
    /// <param name="hitInfo">The hit info that was used to find the pickable.</param>
    /// <returns>The pickable view that is currently under the mouse cursor.</returns>
    private IPickableTarget? GetPickableView(Camera? camera, out HitInfo hitInfo)
    {
        hitInfo = default;
        if (camera == null) return null;

        if (this.HighlightedPickable is not RenderNode3D selectedRenderNode)
            return null;

        Vector2 viewPortPoint = InputManager.Instance.CursorPosition / camera.Viewport.Size;
        var ray = camera.ViewportRay(viewPortPoint);
        if (!selectedRenderNode.RayCast(ray, out hitInfo))
            return null;

        Node3D? node = selectedRenderNode;
        while (node != null) // Traverse the scene graph to find the selected object.
        {
            if (node is IPickableTarget pickableView)
                return pickableView;

            node = node.Parent; // Go up in the scene graph.
        }

        return null;
    }
}
