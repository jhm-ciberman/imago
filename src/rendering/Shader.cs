using System.Text;
using Veldrid;


namespace LifeSim.Rendering
{
    public class Shader : System.IDisposable
    {
        private readonly Veldrid.Shader[] _shaders;

        public Shader(Veldrid.Shader[] shaders)
        {
            this._shaders = shaders;
        }

        public ResourceLayoutDescription[] BuildResourceLayouts()
        {
            return new[] {
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                ),
            };
        }

        public ShaderSetDescription BuildShaderSetDescription()
        {
            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            );

            return new ShaderSetDescription(new [] { vertexLayout }, this._shaders);
        }

        public void Dispose()
        {
            System.Console.WriteLine("Dispose shader");
            foreach (var shader in this._shaders) {
                shader.Dispose();
            }
        }

        ~Shader() {
            this.Dispose();
        }
    }
}