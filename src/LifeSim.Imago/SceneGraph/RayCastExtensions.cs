using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Assets.Meshes;
using LifeSim.Imago.Utilities;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Contains information about a ray hit including distance, position, normal, and mesh data.
/// </summary>
public struct HitInfo
{
    /// <summary>
    /// Gets or sets the distance from the ray origin to the hit point.
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// Gets or sets the surface normal at the hit point.
    /// </summary>
    public Vector3 Normal { get; set; }

    /// <summary>
    /// Gets or sets the world position of the hit point.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets the mesh data that was hit.
    /// </summary>
    public MeshData Mesh { get; set; }

    /// <summary>
    /// Gets or sets the index of the triangle that was hit.
    /// </summary>
    public int TriangleIndex { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HitInfo"/> struct.
    /// </summary>
    /// <param name="mesh">The mesh data that was hit.</param>
    /// <param name="triangleIndex">The index of the triangle that was hit.</param>
    /// <param name="distance">The distance from the ray origin to the hit point.</param>
    /// <param name="normal">The surface normal at the hit point.</param>
    /// <param name="position">The world position of the hit point.</param>
    public HitInfo(MeshData mesh, int triangleIndex, float distance, Vector3 normal, Vector3 position)
    {
        this.Mesh = mesh;
        this.TriangleIndex = triangleIndex;
        this.Distance = distance;
        this.Normal = normal;
        this.Position = position;
    }
}

/// <summary>
/// Provides extension methods for performing ray casting operations on meshes.
/// </summary>
public static class RayCastExtensions
{

    /// <summary>
    /// Performs a RayCast against the vertices of this mesh.
    /// </summary>
    /// <param name="mesh">The mesh to use.</param>
    /// <param name="ray">The ray to use. This ray should be in object-local space.</param>
    /// <param name="hitInfo">If the RayCast is successful, contains the hit information.</param>
    /// <returns>True if the <see cref="Ray"/> intersects the mesh; false otherwise</returns>
    public static bool RayCast(this MeshData mesh, Ray ray, out HitInfo hitInfo)
    {
        hitInfo = new HitInfo();
        hitInfo.Mesh = mesh;
        hitInfo.Distance = float.MaxValue;
        bool result = false;

        for (int i = 0; i < mesh.Indices.Length - 2; i += 3)
        {
            Vector3 v0 = mesh.Positions[mesh.Indices[i + 0]];
            Vector3 v1 = mesh.Positions[mesh.Indices[i + 1]];
            Vector3 v2 = mesh.Positions[mesh.Indices[i + 2]];

            if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
            {
                if (newDistance < hitInfo.Distance)
                {
                    hitInfo.TriangleIndex = i / 3;
                    hitInfo.Distance = newDistance;
                    hitInfo.Normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
                    hitInfo.Position = ray.Origin + ray.Direction * newDistance;
                    result = true;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Performs a RayCast against the vertices of this mesh.
    /// </summary>
    /// <param name="mesh">The mesh instance</param>
    /// <param name="ray">The ray to use. This ray should be in object-local space.</param>
    /// <param name="hitInfos">If the RayCast is successful, contains the hit
    /// information of every hit. The distances are not sorted.</param>
    /// <returns>True if the <see cref="Ray"/> intersects the mesh; false otherwise</returns>
    public static bool RayCast(this MeshData mesh, Ray ray, List<HitInfo> hitInfos)
    {
        hitInfos.Clear();
        for (int i = 0; i < mesh.Indices.Length - 2; i += 3)
        {
            Vector3 v0 = mesh.Positions[mesh.Indices[i + 0]];
            Vector3 v1 = mesh.Positions[mesh.Indices[i + 1]];
            Vector3 v2 = mesh.Positions[mesh.Indices[i + 2]];

            if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
            {
                HitInfo hitInfo = new HitInfo();
                hitInfo.Mesh = mesh;
                hitInfo.TriangleIndex = i / 3;
                hitInfo.Distance = newDistance;
                hitInfo.Normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
                hitInfo.Position = ray.Origin + ray.Direction * newDistance;
                hitInfos.Add(hitInfo);
            }
        }

        return hitInfos.Count > 0;
    }
}
