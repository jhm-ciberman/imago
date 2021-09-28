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

        public ResourceLayout? MaterialResourceLayout;

        public ShaderVariant(ResourceFactory factory, VertexFormat vertexFormat, ResourceLayout? materialResourceLayout, ShaderSource source)
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

            foreach (var lqayot in vertexFormat.Layouts) {
                foreach (var element in lqayot.Elements) {
                    macros.Add(new MacroDefinition("USE_" + element.Name.ToUpperInvariant()));
                }
            }

            return macros.ToArray();
        }

        private VertexLayoutDescription[] GetVertexLayout(VertexFormat vertexFormat)
        {
            if (! vertexFormat.IsSurface) 
                return vertexFormat.Layouts;

            var arr = new VertexLayoutDescription[vertexFormat.Layouts.Length + 1];
            arr[0] = new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
            );
            Array.Copy(vertexFormat.Layouts, 0, arr, 1, vertexFormat.Layouts.Length);
            return arr;
        }

        private ShaderDescription _CompileGlslToSpirv(string sourceText, string fileName, ShaderStages stage, GlslCompileOptions options)
        {
            try {
                var result = SpirvCompilation.CompileGlslToSpirv(sourceText, fileName, stage, options);
                return new ShaderDescription(stage, result.SpirvBytes, "main");
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