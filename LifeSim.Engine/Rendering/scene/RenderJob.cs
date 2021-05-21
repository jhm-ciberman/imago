using System;
using System.Collections.Generic;

namespace LifeSim.Engine.Rendering
{
    public class RenderJob
    {
        const uint BINDING_PASS = 0;
        const uint BINDING_TRANSFORM = 1;
        const uint BINDING_MATERIAL = 2;
        const uint BINDING_INSTANCE = 3;

        private Veldrid.Pipeline? _currentPipeline;
        private Mesh? _currentMesh;
        private Veldrid.ResourceSet? _currentMaterialResourceSet;
        private Veldrid.ResourceSet _passResourceSet;

        private uint[] _instanceOffsets = new uint[1];
        private uint[] _transformOffsets = new uint[1];

        public RenderJob(Veldrid.ResourceSet passResourceSet)
        {
            this._passResourceSet = passResourceSet;
        }

        public void DrawRenderList(Veldrid.CommandList commandList, Veldrid.ResourceSet transformsResourceSet, IReadOnlyList<RenderItem> renderables)
        {
            this._currentPipeline = null;
            this._currentMesh = null;
            this._currentMaterialResourceSet = null;

            for (int i = 0; i < renderables.Count; i++) {
                RenderItem item = renderables[i];

                var pipeline = item.shader.GetPipeline(item.mesh.vertexFormat);

                if (this._currentPipeline != pipeline) {
                    commandList.SetPipeline(pipeline);
                    commandList.SetGraphicsResourceSet(BINDING_PASS, this._passResourceSet);
                    this._currentPipeline = pipeline;
                    this._currentMaterialResourceSet = null;
                }

                if (this._currentMaterialResourceSet != item.materialResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_MATERIAL, item.materialResourceSet);
                    this._currentMaterialResourceSet = item.materialResourceSet;
                }

                this._transformOffsets[0] = item.transformBufferOffset;
                commandList.SetGraphicsResourceSet(BINDING_TRANSFORM, transformsResourceSet, this._transformOffsets);

                this._instanceOffsets[0] = item.instanceBufferOffset;
                commandList.SetGraphicsResourceSet(BINDING_INSTANCE, item.instanceResourceSet, this._instanceOffsets);

                if (this._currentMesh != item.mesh) {
                    commandList.SetVertexBuffer(0, item.mesh.vertexBuffer, 0);
                    commandList.SetIndexBuffer(item.mesh.indexBuffer, Veldrid.IndexFormat.UInt16);
                    this._currentMesh = item.mesh;
                }

                commandList.DrawIndexed(
                    indexCount: item.mesh.indexCount,
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0
                );
            }
        }
    }
}