using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeSim.Rendering
{
    public class ShaderSource
    {
        private readonly string _shadersBasePath = "./res/shaders/";

        public string VertFilename;
        public string FragFilename;
        public string VertCode;
        public string FragCode;

        public ShaderSource(string vertFilename, string fragFilename, string vertCode, string fragCode)
        {
            this.VertFilename = vertFilename;
            this.FragFilename = fragFilename;
            this.VertCode = vertCode;
            this.FragCode = fragCode;
        }

        public ShaderSource(string vertFilename, string fragFilename)
        {
            this.VertFilename = vertFilename;
            this.FragFilename = fragFilename;
            this.VertCode = this._Load(vertFilename);
            this.FragCode = this._Load(fragFilename); ;
        }

        private static readonly Regex _includeRegex = new Regex("^#include\\s+\"([^\"]+)\"");

        private string _Load(string filename)
        {
            var fullPath = this._ResolvePath(filename);
            return this._GetGlsl(fullPath);
        }

        private string _GetGlsl(string path)
        {
            // Substitute include files
            using StreamReader reader = new StreamReader(path);
            var sb = new StringBuilder();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                var match = ShaderSource._includeRegex.Match(line);
                if (match.Success)
                {
                    var filename = match.Groups[1].Value;
                    var fullFilePath = this._ResolvePath(filename);
                    var includedContent = this._GetGlsl(fullFilePath);
                    sb.AppendLine(includedContent);
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        private string _ResolvePath(string filename)
        {
            var fullFilePath = Path.Combine(this._shadersBasePath, filename);
            if (!File.Exists(fullFilePath))
            {
                throw new Exception($"The shader file \"{fullFilePath}\" was not found");
            }
            return fullFilePath;
        }
    }
}