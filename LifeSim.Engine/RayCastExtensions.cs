using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid.Utilities;

namespace LifeSim.Engine;

public static class RayCastExtensions
{

    /// <summary>
    /// Performs a RayCast against the vertices of this mesh.
    /// </summary>
    /// <param name="ray">The ray to use. This ray should be in object-local space.</param>
    /// <param name="distance">If the RayCast is successful, contains the distance 
    /// from the <see cref="Ray"/> origin that the hit occurred.</param>
    /// <returns>True if the <see cref="Ray"/> intersects the mesh; false otherwise</returns>
    public static bool RayCast(this IMeshData mesh, Ray ray, out float distance)
    {
        distance = float.MaxValue;
        bool result = false;
        for (int i = 0; i < mesh.Indices.Length - 2; i += 3)
        {
            Vector3 v0 = mesh.Positions[mesh.Indices[i + 0]];
            Vector3 v1 = mesh.Positions[mesh.Indices[i + 1]];
            Vector3 v2 = mesh.Positions[mesh.Indices[i + 2]];

            if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
            {
                if (newDistance < distance)
                {
                    distance = newDistance;
                }

                result = true;
            }
        }

        return result;
    }

    /// <summary>
    /// Performs a RayCast against the vertices of this mesh.
    /// </summary>
    /// <param name="ray">The ray to use. This ray should be in object-local space.</param>
    /// <param name="distances">All of the distances at which the ray passes through the mesh.</param>
    /// <returns>The number of intersections.</returns>
    public static int RayCast(this IMeshData mesh, Ray ray, List<float> distances)
    {
        int hits = 0;
        for (int i = 0; i < mesh.Indices.Length - 2; i += 3)
        {
            Vector3 v0 = mesh.Positions[mesh.Indices[i + 0]];
            Vector3 v1 = mesh.Positions[mesh.Indices[i + 1]];
            Vector3 v2 = mesh.Positions[mesh.Indices[i + 2]];

            if (ray.Intersects(ref v0, ref v1, ref v2, out float newDistance))
            {
                hits++;
                distances.Add(newDistance);
            }
        }

        return hits;
    }
}