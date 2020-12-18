using Veldrid;

namespace LifeSim.Rendering
{
    public class Material
    {
        private Pipeline _pipeline;

        //private Shader shader;

        public Material(Pipeline pipeline) 
        {
            this._pipeline = pipeline;
        }

        public Pipeline pipeline => this._pipeline;

        public void Dispose()
        {
            this._pipeline.Dispose();

        }
    }
}