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

            this._scene = new DemoScene(this._renderer);
            this._window.onResize += this.OnResize;

            this.OnResize(this._window);
            this._window.RunMainLoop(this.Update);
        }

        public void OnResize(Window window) 
        {
            this._renderer.Resize(this._window.width, this._window.height);

            float aspect = (float) this._window.width / (float) this._window.height;
            foreach (var camera in this._scene.cameras) {
                camera.aspect = aspect;
            }
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
