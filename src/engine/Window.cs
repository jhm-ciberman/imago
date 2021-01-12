using System.Diagnostics;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Engine
{
    public class Window
    {
        private Sdl2Window _window;
        private Viewport _viewport;
        public Viewport viewport => this._viewport;
        private InputInstance _input;

        public Window()
        {
            WindowCreateInfo windowCI = new WindowCreateInfo() {
                X = 100,
                Y = 100,
                WindowWidth = 1024,
                WindowHeight = 600,
                WindowTitle = "Veldrid Tutorial"
            };
            this._window = VeldridStartup.CreateWindow(ref windowCI);

            this._viewport = new Viewport(this.width, this.height);
            this._window.Resized += () => {
                this._viewport.Resize(this.width, this.height);
            };
            this._input = new InputInstance(this._window);
            Input.SetInstance(this._input);
        }

        public void GoFullscreenMode()
        {
            this._window.WindowState = Veldrid.WindowState.BorderlessFullScreen;
        }

        public void GoWindowMode()
        {
            this._window.WindowState = Veldrid.WindowState.Normal;
        }

        public uint width  => (uint) this._window.Width;
        public uint height => (uint) this._window.Height;

        public void RunMainLoop(System.Action<float> mainLoop)
        {
            Stopwatch sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            while (this._window.Exists)
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                float deltaSeconds = (float)(newElapsed - previousElapsed);
                previousElapsed = newElapsed;

                mainLoop.Invoke(deltaSeconds);

                if (Input.GetKeyDown(Veldrid.Key.F4)) {
                    this.GoFullscreenMode();
                }

                this._input.UpdateFrameInput();
            }
        }

        public void Close() => this._window.Close();

        public string title { get => this._window.Title; set => this._window.Title = value; }



        public Sdl2Window nativeWindow => this._window;

        public void PumpEvents() => this._window.PumpEvents();
    }
}