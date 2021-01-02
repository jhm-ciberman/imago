using System.Collections.Generic;
using System.IO;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class MaterialManager
    {
        private ResourceFactory _factory;
        
        private string _shadersBasePath = "./res/shaders/";

        private ResourceLayout _passResourceLayout;
        private ResourceLayout _materialLayout;
        private ResourceLayout _fullscreenMaterialLayout;
        private ResourceLayout _spritesPassLayout;
        private ResourceLayout _spritesMaterialLayout;

        private Shader _errorShader;
        private Shader _baseShader;
        private Shader _skinnedShader;
        private Shader _fullscreenShader;
        private Shader _spritesShader;

        private Pass _opaquePass;
        private Pass _skinnedPass;
        private Pass _fullscreenPass;
        private Pass _spritesPass;

        private GraphicsDevice _gd;

        public MaterialManager(GraphicsDevice gd, IRenderTexture mainRenderTexture, IRenderTexture fullscreenRenderTexture, SceneContext sceneContext)
        {
            this._gd = gd;
            this._factory = gd.ResourceFactory;  

            this._spritesPassLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(new [] {
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            }));

            this._passResourceLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)
            ));

            this._materialLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));

            this._fullscreenMaterialLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(new [] {
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
            }));

            this._spritesMaterialLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(new [] {
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
            }));

            // Vertex layouts

            var posOnlyVertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
            );

            var baseVertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            );

            var skinnedVertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Joints", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UShort4),
                new VertexElementDescription("Weights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
            );

            var spritesVertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
            );

            // Shaders

            this._errorShader = this._MakeShader(new ShaderDescription {
                filename = "error", 
                passResourcelayout     = this._spritesPassLayout, 
                materialResourcelayout = this._materialLayout, 
                objectResourcelayout   = sceneContext.objectLayout,
                vertexLayouts          = new[] { baseVertexLayout }
            });

            this._baseShader = this._MakeShader(new ShaderDescription { 
                filename = "base", 
                passResourcelayout     = this._passResourceLayout, 
                materialResourcelayout = this._materialLayout, 
                objectResourcelayout   = sceneContext.objectLayout, 
                vertexLayouts          = new[] { baseVertexLayout }
            });

            this._skinnedShader = this._MakeShader(new ShaderDescription {
                filename = "skinned", 
                passResourcelayout     = this._passResourceLayout, 
                materialResourcelayout = this._materialLayout, 
                objectResourcelayout   = sceneContext.skinedObjectLayout, 
                vertexLayouts          = new[] { skinnedVertexLayout }
            });

            this._fullscreenShader = this._MakeShader(new ShaderDescription {
                filename = "fullscreen", 
                passResourcelayout     = null, 
                materialResourcelayout = this._fullscreenMaterialLayout, 
                objectResourcelayout   = null, 
                vertexLayouts          = new[] { posOnlyVertexLayout },
            });

            this._spritesShader = this._MakeShader(new ShaderDescription {
                filename = "sprites", 
                passResourcelayout     = this._spritesPassLayout, 
                materialResourcelayout = this._spritesMaterialLayout, 
                objectResourcelayout   = null, 
                vertexLayouts          = new[] { spritesVertexLayout },
            });

            // Blend
            
            var blend = new BlendStateDescription(RgbaFloat.Black, BlendAttachmentDescription.OverrideBlend, BlendAttachmentDescription.OverrideBlend);

            // Passes

            this._opaquePass = this._MakePass(new PassDescription(
                this._baseShader, mainRenderTexture, FaceCullMode.Front, PolygonFillMode.Solid, blend, 
                new[] { sceneContext.camera3DInfoBuffer, sceneContext.lightInfoBuffer }
            ));

            this._skinnedPass = this._MakePass(new PassDescription(
                this._skinnedShader, mainRenderTexture, FaceCullMode.Front, PolygonFillMode.Solid, blend, 
                new[] { sceneContext.camera3DInfoBuffer, sceneContext.lightInfoBuffer}
            ));

            this._fullscreenPass = this._MakePass(new PassDescription(
                this._fullscreenShader, fullscreenRenderTexture, FaceCullMode.Front, PolygonFillMode.Solid, BlendStateDescription.SingleOverrideBlend, 
                new BindableResource[] { }
            ));

            this._spritesPass = this._MakePass(new PassDescription(
                this._spritesShader, mainRenderTexture, FaceCullMode.None, PolygonFillMode.Solid, BlendStateDescription.SingleAlphaBlend, 
                new[] { sceneContext.camera2DInfoBuffer }
            ));

        }

        public Material MakeOpaque(GPUTexture texture)
        {
            return new Material(this._opaquePass, this._factory.CreateResourceSet(new ResourceSetDescription(
                this._materialLayout, texture.deviceTexture, texture.sampler
            )));
        }

        public Material MakeSkinned(GPUTexture texture)
        {
            return new Material(this._skinnedPass, this._factory.CreateResourceSet(new ResourceSetDescription(
                this._materialLayout, texture.deviceTexture, texture.sampler
            )));
        }

        public Material MakeFullscreen(Veldrid.Texture texture)
        {
            return new Material(this._fullscreenPass, this._factory.CreateResourceSet(new ResourceSetDescription(
                this._fullscreenMaterialLayout, texture, this._gd.LinearSampler
            )));
        }

        public Material MakeSprites(Veldrid.Texture texture)
        {
            return new Material(this._spritesPass, this._factory.CreateResourceSet(new ResourceSetDescription(
                this._spritesMaterialLayout, texture, this._gd.LinearSampler
            )));
        }

        private Shader _MakeShader(ShaderDescription description)
        {
            StringBuilder vertex = new StringBuilder();
            StringBuilder fragment = new StringBuilder();

            var lines = File.ReadAllLines(Path.Combine(this._shadersBasePath, description.filename + ".glsl"));
            StringBuilder? current = null;
            foreach (var line in lines) {
                if (line.Contains("#shader")) {
                    if (line.Contains("vertex")) {
                        current = vertex;
                    } else if (line.Contains("fragment")) {
                        current = fragment;
                    } else {
                        throw new System.Exception("Unrecognized type of shader: " + line);
                    }
                } else {
                    current?.AppendLine(line);
                }
            }
            
            var vertBytes = Encoding.UTF8.GetBytes(vertex.ToString());
            var fragBytes = Encoding.UTF8.GetBytes(fragment.ToString());
            var shaders = this._factory.CreateFromSpirv(
                new Veldrid.ShaderDescription(ShaderStages.Vertex  , vertBytes, "main"),
                new Veldrid.ShaderDescription(ShaderStages.Fragment, fragBytes, "main")
            );

            var shaderSet = new ShaderSetDescription(description.vertexLayouts, shaders);

            return new Shader(
                shaderSet, 
                description.passResourcelayout,
                description.materialResourcelayout,
                description.objectResourcelayout
            );
        }

        private Pass _MakePass(PassDescription description)
        {
            var rasterizerState = new RasterizerStateDescription(
                description.faceCullMode,
                description.polygonFillMode,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            );

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.ShaderSet = description.shader.shaderSet;
            pipelineDescription.BlendState = description.blendState;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            pipelineDescription.RasterizerState = rasterizerState;
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = description.shader.GetResourceLayouts();
            pipelineDescription.Outputs = description.renderTexture.outputDescription; 

            var pipeline = this._factory.CreateGraphicsPipeline(pipelineDescription);


            ResourceSet? resourceSet = null;
            var passLayout = description.shader.passResourcelayout;
            if (passLayout != null) {
                resourceSet = this._factory.CreateResourceSet(
                    new ResourceSetDescription(passLayout, description.resources)
                );
            }

            return new Pass(pipeline, resourceSet);
        }

    }
}