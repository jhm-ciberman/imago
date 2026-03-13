using System;
using System.Collections.Generic;
using System.Numerics;
using Imago.SceneGraph.Prefabs;

namespace Imago.Assets.Meshes;

/// <summary>
/// A fluent accumulator for composing 3D primitive shapes into a single merged mesh.
/// Shapes are centered on XZ with their bottom at Y=0, and can be offset via the <c>position</c> parameter.
/// </summary>
public class PrimitiveMeshBuilder
{
    private readonly List<Vector3> _positions = [];

    private readonly List<Vector3> _normals = [];

    private readonly List<Vector2> _texCoords = [];

    private readonly List<ushort> _indices = [];

    /// <summary>
    /// Gets the number of vertices accumulated so far.
    /// </summary>
    public int VertexCount => this._positions.Count;

    /// <summary>
    /// Adds an axis-aligned box. The box is centered on XZ with its bottom at Y=0.
    /// </summary>
    /// <param name="width">The size along the X axis.</param>
    /// <param name="height">The size along the Y axis.</param>
    /// <param name="depth">The size along the Z axis.</param>
    /// <param name="position">An offset applied to all vertices.</param>
    /// <returns>This builder, for chaining.</returns>
    public PrimitiveMeshBuilder AddBox(float width, float height, float depth, Vector3 position = default)
    {
        float hw = width / 2f;
        float hd = depth / 2f;

        // 8 corner positions (bottom at Y=0)
        Vector3 bfl = new Vector3(-hw, 0, hd) + position;
        Vector3 bfr = new Vector3(hw, 0, hd) + position;
        Vector3 bbl = new Vector3(-hw, 0, -hd) + position;
        Vector3 bbr = new Vector3(hw, 0, -hd) + position;
        Vector3 tfl = new Vector3(-hw, height, hd) + position;
        Vector3 tfr = new Vector3(hw, height, hd) + position;
        Vector3 tbl = new Vector3(-hw, height, -hd) + position;
        Vector3 tbr = new Vector3(hw, height, -hd) + position;

        // Front (+Z)
        this.AddFace(bfl, bfr, tfr, tfl, Vector3.UnitZ);
        // Back (-Z)
        this.AddFace(bbr, bbl, tbl, tbr, -Vector3.UnitZ);
        // Right (+X)
        this.AddFace(bfr, bbr, tbr, tfr, Vector3.UnitX);
        // Left (-X)
        this.AddFace(bbl, bfl, tfl, tbl, -Vector3.UnitX);
        // Top (+Y)
        this.AddFace(tfl, tfr, tbr, tbl, Vector3.UnitY);
        // Bottom (-Y)
        this.AddFace(bbl, bbr, bfr, bfl, -Vector3.UnitY);

        return this;
    }

    /// <summary>
    /// Adds a cylinder. The cylinder is centered on XZ with its bottom at Y=0.
    /// Sides use smooth normals; caps use flat normals.
    /// </summary>
    /// <param name="radius">The radius of the cylinder.</param>
    /// <param name="height">The height of the cylinder.</param>
    /// <param name="segments">The number of segments around the circumference.</param>
    /// <param name="position">An offset applied to all vertices.</param>
    /// <returns>This builder, for chaining.</returns>
    public PrimitiveMeshBuilder AddCylinder(float radius, float height, int segments = 16, Vector3 position = default)
    {
        float step = MathF.PI * 2f / segments;

        // --- Side ---
        ushort sideBase = (ushort)this._positions.Count;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * step;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            float u = (float)i / segments;

            Vector3 normal = new Vector3(cos, 0, sin);
            Vector3 bottom = new Vector3(cos * radius, 0, sin * radius) + position;
            Vector3 top = new Vector3(cos * radius, height, sin * radius) + position;

            this._positions.Add(bottom);
            this._normals.Add(normal);
            this._texCoords.Add(new Vector2(u, 1));

            this._positions.Add(top);
            this._normals.Add(normal);
            this._texCoords.Add(new Vector2(u, 0));
        }

        for (int i = 0; i < segments; i++)
        {
            ushort bl = (ushort)(sideBase + i * 2);
            ushort tl = (ushort)(sideBase + i * 2 + 1);
            ushort br = (ushort)(sideBase + (i + 1) * 2);
            ushort tr = (ushort)(sideBase + (i + 1) * 2 + 1);

            this._indices.Add(bl);
            this._indices.Add(tl);
            this._indices.Add(tr);

            this._indices.Add(bl);
            this._indices.Add(tr);
            this._indices.Add(br);
        }

