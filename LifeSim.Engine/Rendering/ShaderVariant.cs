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
                // Messages start always with "Compilation failed: Fragment:145: ..."
                string prefix = "Compilation failed: " + stage.ToString() + ":";
                if (e.Message.Contains(prefix))
                {
                    this.ParseError(e.Message, out int lineNumber, out string fileName, out string message);

                    string[] sourceCodeLines = sourceText.Split('\n');
                    int linesRange = 3; // Show 3 lines before and after the error
                    int startLine = lineNumber - linesRange;
                    int endLine = lineNumber + linesRange;
                    startLine = Math.Max(0, startLine);
                    endLine = Math.Min(sourceCodeLines.Length - 1, endLine);

                    string exceptionStr = "Compilation failed at line " + lineNumber + ":\n";
                    exceptionStr += message + "\n";
                    exceptionStr += "Source code:\n";

                    for (int i = startLine; i <= endLine; i++)
                    {
                        // pad the line number with spaces
                        string lineNumberStr = i.ToString().PadLeft(5) + ": ";

                        exceptionStr += lineNumberStr + sourceCodeLines[i] + "\n";
                        if (i == lineNumber)
                            exceptionStr += new string(' ', lineNumberStr.Length) + "^^^^ ERROR HERE ^^^^\n";
                    }
                    throw new Exception(exceptionStr);
                }

                Console.WriteLine(sourceText);
                throw e;
            }
        }

        private void ParseError(string originalExceptionMessage, out int lineNumber, out string filename, out string errorMessage)
        {
            // The format is "Compilation failed: FileName:LineNumber:ErrorMessage".
            // The ErrorMessage can contain colons.

            string[] parts = originalExceptionMessage.Split(':');
            filename = parts[1];
            lineNumber = int.Parse(parts[2]);
            errorMessage = parts[3];

            if (parts.Length > 3)
            {
                for (int i = 4; i < parts.Length; i++)
                    errorMessage += ":" + parts[i];
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