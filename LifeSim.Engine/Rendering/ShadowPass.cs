using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public partial class ShadowPass : IDisposable, IPipelineProvider
    {
        public ShadowMapTexture ShadowmapTexture { get; private set; }

        private readonly ResourceLayout _resourceLayout;
        private readonly ResourceSet _resourceSet;
        private readonly GraphicsDevice _gd;
        private readonly DeviceBuffer _shadowmapInfoBuffer;
        private readonly RenderJob _renderJob;
        private readonly SceneStorage _storage;

        private Matrix4x4[] _viewProjectionMatrices { get; } = new Matrix4x4[4];
        private Matrix4x4[] _scalingMatrices { get; } = new Matrix4x4[4];

        public ShadowMapConfig Config { get; }

        private bool _shadowMapTextureSizeDirty = false;

        private readonly float[] _splitDistances = new float[5]; // 4 splits + 1 for far plane

        private readonly RenderQueue[] _renderQueues;

        public ShadowPass(GraphicsDevice gd, SceneStorage storage)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this.Config = new ShadowMapConfig();
            this._storage = storage;

            this._renderQueues = new RenderQueue[4];
            for (int i = 0; i < this._renderQueues.Length; i++)
            {
                this._renderQueues[i] = new RenderQueue();
            }

            this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ShadowMapDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            uint width = this.Config.ShadowMapSize * this.Config.CascadesCount;
            uint height = this.Config.ShadowMapSize;
            this.ShadowmapTexture = new ShadowMapTexture(gd, width, height);

            this._shadowmapInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._shadowmapInfoBuffer));

            this._renderJob = new RenderJob(this._gd, this._resourceSet, true);

            this.Config.OnCascadeCountChanged += this.OnCascadeCountChanged;
            this.Config.OnShadowMapSizeChanged += this.OnShadowMapSizeChanged;

            float verticalFlip = this._gd.IsUvOriginTopLeft ? -1.0f : 1.0f;
            uint cascadeCount = this.Config.CascadesCount == 3 ? 4 : this.Config.CascadesCount;
            float horizontalScale = 1f / cascadeCount;
            for (int i = 0; i < this._viewProjectionMatrices.Length; i++)
            {
                this._viewProjectionMatrices[i] = Matrix4x4.Identity;
                this._scalingMatrices[i] = Matrix4x4.CreateScale(.5f, .5f * verticalFlip, 1f)
                                        * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f);
            }
        }

        private void OnShadowMapSizeChanged(uint size)
        {
            this._shadowMapTextureSizeDirty = true;
        }

        private void OnCascadeCountChanged(uint cascadesCount)
        {
            this._shadowMapTextureSizeDirty = true;
        }

        public void Render(CommandList commandList, IReadOnlyList<Renderable> renderables, Camera3D camera, Vector3 mainLightDirection)
        {
            this.UpdateSplitDistances(camera);

            this.UpdateShadowMapTextureSize();

            commandList.SetFramebuffer(this.ShadowmapTexture.Framebuffer);
            commandList.ClearDepthStencil(1f);

            for (int i = 0; i < this.Config.CascadesCount; i++)
            {
                this.UpdateCascadeMatrix(camera, mainLightDirection, i);

                BoundingFrustum shadowFrustum = new BoundingFrustum(this._viewProjectionMatrices[i]);
                this._renderQueues[i].AddToRenderQueue(renderables, shadowFrustum, camera.Position);

                commandList.UpdateBuffer(this._shadowmapInfoBuffer, 0, this._viewProjectionMatrices[i]);
                this._renderJob.DrawRenderList(commandList, this._renderQueues[i]);
            }
        }

        public Matrix4x4 GetShadowCascadeViewProjectionMatrix(int cascadeIndex)
        {
            return this._viewProjectionMatrices[cascadeIndex] * this._scalingMatrices[cascadeIndex];
        }

        private void UpdateShadowMapTextureSize()
        {
            if (!this._shadowMapTextureSizeDirty) return;

            uint count = this.Config.CascadesCount == 3 ? 4 : this.Config.CascadesCount;
            uint width = this.Config.ShadowMapSize * count;
            uint height = this.Config.ShadowMapSize;
            this.ShadowmapTexture.Resize(width, height);
            this._shadowMapTextureSizeDirty = false;
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
            this._resourceLayout.Dispose();
            this.ShadowmapTexture.Dispose();
            this._shadowmapInfoBuffer.Dispose();
        }

        Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            );

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
            {
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = shaderVariant.ShaderSetDescription,
                BlendState = BlendStateDescription.Empty,
                RasterizerState = rasterizerState,
                Outputs = this.ShadowmapTexture.OutputDescription,
                ResourceLayouts = this._GetResourceLayouts(shaderVariant),
            });
        }

        private ResourceLayout[] _GetResourceLayouts(ShaderVariant shaderVariant)
        {
            Debug.Assert(shaderVariant.MaterialResourceLayout != null);

            var resources = new List<ResourceLayout> {
                this._resourceLayout,
                this._storage.TransformResourceLayout,
                shaderVariant.MaterialResourceLayout,
                this._storage.InstanceResourceLayout
            };

            if (shaderVariant.VertexFormat.IsSkinned)
            {
                resources.Add(this._storage.SkeletonResourceLayout);
            }

            return resources.ToArray();
        }

        public void UpdateSplitDistances(Camera3D camera)
        {
            // Lerp between uniform and logarithmic split distances.
            // https://developer.nvidia.com/gpugems/gpugems3/part-ii-light-and-shadows/chapter-10-parallel-split-shadow-maps-programmable-gpus

            float near = camera.NearPlane;
            float far = MathF.Min(camera.FarPlane, this.Config.MaximumShadowsDistance);

            uint count = this.Config.CascadesCount;

            this._splitDistances[0] = near;
            this._splitDistances[count] = far;

            for (int i = 1; i < count; i++)
            {
                float uniformDistance = near + ((far - near) * i / count);
                float logarithmicDistance = MathF.Pow(near * (far / near), i / count);
                this._splitDistances[i] = MathUtils.Lerp(uniformDistance, logarithmicDistance, this.Config.SplitLambda);
            }
        }

        public void UpdateCascadeMatrix(Camera3D camera, Vector3 lightDirection, int index)
        {
            float near = this._splitDistances[index];
            float far = this._splitDistances[index + 1];
            Matrix4x4 cameraProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(camera.FieldOfView, camera.AspectRatio, near, far);
            Matrix4x4 cameraViewProjectionMatrix = camera.ViewMatrix * cameraProjectionMatrix;

            BoundingFrustum mainCameraFrustum = new BoundingFrustum(cameraViewProjectionMatrix);

            FrustumCorners corners = mainCameraFrustum.GetCorners();

            float sphereDiameter = MathF.Max(
                Vector3.Distance(corners.FarBottomLeft, corners.FarTopRight),
                Vector3.Distance(corners.NearBottomLeft, corners.FarTopRight)
            );

            sphereDiameter = MathF.Round(sphereDiameter * 16) / 16;

            Matrix4x4 lightViewMatrix = Matrix4x4.CreateLookAt(lightDirection, Vector3.Zero, Vector3.UnitY);
            Matrix4x4.Invert(lightViewMatrix, out Matrix4x4 lightViewMatrixInverse);

            Span<Vector3> frustumCornersWS = stackalloc Vector3[8];
            frustumCornersWS[0] = corners.FarBottomLeft;
            frustumCornersWS[1] = corners.FarBottomRight;
            frustumCornersWS[2] = corners.FarTopLeft;
            frustumCornersWS[3] = corners.FarTopRight;
            frustumCornersWS[4] = corners.NearBottomLeft;
            frustumCornersWS[5] = corners.NearBottomRight;
            frustumCornersWS[6] = corners.NearTopLeft;
            frustumCornersWS[7] = corners.NearTopRight;

            Vector3 minLS = new Vector3(float.MaxValue);
            Vector3 maxLS = new Vector3(float.MinValue);
            for (int i = 0; i < 8; i++)
            {
                Vector3 frustumCornerLS = Vector3.Transform(frustumCornersWS[i], lightViewMatrix);
                minLS = Vector3.Min(minLS, frustumCornerLS);
                maxLS = Vector3.Max(maxLS, frustumCornerLS);
            }

            float f = sphereDiameter / this.Config.ShadowMapSize;

            Vector3 frustumCenterWS = (corners.FarBottomLeft + corners.FarTopRight + corners.FarBottomRight + corners.FarTopLeft
            + corners.NearBottomLeft + corners.NearTopRight + corners.NearBottomRight + corners.NearTopLeft) / 8f;

            Vector3 centerLS = Vector3.Transform(frustumCenterWS, lightViewMatrix);
            centerLS.X = MathF.Round(centerLS.X / f) * f;
            centerLS.Y = MathF.Round(centerLS.Y / f) * f;
            centerLS.Z = maxLS.Z;
            Vector3 centerWS = Vector3.Transform(centerLS, lightViewMatrixInverse);

            //GizmosLayer.Default.DrawWireSphere(frustumCenterWS, sphereDiameter / 2f, LifeSim.Color.Red);
            //GizmosLayer.Default.DrawWireSphere(frustumCenterWS, sphereDiameter / 10f, LifeSim.Color.Cyan);

            lightViewMatrix = Matrix4x4.CreateLookAt(centerWS, centerWS - lightDirection, Vector3.UnitY);

            Matrix4x4 lightProjectionMatrix = Matrix4x4.CreateOrthographic(sphereDiameter, sphereDiameter, 0, maxLS.Z - minLS.Z);

            this._viewProjectionMatrices[index] = lightViewMatrix * lightProjectionMatrix;
        }
    }
}