using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class Shader
    {
        public readonly ShaderSetDescription description;

        public Shader(ShaderSetDescription description)
        {
            this.description = description;
        }

        public void Dispose()
        {
            foreach (var shader in this.description.Shaders) {
                shader.Dispose();
            }
        }
    }
}