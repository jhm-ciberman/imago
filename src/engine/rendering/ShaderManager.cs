using System.Collections.Generic;
using System.IO;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering
{
    public class ShaderManager
    {
        public struct Shader
        {
            public Veldrid.Shader[] shaders;
            public Shader(Veldrid.Shader[] shaders) { this.shaders = shaders; }
        }

        private string _shadersBasePath = "./res/shaders/";

        private Veldrid.ResourceFactory _factory;

        private Dictionary<ShaderVariant, Shader> _shaderVariants = new Dictionary<ShaderVariant, Shader>();

        public ShaderManager(Veldrid.ResourceFactory factory)
        {
            this._factory = factory;
        }   
    
        public Shader GetShader(ShaderVariant shaderVariant)
        {
            if (! this._shaderVariants.TryGetValue(shaderVariant, out Shader shaders)) {
                shaders = this._MakeShader(shaderVariant);
                lock (this._shaderVariants) {
                    this._shaderVariants.Add(shaderVariant, shaders);
                }
                return shaders;
            }
            return shaders;
        }

        private Shader _MakeShader(ShaderVariant shaderVariant)
        {
            StringBuilder vertex = new StringBuilder();
            StringBuilder fragment = new StringBuilder();

            var filename = Path.Combine(this._shadersBasePath, shaderVariant.shaderName + ".glsl");
            var lines = File.ReadAllLines(filename);
            StringBuilder? current = null;
            foreach (var line in lines) {
                if (line.Contains("#shader")) {
                    if (line.Contains("vertex")) {
                        current = vertex;
                    } else if (line.Contains("fragment")) {
                        current = fragment;
                    } else {
                        throw new System.Exception("Unrecognized type of shader: " + line);
                    }
                } else {
                    current?.AppendLine(line);
                }
            }
            
            StringBuilder macros = new StringBuilder();
            macros.AppendLine("#version 450");
            if (shaderVariant.keywords != null) {
                foreach (var keyword in shaderVariant.keywords) {
                    macros.AppendJoin(" ", "#define", keyword).AppendLine();
                }
            }
            var macrosStr = macros.ToString();

            var vertBytes = Encoding.UTF8.GetBytes(macrosStr + vertex.ToString());
            var fragBytes = Encoding.UTF8.GetBytes(macrosStr + fragment.ToString());
            return new Shader(this._factory.CreateFromSpirv(
                new Veldrid.ShaderDescription(ShaderStages.Vertex  , vertBytes, "main"),
                new Veldrid.ShaderDescription(ShaderStages.Fragment, fragBytes, "main")
            ));
        }
    }
}