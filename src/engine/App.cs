using LifeSim.Demo;
using LifeSim.Engine;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine
{
    public class App
    {
        private Window _window;
        private GPURenderer _renderer;
        private IStage _stage;

        public App(string[] args, System.Func<App, IStage> stageFactory)
        {
            this._window = new Window();
            var graphicsBackend = App.ParseGraphicsBackend(args);
            this._renderer = new GPURenderer(this._window, graphicsBackend);

            this._window.viewport.onResize += this.OnResize;

            this.OnResize();
            this._stage = stageFactory.Invoke(this); 
            this._window.RunMainLoop(this.Update);
        }

        public Window window => this._window;
        
        public Viewport viewport => this._window.viewport;

        public uint selectedObjectID => this._renderer.selectedObjectID;
        
        public AssetManager assetManager => this._renderer.assetManager;

        private static Veldrid.GraphicsBackend ParseGraphicsBackend(string[] args)
        {
            Veldrid.GraphicsBackend backend = Veldrid.GraphicsBackend.Vulkan;
            if (args.Length > 0) {
                switch (args[0]) {
                    case "vulkan": backend = Veldrid.GraphicsBackend.Vulkan; break;
                    case "metal": backend = Veldrid.GraphicsBackend.Metal; break;
                    case "directx11": backend = Veldrid.GraphicsBackend.Direct3D11; break;
                    case "opengl": backend = Veldrid.GraphicsBackend.OpenGL; break;
                    case "opengles": backend = Veldrid.GraphicsBackend.OpenGLES; break;
                }
            }
            return backend;
        }

        private void OnResize() 
        {
            this._renderer.Resize(this._window.viewport.width, this._window.viewport.height);
        }

        public void SetStage(IStage stage)
        {
            this._stage = stage;
        }

        private void Update(float deltaTime)
        {
            var fps = (1f / deltaTime).ToString("0.00");
            var dt = (int) (deltaTime * 1000000);

            var mouse = "(" + Input.mousePosition.X + ", " +Input.mousePosition.Y + ")";
            this._window.title = "Hello world" + " (" + this._renderer.backendType.ToString() + ") frame = " + dt + "microseg FPS = " + fps + " Mouse: " + mouse;

            if (Input.GetKeyDown(Veldrid.Key.Escape) && ! Input.mouseIsLocked) {
                this._window.Close();
                return;
            }

            this._stage.Update(deltaTime);
            this._renderer.Render(this._stage);
        }
    }

}
