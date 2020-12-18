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
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Veldrid Tutorial"
            };
            this._window = VeldridStartup.CreateWindow(ref windowCI);
        }

        public string title { get => this._window.Title; set => this._window.Title = value; }

        public Sdl2Window window => this._window;

        public Sdl2Window nativeWindow => this._window;

        public bool exists => this._window.Exists;

        public void PumpEvents() => this._window.PumpEvents();
    }
}