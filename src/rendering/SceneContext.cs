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
            public Matrix4x4 viewMatrix;
            public Matrix4x4 projectionMatrix;
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
            public System.UInt32 pickingID;
            private float _padding0;
            private float _padding1;
            private float _padding2;
        }

        public readonly DeviceBuffer lightInfoBuffer;
        public readonly DeviceBuffer camera2DInfoBuffer;
        public readonly DeviceBuffer camera3DInfoBuffer;

        public readonly DeviceBuffer modelInfoBuffer;
        public readonly DeviceBuffer bonesInfoBuffer;

        private ResourceFactory _factory;
        private Dictionary<Renderable3D, ResourceSet> _resourceSets = new Dictionary<Renderable3D, ResourceSet>();

        public readonly ResourceLayout objectLayout;
        public readonly ResourceLayout skinedObjectLayout;

        private readonly ResourceSet objectResourceSet;
        private readonly ResourceSet skinnedResourceSet;

        private BonesInfo _bonesInfo = BonesInfo.New();

        public SceneContext(ResourceFactory factory)
        {
            this._factory = factory;  

            this.objectLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ObjectInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            this.skinedObjectLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ObjectInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("BonesInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            this.camera3DInfoBuffer = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.camera2DInfoBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.lightInfoBuffer  = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<LightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        
            this.modelInfoBuffer = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<ObjectInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.bonesInfoBuffer = factory.CreateBuffer(new BufferDescription(64 * BonesInfo.maxNumberOfBones, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        
            this.objectResourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(this.objectLayout, this.modelInfoBuffer));
            this.skinnedResourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(this.skinedObjectLayout, this.modelInfoBuffer, this.bonesInfoBuffer));
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
                resourceSet = this.skinnedResourceSet;
            } else {
                resourceSet = this.objectResourceSet;
            }

            this._resourceSets[renderable] = resourceSet;
            return resourceSet;
        }

        public void SetupLightInfoBuffer(CommandList commandList, Scene3D scene)
        {
            LightInfo lightInfo = new LightInfo();
            lightInfo.ambientColor = scene.ambientColor;
            lightInfo.mainLightColor = scene.sunColor;
            lightInfo.mainLightPosition = scene.sunPosition;
            commandList.UpdateBuffer(this.lightInfoBuffer, 0, ref lightInfo);
        }

        public void SetupCamera3DInfoBuffer(CommandList commandList, Camera3D camera)
        {
            CameraInfo cameraInfo;
            cameraInfo.projectionMatrix = camera.projectionMatrix;
            cameraInfo.viewMatrix = camera.viewMatrix;
            commandList.UpdateBuffer(this.camera3DInfoBuffer, 0, ref cameraInfo);
        }

        public void SetupCamera2DInfoBuffer(CommandList commandList, ref Matrix4x4 projection)
        {
            commandList.UpdateBuffer(this.camera2DInfoBuffer, 0, ref projection);
        }

        public void SetupObjectInfoBuffer(CommandList commandList, Renderable3D renderable)
        {
            commandList.UpdateBuffer(this.modelInfoBuffer, 0, new ObjectInfo {
                modelMatrix = renderable.worldMatrix,
                pickingID = renderable.pickingID,
            });
        }

        public void SetupBonesInfoBuffer(CommandList commandList, SkinnedRenderable3D skinnedRenderable)
        {
            skinnedRenderable.CopyMatricesToBuffer(ref this._bonesInfo);
            commandList.UpdateBuffer(this.bonesInfoBuffer, 0, this._bonesInfo.GetBlittable());
        }
    }
}