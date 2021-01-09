using LifeSim.Generation;
using LifeSim.Rendering;

namespace LifeSim
{
    class App
    {
        static void Main(string[] args)
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

            new App(backend);
        }

        private Window _window;
        private GPURenderer _renderer;
        private DemoStage _stage;

        public App(Veldrid.GraphicsBackend graphicsBackend)
        {
            this._window = new Window();
            this._renderer = new GPURenderer(this._window, graphicsBackend);

            this._stage = new DemoStage(this._renderer, this._window.viewport);
            this._window.viewport.onResize += this.OnResize;

            this.OnResize();
            this._window.RunMainLoop(this.Update);
        }

        public void OnResize() 
        {
            this._renderer.Resize(this._window.viewport.width, this._window.viewport.height);
        }

        protected void Update(float deltaTime)
        {
            var fps = (1f / deltaTime).ToString("0.00");
            var dt = (int) (deltaTime * 1000000);

            var mouse = "(" + Input.MousePosition.X + ", " +Input.MousePosition.Y + ")";
            this._window.title = "Hello world" + " (" + this._renderer.backendType.ToString() + ") frame = " + dt + "microseg FPS = " + fps + " Mouse: " + mouse;

            if (Input.GetKeyDown(Veldrid.Key.Escape)) {
                this._window.Close();
                return;
            }

            this._stage.Update(deltaTime);
            this._renderer.Render(this._stage);
        }
    }

}
