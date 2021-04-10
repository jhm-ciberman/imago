using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering
{
    public class ShaderManager
    {
        public struct Shader
        {
            public Veldrid.Shader[] shaders;
            public Shader(Veldrid.Shader[] shaders) { this.shaders = shaders; }
        }

        private readonly string _shadersBasePath = "./res/shaders/";

        private readonly Veldrid.ResourceFactory _factory;

        private readonly Dictionary<ShaderVariant, Shader> _shaderVariants = new Dictionary<ShaderVariant, Shader>();

        public ShaderManager(Veldrid.ResourceFactory factory)
        {
            this._factory = factory;
        }   
    
        public Shader GetShader(ShaderVariant shaderVariant)
        {
            if (! this._shaderVariants.TryGetValue(shaderVariant, out Shader shaders)) {
                shaders = this._MakeShader(shaderVariant);
                lock (this._shaderVariants) {
                    this._shaderVariants.Add(shaderVariant, shaders);
                }
                return shaders;
            }
            return shaders;
        }

        private static readonly Regex _includeRegex = new Regex("^#include\\s+\"([^\"]+)\"");

        private string _GetGlsl(string path)
        {
            // Substitute include files
            using StreamReader reader = new StreamReader(path);
            var sb = new StringBuilder();
            while (! reader.EndOfStream) {
                var line = reader.ReadLine();
                if (line == null) break;
                var match = ShaderManager._includeRegex.Match(line);
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

        private SpirvCompilationResult _CompileGlslToSpirv(string filename, ShaderStages shaderStages, GlslCompileOptions options)
        {
            var fullPath = this._ResolvePath(filename);
            var text = this._GetGlsl(fullPath);
            return SpirvCompilation.CompileGlslToSpirv(text.ToString(), fullPath, shaderStages, options);
        }

        private Shader _MakeShader(ShaderVariant shaderVariant)
        {
            var macros = new MacroDefinition[shaderVariant.keywords.Length];
            for (int i = 0; i < shaderVariant.keywords.Length; i++) {
                macros[i++].Name = shaderVariant.keywords[i];
            }

            var options = new GlslCompileOptions(true, macros);

            var vertResult = this._CompileGlslToSpirv(shaderVariant.shaderName + ".vert.glsl", ShaderStages.Vertex, options);
            var fragResult = this._CompileGlslToSpirv(shaderVariant.shaderName + ".frag.glsl", ShaderStages.Fragment, options);
            
            return new Shader(this._factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vertResult.SpirvBytes, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragResult.SpirvBytes, "main")
            ));
        }
    }
}