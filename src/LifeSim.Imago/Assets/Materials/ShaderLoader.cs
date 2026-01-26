using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeSim.Imago.Assets.Materials;

/// <summary>
/// Specifies the render pass for vertex shader assembly.
/// </summary>
public enum ShaderPass
{
    /// <summary>
    /// Forward rendering pass.
    /// </summary>
    Forward,

    /// <summary>
    /// Shadow map rendering pass.
    /// </summary>
    Shadow,

    /// <summary>
    /// Mouse picking pass.
    /// </summary>
    Picking
}

/// <summary>
/// Provides static methods for loading GLSL shader code, including handling of #include directives.
/// </summary>
public static partial class ShaderLoader
{
    private const string UserCodePlaceholder = "{{USER_CODE}}";
    private const string DefaultVertexShader = "surfaces/standard.vert.glsl";
    private const string DefaultFragmentShader = "surfaces/standard.frag.glsl";

    private static readonly string _shadersBasePath = "./res/shaders/";

    [GeneratedRegex("^#include\\s+\"([^\"]+)\"")]
    private static partial Regex IncludeRegex();

    private static readonly Regex _includeRegex = IncludeRegex();

    /// <summary>
    /// Loads the content of a shader file, resolving its path and processing any `#include` directives.
    /// </summary>
    /// <param name="filename">The name of the shader file to load.</param>
    /// <returns>The full GLSL source code with all includes resolved.</returns>
    public static string Load(string filename)
    {
        var fullPath = ResolvePath(filename);
        return GetGlsl(fullPath);
    }

    /// <summary>
    /// Assembles a surface (fragment) shader by combining a template with user code.
    /// </summary>
    /// <param name="userCodePath">Path to the user's surface shader code, or null for default.</param>
    /// <param name="pass">The render pass to assemble the shader for.</param>
    /// <returns>The assembled GLSL source code with all includes resolved.</returns>
    public static string AssembleSurfaceShader(string? userCodePath, ShaderPass pass)
    {
        var templatePath = pass switch
        {
            ShaderPass.Forward => "include/templates/forward.frag.glsl",
            ShaderPass.Shadow => "include/templates/shadow.frag.glsl",
            ShaderPass.Picking => "include/templates/picking.frag.glsl",
            _ => throw new ArgumentOutOfRangeException(nameof(pass), pass, "Unknown vertex shader pass")
        };

        var template = Load(templatePath);
        var userCode = LoadRaw(userCodePath ?? DefaultFragmentShader);
        return template.Replace(UserCodePlaceholder, userCode);
    }

    /// <summary>
    /// Assembles a vertex shader by combining a pass-specific template with user code.
    /// </summary>
    /// <param name="userCodePath">Path to the user's vertex shader code, or null for default.</param>
    /// <param name="pass">The render pass to assemble the shader for.</param>
    /// <returns>The assembled GLSL source code with all includes resolved.</returns>
    public static string AssembleVertexShader(string? userCodePath, ShaderPass pass)
    {
        var templatePath = pass switch
        {
            ShaderPass.Forward => "include/templates/forward.vert.glsl",
            ShaderPass.Shadow => "include/templates/shadow.vert.glsl",
            ShaderPass.Picking => "include/templates/picking.vert.glsl",
            _ => throw new ArgumentOutOfRangeException(nameof(pass), pass, "Unknown vertex shader pass")
        };

        var template = Load(templatePath);
        var userCode = LoadRaw(userCodePath ?? DefaultVertexShader);
        return template.Replace(UserCodePlaceholder, userCode);
    }

    /// <summary>
    /// Loads a shader file without processing includes. Used for user shader fragments
    /// that will be injected into templates.
    /// </summary>
    /// <param name="filename">The name of the shader file to load.</param>
    /// <returns>The raw file contents.</returns>
    private static string LoadRaw(string filename)
    {
        var fullPath = ResolvePath(filename);
        return File.ReadAllText(fullPath);
    }

    private static string GetGlsl(string path)
    {
        using StreamReader reader = new StreamReader(path);
        var sb = new StringBuilder();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line == null) break;
            var match = _includeRegex.Match(line);
            if (match.Success)
            {
                var filename = match.Groups[1].Value;
                var fullFilePath = ResolvePath(filename);
                var includedContent = GetGlsl(fullFilePath);
                sb.AppendLine(includedContent);
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        return sb.ToString();
    }

    private static string ResolvePath(string filename)
    {
        var fullFilePath = Path.Combine(_shadersBasePath, filename);
        if (!File.Exists(fullFilePath))
            throw new FileNotFoundException($"Could not find shader file {filename}");
        return fullFilePath;
    }
}
