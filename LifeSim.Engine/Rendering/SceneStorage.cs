using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class SceneStorage : IDisposable
{
    public const int MIN_BUFFER_BLOCKS = 1024;
    private readonly GraphicsDevice _gd;
    private readonly List<DataBuffer> _instanceDataBuffers = new List<DataBuffer>();
    private readonly List<DataBuffer> _transformDataBuffers = new List<DataBuffer>();
    private readonly List<DataBuffer> _skeletonDataBuffers = new List<DataBuffer>();
    public readonly ResourceLayout TransformResourceLayout;
    public readonly ResourceLayout InstanceResourceLayout;
    public readonly ResourceLayout SkeletonResourceLayout;

    private readonly List<Skeleton> _skeletons = new List<Skeleton>();

    public SceneStorage(GraphicsDevice gd)
    {
        this._gd = gd;
        var factory = gd.ResourceFactory;
        this.InstanceResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("InstanceDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));
        this.InstanceResourceLayout.Name = "InstanceData Resource Layout";

        this.TransformResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("TransformDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        this.TransformResourceLayout.Name = "TransformData Resource Layout";

        this.SkeletonResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("BonesDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        this.SkeletonResourceLayout.Name = "BonesData Resource Layout";
    }

    internal DataBlock RequestTransformDataBlock()
    {
        for (int i = 0; i < this._transformDataBuffers.Count; i++)
        {
            var buffer = this._transformDataBuffers[i];
            if (!buffer.IsFull)
            {
                return buffer.RequestBlock();
            }
        }

        var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, 64, this.TransformResourceLayout);
        newBuffer.Name = "TransformDataBuffer " + this._transformDataBuffers.Count;
        this._transformDataBuffers.Add(newBuffer);
        return newBuffer.RequestBlock();
    }

    internal void RegisterSkeleton(Skeleton skeleton)
    {
        this._skeletons.Add(skeleton);
    }

    internal DataBlock RequestInstanceDataBlock(MaterialDefinition material)
    {
        var blockSize = material.InstanceDataBlockSize;
        for (int i = 0; i < this._instanceDataBuffers.Count; i++)
        {
            var buffer = this._instanceDataBuffers[i];
            if (buffer.BlockSize == blockSize && !buffer.IsFull)
            {
                return buffer.RequestBlock();
            }
        }

        var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, blockSize, this.InstanceResourceLayout);
        newBuffer.Name = "InstanceDataBuffer " + this._instanceDataBuffers.Count;
        this._instanceDataBuffers.Add(newBuffer);
        return newBuffer.RequestBlock();
    }

    internal void UnregisterSkeleton(Skeleton skeleton)
    {
        this._skeletons.Remove(skeleton);
    }

    internal DataBlock RequestSkeletonDataBlock()
    {
        for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
        {
            var buffer = this._skeletonDataBuffers[i];
            if (!buffer.IsFull)
            {
                return buffer.RequestBlock();
            }
        }

        var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS / Skeleton.MAX_NUMBER_OF_BONES, Skeleton.MAX_NUMBER_OF_BONES * 64, this.SkeletonResourceLayout);
        newBuffer.Name = "SkeletonDataBuffer " + this._skeletonDataBuffers.Count;
        this._skeletonDataBuffers.Add(newBuffer);
        return newBuffer.RequestBlock();
    }

    internal void UpdateBuffers(CommandList commandList)
    {
        for (int i = 0; i < this._instanceDataBuffers.Count; i++)
        {
            this._instanceDataBuffers[i].UploadToGPU(commandList);
        }

        for (int i = 0; i < this._transformDataBuffers.Count; i++)
        {
            this._transformDataBuffers[i].UploadToGPU(commandList);
        }

        foreach (var skeleton in this._skeletons)
        {
            skeleton.Update();
        }

        for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
        {
            this._skeletonDataBuffers[i].UploadToGPU(commandList);
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < this._instanceDataBuffers.Count; i++)
        {
            this._instanceDataBuffers[i].Dispose();
        }

        for (int i = 0; i < this._transformDataBuffers.Count; i++)
        {
            this._transformDataBuffers[i].Dispose();
        }

        for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
        {
            this._skeletonDataBuffers[i].Dispose();
        }

        foreach (var skeleton in this._skeletons)
        {
            skeleton.Dispose();
        }
    }
}