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
        private readonly string _shadersBasePath = "./res/shaders/";

        private readonly Veldrid.ResourceFactory _factory;

        private readonly Dictionary<ShaderVariantDescription, ShaderVariant> _shaderVariants = new Dictionary<ShaderVariantDescription, ShaderVariant>();

        public ShaderManager(Veldrid.ResourceFactory factory)
        {
            this._factory = factory;
        }   
    
        public ShaderVariant GetShaderVariant(ShaderVariantDescription description)
        {
            ShaderVariant? shaderVariant;
            lock (this._shaderVariants) {
                if (this._shaderVariants.TryGetValue(description, out shaderVariant)) {
                    return shaderVariant;
                }
            }

            shaderVariant = this._MakeShaderVariant(description);

            lock (this._shaderVariants) {
                this._shaderVariants.Add(description, shaderVariant);
            }

            return shaderVariant;
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

        private ShaderVariant _MakeShaderVariant(ShaderVariantDescription description)
        {
            var macros = new MacroDefinition[description.keywords.Length];
            for (int i = 0; i < description.keywords.Length; i++) {
                macros[i++].Name = description.keywords[i];
            }

            var options = new GlslCompileOptions(true, macros);

            var vertResult = this._CompileGlslToSpirv(description.shaderName + ".vert.glsl", ShaderStages.Vertex, options);
            var fragResult = this._CompileGlslToSpirv(description.shaderName + ".frag.glsl", ShaderStages.Fragment, options);
            
            var shaders = this._factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vertResult.SpirvBytes, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragResult.SpirvBytes, "main")
            );

            return new ShaderVariant(description, shaders);
        }
    }
}