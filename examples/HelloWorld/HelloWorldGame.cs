using System;
using System.Numerics;
using Imago;
using Imago.Graphics3D;
using Imago.Graphics3D.Materials;
using Imago.Graphics3D.Meshes;
using Imago.Graphics3D.SceneGraph;
using Imago.Graphics3D.SceneGraph.Cameras;
using Imago.Graphics3D.SceneGraph.Nodes;
using Imago.Support.Drawing;
using Imago.Textures;

namespace HelloWorld;

internal sealed class HelloWorldGame : Game, IDisposable
{
    private static void Main()
    {
        Application.Configure<HelloWorldGame>()
            .WithTitle("Imago HelloWorld")
            .WithModules([new Graphics3DModule()])
            .Run();
    }

    private readonly RenderNode3D _cube;

    private float _time;

    public HelloWorldGame()
    {
        var scene = new Scene3DLayer
        {
            Root = new Node3D { Name = "Root" },
        };

        var camera = new PerspectiveCamera
        {
            Position = new Vector3(4f, 4f, 4f),
        };
        camera.LookAt(new Vector3(0f, 0.5f, 0f));
        scene.Camera = camera;

        var material = Material.Create<StandardMaterial>();
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

        this.Application.Stage.AddLayer(scene);
    }

    protected override void Update(float deltaTime)
    {
        this._time += deltaTime;
        this._cube.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this._time);
    }

    public void Dispose()
    {
        this._cube.Dispose();
    }
}
