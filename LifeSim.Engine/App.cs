using System;
using System.Diagnostics;
using System.Runtime;
using LifeSim.Rendering;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Engine
{
    public class App
    {
        private readonly Sdl2Window _window;

        private readonly InputInstance _input;

        private readonly Renderer _renderer;

        private IStage? _stage = null;

        public App()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            
            WindowCreateInfo windowCI = new WindowCreateInfo(100, 100, 1024, 600, Veldrid.WindowState.Normal, "Medieval Life");
            this._window = VeldridStartup.CreateWindow(ref windowCI);
            this.Viewport = new Viewport((uint) this._window.Width, (uint) this._window.Height);

            var graphicsBackend = App.ParseGraphicsBackend(Environment.GetCommandLineArgs());
            this._renderer = new Renderer(this._window, graphicsBackend);

            this._window.Resized += this.OnResize;

            this.OnResize();

            this._input = new InputInstance(this._window);
            Input.SetInstance(this._input);
        }

        public Viewport Viewport { get; }

        public SceneStorage Storage => this._renderer.SceneStorage;

        public void Run(IStage stage)
        {
            this.SetStage(stage);
            this._MainLoop();
        }

        private static Veldrid.GraphicsBackend ParseGraphicsBackend(string[] args)
        {
            Veldrid.GraphicsBackend backend = Veldrid.GraphicsBackend.Vulkan;
            if (args.Length > 0) {
                foreach (var arg in args) {
                    switch (arg.ToLower()) {
                        case "vulkan": 
                        case "vk":
                            backend = Veldrid.GraphicsBackend.Vulkan; 
                            break;
                        case "metal": 
                            backend = Veldrid.GraphicsBackend.Metal; 
                            break;
                        case "directx":
                        case "dx11":
                        case "dx":
                        case "directx11": 
                            backend = Veldrid.GraphicsBackend.Direct3D11; 
                            break;
                        case "gl":
                        case "opengl": 
                            backend = Veldrid.GraphicsBackend.OpenGL; 
                            break;
                        case "gles":
                        case "opengles": 
                            backend = Veldrid.GraphicsBackend.OpenGLES; 
                            break;
                    }
                }
            }
            return backend;
        }

        private void OnResize() 
        {
            uint width = (uint) this._window.Width;
            uint height = (uint) this._window.Height;
            this.Viewport.Resize(width, height);
            this._renderer.Resize(width, height, this.Viewport.Width, this.Viewport.Height);
        }

        public void SetStage(IStage stage)
        {
            this._stage = stage;
        }

        public void Quit()
        {
            this._renderer.WaitForGPU();
            this._window.Close();
        }

        private void _MainLoop()
        {
            Stopwatch sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            while (this._window.Exists)
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                float deltaTime = (float)(newElapsed - previousElapsed);
                previousElapsed = newElapsed;

                this._renderer.MousePicker.Update(Input.MousePosition);
                
                var fps = (1f / deltaTime).ToString("0.00");
                var dt = (deltaTime * 1000).ToString("0.00");

                var mouse = "(" + Input.MousePosition.X + ", " +Input.MousePosition.Y + ")";
                this._window.Title = "Medieval Life" + " (" + this._renderer.BackendType.ToString() + ") frame = " + dt + "ms FPS = " + fps + " Mouse: " + mouse;

                if (Input.GetKeyDown(Veldrid.Key.Escape) && ! Input.MouseIsLocked) {
                    this._window.Close();
                    return;
                }
              
                if (Input.GetKeyDown(Veldrid.Key.F4)) {
                    this._window.WindowState = this._window.WindowState == Veldrid.WindowState.BorderlessFullScreen
                        ? Veldrid.WindowState.Normal
                        : Veldrid.WindowState.BorderlessFullScreen;
                }

                this._renderer.ImguiRenderer.Update(deltaTime, this._input.InputSnapshot);

                if (this._stage != null) {
                    this._stage.Update(deltaTime);
                    this._stage.RenderFrame(this._renderer);
                    this._renderer.Render();
                }

                this._input.UpdateFrameInput(); // For next frame
            }
        }
    }

}
