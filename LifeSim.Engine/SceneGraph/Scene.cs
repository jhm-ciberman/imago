using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Controls;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.SceneGraph;

public class Scene : Node3D
{
    public Color? BackgroundColor { get; set; } = Color.CoolGray;
    public DirectionalLight MainLight { get; set; } = new DirectionalLight();
    public ColorF AmbientColor { get; set; } = new ColorF(.2f, .2f, .2f, 100f / 255f);
    public RenderNode3D? SelectedNode { get; set; } // This is set by the mouse picking pass.
    public GizmosLayer Gizmos { get; } = new GizmosLayer();
    public Camera3D? Camera { get; set; } = null;
    public IReadOnlyList<CanvasLayer> CanvasLayers => this._canvasLayers;

    public UILayer? UILayer { get; set; } = null;

    public IReadOnlyList<IParticleSystem> ParticleSystems => this._particleSystems;

    private readonly List<IParticleSystem> _particleSystems = new List<IParticleSystem>();
    private readonly List<CanvasLayer> _canvasLayers = new List<CanvasLayer>();

    private readonly List<Node3D> _transformDirtyList = new List<Node3D>();

    private readonly Dictionary<uint, RenderNode3D> _pickingIdToRenderNode = new Dictionary<uint, RenderNode3D>();

    public Scene()
    {
        this.Scene = this;
    }

    public void AddParticleSystem(IParticleSystem particleSystem)
    {
        this._particleSystems.Add(particleSystem);
    }

    public void RemoveParticleSystem(IParticleSystem particleSystem)
    {
        this._particleSystems.Remove(particleSystem);
    }

    public void AddCanvasLayer(CanvasLayer canvasLayer)
    {
        this._canvasLayers.Add(canvasLayer);
    }

    public void RemoveCanvasLayer(CanvasLayer canvasLayer)
    {
        this._canvasLayers.Remove(canvasLayer);
    }

    internal IEnumerable<Renderable> GetCulledRenderables(Camera3D camera)
    {
        throw new NotImplementedException();
    }

    public virtual void OnBeforeRender()
    {
        // 
    }

    public virtual void RenderImGui()
    {
        // 
    }

    public virtual void Update(float deltaTime)
    {
        //
    }

    public void RegisterPickingId(uint pickingId, RenderNode3D renderNode)
    {
        this._pickingIdToRenderNode.Add(pickingId, renderNode);
    }

    public void UnregisterPickingId(uint pickingId)
    {
        this._pickingIdToRenderNode.Remove(pickingId);
    }

    public RenderNode3D? SelectedRenderNode
    {
        get
        {
            uint pickingId = Renderer.Instance.MousePickerObjectID;
            this._pickingIdToRenderNode.TryGetValue(pickingId, out var renderNode);
            return renderNode;
        }
    }

    public void EndUpdate()
    {
        for (int i = 0; i < this._canvasLayers.Count; i++)
        {
            this._canvasLayers[i].UpdateTransforms();
        }

        this.UpdateDirtyTransforms();
    }

    private void UpdateDirtyTransforms()
    {
        if (this._transformDirtyList.Count == 0) return;

        Matrix4x4 identity = Matrix4x4.Identity;

        for (int i = 0; i < this._transformDirtyList.Count; i++)
        {
            Node3D node = this._transformDirtyList[i];
            if (!node.TransformIsDirty) continue;

            // Search for the top dirty node
            Node3D topDirty = node;
            while (true)
            {
                if (node.TransformIsDirty) topDirty = node;
                if (node.Parent == null) break;
                node = node.Parent;
            }

            if (topDirty.Parent != null)
            {
                topDirty.UpdateWorldMatrix(ref topDirty.Parent.WorldMatrix);
            }
            else
            {
                topDirty.UpdateWorldMatrix(ref identity);
            }
        }

        this._transformDirtyList.Clear();
    }

    internal void NotifyNodeAdded(Node3D node)
    {
        this._transformDirtyList.Add(node);
    }

    internal void NotifyNodeRemoved(Node3D node)
    {
        this._transformDirtyList.Remove(node);
    }

    internal void NotifyTransformDirty(Node3D node)
    {
        this._transformDirtyList.Add(node);
    }

}