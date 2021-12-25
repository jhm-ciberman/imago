using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering
{
    public class ShaderVariant : IDisposable
    {
        public VertexFormat VertexFormat;

        public ShaderSetDescription ShaderSetDescription;

        public ResourceLayout? MaterialResourceLayout;

        public ShaderVariant(GraphicsDevice gd, VertexFormat vertexFormat, ResourceLayout? materialResourceLayout, string vertexCode, string fragmentCode)
        {
            bool debug = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;
#if DEBUG
            debug = true;
#endif

            var macros = this.GetMacroDefinitions(vertexFormat, gd.BackendType);
            var options = new GlslCompileOptions(debug: debug, macros);
            var vertGlslShader = this._CompileGlslToSpirv(vertexCode, ShaderStages.Vertex, options);
            var fragGlslShader = this._CompileGlslToSpirv(fragmentCode, ShaderStages.Fragment, options);

            this.VertexFormat = vertexFormat;

            this.MaterialResourceLayout = materialResourceLayout;
            var layout = this.GetVertexLayout(vertexFormat);
            this.ShaderSetDescription = new ShaderSetDescription(layout, gd.ResourceFactory.CreateFromSpirv(vertGlslShader, fragGlslShader));
        }

        private MacroDefinition[] GetMacroDefinitions(VertexFormat vertexFormat, GraphicsBackend backend)
        {
            var macros = new List<MacroDefinition>();

            switch (backend)
            {
                case GraphicsBackend.Direct3D11:
                    macros.Add(new MacroDefinition("D3D11"));
                    break;
                case GraphicsBackend.Vulkan:
                    //macros.Add(new MacroDefinition("VULKAN"));
                    break;
                case GraphicsBackend.OpenGL:
                    macros.Add(new MacroDefinition("OPENGL"));
                    break;
                case GraphicsBackend.Metal:
                    macros.Add(new MacroDefinition("METAL"));
                    break;
                case GraphicsBackend.OpenGLES:
                    macros.Add(new MacroDefinition("OPENGLES"));
                    break;
            }

            foreach (var lqayot in vertexFormat.Layouts)
            {
                foreach (var element in lqayot.Elements)
                {
                    macros.Add(new MacroDefinition("USE_" + element.Name.ToUpperInvariant()));
                }
            }

            return macros.ToArray();
        }

        private VertexLayoutDescription[] GetVertexLayout(VertexFormat vertexFormat)
        {
            if (!vertexFormat.IsSurface)
                return vertexFormat.Layouts;

            var arr = new VertexLayoutDescription[vertexFormat.Layouts.Length + 1];
            arr[0] = new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
            );
            Array.Copy(vertexFormat.Layouts, 0, arr, 1, vertexFormat.Layouts.Length);
            return arr;
        }

        private ShaderDescription _CompileGlslToSpirv(string sourceText, ShaderStages stage, GlslCompileOptions options)
        {
            try
            {
                var result = SpirvCompilation.CompileGlslToSpirv(sourceText, stage.ToString(), stage, options);
                return new ShaderDescription(stage, result.SpirvBytes, "main");
            }
            catch (SpirvCompilationException e)
            {
                Console.WriteLine(sourceText);
                throw e;
            }
        }

        public void Dispose()
        {
            var nativeShaders = this.ShaderSetDescription.Shaders;
            for (int i = 0; i < nativeShaders.Length; i++)
            {
                nativeShaders[i].Dispose();
            }
        }
    }
}