using System;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Engine
{
    public class App
    {
        public Rendering.Viewport Viewport { get; }

        public SceneStorage Storage => this._renderer.Storage;

        private readonly Sdl2Window _window;

        private readonly InputInstance _input;

        private readonly Renderer _renderer;

        private double _simulationTime = 0;
        private double _renderingTime = 0;

        private double _frameTime = 0;

        public bool UseMultiThreadRendering { get; set; } = true;

        public Scene? CurrentScene { get; set; } = null;

        private bool _running = false;
        public uint MousePickerObjectID => this._renderer.MousePickerObjectID;

        public App(string windowTitle, GraphicsBackend? backend = null)
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            WindowCreateInfo windowCI = new WindowCreateInfo(100, 100, 1024, 600, Veldrid.WindowState.Normal, windowTitle);
            this._window = VeldridStartup.CreateWindow(ref windowCI);
            this.Viewport = new Rendering.Viewport((uint)this._window.Width, (uint)this._window.Height);

            this._renderer = new Renderer(this._window, backend);

            this._window.Resized += this.OnResize;

            this.OnResize();

            this._input = new InputInstance(this._window);
            Input.SetInstance(this._input);
        }

        public void SetScene(Scene scene)
        {
            this.CurrentScene = scene;
        }

        private void OnResize()
        {
            uint width = (uint) this._window.Width;
            uint height = (uint) this._window.Height;
            this.Viewport.Resize(width, height);
            this._renderer.Resize(width, height, this.Viewport.Width, this.Viewport.Height);
        }

        public void Quit()
        {
            this._renderer.Dispose();
            this._window.Close();
        }

        public void Run()
        {
            if (this._running) return;

            this._running = true;

            Stopwatch sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            while (this._window.Exists)
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                float deltaTime = (float)(newElapsed - previousElapsed);
                previousElapsed = newElapsed;

                var fps = (1f / deltaTime).ToString("0.00");
                var dt = (deltaTime * 1000).ToString("0.00");

                this._window.Title = "Medieval Life" + " (" + this._renderer.BackendType.ToString() + ") frame = " + dt + "ms FPS = " + fps;

                if (Input.GetKeyDown(Key.Escape) && !Input.MouseIsLocked)
                {
                    this._window.Close();
                    return;
                }

                if (Input.GetKeyDown(Key.F4))
                {
                    this._window.WindowState = this._window.WindowState == Veldrid.WindowState.BorderlessFullScreen
                        ? WindowState.Normal
                        : WindowState.BorderlessFullScreen;
                }

                var scene = this.CurrentScene;
                if (scene != null)
                {
                    var swFrame = Stopwatch.StartNew();

                    if (this.UseMultiThreadRendering)
                    {
                        var simulation = Task.Run(() => this._Update(scene, deltaTime));
                        var rendering = Task.Run(() => this._Render(scene, deltaTime));
                        Task.WaitAll(simulation, rendering);
                    }
                    else
                    {
                        this._Update(scene, deltaTime);
                        this._Render(scene, deltaTime);
                    }

                    swFrame.Stop();
                    this._frameTime = swFrame.Elapsed.TotalMilliseconds;
                    var totalTime = this._simulationTime + this._renderingTime;
                    //System.Console.WriteLine("Frame time: " + this._frameTime + " - Saved: " + (this._frameTime - totalTime).ToString("0.00"));
                }

                ImGuiNET.ImGui.Begin("Debug");
                ImGuiNET.ImGui.Text("Simulation time: " + this._simulationTime.ToString("0.00") + "ms");
                ImGuiNET.ImGui.Text("Rendering time: " + this._renderingTime.ToString("0.00") + "ms");
                ImGuiNET.ImGui.End();

                this._input.UpdateFrameInput(); // For next frame
            }
        }

        private void _Update(Scene scene, float deltaTime)
        {
            Stopwatch swSimulation = Stopwatch.StartNew();
            scene.Update(deltaTime);
            scene.UpdateTransforms();
            swSimulation.Stop();
            this._simulationTime = swSimulation.Elapsed.TotalMilliseconds;
            //System.Console.WriteLine("Simulation time: " + this._simulationTime);
        }

        private void _Render(Scene scene, float deltaTime)
        {
            Stopwatch swRendering = Stopwatch.StartNew();

            this._renderer.Render(scene, deltaTime, this._input.InputSnapshot);

            swRendering.Stop();
            this._renderingTime = swRendering.Elapsed.TotalMilliseconds;
            //System.Console.WriteLine("Rendering time: " + this._renderingTime);
        }
    }
}
