using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Utils;

namespace LifeSim.Engine.SceneGraph;

public class ImmediateRenderNode3D : Node3D
{
    public ushort[] Indices { get; set; } = Array.Empty<ushort>();
    public Vector3[] Positions { get; set; } = Array.Empty<Vector3>();
    public Vector2[] TextureCoords { get; set; } = Array.Empty<Vector2>();
    public Color Color { get; set; } = Color.White;

    public ITexture? Texture { get; set; } = null;

    public Shader? Shader { get; set; } = null; // null = default shader

    public bool Visible { get; set; } = true;

    internal override void AttachToSceneRecursive(Scene scene)
    {
        Renderer.Instance.AddImmediateRenderNode(this);

        base.AttachToSceneRecursive(scene);
    }

    internal override void DetachFromSceneRecursive()
    {
        Renderer.Instance.RemoveImmediateRenderNode(this);

        base.DetachFromSceneRecursive();
    }

    internal void Render(ImmediateBatcher immediateModeBatcher)
    {
        if (!this.Visible)
        {
            return;
        }

        if (this.Positions.Length == 0 || this.Indices.Length == 0)
        {
            return;
        }

        immediateModeBatcher.Draw(this.Shader, this.Texture, this.Indices, this.Positions, this.TextureCoords, this.Color);
    }
}