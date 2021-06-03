using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    internal class FullScreenQuad : System.IDisposable
    {
        public DeviceBuffer vertexBuffer;

        public FullScreenQuad(GraphicsDevice gd)
        {
            var factory = gd.ResourceFactory;
            this.vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 6, BufferUsage.VertexBuffer));
            (float top, float bottom) = gd.IsUvOriginTopLeft ? (1f, 0f) : (0f, 1f);
            gd.UpdateBuffer(this.vertexBuffer, 0, new[] {
                new Vector4(-1f, -1f, 0f, top   ), // x, y, u, v
                new Vector4( 1f, -1f, 1f, top   ),
                new Vector4( 1f,  1f, 1f, bottom),

                new Vector4(-1f, -1f, 0f, top   ),
                new Vector4( 1f,  1f, 1f, bottom),
                new Vector4(-1f,  1f, 0f, bottom),
            });
        }
        
        public void Dispose()
        {
            this.vertexBuffer.Dispose();
        }
    }
}