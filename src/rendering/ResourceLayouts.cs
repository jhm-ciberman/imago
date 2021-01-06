using Veldrid;

namespace LifeSim.Rendering
{
    public class ResourceLayouts
    {
        public struct Passes
        {
            public ResourceLayout color;
            public ResourceLayout shadowMap;
            public ResourceLayout sprites;
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

        public ResourceLayouts(ResourceFactory factory)
        {
            this.passes = new Passes {
                sprites = factory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )),

                shadowMap = factory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ShadowMapInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )),

                color = factory.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("ShadowMapTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                )),
            };


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
    }
}