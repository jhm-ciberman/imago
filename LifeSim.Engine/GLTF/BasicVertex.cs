using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Rendering;
using Veldrid;

namespace LifeSim.Engine.GLTF
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BasicVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public BasicVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
        }

        public BasicVertex(Vector3 position, Vector2 uv)
        {
            this.position = position;
            this.normal = Vector3.Zero;
            this.uv = uv;
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
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                ));
                _vertexFormat.isSurface = true;
                return _vertexFormat;
            }
        }
    }
}