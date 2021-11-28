using System;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using LifeSim.Engine.Rendering;
using Veldrid;
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

        public Rendering.Viewport Viewport { get; }

        public SceneStorage Storage => this._renderer.Storage;

        public void Run(IStage stage)
        {
            this.SetStage(stage);
            this._MainLoop();
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
            this._renderer.Dispose();
            this._window.Close();
        }

        private double _simulationTime = 0;
        private double _renderingTime = 0;

        private double _frameTime = 0;

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

                this._window.Title = "Medieval Life" + " (" + this._renderer.BackendType.ToString() + ") frame = " + dt + "ms FPS = " + fps;

                if (Input.GetKeyDown(Veldrid.Key.Escape) && !Input.MouseIsLocked)
                {
                    this._window.Close();
                    return;
                }

                if (Input.GetKeyDown(Veldrid.Key.F4))
                {
                    this._window.WindowState = this._window.WindowState == Veldrid.WindowState.BorderlessFullScreen
                        ? Veldrid.WindowState.Normal
                        : Veldrid.WindowState.BorderlessFullScreen;
                }



                this._renderer.ImGuiPass.Update(deltaTime, this._input.InputSnapshot);



                if (this._stage != null)
                {
                    bool useMultiThreading = true;

                    var swFrame = Stopwatch.StartNew();

                    if (useMultiThreading)
                    {
                        var simulation = Task.Run(() =>
                        {
                            Stopwatch swSimulation = Stopwatch.StartNew();
                            this._stage.Update(deltaTime);
                            swSimulation.Stop();
                            this._simulationTime = swSimulation.Elapsed.TotalMilliseconds;
                            System.Console.WriteLine("Simulation time: " + this._simulationTime);
                        });
                        var rendering = Task.Run(() =>
                        {
                            Stopwatch swRendering = Stopwatch.StartNew();

                            this._renderer.BeginRender();
                            this._stage.RenderFrame(this._renderer);
                            this._renderer.Render();

                            swRendering.Stop();
                            this._renderingTime = swRendering.Elapsed.TotalMilliseconds;
                            System.Console.WriteLine("Rendering time: " + this._renderingTime);
                        });
                        Task.WaitAll(simulation, rendering);
                    }
                    else
                    {
                        Stopwatch swSimulation = Stopwatch.StartNew();
                        this._stage.Update(deltaTime);
                        swSimulation.Stop();
                        this._simulationTime = swSimulation.Elapsed.TotalMilliseconds;
                        System.Console.WriteLine("Simulation time: " + this._simulationTime);

                        Stopwatch swRendering = Stopwatch.StartNew();
                        this._renderer.BeginRender();
                        this._stage.RenderFrame(this._renderer);
                        this._renderer.Render();
                        swRendering.Stop();
                        this._renderingTime = swRendering.Elapsed.TotalMilliseconds;
                        System.Console.WriteLine("Rendering time: " + this._renderingTime);
                    }

                    swFrame.Stop();
                    this._frameTime = swFrame.Elapsed.TotalMilliseconds;
                    var totalTime = this._simulationTime + this._renderingTime;
                    System.Console.WriteLine("Frame time: " + this._frameTime + " - Saved: " + (this._frameTime - totalTime).ToString("0.00"));


                }

                ImGuiNET.ImGui.Begin("Debug");
                ImGuiNET.ImGui.Text("Simulation time: " + this._simulationTime.ToString("0.00") + "ms");
                ImGuiNET.ImGui.Text("Rendering time: " + this._simulationTime.ToString("0.00") + "ms");
                ImGuiNET.ImGui.End();

                this._input.UpdateFrameInput(); // For next frame
            }
        }
    }

}
