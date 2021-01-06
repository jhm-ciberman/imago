using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.SceneGraph;
using Veldrid;

namespace LifeSim.Rendering
{
    public class SceneContext : System.IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        struct CameraInfo
        {
            public Matrix4x4 viewProjectionMatrix;
            public Matrix4x4 shadowMapMatrix;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LightInfo
        {
            public Vector3 ambientColor;
            private float _padding0;
            public Vector3 mainLightColor;
            private float _padding1;
            public Vector3 mainLightPosition;
            private float _padding2;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ObjectInfo
        {
            public Matrix4x4 modelMatrix;
            public Vector4 albedoColor;
            public System.UInt32 pickingID;
            private float _padding0;
            private float _padding1;
            private float _padding2;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ShadowMapInfo
        {
            public Matrix4x4 shadowMatrix;
        }

        public readonly DeviceBuffer lightInfoBuffer;
        public readonly DeviceBuffer camera2DInfoBuffer;
        public readonly DeviceBuffer camera3DInfoBuffer;
        public readonly DeviceBuffer shadowmapInfoBuffer;

        public readonly DeviceBuffer modelInfoBuffer;
        public readonly DeviceBuffer bonesInfoBuffer;

        private ResourceFactory _factory;
        private Dictionary<Renderable3D, ResourceSet> _resourceSets = new Dictionary<Renderable3D, ResourceSet>();

        private readonly ResourceSet _objectResourceSet;
        private readonly ResourceSet _skinnedResourceSet;

        public readonly  Framebuffer shadowmapFramebuffer;
        public readonly  Texture shadowmapTexture;

        private BonesInfo _bonesInfo = BonesInfo.New();

        private Matrix4x4 _shadowMapScaling;

        public SceneContext(ResourceLayouts layouts, GraphicsDevice graphicsDevice)
        {
            var factory = graphicsDevice.ResourceFactory;
            this._factory = factory;  

            this.camera3DInfoBuffer   = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.camera2DInfoBuffer   = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.lightInfoBuffer      = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<LightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.shadowmapInfoBuffer  = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<ShadowMapInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        
            this.modelInfoBuffer = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<ObjectInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.bonesInfoBuffer = factory.CreateBuffer(new BufferDescription(64 * BonesInfo.maxNumberOfBones, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        
            this._objectResourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(layouts.renderables.regular, this.modelInfoBuffer));
            this._skinnedResourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(layouts.renderables.skinned, this.modelInfoBuffer, this.bonesInfoBuffer));

            uint shadowMapSize = 2048;
            this.shadowmapTexture = this._factory.CreateTexture(TextureDescription.Texture2D(shadowMapSize, shadowMapSize, 1, 1, PixelFormat.D32_Float_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled));
            this.shadowmapFramebuffer = this._factory.CreateFramebuffer(new FramebufferDescription(
                this.shadowmapTexture, System.Array.Empty<Texture>()
            ));

            if (graphicsDevice.IsUvOriginTopLeft) {
                this._shadowMapScaling = Matrix4x4.CreateScale(.5f, -.5f, 1f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f);
            } else {
                this._shadowMapScaling = Matrix4x4.CreateScale(.5f, .5f, 1f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f);
            }
        }

        public void Dispose()
        {
            this.camera2DInfoBuffer.Dispose();
            this.camera3DInfoBuffer.Dispose();
            this.lightInfoBuffer.Dispose();
            this.modelInfoBuffer.Dispose();
            this.bonesInfoBuffer.Dispose();
        }

        public ResourceSet GetObjectResourceSet(Renderable3D renderable)
        {
            if (this._resourceSets.TryGetValue(renderable, out ResourceSet? resourceSet)) {
                return resourceSet;
            }
            if (renderable is SkinnedRenderable3D skinnedRenderable) {
                resourceSet = this._skinnedResourceSet;
            } else {
                resourceSet = this._objectResourceSet;
            }

            this._resourceSets[renderable] = resourceSet;
            return resourceSet;
        }

        public void SetupLightInfoBuffer(CommandList commandList, Scene3D scene)
        {
            LightInfo lightInfo = new LightInfo();
            lightInfo.ambientColor = scene.ambientColor;
            lightInfo.mainLightColor = scene.mainLight.color;
            lightInfo.mainLightPosition = scene.mainLight.position;
            commandList.UpdateBuffer(this.lightInfoBuffer, 0, ref lightInfo);
        }

        public void SetupCamera3DInfoBuffer(CommandList commandList, Camera3D camera, DirectionalLight mainLight)
        {
            CameraInfo cameraInfo = new CameraInfo();
            cameraInfo.viewProjectionMatrix = camera.viewProjectionMatrix;
            cameraInfo.shadowMapMatrix = mainLight.shadowMapMatrix * this._shadowMapScaling;
            commandList.UpdateBuffer(this.camera3DInfoBuffer, 0, ref cameraInfo);
        }

        public void SetupCamera2DInfoBuffer(CommandList commandList, ref Matrix4x4 projection)
        {
            commandList.UpdateBuffer(this.camera2DInfoBuffer, 0, ref projection);
        }

        public void SetupShadowMapBuffer(CommandList commandList, DirectionalLight light)
        {
            ShadowMapInfo shadowMapInfo = new ShadowMapInfo();
            shadowMapInfo.shadowMatrix = light.shadowMapMatrix;
            commandList.UpdateBuffer(this.shadowmapInfoBuffer, 0, ref shadowMapInfo);
        }

        public void SetupObjectInfoBuffer(CommandList commandList, Renderable3D renderable)
        {
            ObjectInfo objectInfo = new ObjectInfo();
            objectInfo.modelMatrix = renderable.worldMatrix;
            objectInfo.albedoColor = renderable.albedoColor;
            objectInfo.pickingID = renderable.pickingID;
            commandList.UpdateBuffer(this.modelInfoBuffer, 0, ref objectInfo);
        }

        public void SetupBonesInfoBuffer(CommandList commandList, SkinnedRenderable3D skinnedRenderable)
        {
            skinnedRenderable.CopyMatricesToBuffer(ref this._bonesInfo);
            commandList.UpdateBuffer(this.bonesInfoBuffer, 0, this._bonesInfo.GetBlittable());
        }
    }
}