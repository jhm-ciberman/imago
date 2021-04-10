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

        private readonly string _shadersBasePath = "./res/shaders/";

        private readonly Veldrid.ResourceFactory _factory;

        private readonly Dictionary<ShaderVariant, Shader> _shaderVariants = new Dictionary<ShaderVariant, Shader>();

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
            StringBuilder vert = new StringBuilder();
            StringBuilder frag = new StringBuilder();

            var filenameVert = Path.Combine(this._shadersBasePath, shaderVariant.shaderName + ".vert.glsl");
            var filenameFrag = Path.Combine(this._shadersBasePath, shaderVariant.shaderName + ".frag.glsl");
            var textVert = File.ReadAllText(filenameVert);
            var textFrag = File.ReadAllText(filenameFrag);
            
            var macros = new MacroDefinition[shaderVariant.keywords.Length];
            for (int i = 0; i < shaderVariant.keywords.Length; i++) {
                macros[i++].Name = shaderVariant.keywords[i];
            }

            var options = new GlslCompileOptions(true, macros);
            var vertResult = SpirvCompilation.CompileGlslToSpirv(textVert.ToString(), filenameVert, ShaderStages.Vertex, options);
            var fragResult = SpirvCompilation.CompileGlslToSpirv(textFrag.ToString(), filenameFrag, ShaderStages.Fragment, options);

            return new Shader(this._factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vertResult.SpirvBytes, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragResult.SpirvBytes, "main")
            ));
        }
    }
}