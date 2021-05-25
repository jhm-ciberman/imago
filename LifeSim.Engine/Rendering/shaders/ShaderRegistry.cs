using System;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering
{
    public static class ShaderRegistry
    {
        public static VertexFormat posOnlyVertexFormat;
        public static VertexFormat spritesVertexFormat;
        public static VertexFormat skinnedVertexFormat;
        public static VertexFormat regularVertexFormat;

        static ShaderRegistry()
        {
            ShaderRegistry.posOnlyVertexFormat = ShaderRegistry._CreatePosOnlyVertexFormat();
            ShaderRegistry.spritesVertexFormat = ShaderRegistry._CreateSpritesVertexFormat();
            ShaderRegistry.skinnedVertexFormat = ShaderRegistry._CreateSkinnedVertexFormat();
            ShaderRegistry.regularVertexFormat = ShaderRegistry._CreateRegularVertexFormat();
        }

        public static Shader CreateBaseShader(Veldrid.GraphicsDevice gd, ForwardPass pass)
        {
            return new Shader(gd, pass,
                new[] { ShaderRegistry.regularVertexFormat, ShaderRegistry.skinnedVertexFormat }, 
                new ShaderSource("base.vert.glsl", "base.frag.glsl"),
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                ),
                new [] {
                    new Shader.Uniform("AlbedoColor", Shader.UniformType.Vec4),
                    new Shader.Uniform("TextureST", Shader.UniformType.Vec4),
                }
            );
        }

        public static Shader CreateShadowmapShader(Veldrid.GraphicsDevice gd, ShadowmapPass pass)
        {
            return new Shader(gd, pass,
                new[] { ShaderRegistry.regularVertexFormat, ShaderRegistry.skinnedVertexFormat }, 
                new ShaderSource("shadowmap.vert.glsl", "shadowmap.frag.glsl"),
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                ),
                new [] {
                    new Shader.Uniform("AlbedoColor", Shader.UniformType.Vec4),
                    new Shader.Uniform("TextureST", Shader.UniformType.Vec4),
                }
            );
        }

        public static Shader CreateSpritesShader(Veldrid.GraphicsDevice gd, SpritesPass pass)
        {
            return new Shader(gd, pass,
                new[] { ShaderRegistry.spritesVertexFormat }, 
                new ShaderSource("sprites.vert.glsl", "sprites.frag.glsl"),
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                ),
                Array.Empty<Shader.Uniform>()
            );
        }

        public static Shader CreateFullScreenShader(Veldrid.GraphicsDevice gd, FullscreenPass pass)
        {
            return new Shader(gd, pass,
                new[] { ShaderRegistry.posOnlyVertexFormat }, 
                new ShaderSource("fullscreen.vert.glsl", "fullscreen.frag.glsl"),
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                ),
                Array.Empty<Shader.Uniform>()
            );
        }


        
        private static VertexFormat _CreatePosOnlyVertexFormat()
        {
            return new VertexFormat(new [] {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                ),
            });
        }

        private static VertexFormat _CreateRegularVertexFormat()
        {
            return new VertexFormat(new [] {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                ),
                new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                    new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
                ),
            });
        }

        private static VertexFormat _CreateSkinnedVertexFormat()
        {
            return new VertexFormat(new [] {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Joints", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UShort4),
                    new VertexElementDescription("Weights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                ),
                new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                    new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
                ),
            }, new MacroDefinition[] { new MacroDefinition("USE_SKINNED_MESH") });
        }

        private static VertexFormat _CreateSpritesVertexFormat()
        {
            return new VertexFormat(new [] {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
                ),
            });
        }
    }
}