using System.Diagnostics;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim
{
    public class Window
    {
        private Sdl2Window _window;

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
            this._window.Resized += () => this.onResize?.Invoke(this);
        }

        public System.Action<Window> onResize;

        public void GoFullscreenMode()
        {
            
            this._window.WindowState = Veldrid.WindowState.FullScreen;
            this.onResize?.Invoke(this);
        }

        public void GoWindowMode()
        {
            this._window.WindowState = Veldrid.WindowState.Normal;
            this.onResize?.Invoke(this);
        }

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

                var inputSnapshot = this._window.PumpEvents(); //For next frame
                Input.UpdateFrameInput(inputSnapshot);
            }
        }

        public void Close() => this._window.Close();

        public string title { get => this._window.Title; set => this._window.Title = value; }

        public uint width => (uint) this._window.Width;
        public uint height => (uint) this._window.Height;

        public Sdl2Window nativeWindow => this._window;

        public void PumpEvents() => this._window.PumpEvents();
    }
}