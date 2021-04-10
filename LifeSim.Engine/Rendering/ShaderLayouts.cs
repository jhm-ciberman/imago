using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class ShaderLayouts
    {
        public struct Passes
        {
            public ResourceLayout color;
            public ResourceLayout shadowMap;
            public ResourceLayout sprites;
            public ResourceLayout fullscreen;
        }

        public struct Materials
        {
            public ResourceLayout surface;
            public ResourceLayout fullscreen;
            public ResourceLayout sprites;
        }

        public struct Renderables
        {
            public ResourceLayout regular;
            public ResourceLayout skinned;
        }

        public Passes passes;
        public Materials materials;
        public Renderables renderables;

        public ShaderLayouts(Veldrid.ResourceFactory factory)
        {
            this.materials = new Materials {
                surface = factory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )),

                fullscreen = factory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )),

                sprites = factory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )),
            };

            this.renderables = new Renderables {
                regular = factory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ObjectInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )),

                skinned = factory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ObjectInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("BonesInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )),
            };
        }

        
        public static Veldrid.VertexLayoutDescription GetVertexLayout(VertexLayoutKind kind)
        {
            return kind switch {
                VertexLayoutKind.PosOnly => new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                ),
                VertexLayoutKind.Regular => new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                ),
                VertexLayoutKind.Skinned => new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Joints", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UShort4),
                    new VertexElementDescription("Weights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                ),
                VertexLayoutKind.Sprite => new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
                ),
                _ => throw new System.NotSupportedException(),
            };
        }
    }
}