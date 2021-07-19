using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Rendering;
using Veldrid;

namespace LifeSim.Engine.GLTF
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SkinnedVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
        public Vector4UShort joints;
        public Vector4 weights;

        public SkinnedVertex(Vector3 position, Vector3 normal, Vector2 uv, Vector4UShort joints, Vector4 weights)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
            this.joints = joints;
            this.weights = weights;
        }

        private static VertexFormat? _vertexFormat;

        public static VertexFormat vertexFormat
        {
            get
            {
                if (_vertexFormat != null) return _vertexFormat;
                
                _vertexFormat = new VertexFormat(new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Joints", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UShort4),
                    new VertexElementDescription("Weights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                ));
                _vertexFormat.isSkinned = true;
                _vertexFormat.isSurface = true;
                return _vertexFormat;
            }
        }
    }
}