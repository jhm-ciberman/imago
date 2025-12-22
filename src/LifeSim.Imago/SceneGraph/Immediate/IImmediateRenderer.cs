using System;
using System.Numerics;
using LifeSim.Imago.Assets.Materials;
using LifeSim.Imago.Assets.Textures;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph.Immediate;

/// <summary>
/// Defines the contract for immediate mode rendering operations.
/// </summary>
public interface IImmediateRenderer
{

    /// <summary>
    /// Gets or sets whether the immediate mode renderer should use transparency for the next batch of draw calls.
    /// </summary>
    public bool IsTransparencyEnabled { get; set; }

    /// <summary>
    /// Sets the shader to use for the next subsequent draw calls.
    /// </summary>
    /// <param name="shader">The shader to use or null to use the default shader.</param>
    public void SetShader(Shader? shader);
    /// <summary>
    /// Sets the texture to use for the next subsequent draw calls.
    /// </summary>
    /// <param name="texture">The texture to use or null to use the default texture.</param>
    public void SetTexture(ITexture? texture);

    /// <summary>
    /// Draws a batch of vertices in immediate mode.
    /// </summary>
    /// <param name="indices">The indices of the vertices to draw.</param>
    /// <param name="vertices">The vertices to draw.</param>
    public void DrawVertices(ReadOnlySpan<ushort> indices, ReadOnlySpan<ImmediateVertex> vertices);

    /// <summary>
    /// Draws a quad in immediate mode. The quad is drawn using two triangles. The vertices should be in counter-clockwise order.
    /// </summary>
    /// <param name="v1">The first vertex.</param>
    /// <param name="v2">The second vertex.</param>
    /// <param name="v3">The third vertex.</param>
    /// <param name="v4">The fourth vertex.</param>
    /// <param name="t1">The first texture coordinate.</param>
    /// <param name="t2">The second texture coordinate.</param>
    /// <param name="t3">The third texture coordinate.</param>
    /// <param name="t4">The fourth texture coordinate.</param>
    /// <param name="color">The color to tint the quad with.</param>
    public void DrawQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2 t1, Vector2 t2, Vector2 t3, Vector2 t4, Color color);

    /// <summary>
    /// Draws a triangle in immediate mode. The vertices should be in counter-clockwise order.
    /// </summary>
    /// <param name="v1">The first vertex.</param>
    /// <param name="v2">The second vertex.</param>
    /// <param name="v3">The third vertex.</param>
    /// <param name="t1">The first texture coordinate.</param>
    /// <param name="t2">The second texture coordinate.</param>
    /// <param name="t3">The third texture coordinate.</param>
    /// <param name="color">The color to tint the triangle with.</param>
    public void DrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 t1, Vector2 t2, Vector2 t3, Color color);
}
