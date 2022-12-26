using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Meshes;
using LifeSim.Support;
using Veldrid.Utilities;

namespace LifeSim.Engine.SceneGraph;

public struct DebugLine
{
    public Vector3 Start;
    public Vector3 End;
    public Color Color;
    public float LifeTime;
    public bool DrawInFront;
}

public class GizmosLayer
{
    private readonly SwapPopList<DebugLine> _lines = new SwapPopList<DebugLine>();
    public IReadOnlyList<DebugLine> Lines => this._lines;

    public static GizmosLayer Default { get; private set; } = null!;

    public GizmosLayer()
    {
        Default = this;
    }


    /// <summary>
    /// Updates the gizmos layer.
    /// </summary>
    /// <param name="deltaTime">The delta time.</param>
    public void Update(float deltaTime)
    {
        for (int i = 0; i < this._lines.Count; i++)
        {
            var line = this._lines[i];
            line.LifeTime -= deltaTime;
            this._lines[i] = line;
            if (line.LifeTime <= 0)
            {
                this._lines.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Draws a line from start to end with the given color.
    /// </summary>
    /// <param name="start">The start point of the line.</param>
    /// <param name="end">The end point of the line.</param>
    /// <param name="color">The color of the line.</param>
    /// <param name="lifeTime">The amount of time the line will be drawn. A value of 0 means that the line will be drawn until the next frame.</param>
    /// <param name="drawInFront">Whether the line should be drawn in front other objects or using depth.</param>
    public void DrawLine(Vector3 start, Vector3 end, Color color, float lifeTime = 0, bool drawInFront = false)
    {
        this._lines.Add(new DebugLine
        {
            Start = start,
            End = end,
            Color = color,
            LifeTime = lifeTime,
            DrawInFront = drawInFront
        });
    }

    /// <summary>
    /// Draws a frustum with the given color.
    /// </summary>
    /// <param name="frustum">The frustum to draw.</param>
    /// <param name="color">The color of the frustum.</param>
    /// <param name="lifeTime">The amount of time the frustum will be drawn. A value of 0 means that the frustum will be drawn until the next frame.</param>
    /// <param name="drawInFront">Whether the frustum should be drawn in front other objects or using depth.</param>
    public void DrawFrustum(ref BoundingFrustum frustum, Color color, float lifeTime = 0, bool drawInFront = false)
    {
        var corners = frustum.GetCorners();
        this.DrawLine(corners.FarBottomLeft, corners.FarBottomRight, color, lifeTime, drawInFront);
        this.DrawLine(corners.FarBottomRight, corners.FarTopRight, color, lifeTime, drawInFront);
        this.DrawLine(corners.FarTopRight, corners.FarTopLeft, color, lifeTime, drawInFront);
        this.DrawLine(corners.FarTopLeft, corners.FarBottomLeft, color, lifeTime, drawInFront);

        this.DrawLine(corners.NearBottomLeft, corners.NearBottomRight, color, lifeTime, drawInFront);
        this.DrawLine(corners.NearBottomRight, corners.NearTopRight, color, lifeTime, drawInFront);
        this.DrawLine(corners.NearTopRight, corners.NearTopLeft, color, lifeTime, drawInFront);
        this.DrawLine(corners.NearTopLeft, corners.NearBottomLeft, color, lifeTime, drawInFront);

        this.DrawLine(corners.NearBottomLeft, corners.FarBottomLeft, color, lifeTime, drawInFront);
        this.DrawLine(corners.NearBottomRight, corners.FarBottomRight, color, lifeTime, drawInFront);
        this.DrawLine(corners.NearTopRight, corners.FarTopRight, color, lifeTime, drawInFront);
        this.DrawLine(corners.NearTopLeft, corners.FarTopLeft, color, lifeTime, drawInFront);
    }

    /// <summary>
    /// Draws a wireframe sphere at the given position.
    /// </summary>
    /// <param name="position">The position of the sphere.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="color">The color of the sphere.</param>
    /// <param name="lifeTime">The amount of time the sphere will be visible. A value of 0 means that the sphere will be drawn until the next frame.</param>
    /// <param name="drawInFront">Whether the sphere should be drawn in front other objects or using depth.</param>
    public void DrawWireSphere(Vector3 position, float radius, Color color, float lifeTime = 0, bool drawInFront = false)
    {
        float deltaStep = MathF.PI * 2 / 12;
        for (float theta = 0; theta < MathF.PI * 2; theta += deltaStep)
        {

            for (float phi = 0; phi < MathF.PI * 2; phi += deltaStep)
            {
                Vector3 start = position + radius * new Vector3(
                        MathF.Cos(theta) * MathF.Cos(phi),
                        MathF.Sin(phi),
                        MathF.Sin(theta) * MathF.Cos(phi)
                    );
                Vector3 end = position + radius * new Vector3(
                        MathF.Cos(theta + deltaStep) * MathF.Cos(phi),
                        MathF.Sin(phi),
                        MathF.Sin(theta + deltaStep) * MathF.Cos(phi)
                    );
                this.DrawLine(start, end, color, lifeTime, drawInFront);
            }

            for (float phi = 0; phi < MathF.PI * 2; phi += deltaStep)
            {
                Vector3 start = position + radius * new Vector3(
                        MathF.Cos(theta) * MathF.Cos(phi),
                        MathF.Sin(phi),
                        MathF.Sin(theta) * MathF.Cos(phi)
                    );
                Vector3 end = position + radius * new Vector3(
                        MathF.Cos(theta) * MathF.Cos(phi + deltaStep),
                        MathF.Sin(phi + deltaStep),
                        MathF.Sin(theta) * MathF.Cos(phi + deltaStep)
                    );
                this.DrawLine(start, end, color, lifeTime, drawInFront);
            }
        }
    }


    /// <summary>
    /// Draws a wireframe cube at the given position.
    /// </summary>
    /// <param name="position">The position of the cube.</param>
    /// <param name="size">The size of the cube.</param>
    /// <param name="color">The color of the cube.</param>
    /// <param name="lifeTime">The amount of time the cube will be visible. A value of 0 means that the cube will be drawn until the next frame.</param>
    /// <param name="drawInFront">Whether the cube should be drawn in front other objects or using depth.</param>
    public void DrawWireCube(Vector3 center, Vector3 size, Color color, float lifeTime = 0, bool drawInFront = false)
    {
        var halfSize = size / 2;
        var min = center - halfSize;
        var max = center + halfSize;

        Span<Vector3> bottom = stackalloc Vector3[4] {
            new Vector3(min.X, min.Y, min.Z), // Back Left
            new Vector3(max.X, min.Y, min.Z), // Back Right
            new Vector3(max.X, min.Y, max.Z), // Front Right
            new Vector3(min.X, min.Y, max.Z), // Front Left
        };

        Span<Vector3> top = stackalloc Vector3[4] {
            new Vector3(min.X, max.Y, min.Z), // Back Left
            new Vector3(max.X, max.Y, min.Z), // Back Right
            new Vector3(max.X, max.Y, max.Z), // Front Right
            new Vector3(min.X, max.Y, max.Z), // Front Left
        };

        // Draw a wired cube
        for (int i = 0; i < 4; i++)
        {
            this.DrawLine(bottom[i], bottom[(i + 1) % 4], color, lifeTime, drawInFront);
            this.DrawLine(top[i], top[(i + 1) % 4], color, lifeTime, drawInFront);
            this.DrawLine(bottom[i], top[i], color, lifeTime, drawInFront);
        }
    }

    private static void VecOrthogonalBasis(Vector3 v, out Vector3 u, out Vector3 w)
    {
        if (MathF.Abs(v.X) > MathF.Abs(v.Y))
        {
            float invLen = 1.0f / MathF.Sqrt(v.X * v.X + v.Z * v.Z);
            u = new Vector3(-v.Z * invLen, 0.0f, v.X * invLen);
        }
        else
        {
            float invLen = 1.0f / MathF.Sqrt(v.Y * v.Y + v.Z * v.Z);
            u = new Vector3(0.0f, v.Z * invLen, -v.Y * invLen);
        }
        w = Vector3.Cross(v, u);
    }


    /// <summary>
    /// Draws a circle at the given position.
    /// </summary>
    /// <param name="position">The position of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="planeNormal">The normal of the plane the circle is drawn on.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="segments">The amount of segments the circle will be divided into.</param>
    /// <param name="lifeTime">The amount of time the circle will be visible. A value of 0 means that the circle will be drawn until the next frame.</param>
    /// <param name="drawInFront">Whether the circle should be drawn in front other objects or using depth.</param>
    public void DrawCircle(Vector3 center, float radius, Vector3 planeNormal, Color color, int segments = 12, float lifeTime = 0, bool drawInFront = false)
    {
        float deltaStep = MathF.PI * 2 / segments;

        VecOrthogonalBasis(planeNormal, out Vector3 u, out Vector3 w);


        for (float theta = 0; theta < MathF.PI * 2; theta += deltaStep)
        {
            Vector3 start = center + radius * new Vector3(
                    MathF.Cos(theta) * u.X + MathF.Sin(theta) * w.X,
                    MathF.Cos(theta) * u.Y + MathF.Sin(theta) * w.Y,
                    MathF.Cos(theta) * u.Z + MathF.Sin(theta) * w.Z
                );
            Vector3 end = center + radius * new Vector3(
                    MathF.Cos(theta + deltaStep) * u.X + MathF.Sin(theta + deltaStep) * w.X,
                    MathF.Cos(theta + deltaStep) * u.Y + MathF.Sin(theta + deltaStep) * w.Y,
                    MathF.Cos(theta + deltaStep) * u.Z + MathF.Sin(theta + deltaStep) * w.Z
                );
            this.DrawLine(start, end, color, lifeTime, drawInFront);
        }
    }

    public void DrawCone(Vector3 basePosition, Vector3 apexPosition, float baseRadius, Color color, int segments = 12, float lifeTime = 0, bool drawInFront = false)
    {
        float deltaStep = MathF.PI * 2 / segments;
        Vector3 direction = apexPosition - basePosition;

        VecOrthogonalBasis(Vector3.Normalize(direction), out Vector3 u, out Vector3 w);


        for (float theta = 0; theta < MathF.PI * 2; theta += deltaStep)
        {
            Vector3 start = basePosition + baseRadius * new Vector3(
                    MathF.Cos(theta) * u.X + MathF.Sin(theta) * w.X,
                    MathF.Cos(theta) * u.Y + MathF.Sin(theta) * w.Y,
                    MathF.Cos(theta) * u.Z + MathF.Sin(theta) * w.Z
                );
            Vector3 end = basePosition + baseRadius * new Vector3(
                    MathF.Cos(theta + deltaStep) * u.X + MathF.Sin(theta + deltaStep) * w.X,
                    MathF.Cos(theta + deltaStep) * u.Y + MathF.Sin(theta + deltaStep) * w.Y,
                    MathF.Cos(theta + deltaStep) * u.Z + MathF.Sin(theta + deltaStep) * w.Z
                );
            this.DrawLine(start, end, color, lifeTime, drawInFront);
            this.DrawLine(apexPosition, end, color, lifeTime, drawInFront);
        }
    }

    /// <summary>
    /// Draws an arrow from the start position to the end position.
    /// </summary>
    /// <param name="start">The start position of the arrow.</param>
    /// <param name="end">The end position of the arrow.</param>
    /// <param name="color">The color of the arrow.</param>
    /// <param name="size">The size of the head of the arrow in world units.</param>
    /// <param name="lifeTime">The amount of time the arrow will be visible. A value of 0 means that the arrow will be drawn until the next frame.</param>
    /// <param name="drawInFront">Whether the arrow should be drawn in front other objects or using depth.</param>
    public void DrawArrow(Vector3 start, Vector3 end, Color color, float size = -1, float lifeTime = 0, bool drawInFront = false)
    {
        var length = (end - start).Length();
        var coneEnd = end;
        float coneLength = size >= 0 ? size : length * 0.2f;
        var coneStart = end - coneLength * Vector3.Normalize(end - start);

        this.DrawLine(start, end, color, lifeTime, drawInFront);
        this.DrawCone(coneStart, coneEnd, coneLength, color, 12, lifeTime, drawInFront);
    }

    /// <summary>
    /// Draws a cross at the given position.
    /// </summary>
    /// <param name="position">The position of the cross.</param>
    /// <param name="length">The lenght of the cross.</param>
    /// <param name="color">The color of the cross.</param>
    /// <param name="lifeTime">The amount of time the cross will be visible. A value of 0 means that the cross will be drawn until the next frame.</param>
    /// <param name="drawInFront">Whether the cross should be drawn in front other objects or using depth.</param>
    public void DrawCross(Vector3 position, float length, Color color, float lifeTime = 0, bool drawInFront = false)
    {
        this.DrawLine(position + new Vector3(-length, 0, 0), position + new Vector3(length, 0, 0), color, lifeTime, drawInFront);
        this.DrawLine(position + new Vector3(0, -length, 0), position + new Vector3(0, length, 0), color, lifeTime, drawInFront);
        this.DrawLine(position + new Vector3(0, 0, -length), position + new Vector3(0, 0, length), color, lifeTime, drawInFront);
    }

    /// <summary>
    /// Draws a plane with an arrow pointing in the normal direction.
    /// </summary>
    /// <param name="position">The position of the plane.</param>
    /// <param name="normal">The normal of the plane.</param>
    /// <param name="planeColor">The color of the plane.</param>
    /// <param name="normalColor">The color of the arrow.</param>
    /// <param name="lifeTime">The amount of time the plane will be visible. A value of 0 means that the plane will be drawn until the next frame.</param>
    /// <param name="drawInFront">Whether the plane should be drawn in front other objects or using depth.</param>
    public void DrawPlane(Vector3 position, Vector3 planeNormal, Color planeColor, Color normalColor, float lifeTime = 0, bool drawInFront = false)
    {
        VecOrthogonalBasis(planeNormal, out Vector3 u, out Vector3 w);

        var p1 = position + u * -0.5f + w * -0.5f;
        var p2 = position + u * 0.5f + w * -0.5f;
        var p3 = position + u * 0.5f + w * 0.5f;
        var p4 = position + u * -0.5f + w * 0.5f;

        this.DrawLine(p1, p2, planeColor, lifeTime, drawInFront);
        this.DrawLine(p2, p3, planeColor, lifeTime, drawInFront);
        this.DrawLine(p3, p4, planeColor, lifeTime, drawInFront);
        this.DrawLine(p4, p1, planeColor, lifeTime, drawInFront);

        this.DrawArrow(position, position + planeNormal, normalColor, -1f, lifeTime, drawInFront);
    }

    public void DrawWireMesh(IMeshData mesh, ref Matrix4x4 transform, Color color, float lifeTime = 0, bool drawInFront = false)
    {
        for (int i = 0; i < mesh.Indices.Length; i += 3)
        {
            var i1 = mesh.Indices[i];
            var i2 = mesh.Indices[i + 1];
            var i3 = mesh.Indices[i + 2];

            var v1 = Vector3.Transform(mesh.Positions[i1], transform);
            var v2 = Vector3.Transform(mesh.Positions[i2], transform);
            var v3 = Vector3.Transform(mesh.Positions[i3], transform);

            this.DrawLine(v1, v2, color, lifeTime, drawInFront);
            this.DrawLine(v2, v3, color, lifeTime, drawInFront);
            this.DrawLine(v3, v1, color, lifeTime, drawInFront);
        }
    }

    /// <summary>
    /// Draws a circle at the given position, aligned in the XZ plane.
    /// </summary>
    /// <param name="position">The position of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="segments">The amount of segments the circle will be divided into.</param>
    /// <param name="lifeTime">The amount of time the circle will be visible. A value of 0 means that the circle will be drawn until the next frame.</param>
    /// <param name="drawInFront">Whether the circle should be drawn in front other objects or using depth.</param>
    public void DrawCircle(Vector3 position, float radius, Color color, int segments = 32, float lifeTime = 0, bool drawInFront = false)
    {
        var step = MathF.PI * 2 / segments;
        var angle = 0f;

        for (int i = 0; i < segments; i++)
        {
            var p1 = position + new Vector3(MathF.Cos(angle), 0, MathF.Sin(angle)) * radius;
            var p2 = position + new Vector3(MathF.Cos(angle + step), 0, MathF.Sin(angle + step)) * radius;

            this.DrawLine(p1, p2, color, lifeTime, drawInFront);

            angle += step;
        }
    }
}
