using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.Rendering.Buffers;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago;

public class Skeleton : IDisposable
{
    /// <summary>
    /// Gets the maximum number of bones in a skeleton.
    /// </summary>
    public const int MAX_NUMBER_OF_BONES = 64;

    /// <summary>
    /// Gets the joints of the skeleton.
    /// </summary>
    public IList<Node3D> Joints { get; } = new List<Node3D>();

    /// <summary>
    /// Gets a value indicating whether the skeleton has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// Gets the inverse bind matrices of the skeleton.
    /// </summary>
    public IList<Matrix4x4> InverseBindMatrices { get; }

    /// <summary>
    /// Gets the resource set of the buffer containing the bone data.
    /// </summary>
    public Veldrid.ResourceSet ResourceSet { get; }

    /// <summary>
    /// Gets the offset of the bone data in the buffer.
    /// </summary>
    public uint BoneDataOffset => this._dataBlock.BlockIndex * MAX_NUMBER_OF_BONES;

    /// <summary>
    /// Gets the bone matrices of the skeleton.
    /// </summary>
    public Matrix4x4[] BonesMatrices { get; }

    /// <summary>
    /// Gets or sets the inverse root transform of the skeleton.
    /// </summary>
    public Matrix4x4 InverseRootTransform { get; set; }

    private DataBlock _dataBlock;

    /// <summary>
    /// Initializes a new instance of the <see cref="Skeleton"/> class.
    /// </summary>
    /// <param name="joints">The joints of the skeleton.</param>
    /// <param name="inverseBindMatrices">The inverse bind matrices of the skeleton.</param>
    public Skeleton(IList<Node3D> joints, IList<Matrix4x4> inverseBindMatrices)
    {
        this.Joints = joints;
        this.InverseBindMatrices = inverseBindMatrices;
        this.BonesMatrices = new Matrix4x4[this.Joints.Count];
        this._dataBlock = Renderer.Instance.RequestSkeletonDataBlock();
        this.ResourceSet = this._dataBlock.Buffer.ResourceSet;
        Renderer.Instance.RegisterDisposable(this);
    }

    /// <summary>
    /// Updates the bone matrices of the skeleton.
    /// </summary>
    public void Update()
    {
        for (int i = 0; i < this.Joints.Count; i++)
        {
            this.BonesMatrices[i] = this.InverseBindMatrices[i] * this.Joints[i].WorldMatrix * this.InverseRootTransform;
        }

        this._dataBlock.WriteSpan<Matrix4x4>(this.BonesMatrices);
    }

    /// <summary>
    /// Disposes the skeleton.
    /// </summary>
    public void Dispose()
    {
        if (this.IsDisposed) return;
        this.IsDisposed = true;

        this._dataBlock.Dispose();
        Renderer.Instance.UnregisterDisposable(this);
    }
}
