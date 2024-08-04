using System;
using System.Collections.Generic;
using LifeSim.Imago.Graphics.Materials;
using LifeSim.Imago.Graphics.Rendering.Buffers;
using Veldrid;
using Texture = LifeSim.Imago.Graphics.Textures.Texture;

namespace LifeSim.Imago.Graphics.Rendering;

public partial class RendererResources : IDisposable
{
    public const int MIN_BUFFER_BLOCKS = 1024;

    public ResourceLayout TransformResourceLayout { get; }

    public ResourceLayout InstanceResourceLayout { get; }

    public ResourceLayout SkeletonResourceLayout { get; }

    private readonly GraphicsDevice _gd;
    private readonly List<DataBuffer> _instanceDataBuffers = [];
    private readonly List<DataBuffer> _transformDataBuffers = [];
    private readonly List<DataBuffer> _skeletonDataBuffers = [];
    private readonly List<Texture> _dirtyTextures = [];
    private readonly List<Material> _dirtyMaterials = [];

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

        var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, instanceDataBlockSize, this.InstanceResourceLayout);
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

        var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, 64, this.TransformResourceLayout);
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

        var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS / Skeleton.MAX_NUMBER_OF_BONES, Skeleton.MAX_NUMBER_OF_BONES * 64, this.SkeletonResourceLayout);
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
        this._dirtyTextures.Add(texture);
    }

    /// <summary>
    /// Notifies the renderer that the given material's resources are dirty and need to be updated.
    /// </summary>
    /// <param name="material">The material to update.</param>
    internal void NotifyMaterialResourcesDirty(Material material)
    {
        this._dirtyMaterials.Add(material);
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

        if (this._dirtyMaterials.Count > 0)
        {
            foreach (var material in this._dirtyMaterials)
            {
                material.Update();
            }
            this._dirtyMaterials.Clear();
        }

        if (this._dirtyTextures.Count > 0)
        {
            foreach (var resource in this._dirtyTextures)
            {
                resource.Update(commandList);
            }
            this._dirtyTextures.Clear();
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
