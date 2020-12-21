using LifeSim.Rendering;
using System.Numerics;
using System.IO;

namespace LifeSim
{
    class App
    {
        static void Main()
        {
            new App();
        }

        private Window _window;
        private GPURenderer _renderer;
        private DemoScene _scene;

        public App()
        {
            this._window = new Window();
            this._renderer = new GPURenderer(this._window);

            this._scene = new DemoScene(this._window, this._renderer);
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
            var dt = (deltaTime * 1000).ToString("0.00");

            var mouse = "(" + Input.MousePosition.X + ", " +Input.MousePosition.Y + ")";
            this._window.title = "Hello world" + " (" + this._renderer.backendType.ToString() + ") frame = " + dt + "ms FPS = " + fps + " Mouse: " + mouse;

            if (Input.GetKey(Veldrid.Key.Escape)) {
                this._window.Close();
                return;
            }

            this._scene.Update(deltaTime);
            this._renderer.Render(this._scene);
        }
    }

}
