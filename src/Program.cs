using BenchmarkDotNet.Running;
using LifeSim.Demo;
using LifeSim.Engine;

namespace LifeSim
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            System.Func<App, IStage?> stageFactory = Program.MakeGameStage;
            foreach (var arg in args) {
                if (arg == "--dev") {
                    stageFactory = Program.MakeDevStage;
                } else if (arg == "--demo") {
                    stageFactory = Program.MakeDemoStage;
                }
            }
            var app = new App(args);
            var stage = stageFactory(app);
            if (stage != null) {
                app.Run(stage);
            }
        }

        public static IStage MakeGameStage(App app)
        {
            return new LoadingRenderStage(app);
        }

        public static IStage MakeDevStage(App app)
        {
            return new Editor.AssetPreviewStage(app);
        }

        public static IStage MakeDemoStage(App app)
        {
            return new DemoStage(app);
        }
    }
}