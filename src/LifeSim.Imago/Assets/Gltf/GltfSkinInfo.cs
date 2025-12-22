using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Imago.Assets.Gltf;

/// <summary>
/// Represents skinning information for a glTF model, including joint names and inverse bind matrices.
/// </summary>
public class GltfSkinInfo
{
    /// <summary>
    /// Gets the name of the root joint of the skin, if specified.
    /// </summary>
    public string? Root { get; }

    /// <summary>
    /// Gets a list of the names of the joints (nodes) that make up this skin.
    /// </summary>
    public IList<string> JointNames { get; }

    /// <summary>
    /// Gets a list of inverse bind matrices for the joints in the skin.
    /// </summary>
    public IList<Matrix4x4> InverseBindMatrices { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfSkinInfo"/> class.
    /// </summary>
    /// <param name="inverseBindMatrices">The inverse bind matrices for the joints.</param>
    /// <param name="jointNames">The names of the joints.</param>
    /// <param name="root">The name of the root joint.</param>
    public GltfSkinInfo(IList<Matrix4x4> inverseBindMatrices, IList<string> jointNames, string? root)
    {
        this.Root = root;
        this.JointNames = jointNames;
        this.InverseBindMatrices = inverseBindMatrices;
    }
}
