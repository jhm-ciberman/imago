using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public static class ShaderSource
{
    private static readonly string _shadersBasePath = "./res/shaders/";

    private static readonly Regex _includeRegex = new Regex("^#include\\s+\"([^\"]+)\"");

    public static string Load(string filename)
    {
        var fullPath = _ResolvePath(filename);
        return _GetGlsl(fullPath);
    }

    private static string _GetGlsl(string path)
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
                var fullFilePath = _ResolvePath(filename);
                var includedContent = _GetGlsl(fullFilePath);
                sb.AppendLine(includedContent);
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        return sb.ToString();
    }

    private static string _ResolvePath(string filename)
    {
        var fullFilePath = Path.Combine(_shadersBasePath, filename);
        if (!File.Exists(fullFilePath))
        {
            throw new Exception($"The shader file \"{fullFilePath}\" was not found");
        }
        return fullFilePath;
    }
}