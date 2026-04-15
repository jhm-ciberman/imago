using System.Numerics;
using Imago;
using Imago.Assets.Materials;
using Imago.Assets.Meshes;
using Imago.Assets.Textures;
using Imago.Rendering;
using Imago.SceneGraph;
using Imago.SceneGraph.Cameras;
using Imago.SceneGraph.Nodes;
using Imago.Support.Drawing;

namespace HelloWorld;

internal static class Program
{
    private static void Main()
    {
        using var app = new HelloWorldApp();
        app.Run();
    }
}

internal sealed class HelloWorldApp : Application
{
    private readonly RenderNode3D _cube;

    private float _time;

    public HelloWorldApp()
    {
        var scene = new Scene3D
        {
            Root = new Node3D { Name = "Root" },
        };

        var camera = new PerspectiveCamera
        {
            Position = new Vector3(4f, 4f, 4f),
        };
        camera.LookAt(new Vector3(0f, 0.5f, 0f));
        scene.Camera = camera;

        var material = Renderer.Instance.MakeMaterial<StandardMaterial>();
        material.Texture = Texture.White;

        var floorMesh = new PrimitiveMeshBuilder()
            .AddPlane(10f, 10f)
            .BuildMesh();

        var floor = new RenderNode3D(floorMesh)
        {
            Name = "Floor",
            Material = material,
            AlbedoColor = new ColorF(0.40f, 0.50f, 0.35f, 1f),
        };
        scene.Root.AddChild(floor);

        var cubeMesh = new PrimitiveMeshBuilder()
            .AddBox(1f, 1f, 1f)
            .BuildMesh();

        this._cube = new RenderNode3D(cubeMesh)
        {
            Name = "Cube",
            Material = material,
            Position = new Vector3(0f, 0.5f, 0f),
            AlbedoColor = new ColorF(0.90f, 0.40f, 0.20f, 1f),
        };
        scene.Root.AddChild(this._cube);

        this.Stage.Scene3D = scene;
    }

    protected override void OnUpdate(float deltaTime)
    {
        this._time += deltaTime;
        this._cube.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this._time);
    }

    protected override void UpdateWindowTitle()
    {
        if (!this.Window.Exists)
        {
            return;
        }

        this.Window.Title = $"Imago HelloWorld ({this.Renderer.BackendType}) - {this.Ticker.FramesPerSecond:0.0} FPS";
    }
}
