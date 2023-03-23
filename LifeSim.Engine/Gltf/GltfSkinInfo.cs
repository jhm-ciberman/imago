using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.Gltf;

public class GltfSkinInfo
{
    public string? Root { get; }
    public IList<string> JointNames { get; }
    public IList<Matrix4x4> InverseBindMatrices { get; }

    public GltfSkinInfo(IList<Matrix4x4> inverseBindMatrices, IList<string> jointNames, string? root)
    {
        this.Root = root;
        this.JointNames = jointNames;
        this.InverseBindMatrices = inverseBindMatrices;
    }
}
