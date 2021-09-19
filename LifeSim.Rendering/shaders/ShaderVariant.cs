using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class ShaderVariant : IDisposable
    {
        public VertexFormat VertexFormat;

        public ShaderSetDescription ShaderSetDescription;

        public ResourceLayout MaterialResourceLayout;

        public ShaderVariant(Veldrid.ResourceFactory factory, VertexFormat vertexFormat, ResourceLayout materialResourceLayout, ShaderSource source)
        {
            var macros = this.GetMacroDefinitions(vertexFormat);
            var options = new GlslCompileOptions(debug: true, macros);
            var vertGlslShader = this._CompileGlslToSpirv(source.VertCode, source.VertFilename, ShaderStages.Vertex, options);
            var fragGlslShader = this._CompileGlslToSpirv(source.FragCode, source.FragFilename, ShaderStages.Fragment, options);

            this.VertexFormat = vertexFormat;

            this.MaterialResourceLayout = materialResourceLayout;
            var layout = this.GetVertexLayout(vertexFormat);
            this.ShaderSetDescription = new ShaderSetDescription(layout, factory.CreateFromSpirv(vertGlslShader, fragGlslShader));
        }

        private MacroDefinition[] GetMacroDefinitions(VertexFormat vertexFormat)
        {
            var macros = new List<MacroDefinition>();
            //if (vertexFormat.isSkinned)
            //    macros.Add(new MacroDefinition("USE_SKINNED_MESH"));

            foreach (var element in vertexFormat.Layout.Elements)
                macros.Add(new MacroDefinition("USE_" + element.Name.ToUpperInvariant()));

            return macros.ToArray();
        }

        private VertexLayoutDescription[] GetVertexLayout(VertexFormat vertexFormat)
        {
            if (! vertexFormat.IsSurface) 
                return new VertexLayoutDescription[] { vertexFormat.Layout };

            return new VertexLayoutDescription[] {
                new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                    new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
                ),
                vertexFormat.Layout
            };
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
            var nativeShaders = this.ShaderSetDescription.Shaders;
            for (int i = 0; i < nativeShaders.Length; i++) {
                nativeShaders[i].Dispose();
            }
        }
    }
}