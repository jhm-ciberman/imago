using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LifeSim.Engine.Rendering
{
    public class ShaderSource
    {
        private readonly string _shadersBasePath = "./res/shaders/";

        public string vertFilename;
        public string fragFilename;
        public string vertCode;
        public string fragCode;

        public ShaderSource(string vertFilename, string fragFilename, string vertCode, string fragCode)
        {
            this.vertFilename = vertFilename;
            this.fragFilename = fragFilename;
            this.vertCode = vertCode;
            this.fragCode = fragCode;
        }

        public ShaderSource(string vertFilename, string fragFilename)
        {
            this.vertFilename = vertFilename;
            this.fragFilename = fragFilename;
            this.vertCode = this._Load(vertFilename);
            this.fragCode = this._Load(fragFilename);;
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
            while (! reader.EndOfStream) {
                var line = reader.ReadLine();
                if (line == null) break;
                var match = ShaderSource._includeRegex.Match(line);
                if (match.Success) {
                    var filename = match.Groups[1].Value;
                    var fullFilePath = this._ResolvePath(filename);
                    var includedContent = this._GetGlsl(fullFilePath);
                    sb.AppendLine(includedContent);
                } else {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        private string _ResolvePath(string filename)
        {
            var fullFilePath = Path.Combine(this._shadersBasePath, filename);
            if (! File.Exists(fullFilePath)) {
                throw new Exception($"The shader file \"{fullFilePath}\" was not found");
            }
            return fullFilePath;
        }
    }
}