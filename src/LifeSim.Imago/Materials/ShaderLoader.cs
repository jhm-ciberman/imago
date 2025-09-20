using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeSim.Imago.Graphics;

/// <summary>
/// Provides static methods for loading GLSL shader code, including handling of #include directives.
/// </summary>
public static partial class ShaderLoader
{
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
    /// Recursively loads GLSL shader code from the specified path, resolving `#include` directives.
    /// </summary>
    /// <param name="path">The full path to the GLSL shader file.</param>
    /// <returns>The processed GLSL source code.</returns>
    private static string GetGlsl(string path)
    {
        // Substitute include files
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