        // --- Top cap ---
        this.AddCapFan(radius, height, segments, step, Vector3.UnitY, position);

        // --- Bottom cap ---
        this.AddCapFan(radius, 0, segments, step, -Vector3.UnitY, position);

        return this;
    }

    /// <summary>
    /// Adds a UV sphere. The sphere is centered on XZ with its bottom at Y=0.
    /// </summary>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="rings">The number of horizontal rings (latitude divisions).</param>
    /// <param name="segments">The number of vertical segments (longitude divisions).</param>
    /// <param name="position">An offset applied to all vertices.</param>
    /// <returns>This builder, for chaining.</returns>
    public PrimitiveMeshBuilder AddSphere(float radius, int rings = 12, int segments = 16, Vector3 position = default)
    {
        Vector3 center = new Vector3(0, radius, 0) + position;
        ushort baseIndex = (ushort)this._positions.Count;

        for (int ring = 0; ring <= rings; ring++)
        {
            float phi = MathF.PI * ring / rings;
            float cosPhi = MathF.Cos(phi);
            float sinPhi = MathF.Sin(phi);

            for (int seg = 0; seg <= segments; seg++)
            {
                float theta = MathF.PI * 2f * seg / segments;
                float cosTheta = MathF.Cos(theta);
                float sinTheta = MathF.Sin(theta);

                Vector3 normal = new Vector3(sinPhi * cosTheta, cosPhi, sinPhi * sinTheta);
                Vector3 pos = center + normal * radius;
                Vector2 uv = new Vector2((float)seg / segments, (float)ring / rings);

                this._positions.Add(pos);
                this._normals.Add(normal);
                this._texCoords.Add(uv);
            }
        }

        int stride = segments + 1;
        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                ushort tl = (ushort)(baseIndex + ring * stride + seg);
                ushort tr = (ushort)(baseIndex + ring * stride + seg + 1);
                ushort bl = (ushort)(baseIndex + (ring + 1) * stride + seg);
                ushort br = (ushort)(baseIndex + (ring + 1) * stride + seg + 1);

                this._indices.Add(tl);
                this._indices.Add(tr);
                this._indices.Add(bl);

                this._indices.Add(tr);
                this._indices.Add(br);
                this._indices.Add(bl);
            }
        }

        return this;
    }

    /// <summary>
    /// Adds a flat plane on the XZ plane at Y=0, with its normal pointing up.
    /// </summary>
    /// <param name="width">The size along the X axis.</param>
    /// <param name="depth">The size along the Z axis.</param>
    /// <param name="position">An offset applied to all vertices.</param>
    /// <returns>This builder, for chaining.</returns>
    public PrimitiveMeshBuilder AddPlane(float width, float depth, Vector3 position = default)
    {
        float hw = width / 2f;
        float hd = depth / 2f;

        Vector3 bl = new Vector3(-hw, 0, -hd) + position;
        Vector3 br = new Vector3(hw, 0, -hd) + position;
        Vector3 tr = new Vector3(hw, 0, hd) + position;
        Vector3 tl = new Vector3(-hw, 0, hd) + position;

        this.AddFace(tl, tr, br, bl, Vector3.UnitY);

        return this;
    }

    /// <summary>
    /// Builds a <see cref="BasicMeshData"/> from the accumulated geometry.
    /// </summary>
    /// <returns>The mesh data.</returns>
    public BasicMeshData BuildData()
    {
        return new BasicMeshData(
            this._indices.ToArray(),
            this._positions.ToArray(),
            this._normals.ToArray(),
            this._texCoords.ToArray()
        );
    }

    /// <summary>
    /// Builds a GPU-ready <see cref="Mesh"/> from the accumulated geometry.
    /// </summary>
    /// <returns>The mesh.</returns>
    public Mesh BuildMesh()
    {
        return new Mesh(this.BuildData());
    }

    /// <summary>
    /// Builds a <see cref="MeshPrefab"/> from the accumulated geometry.
    /// </summary>
    /// <returns>The mesh prefab.</returns>
    public MeshPrefab BuildPrefab()
    {
        return new MeshPrefab(this.BuildMesh());
    }

    /// <summary>
    /// Adds a rectangular frustum (tapered box). The shape is centered on XZ at both the bottom
    /// and top, with independently sized ends. Useful for tapered posts, table legs, pedestals, etc.
    /// </summary>
    /// <param name="bottomWidth">The width (X axis) at the bottom.</param>
    /// <param name="bottomDepth">The depth (Z axis) at the bottom.</param>
    /// <param name="topWidth">The width (X axis) at the top.</param>
    /// <param name="topDepth">The depth (Z axis) at the top.</param>
    /// <param name="height">The height (Y axis).</param>
    /// <param name="position">An offset applied to all vertices.</param>
    /// <returns>This builder, for chaining.</returns>
    public PrimitiveMeshBuilder AddFrustum(
        float bottomWidth, float bottomDepth,
        float topWidth, float topDepth,
        float height, Vector3 position = default
    )
    {
        float bhw = bottomWidth / 2f;
        float bhd = bottomDepth / 2f;
        float thw = topWidth / 2f;
        float thd = topDepth / 2f;

        Vector3 bfl = new Vector3(-bhw, 0, bhd) + position;
        Vector3 bfr = new Vector3(bhw, 0, bhd) + position;
        Vector3 bbl = new Vector3(-bhw, 0, -bhd) + position;
        Vector3 bbr = new Vector3(bhw, 0, -bhd) + position;
        Vector3 tfl = new Vector3(-thw, height, thd) + position;
        Vector3 tfr = new Vector3(thw, height, thd) + position;
        Vector3 tbl = new Vector3(-thw, height, -thd) + position;
        Vector3 tbr = new Vector3(thw, height, -thd) + position;

        this.AddQuad(bfl, bfr, tfr, tfl); // front (+Z)
        this.AddQuad(bbr, bbl, tbl, tbr); // back (-Z)
        this.AddQuad(bfr, bbr, tbr, tfr); // right (+X)
        this.AddQuad(bbl, bfl, tfl, tbl); // left (-X)
        this.AddQuad(tfl, tfr, tbr, tbl); // top (+Y)
        this.AddQuad(bbl, bbr, bfr, bfl); // bottom (-Y)

        return this;
    }

    /// <summary>
    /// Adds a quad defined by four explicit vertices. The normal is computed automatically
    /// from the winding order (counter-clockwise when viewed from the front).
    /// </summary>
    /// <param name="a">Bottom-left vertex.</param>
    /// <param name="b">Bottom-right vertex.</param>
    /// <param name="c">Top-right vertex.</param>
    /// <param name="d">Top-left vertex.</param>
    /// <returns>This builder, for chaining.</returns>
    public PrimitiveMeshBuilder AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        var normal = Vector3.Normalize(Vector3.Cross(b - a, d - a));
        this.AddFace(a, b, c, d, normal);
        return this;
    }

    private void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
    {
        ushort baseIndex = (ushort)this._positions.Count;

        this._positions.Add(a);
        this._positions.Add(b);
        this._positions.Add(c);
        this._positions.Add(d);

        this._normals.Add(normal);
        this._normals.Add(normal);
        this._normals.Add(normal);
        this._normals.Add(normal);

        this._texCoords.Add(new Vector2(0, 1));
        this._texCoords.Add(new Vector2(1, 1));
        this._texCoords.Add(new Vector2(1, 0));
        this._texCoords.Add(new Vector2(0, 0));

        this._indices.Add(baseIndex);
        this._indices.Add((ushort)(baseIndex + 1));
        this._indices.Add((ushort)(baseIndex + 2));

        this._indices.Add(baseIndex);
        this._indices.Add((ushort)(baseIndex + 2));
        this._indices.Add((ushort)(baseIndex + 3));
    }

    private void AddCapFan(float radius, float y, int segments, float step, Vector3 normal, Vector3 position)
    {
        bool isTop = normal.Y > 0;
        ushort centerIndex = (ushort)this._positions.Count;

        this._positions.Add(new Vector3(0, y, 0) + position);
        this._normals.Add(normal);
        this._texCoords.Add(new Vector2(0.5f, 0.5f));

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * step;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            this._positions.Add(new Vector3(cos * radius, y, sin * radius) + position);
            this._normals.Add(normal);
            this._texCoords.Add(new Vector2(cos * 0.5f + 0.5f, sin * 0.5f + 0.5f));
        }

        for (int i = 0; i < segments; i++)
        {
            ushort current = (ushort)(centerIndex + 1 + i);
            ushort next = (ushort)(centerIndex + 2 + i);

            if (isTop)
            {
                this._indices.Add(centerIndex);
                this._indices.Add(next);
                this._indices.Add(current);
            }
            else
            {
                this._indices.Add(centerIndex);
                this._indices.Add(current);
                this._indices.Add(next);
            }
        }
    }
}
