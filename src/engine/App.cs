using System;
using System.Diagnostics;
using LifeSim.Engine.Rendering;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Engine
{
    public class App
    {
        private readonly Sdl2Window _window;

        public Viewport viewport { get; }

        private readonly InputInstance _input;

        private readonly GPURenderer _renderer;

        public uint selectedObjectID => this._renderer.selectedObjectID;
        
        public ResourceFactory assetManager => this._renderer.assetManager;

        private Stage? _stage = null;

        public App(string[] args)
        {
            WindowCreateInfo windowCI = new WindowCreateInfo(100, 100, 1024, 600, Veldrid.WindowState.Normal, "Medieval Life");
            this._window = VeldridStartup.CreateWindow(ref windowCI);
            this.viewport = new Viewport((uint) this._window.Width, (uint) this._window.Height);

            var graphicsBackend = App.ParseGraphicsBackend(args);
            this._renderer = new GPURenderer(this._window, graphicsBackend);

            this._window.Resized += this.OnResize;

            this.OnResize();

            this._window.Resized += this.OnResize;

            this._input = new InputInstance(this._window);
            Input.SetInstance(this._input);
        }

        public void Run(Stage stage)
        {
            this.SetStage(stage);
            this._MainLoop();
        }

        public FrameProfiler.FrameStats opaqueDrawStats => this._renderer.baseStats;
        public FrameProfiler.FrameStats shadowmapDrawStats => this._renderer.shadowmapStats;

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
            uint width = (uint) this._window.Width;
            uint height = (uint) this._window.Height;
            this.viewport.Resize(width, height);
            this._renderer.Resize(width, height);
        }

        public void SetStage(Stage stage)
        {
            this._stage = stage;
        }

        public IntPtr GetImGUITexture(GPUTexture texture)
        {
            return this._renderer.GetImGUITexture(texture);
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

                this._renderer.mousePickingPosition = Input.mousePosition;
                
                var fps = (1f / deltaTime).ToString("0.00");
                var dt = (deltaTime * 1000).ToString("0.00");

                var mouse = "(" + Input.mousePosition.X + ", " +Input.mousePosition.Y + ")";
                this._window.Title = "Medieval Life" + " (" + this._renderer.backendType.ToString() + ") frame = " + dt + "ms FPS = " + fps + " Mouse: " + mouse;

                if (Input.GetKeyDown(Veldrid.Key.Escape) && ! Input.mouseIsLocked) {
                    this._window.Close();
                    return;
                }
                if (Input.GetKeyDown(Veldrid.Key.F4)) {
                    this._window.WindowState = Veldrid.WindowState.BorderlessFullScreen;
                }

                this._renderer.Update(deltaTime, this._input.inputSnapshot);
                if (this._stage != null) {
                    this._stage.Update(deltaTime);
                    this._renderer.Render(this._stage);
                }

                this._input.UpdateFrameInput(); // For next frame
            }
        }
    }

}
