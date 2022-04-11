using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering;

public static class ShaderCompiler
{
    public static Veldrid.Shader[] CompileShaders(GraphicsDevice gd, string vertexCode, string fragmentCode, IEnumerable<MacroDefinition>? macros = null)
    {
        var macroDefinitions = GetMacroDefinitions(macros, gd.BackendType);
        var debug = IsDebug(gd.BackendType);
        var options = new GlslCompileOptions(debug, macroDefinitions);
        var vertGlslShader = CompileGlslToSpirv(vertexCode, ShaderStages.Vertex, options);
        var fragGlslShader = CompileGlslToSpirv(fragmentCode, ShaderStages.Fragment, options);
        return gd.ResourceFactory.CreateFromSpirv(vertGlslShader, fragGlslShader);
    }

    private static bool IsDebug(GraphicsBackend backendType)
    {
#if DEBUG
        return true;
#else
        return backendType == GraphicsBackend.OpenGL || backendType == GraphicsBackend.OpenGLES;
#endif
    }

    private static MacroDefinition[] GetMacroDefinitions(IEnumerable<MacroDefinition>? macros, GraphicsBackend backend)
    {
        var list = macros == null ? new List<MacroDefinition>() : new List<MacroDefinition>(macros);
        switch (backend)
        {
            case GraphicsBackend.Direct3D11:
                list.Add(new MacroDefinition("D3D11"));
                break;
            case GraphicsBackend.Vulkan:
                //macros.Add(new MacroDefinition("VULKAN"));
                break;
            case GraphicsBackend.OpenGL:
                list.Add(new MacroDefinition("OPENGL"));
                break;
            case GraphicsBackend.Metal:
                list.Add(new MacroDefinition("METAL"));
                break;
            case GraphicsBackend.OpenGLES:
                list.Add(new MacroDefinition("OPENGLES"));
                break;
            default:
                break;
        }
        return list.ToArray();
    }

    private static ShaderDescription CompileGlslToSpirv(string sourceText, ShaderStages stage, GlslCompileOptions options)
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
                ParseError(e.Message, out int lineNumber, out string _, out string message);

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

                    exceptionStr += lineNumberStr + sourceCodeLines[i - 1] + "\n";
                    if (i == lineNumber)
                        exceptionStr += new string(' ', lineNumberStr.Length) + "^^^^ ERROR HERE ^^^^\n";
                }
                throw new Exception(exceptionStr);
            }

            Console.WriteLine(sourceText);
            throw e;
        }
    }

    private static void ParseError(string originalExceptionMessage, out int lineNumber, out string filename, out string errorMessage)
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
}