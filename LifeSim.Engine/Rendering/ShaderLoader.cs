using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeSim.Engine.Rendering;

public static class ShaderLoader
{
    private static readonly string _shadersBasePath = "./res/shaders/";

    private static readonly Regex _includeRegex = new Regex("^#include\\s+\"([^\"]+)\"");

    public static string Load(string filename)
    {
        var fullPath = ResolvePath(filename);
        return GetGlsl(fullPath);
    }

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
        {
            throw new FileNotFoundException($"Could not find shader file {filename}");
        }
        return fullFilePath;
    }
}
