using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Rendering;

public class Skeleton : IDisposable
{
    public const int MAX_NUMBER_OF_BONES = 64;
    public readonly IList<Node3D> Joints = new List<Node3D>();
    public readonly IList<Matrix4x4> InverseBindMatrices;

    private readonly Matrix4x4[] _bonesMatrices;

    private readonly SceneStorage _storage;

    public Veldrid.ResourceSet ResourceSet { get; }

    private DataBlock _dataBlock;

    public int BufferId => this._dataBlock.Buffer.Id;

    public uint BoneDataOffset => this._dataBlock.BlockIndex * MAX_NUMBER_OF_BONES;

    public Skeleton(Renderer renderer, IList<Node3D> joints, IList<Matrix4x4> inverseBindMatrices)
    {
        this._storage = renderer.Storage;
        this.Joints = joints;
        this.InverseBindMatrices = inverseBindMatrices;
        this._bonesMatrices = new Matrix4x4[this.Joints.Count];
        this._dataBlock = this._storage.RequestSkeletonDataBlock();
        this.ResourceSet = this._dataBlock.Buffer.ResourceSet;
        this._storage.RegisterSkeleton(this);
    }

    public Matrix4x4[] BonesMatrices => this._bonesMatrices;

    private Matrix4x4 _rootTransform;

    public Matrix4x4 RootTransform
    {
        get => this._rootTransform;
        set => this._rootTransform = value;
    }

    public void Update()
    {
        Matrix4x4.Invert(this._rootTransform, out Matrix4x4 inverseMeshWorldMatrix);

        for (int i = 0; i < this.Joints.Count; i++)
        {
            this._bonesMatrices[i] = this.InverseBindMatrices[i] * this.Joints[i].WorldMatrix * inverseMeshWorldMatrix;
        }

        this._dataBlock.WriteSpan<Matrix4x4>(this._bonesMatrices);
    }

    public void Dispose()
    {
        this._dataBlock.FreeBlock();
        this._storage.UnregisterSkeleton(this);
    }
}