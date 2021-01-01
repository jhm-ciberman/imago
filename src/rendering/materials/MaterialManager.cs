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
        
        private Dictionary<uint, Veldrid.Pipeline> _pipelines = new Dictionary<uint, Veldrid.Pipeline>(); // Key: Pass.id
        private Dictionary<Material, Veldrid.ResourceSet> _materialResources = new Dictionary<Material, Veldrid.ResourceSet>();
        private Dictionary<ResourceLayoutDescription, ResourceLayout> _resourceLayouts = new Dictionary<ResourceLayoutDescription, ResourceLayout>();

        private ResourceLayout _passResourceLayout;
        private ResourceLayout _materialLayout;

        private Shader _errorShader;
        private Shader _baseShader;
        private Shader _skinnedShader;
        private Pass _opaquePass;
        private Pass _skinnedPass;

        public MaterialManager(ResourceFactory factory, IRenderTexture renderTexture, SceneContext sceneContext)
        {
            this._factory = factory;
            //Material.onRefCountZero += (material) => material.Dispose();    

            this._passResourceLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)
            ));

            this._materialLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));

            // Vertex layouts

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

            // Shaders

            this._errorShader = this._MakeShader(new ShaderDescription("error", 
                new[] { this._passResourceLayout, this._materialLayout, sceneContext.objectLayout }, 
                new[] { baseVertexLayout }
            ));

            this._baseShader = this._MakeShader(new ShaderDescription("base", 
                new[] { this._passResourceLayout, this._materialLayout, sceneContext.objectLayout }, 
                new[] { baseVertexLayout }
            ));

            this._skinnedShader = this._MakeShader(new ShaderDescription("skinned", 
                new[] { this._passResourceLayout, this._materialLayout, sceneContext.skinedObjectLayout }, 
                new[] { skinnedVertexLayout }
            ));



            this._opaquePass = this._MakePass(new PassDescription(
                this._baseShader, renderTexture, PolygonFillMode.Solid, BlendStateDescription.SingleOverrideBlend, 
                new[] { sceneContext.cameraInfoBuffer, sceneContext.lightInfoBuffer }
            ));

            this._skinnedPass = this._MakePass(new PassDescription(
                this._skinnedShader, renderTexture, PolygonFillMode.Solid, BlendStateDescription.SingleOverrideBlend, 
                new[] { sceneContext.cameraInfoBuffer, sceneContext.lightInfoBuffer}
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

            return new Shader(shaderSet, description.resourcelayouts);
        }

        private Pass _MakePass(PassDescription description)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.Front,
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
            pipelineDescription.ResourceLayouts = description.shader.resourceLayouts;
            pipelineDescription.Outputs = description.renderTexture.outputDescription; 

            var pipeline = this._factory.CreateGraphicsPipeline(pipelineDescription);


            var passLayout = description.shader.resourceLayouts[0];
            var resourceSet = this._factory.CreateResourceSet(
                new ResourceSetDescription(passLayout, description.resources)
            );

            return new Pass(pipeline, resourceSet);
        }

    }
}