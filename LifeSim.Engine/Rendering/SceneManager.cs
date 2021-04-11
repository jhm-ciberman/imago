using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class SceneManager : System.IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct CameraInfo
        {
            public Matrix4x4 viewProjectionMatrix;
            public Matrix4x4 shadowMapMatrix;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LightInfo
        {
            public ColorF ambientColor;
            public ColorF mainLightColor;
            public Vector3 mainLightDirection;
            private readonly float _padding0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ObjectInfo
        {
            public Matrix4x4 modelMatrix;
            public ColorF albedoColor;
            public System.UInt32 pickingID;
            private readonly float _padding0;
            private readonly float _padding1;
            private readonly float _padding2;
            public Vector4 textureST;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ShadowMapInfo
        {
            public Matrix4x4 shadowMatrix;
        }

        public readonly DeviceBuffer lightInfoBuffer;
        public readonly DeviceBuffer camera2DInfoBuffer;
        public readonly DeviceBuffer camera3DInfoBuffer;
        public readonly DeviceBuffer shadowmapInfoBuffer;

        public readonly DeviceBuffer modelInfoBuffer;
        public readonly DeviceBuffer bonesInfoBuffer;

        private readonly Veldrid.ResourceFactory _factory;
        private readonly Dictionary<RenderNode3D, ResourceSet> _resourceSets = new Dictionary<RenderNode3D, ResourceSet>();

        private readonly ResourceSet _objectResourceSet;
        private readonly ResourceSet _skinnedResourceSet;

        public readonly  Framebuffer shadowmapFramebuffer;
        public readonly  Veldrid.Texture shadowmapTexture;

        private BonesInfo _bonesInfo = BonesInfo.New();

        private Matrix4x4 _shadowMapScaling;

        public SceneManager(ShaderLayouts layouts, GraphicsDevice graphicsDevice)
        {
            var factory = graphicsDevice.ResourceFactory;
            this._factory = factory;  

            this.camera3DInfoBuffer   = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.camera2DInfoBuffer   = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.lightInfoBuffer      = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<LightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.shadowmapInfoBuffer  = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<ShadowMapInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        
            this.modelInfoBuffer = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<ObjectInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.bonesInfoBuffer = factory.CreateBuffer(new BufferDescription(64 * BonesInfo.MAX_NUMBER_OF_BONES, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        
            this._objectResourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(layouts.renderables.regular, this.modelInfoBuffer));
            this._skinnedResourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(layouts.renderables.skinned, this.modelInfoBuffer, this.bonesInfoBuffer));

            uint shadowMapSize = 4096;
            this.shadowmapTexture = this._factory.CreateTexture(TextureDescription.Texture2D(shadowMapSize, shadowMapSize, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
            this.shadowmapFramebuffer = this._factory.CreateFramebuffer(new FramebufferDescription(
                this.shadowmapTexture, System.Array.Empty<Veldrid.Texture>()
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

        public ResourceSet GetObjectResourceSet(RenderNode3D renderable)
        {
            if (this._resourceSets.TryGetValue(renderable, out ResourceSet? resourceSet)) {
                return resourceSet;
            }

            resourceSet = renderable is SkinRenderNode3D ? this._skinnedResourceSet : this._objectResourceSet;

            this._resourceSets[renderable] = resourceSet;
            return resourceSet;
        }

        public void SetupLightInfoBuffer(CommandList commandList, Scene3D scene)
        {
            LightInfo lightInfo = new LightInfo();
            lightInfo.ambientColor = scene.ambientColor;
            lightInfo.mainLightColor = scene.mainLight.color;
            lightInfo.mainLightDirection = Vector3.Normalize(scene.mainLight.direction);
            commandList.UpdateBuffer(this.lightInfoBuffer, 0, ref lightInfo);
        }

        public void SetupCamera3DInfoBuffer(CommandList commandList, Camera3D camera, DirectionalLight mainLight)
        {
            CameraInfo cameraInfo = new CameraInfo();
            cameraInfo.viewProjectionMatrix = camera.viewProjectionMatrix;
            cameraInfo.shadowMapMatrix = mainLight.GetShadowMapMatrix(camera.position) * this._shadowMapScaling;
            commandList.UpdateBuffer(this.camera3DInfoBuffer, 0, ref cameraInfo);
        }

        public void SetupCamera2DInfoBuffer(CommandList commandList, ref Matrix4x4 projection)
        {
            commandList.UpdateBuffer(this.camera2DInfoBuffer, 0, ref projection);
        }

        public void SetupShadowMapBuffer(CommandList commandList, Camera3D camera, DirectionalLight mainLight)
        {
            ShadowMapInfo shadowMapInfo = new ShadowMapInfo();
            shadowMapInfo.shadowMatrix = mainLight.GetShadowMapMatrix(camera.frustumCullingCamera.position);
            commandList.UpdateBuffer(this.shadowmapInfoBuffer, 0, ref shadowMapInfo);
        }

        public void SetupObjectInfoBuffer(CommandList commandList, RenderNode3D renderable)
        {
            ObjectInfo objectInfo = new ObjectInfo();
            objectInfo.modelMatrix = renderable.worldMatrix;
            objectInfo.albedoColor = renderable.albedoColor;
            objectInfo.pickingID = renderable.pickingID;
            objectInfo.textureST = renderable.textureST;
            commandList.UpdateBuffer(this.modelInfoBuffer, 0, ref objectInfo);
        }

        public void SetupBonesInfoBuffer(CommandList commandList, SkinRenderNode3D skinnedRenderable)
        {
            skinnedRenderable.CopyMatricesToBuffer(ref this._bonesInfo);
            commandList.UpdateBuffer(this.bonesInfoBuffer, 0, this._bonesInfo.GetBlittable());
        }
    }
}