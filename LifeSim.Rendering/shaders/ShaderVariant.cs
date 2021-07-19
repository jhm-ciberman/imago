using System;
using System.Collections.Generic;
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
            var macros = this.GetMacroDefinitions(vertexFormat);
            var options = new GlslCompileOptions(debug: true, macros);
            var vertGlslShader = this._CompileGlslToSpirv(source.vertCode, source.vertFilename, ShaderStages.Vertex, options);
            var fragGlslShader = this._CompileGlslToSpirv(source.fragCode, source.fragFilename, ShaderStages.Fragment, options);

            this.vertexFormat = vertexFormat;

            this.materialResourceLayout = materialResourceLayout;
            var layout = this.GetVertexLayout(vertexFormat);
            this.shaderSetDescription = new ShaderSetDescription(layout, factory.CreateFromSpirv(vertGlslShader, fragGlslShader));
        }

        private MacroDefinition[] GetMacroDefinitions(VertexFormat vertexFormat)
        {
            var macros = new List<MacroDefinition>();
            //if (vertexFormat.isSkinned)
            //    macros.Add(new MacroDefinition("USE_SKINNED_MESH"));

            foreach (var element in vertexFormat.layout.Elements)
                macros.Add(new MacroDefinition("USE_" + element.Name.ToUpperInvariant()));

            return macros.ToArray();
        }

        private VertexLayoutDescription[] GetVertexLayout(VertexFormat vertexFormat)
        {
            if (! vertexFormat.isSurface) 
                return new VertexLayoutDescription[] { vertexFormat.layout };

            var layouts = new List<VertexLayoutDescription>();
            layouts.Add(new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
            ));
            layouts.Add(vertexFormat.layout);
            return layouts.ToArray();
        }

        private Veldrid.ShaderDescription _CompileGlslToSpirv(string sourceText, string fileName, ShaderStages stage, GlslCompileOptions options)
        {
            try {
                var result = SpirvCompilation.CompileGlslToSpirv(sourceText, fileName, stage, options);
                return new Veldrid.ShaderDescription(stage, result.SpirvBytes, "main");
            } catch (SpirvCompilationException e) {
                Console.WriteLine(sourceText);
                throw e;
            }
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