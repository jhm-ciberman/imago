using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class SceneStorage : IDisposable
{
    public const int MIN_BUFFER_BLOCKS = 1024;

    /// <summary>
    /// Gets the <see cref="Renderer"/> that owns this <see cref="SceneStorage"/>.
    /// </summary>
    public Renderer Renderer { get; }

    private readonly GraphicsDevice _gd;
    private readonly ResourceFactory _factory;
    private readonly List<DataBuffer> _instanceDataBuffers = new List<DataBuffer>();
    private readonly List<DataBuffer> _transformDataBuffers = new List<DataBuffer>();
    private readonly List<DataBuffer> _skeletonDataBuffers = new List<DataBuffer>();

    private readonly List<Texture> _dirtyTextures = new();
    private readonly List<MaterialBase> _dirtyMaterials = new();
    private readonly List<Renderable> _dirtyRenderables = new();

    public ResourceLayout TransformResourceLayout { get; }
    public ResourceLayout InstanceResourceLayout { get; }
    public ResourceLayout SkeletonResourceLayout { get; }

    private readonly List<Skeleton> _skeletons = new List<Skeleton>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneStorage"/> class.
    /// </summary>
    /// <param name="renderer">The <see cref="Renderer"/> that owns this <see cref="SceneStorage"/>.</param>
    public SceneStorage(Renderer renderer)
    {
        this.Renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        this._factory = this._gd.ResourceFactory;
        this.InstanceResourceLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("InstanceDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));
        this.InstanceResourceLayout.Name = "InstanceData Resource Layout";

        this.TransformResourceLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("TransformDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        this.TransformResourceLayout.Name = "TransformData Resource Layout";

        this.SkeletonResourceLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("BonesDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        this.SkeletonResourceLayout.Name = "BonesData Resource Layout";

        Renderable.PipelineDirty += this.OnRenderablePipelineDirty;
        MaterialBase.MaterialResourceSetDirty += this.OnMaterialResourceSetDirty;
        Texture.TextureDirty += this.OnTextureDirty;
    }

    internal DataBlock RequestTransformDataBlock()
    {
        lock (this._transformDataBuffers)
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
    }

    internal DataBlock RequestInstanceDataBlock(int instanceDataBlockSize)
    {
        lock (this._instanceDataBuffers)
        {
            for (int i = 0; i < this._instanceDataBuffers.Count; i++)
            {
                var buffer = this._instanceDataBuffers[i];
                if (buffer.BlockSize == instanceDataBlockSize && !buffer.IsFull)
                {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, instanceDataBlockSize, this.InstanceResourceLayout);
            newBuffer.Name = "InstanceDataBuffer " + this._instanceDataBuffers.Count;
            this._instanceDataBuffers.Add(newBuffer);
            return newBuffer.RequestBlock();
        }
    }

    internal void RegisterSkeleton(Skeleton skeleton)
    {
        lock (this._skeletons)
        {
            this._skeletons.Add(skeleton);
        }
    }

    internal void UnregisterSkeleton(Skeleton skeleton)
    {
        lock (this._skeletons)
        {
            this._skeletons.Remove(skeleton);
        }
    }

    internal DataBlock RequestSkeletonDataBlock()
    {
        lock (this._skeletonDataBuffers)
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
    }

    internal void UpdateBuffers(CommandList commandList)
    {
        lock (this._instanceDataBuffers)
        {
            for (int i = 0; i < this._instanceDataBuffers.Count; i++)
            {
                this._instanceDataBuffers[i].UploadToGPU(commandList);
            }
        }

        lock (this._transformDataBuffers)
        {
            for (int i = 0; i < this._transformDataBuffers.Count; i++)
            {
                this._transformDataBuffers[i].UploadToGPU(commandList);
            }
        }

        lock (this._skeletons)
        {
            foreach (var skeleton in this._skeletons)
            {
                skeleton.Update();
            }
        }

        lock (this._instanceDataBuffers)
        {
            for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
            {
                this._skeletonDataBuffers[i].UploadToGPU(commandList);
            }
        }

        lock (this._dirtyMaterials)
        {
            if (this._dirtyMaterials.Count > 0)
            {
                foreach (var material in this._dirtyMaterials)
                {
                    material.Update(this._factory);
                }
                this._dirtyMaterials.Clear();
            }
        }

        lock (this._dirtyTextures)
        {
            if (this._dirtyTextures.Count > 0)
            {
                foreach (var resource in this._dirtyTextures)
                {
                    resource.Update(this._gd, commandList);
                }
                this._dirtyTextures.Clear();
            }
        }

        lock (this._dirtyRenderables)
        {
            if (this._dirtyRenderables.Count > 0)
            {
                foreach (var renderable in this._dirtyRenderables)
                {
                    renderable.Update(this.Renderer);
                }
                this._dirtyRenderables.Clear();
            }
        }
    }

    private void OnTextureDirty(Texture texture)
    {
        lock (this._dirtyTextures)
        {
            this._dirtyTextures.Add(texture);
        }
    }

    private void OnMaterialResourceSetDirty(MaterialBase material)
    {
        lock (this._dirtyMaterials)
        {
            this._dirtyMaterials.Add(material);
        }
    }

    private void OnRenderablePipelineDirty(Renderable renderable)
    {
        lock (this._dirtyRenderables)
        {
            this._dirtyRenderables.Add(renderable);
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