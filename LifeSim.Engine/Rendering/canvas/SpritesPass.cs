using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class SpritesPass : IDisposable, PipelineCache.IPipelineFactory
    {
        private readonly DeviceBuffer _camera2DInfoBuffer;
        private readonly ResourceSet _passResourceSet;
        private readonly IRenderTexture _renderTexture;
        private readonly ResourceLayout _resourceLayout;
        private readonly GraphicsDevice _gd;

        private Shader? _currentShader;
        private Dictionary<(Shader, Texture), ResourceSet> _resourceSets = new Dictionary<(Shader, Texture), ResourceSet>();
        
        public SpritesPass(GraphicsDevice gd, IRenderTexture renderTexture)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;

            this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            this._camera2DInfoBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._camera2DInfoBuffer));

            this._renderTexture = renderTexture;
        }

        private Veldrid.ResourceSet _GetResourceSetOrNew(Shader shader, Texture texture)
        {
            if (! this._resourceSets.TryGetValue((shader, texture), out Veldrid.ResourceSet? resourceSet)) {
                resourceSet = shader.CreateResourceSet(texture.deviceTexture);
                this._resourceSets.Add((shader, texture), resourceSet);
            }
            return resourceSet;
        }

        public void Dispose()
        {
            this._camera2DInfoBuffer.Dispose();
            this._resourceLayout.Dispose();
            this._passResourceSet.Dispose();

            foreach (var set in this._resourceSets.Values) {
                set.Dispose();
            }
        }

        public void BeginPass(CommandList commandList, ref Matrix4x4 projectionMatrix)
        {
            commandList.SetFramebuffer(this._renderTexture.framebuffer);
            commandList.ClearDepthStencil(1f);
            commandList.UpdateBuffer(this._camera2DInfoBuffer, 0, ref projectionMatrix);

            this._currentShader = null;
        }

        public void SubmitBatches(CommandList commandList, Veldrid.DeviceBuffer sharedIndexBuffer, IList<SpriteBatch> batches)
        {
            for (int i = 0; i < batches.Count; i++) {
                var batch = batches[i];

                commandList.UpdateBuffer(batch.vertexBuffer, 0, batch.vertices);

                if (this._currentShader != batch.shader) {
                    this._currentShader = batch.shader;
                    var pipeline = batch.shader.GetPipeline(VertexFormat.Sprite);
            
                    commandList.SetPipeline(pipeline);
                    commandList.SetGraphicsResourceSet(0, this._passResourceSet);
                }

                commandList.SetVertexBuffer(0, batch.vertexBuffer);
                commandList.SetIndexBuffer(sharedIndexBuffer, IndexFormat.UInt16);

                commandList.SetGraphicsResourceSet(1, batch.resourceSet);
                commandList.DrawIndexed(
                    indexCount: (uint) batch.count * 6,
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0
                );
            }
        }

        Pipeline PipelineCache.IPipelineFactory.MakePipeline(ShaderVariant shaderVariant)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            );

            var resources = new Veldrid.ResourceLayout[] {
                this._resourceLayout,
                shaderVariant.materialResourceLayout,
            };

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription() {
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = shaderVariant.shaderSetDescription,
                BlendState = BlendStateDescription.SingleAlphaBlend,
                RasterizerState = rasterizerState,
                Outputs = this._renderTexture.outputDescription,
                ResourceLayouts = resources,
            });
        }
    }
}