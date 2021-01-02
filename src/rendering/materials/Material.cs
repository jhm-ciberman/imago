using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Material : IMaterial
    {
        struct MaterialInfo
        {
            public Vector4 albedoColor;
        }

        private Pass _pass;
        public Pass pass => this._pass;

        private ResourceSet _resourceSet;


        private GraphicsDevice _gd;

        private DeviceBuffer _infoBuffer;

        private MaterialInfo _data;
        private bool _isDirty = true;

        public Material(Pass pass, GraphicsDevice gd, ResourceLayout materialLayout, GPUTexture texture) 
        {
            this._pass = pass;
            this._gd = gd;
            var factory = this._gd.ResourceFactory;
            this._infoBuffer = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<MaterialInfo>(), BufferUsage.UniformBuffer));
            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                materialLayout, texture.deviceTexture, texture.sampler, this._infoBuffer
            ));

            this._data = new MaterialInfo {
                albedoColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            };
        }

        public Vector4 albedoColor { get { return this._data.albedoColor; } set { this._isDirty = true; this._data.albedoColor = value; } }

        public ResourceSet GetResourceSet()
        {
            if (this._isDirty) {
                this._gd.UpdateBuffer(this._infoBuffer, 0, ref this._data);
                this._isDirty = false;
            }
            return this._resourceSet;
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
            this._infoBuffer.Dispose();
        }
    }
}