using System;
using System.Collections.Generic;
using LifeSim.Imago.Assets.Materials;
using LifeSim.Imago.Rendering.Buffers;
using LifeSim.Imago.SceneGraph;
using Veldrid;
using Texture = LifeSim.Imago.Assets.Textures.Texture;

namespace LifeSim.Imago.Rendering;

internal class RendererResources : IDisposable
{
    public const int MinBufferBlocks = 1024;

    public ResourceLayout TransformResourceLayout { get; }

    public ResourceLayout InstanceResourceLayout { get; }

    public ResourceLayout SkeletonResourceLayout { get; }

    private readonly GraphicsDevice _gd;
    private readonly List<DataBuffer> _instanceDataBuffers = [];
    private readonly List<DataBuffer> _transformDataBuffers = [];
    private readonly List<DataBuffer> _skeletonDataBuffers = [];
    private readonly List<Texture> _dirtyTextures = [];
    private readonly List<Material> _dirtyMaterials = [];
    private readonly object _dirtyLock = new();

    public RendererResources(GraphicsDevice graphicsDevice)
    {
        this._gd = graphicsDevice;

        var factory = graphicsDevice.ResourceFactory;

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


    internal DataBlock RequestInstanceDataBlock(int instanceDataBlockSize)
    {
        for (int i = 0; i < this._instanceDataBuffers.Count; i++)
        {
            var buffer = this._instanceDataBuffers[i];
            if (buffer.BlockSize == instanceDataBlockSize && !buffer.IsFull)
                return buffer.RequestBlock();
        }

        var newBuffer = new DataBuffer(this._gd, MinBufferBlocks, instanceDataBlockSize, this.InstanceResourceLayout);
        newBuffer.Name = "InstanceDataBuffer " + this._instanceDataBuffers.Count;
        this._instanceDataBuffers.Add(newBuffer);
        return newBuffer.RequestBlock();
    }

    internal DataBlock RequestTransformDataBlock()
    {
        for (int i = 0; i < this._transformDataBuffers.Count; i++)
        {
            var buffer = this._transformDataBuffers[i];
            if (!buffer.IsFull)
                return buffer.RequestBlock();
        }

        var newBuffer = new DataBuffer(this._gd, MinBufferBlocks, 64, this.TransformResourceLayout);
        newBuffer.Name = "TransformDataBuffer " + this._transformDataBuffers.Count;
        this._transformDataBuffers.Add(newBuffer);
        return newBuffer.RequestBlock();
    }


    internal DataBlock RequestSkeletonDataBlock()
    {
        for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
        {
            var buffer = this._skeletonDataBuffers[i];
            if (!buffer.IsFull)
                return buffer.RequestBlock();
        }

        var newBuffer = new DataBuffer(this._gd, MinBufferBlocks / Skeleton.MaxNumberOfBones, Skeleton.MaxNumberOfBones * 64, this.SkeletonResourceLayout);
        newBuffer.Name = "SkeletonDataBuffer " + this._skeletonDataBuffers.Count;
        this._skeletonDataBuffers.Add(newBuffer);
        return newBuffer.RequestBlock();
    }

    /// <summary>
    /// Notifies the renderer that the given texture is dirty and needs to be updated.
    /// </summary>
    /// <param name="texture">The texture to update.</param>
    internal void NotifyTextureDirty(Texture texture)
    {
        lock (this._dirtyLock)
        {
            this._dirtyTextures.Add(texture);
        }
    }

    /// <summary>
    /// Notifies the renderer that the given material's resources are dirty and need to be updated.
    /// </summary>
    /// <param name="material">The material to update.</param>
    internal void NotifyMaterialResourcesDirty(Material material)
    {
        lock (this._dirtyLock)
        {
            this._dirtyMaterials.Add(material);
        }
    }

    public void Update(CommandList commandList)
    {
        for (int i = 0; i < this._instanceDataBuffers.Count; i++)
        {
            this._instanceDataBuffers[i].UploadToGPU(commandList);
        }

        for (int i = 0; i < this._transformDataBuffers.Count; i++)
        {
            this._transformDataBuffers[i].UploadToGPU(commandList);
        }

        for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
        {
            this._skeletonDataBuffers[i].UploadToGPU(commandList);
        }

        // Snapshot and clear dirty lists under lock, then process without lock
        Material[] dirtyMaterials;
        Texture[] dirtyTextures;

        lock (this._dirtyLock)
        {
            dirtyMaterials = this._dirtyMaterials.Count > 0 ? [.. this._dirtyMaterials] : [];
            dirtyTextures = this._dirtyTextures.Count > 0 ? [.. this._dirtyTextures] : [];
            this._dirtyMaterials.Clear();
            this._dirtyTextures.Clear();
        }

        foreach (var material in dirtyMaterials)
        {
            material.Update();
        }

        foreach (var texture in dirtyTextures)
        {
            texture.Update(commandList);
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
    }
}
