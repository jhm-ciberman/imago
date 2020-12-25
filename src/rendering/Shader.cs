using System.Text;
using Veldrid;


namespace LifeSim.Rendering
{
    public class Shader : System.IDisposable
    {
        private readonly Veldrid.Shader[] _shaders;

        private bool _isSkinned;

        public Shader(Veldrid.Shader[] shaders, bool isSkinned = false)
        {
            this._shaders = shaders;
            this._isSkinned = isSkinned;
        }

        public ResourceLayoutDescription[] BuildResourceLayouts()
        {
            return new[] {
                new ResourceLayoutDescription(new[] {
                    new ResourceLayoutElementDescription("WorldInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("BonesInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                }),
            };
        }

        public ShaderSetDescription BuildShaderSetDescription()
        {
            int size = this._isSkinned ? 5 : 3;

            var arr = new VertexElementDescription[size];
            
            arr[0] = new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3);
            arr[1] = new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3);
            arr[2] = new VertexElementDescription("TextCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2);

            if (this._isSkinned) {
                arr[3] = new VertexElementDescription("Joints", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UShort4);
                arr[4] = new VertexElementDescription("Weights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4);
            }

            return new ShaderSetDescription(new [] { new VertexLayoutDescription(arr) }, this._shaders);
        }

        public void Dispose()
        {
            foreach (var shader in this._shaders) {
                shader.Dispose();
            }
        }

        ~Shader() {
            this.Dispose();
        }
    }
}