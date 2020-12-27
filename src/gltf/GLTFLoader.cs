using LifeSim.Rendering;

namespace LifeSim.GLTF
{
    public class GLTFLoader
    {
        private GPURenderer _renderer;
        private GLTFModel _model;
        
        public GLTFLoader(GPURenderer renderer, string path)
        {
            this._renderer = renderer;
            this._model = new GLTFModel(path);
        }

        public Skin LoadSkin(int index = 0)
        {
            return this._model.GetSkin(index);
        }

        public Scene3D LoadScene(int index = 0)
        {
            return this._model.GetScene(index);
        }

        public Animation LoadAnimation(int index = 0)
        {
            return this._model.GetAnimation(index);
        }

        public Animation[] LoadAnimations()
        {
            return this._model.GetAnimations();
        }

        public GPUMesh LoadMesh(int meshIndex = 0)
        {
            return this._renderer.MakeMesh(this._model.GetPrimitive(meshIndex).MakeMesh());
        }
    }
}