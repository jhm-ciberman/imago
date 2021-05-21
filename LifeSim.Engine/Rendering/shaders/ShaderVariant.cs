using System;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering
{
    public class ShaderVariant : IDisposable
    {
        public VertexFormat vertexFormat;

        public ShaderSetDescription shaderSetDescription;

        public ResourceLayout materialResourceLayout;

        public ShaderVariant(Veldrid.ResourceFactory factory, VertexFormat vertexFormat, ResourceLayout materialResourceLayout, ShaderSource source)
        {
            var options = this._GetCompileOptions(vertexFormat);
            var vertResult = this._CompileGlslToSpirv(source.vertCode, source.vertFilename, ShaderStages.Vertex, options);
            var fragResult = this._CompileGlslToSpirv(source.fragCode, source.fragFilename, ShaderStages.Fragment, options);

            this.vertexFormat = vertexFormat;

            var vertexLayout = this._GetVertexLayout(vertexFormat);

            this.materialResourceLayout = materialResourceLayout;

            this.shaderSetDescription = new ShaderSetDescription(vertexLayout, factory.CreateFromSpirv(
                new Veldrid.ShaderDescription(ShaderStages.Vertex, vertResult.SpirvBytes, "main"),
                new Veldrid.ShaderDescription(ShaderStages.Fragment, fragResult.SpirvBytes, "main")
            ));
        }

        private SpirvCompilationResult _CompileGlslToSpirv(string sourceText, string fileName, ShaderStages stage, GlslCompileOptions options)
        {
            try {
                return SpirvCompilation.CompileGlslToSpirv(sourceText, fileName, stage, options);
            } catch (SpirvCompilationException e) {
                Console.WriteLine(sourceText);
                throw e;
            }
        }

        private GlslCompileOptions _GetCompileOptions(VertexFormat vertexFormat)
        {
            var defines = vertexFormat == VertexFormat.Skinned
                ? new MacroDefinition[] { new MacroDefinition("USE_SKINNED_MESH") }
                : Array.Empty<MacroDefinition>();

            return new GlslCompileOptions(debug: true, defines);
        }

        private Veldrid.VertexLayoutDescription[] _GetVertexLayout(VertexFormat format)
        {
            return format switch {
                VertexFormat.PosOnly => new [] {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                    ),
                },
                VertexFormat.Regular => new [] {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                    ),
                },
                VertexFormat.Skinned => new [] {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("Joints", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UShort4),
                        new VertexElementDescription("Weights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                    ),
                },
                VertexFormat.Sprite => new [] {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
                    ),
                },
                _ => throw new System.NotSupportedException(),
            };
        }

        public void Dispose()
        {
            var nativeShaders = this.shaderSetDescription.Shaders;
            for (int i = 0; i < nativeShaders.Length; i++) {
                nativeShaders[i].Dispose();
            }
        }
    }
}