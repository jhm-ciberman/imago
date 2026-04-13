using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Imago.Assets.Materials;

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
/// Loads GLSL shader source files and resolves <c>#include</c> directives.
/// </summary>
/// <remarks>
/// All shader paths must begin with an explicit namespace prefix:
/// <list type="bullet">
/// <item><description><c>@imago/</c> resolves to shader files embedded in the Imago assembly.</description></item>
/// <item><description><c>@app/</c> resolves to shader files on disk under <see cref="AppShadersPath"/>.</description></item>
/// </list>
/// Paths without a namespace prefix are rejected. This applies to both the top-level
/// path passed to <see cref="AssembleSurfaceShader"/> or <see cref="AssembleVertexShader"/>
/// and to every <c>#include</c> directive inside a shader file.
/// </remarks>
public static partial class ShaderLoader
{
    private const string UserCodePlaceholder = "{{USER_CODE}}";
    private const string ImagoNamespace = "@imago/";
    private const string AppNamespace = "@app/";
    private const string EmbeddedResourcePrefix = "Imago.Resources.Shaders.";

    private const string DefaultFragmentShader = "@imago/surfaces/standard.frag.glsl";
    private const string DefaultVertexShader = "@imago/surfaces/standard.vert.glsl";

    private static readonly Assembly _imagoAssembly = typeof(ShaderLoader).Assembly;

    [GeneratedRegex("^#include\\s+\"([^\"]+)\"")]
    private static partial Regex IncludeRegex();

    private static readonly Regex _includeRegex = IncludeRegex();

    /// <summary>
    /// Gets or sets the filesystem path used to resolve <c>@app/</c> shader references.
    /// Defaults to <c>"shaders/"</c> relative to the current working directory.
    /// </summary>
    /// <remarks>
    /// Set this at application startup if your project stores shaders in a non-default
    /// location (for example, Medieval Life would set it to <c>"res/shaders/"</c>).
    /// The path may be absolute or relative to the working directory.
    /// </remarks>
    public static string AppShadersPath { get; set; } = "shaders/";

    /// <summary>
    /// Assembles a fragment shader by combining a pass-specific template with user surface code.
    /// </summary>
    /// <param name="userCodePath">
    /// Namespaced path (<c>@imago/</c> or <c>@app/</c>) to the user's fragment surface code,
    /// or <see langword="null"/> to use the built-in standard surface shader.
    /// </param>
    /// <param name="pass">The render pass to assemble for.</param>
    /// <returns>The assembled GLSL source with all <c>#include</c> directives resolved.</returns>
    public static string AssembleSurfaceShader(string? userCodePath, ShaderPass pass)
    {
        var templatePath = pass switch
        {
            ShaderPass.Forward => "@imago/include/templates/forward.frag.glsl",
            ShaderPass.Shadow => "@imago/include/templates/shadow.frag.glsl",
            ShaderPass.Picking => "@imago/include/templates/picking.frag.glsl",
            _ => throw new ArgumentOutOfRangeException(nameof(pass), pass, "Unknown shader pass")
        };

        var template = LoadAndResolveIncludes(templatePath);
        var userCode = ReadShaderSource(userCodePath ?? DefaultFragmentShader);
        return template.Replace(UserCodePlaceholder, userCode);
    }

    /// <summary>
    /// Assembles a vertex shader by combining a pass-specific template with user vertex code.
    /// </summary>
    /// <param name="userCodePath">
    /// Namespaced path (<c>@imago/</c> or <c>@app/</c>) to the user's vertex code,
    /// or <see langword="null"/> to use the built-in standard vertex shader.
    /// </param>
    /// <param name="pass">The render pass to assemble for.</param>
    /// <returns>The assembled GLSL source with all <c>#include</c> directives resolved.</returns>
    public static string AssembleVertexShader(string? userCodePath, ShaderPass pass)
    {
        var templatePath = pass switch
        {
            ShaderPass.Forward => "@imago/include/templates/forward.vert.glsl",
            ShaderPass.Shadow => "@imago/include/templates/shadow.vert.glsl",
            ShaderPass.Picking => "@imago/include/templates/picking.vert.glsl",
            _ => throw new ArgumentOutOfRangeException(nameof(pass), pass, "Unknown shader pass")
        };

        var template = LoadAndResolveIncludes(templatePath);
        var userCode = ReadShaderSource(userCodePath ?? DefaultVertexShader);
        return template.Replace(UserCodePlaceholder, userCode);
    }

    private static string LoadAndResolveIncludes(string path)
    {
        var source = ReadShaderSource(path);
        return ResolveIncludes(source);
    }

    private static string ResolveIncludes(string source)
    {
        var sb = new StringBuilder();
        using var reader = new StringReader(source);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var match = _includeRegex.Match(line);
            if (match.Success)
            {
                var includePath = match.Groups[1].Value;
                var includedContent = LoadAndResolveIncludes(includePath);
                sb.AppendLine(includedContent);
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        return sb.ToString();
    }

    private static string ReadShaderSource(string path)
    {
        if (path.StartsWith(ImagoNamespace, StringComparison.Ordinal))
        {
            var relative = path.Substring(ImagoNamespace.Length);
            var resourceName = EmbeddedResourcePrefix + relative.Replace('/', '.');
            using var stream = _imagoAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new FileNotFoundException(
                    $"Could not find embedded engine shader '{path}' (expected manifest resource '{resourceName}')."
                );
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        if (path.StartsWith(AppNamespace, StringComparison.Ordinal))
        {
            var relative = path.Substring(AppNamespace.Length);
            var fullPath = Path.Combine(AppShadersPath, relative);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    $"Could not find application shader '{path}' at '{fullPath}'. " +
                    $"Set {nameof(ShaderLoader)}.{nameof(AppShadersPath)} to configure the shader directory."
                );
            }

            return File.ReadAllText(fullPath);
        }

        throw new ArgumentException(
            $"Shader path must begin with a known namespace ({ImagoNamespace} or {AppNamespace}): '{path}'.",
            nameof(path)
        );
    }
}
