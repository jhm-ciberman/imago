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
        struct ModelInfo
        {
            public Matrix4x4 modelMatrix;
        }

        public readonly DeviceBuffer lightInfoBuffer;
        public readonly DeviceBuffer cameraInfoBuffer;

        public readonly DeviceBuffer modelInfoBuffer;
        public readonly DeviceBuffer bonesInfoBuffer;

        private ResourceFactory _factory;
        private Dictionary<Renderable3D, ResourceSet> _resourceSets = new Dictionary<Renderable3D, ResourceSet>();

        public readonly ResourceLayout objectLayout;
        public readonly ResourceLayout skinedObjectLayout;

        public SceneContext(ResourceFactory factory)
        {
            this._factory = factory;  

            this.objectLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WorldInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            this.skinedObjectLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("WorldInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("BonesInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            this.cameraInfoBuffer = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.lightInfoBuffer  = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<LightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        
            this.modelInfoBuffer = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<ModelInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.bonesInfoBuffer = factory.CreateBuffer(new BufferDescription(64 * BonesInfo.maxNumberOfBones, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        }

        public void Dispose()
        {
            this.cameraInfoBuffer.Dispose();
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
                resourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(
                    this.skinedObjectLayout, this.modelInfoBuffer, this.bonesInfoBuffer
                ));
            } else {
                resourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(
                    this.objectLayout, this.modelInfoBuffer
                ));
            }
            System.Console.WriteLine("create: " + renderable.name);
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

        public void SetupCameraInfoBuffer(CommandList commandList, Camera3D camera)
        {
            CameraInfo cameraInfo;
            cameraInfo.projectionMatrix = camera.projectionMatrix;
            cameraInfo.viewMatrix = camera.viewMatrix;
            commandList.UpdateBuffer(this.cameraInfoBuffer, 0, ref cameraInfo);
        }
    }
}