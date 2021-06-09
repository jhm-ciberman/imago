using System;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class ShaderVariant : IDisposable
    {
        public VertexFormat vertexFormat;

        public ShaderSetDescription shaderSetDescription;

        public ResourceLayout materialResourceLayout;

        public ShaderVariant(Veldrid.ResourceFactory factory, VertexFormat vertexFormat, ResourceLayout materialResourceLayout, ShaderSource source)
        {
            var options = this._GetCompileOptions(vertexFormat);
            var vertResult = this._CompileGlslToSpirv(source.vertCode, source.vertFilename, ShaderStages.Vertex, options);
            var fragResult = this._CompileGlslToSpirv(source.fragCode, source.fragFilename, ShaderStages.Fragment, options);

            this.vertexFormat = vertexFormat;

            this.materialResourceLayout = materialResourceLayout;

            this.shaderSetDescription = new ShaderSetDescription(vertexFormat.layout, factory.CreateFromSpirv(
                new Veldrid.ShaderDescription(ShaderStages.Vertex, vertResult.SpirvBytes, "main"),
                new Veldrid.ShaderDescription(ShaderStages.Fragment, fragResult.SpirvBytes, "main")
            ));
        }

        private SpirvCompilationResult _CompileGlslToSpirv(string sourceText, string fileName, ShaderStages stage, GlslCompileOptions options)
        {
            try {
                return SpirvCompilation.CompileGlslToSpirv(sourceText, fileName, stage, options);
            } catch (SpirvCompilationException e) {
                Console.WriteLine(sourceText);
                throw e;
            }
        }

        private GlslCompileOptions _GetCompileOptions(VertexFormat vertexFormat)
        {
            return new GlslCompileOptions(debug: true, vertexFormat.macroDefinitions);
        }

        public void Dispose()
        {
            var nativeShaders = this.shaderSetDescription.Shaders;
            for (int i = 0; i < nativeShaders.Length; i++) {
                nativeShaders[i].Dispose();
            }
        }
    }
}