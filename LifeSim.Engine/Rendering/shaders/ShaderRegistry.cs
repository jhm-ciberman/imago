using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public static class ShaderRegistry
    {
        public static Shader CreateBaseShader(Veldrid.GraphicsDevice gd, ForwardPass pass)
        {
            return new Shader(gd, pass,
                new[] { VertexFormat.Regular, VertexFormat.Skinned }, 
                new ShaderSource("base.vert.glsl", "base.frag.glsl"),
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                ),
                new [] {
                    new Shader.Uniform("AlbedoColor", Shader.UniformType.Vec4),
                    new Shader.Uniform("PickingID", Shader.UniformType.UVec4),
                    new Shader.Uniform("TextureST", Shader.UniformType.Vec4),
                }
            );
        }

        public static Shader CreateShadowmapShader(Veldrid.GraphicsDevice gd, ShadowmapPass pass)
        {
            return new Shader(gd, pass,
                new[] { VertexFormat.Regular, VertexFormat.Skinned }, 
                new ShaderSource("shadowmap.vert.glsl", "shadowmap.frag.glsl"),
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                ),
                new [] {
                    new Shader.Uniform("AlbedoColor", Shader.UniformType.Vec4),
                    new Shader.Uniform("PickingID", Shader.UniformType.UVec4),
                    new Shader.Uniform("TextureST", Shader.UniformType.Vec4),
                }
            );
        }

        public static Shader CreateSpritesShader(Veldrid.GraphicsDevice gd, SpritesPass pass)
        {
            return new Shader(gd, pass,
                new[] { VertexFormat.Sprite }, 
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
                new[] { VertexFormat.PosOnly }, 
                new ShaderSource("fullscreen.vert.glsl", "fullscreen.frag.glsl"),
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                ),
                Array.Empty<Shader.Uniform>()
            );
        }
    }
}