using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class SkinnedMeshData : MeshData
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SkinnedVertData
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
            public UShort4 joints;
            public Vector4 weights;

            public SkinnedVertData(Vector3 position, Vector3 normal, Vector2 uv, UShort4 joints, Vector4 weights)
            {
                this.position = position;
                this.normal = normal;
                this.uv = uv;
                this.joints = joints;
                this.weights = weights;
            }
        }

        public readonly IList<UShort4> joints;
        public readonly IList<Vector4> weights;

        public SkinnedMeshData(
            IList<Vector3> positions, IList<ushort> indices, IList<Vector2>? uvs, IList<Vector3>? normals, 
            IList<UShort4> joints, IList<Vector4> weights
        ) 
            : base(positions, indices, uvs, normals)
        {
            this.joints = joints;
            this.weights = weights;
        }

        public override Mesh CreateMesh()
        {
            var boundingBox = BoundingBox.CreateFromVertices(this.positions.ToArray());

            SkinnedVertData[] vertices = ArrayPool<SkinnedVertData>.Shared.Rent(this.positions.Count);
            for(var i = 0; i < this.positions.Count; i++) {
                vertices[i] = new SkinnedVertData(this.positions[i], this.normals[i], this.uvs[i], this.joints[i], this.weights[i]);
            }
            var mesh = Mesh.Create(GetVertexFormat(), vertices, this.indices.ToArray(), ref boundingBox);

            ArrayPool<SkinnedVertData>.Shared.Return(vertices);

            return mesh;
        }

        private static VertexFormat? vertexFormat;
        public static new VertexFormat GetVertexFormat()
        {
            if (vertexFormat == null) {
                vertexFormat = VertexFormat.CreateSurfaceVertexFormat(true, new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Joints", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UShort4),
                    new VertexElementDescription("Weights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                ), new MacroDefinition[] { new MacroDefinition("USE_SKINNED_MESH") });
            }
            return vertexFormat;
        }
    }
}