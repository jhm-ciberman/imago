using System;
using System.IO;
using System.Numerics;
using Imago;
using Imago.Assets.Animations;
using Imago.Assets.Gltf;
using Imago.Assets.Materials;
using Imago.Assets.Meshes;
using Imago.Assets.Textures;
using Imago.Rendering;
using Imago.SceneGraph;
using Imago.SceneGraph.Cameras;
using Imago.SceneGraph.Nodes;
using Imago.SceneGraph.Prefabs;
using Imago.Support.Drawing;

namespace BasicAnimation;

internal static class Program
{
    private static void Main()
    {
        using var app = new BasicAnimationApp();
        app.Run();
    }
}

internal sealed class BasicAnimationApp : Application
{
    private static readonly Vector3 _cameraTarget = new Vector3(0f, 0.5f, 0f);

    private const float _cameraRadius = 2.5f;

    private const float _cameraHeight = 1.3f;

    private const float _cameraSpeed = 0.4f;

    private readonly AnimationPlayer[] _players;

    private readonly PerspectiveCamera _camera;

    private float _cameraAngle;

    public BasicAnimationApp()
    {
        var scene = new Scene3D
        {
            Root = new Node3D { Name = "Root" },
        };

        scene.Environment.MainLight.Direction = new Vector3(-1f, 2f, -1f);
        scene.Environment.MainLight.ShadowMap.MaximumShadowsDistance = 15f;
        scene.Environment.MainLight.ShadowMap.SplitLambda = 1f;
        scene.Environment.MainLight.ShadowMap.Size = 4096;

        this._camera = new PerspectiveCamera();
        this.UpdateCamera();
        scene.Camera = this._camera;

        var floorMaterial = Renderer.Instance.MakeMaterial<StandardMaterial>();
        floorMaterial.Texture = Texture.White;
        floorMaterial.PixelPerfectShadows = false;

        var floorMesh = new PrimitiveMeshBuilder()
            .AddPlane(20f, 20f)
            .BuildMesh();

        var floor = new RenderNode3D(floorMesh)
        {
            Name = "Floor",
            Material = floorMaterial,
            AlbedoColor = new ColorF(0.50f, 0.55f, 0.45f, 1f),
        };
        scene.Root.AddChild(floor);

        var assetsDir = Path.Combine(AppContext.BaseDirectory, "Assets");

        var foxTexture = new ImageTexture(Path.Combine(assetsDir, "fox.png"));
        var foxMaterial = Renderer.Instance.MakeMaterial<StandardMaterial>();
        foxMaterial.Texture = foxTexture;
        foxMaterial.PixelPerfectShadows = false;

        var foxGltf = GltfLoader.LoadFile(Path.Combine(assetsDir, "fox.gltf"));
        var foxPrefab = foxGltf.Scene;
        var foxAnimations = foxGltf.Animations;

        this._players = new AnimationPlayer[3];
        for (int i = 0; i < 3; i++)
        {
            var foxNode = foxPrefab.Instantiate(foxMaterial);
            foxNode.Name = $"Fox_{i}";
            foxNode.Scale = new Vector3(0.01f);
            foxNode.Position = new Vector3(-0.8f + 0.8f * i, 0f, 0f);
            scene.Root.AddChild(foxNode);

            var player = new AnimationPlayer(foxNode);
            player.Play(foxAnimations[i % foxAnimations.Count]);
            this._players[i] = player;
        }

        this.Stage.Scene3D = scene;
    }

    protected override void OnUpdate(float deltaTime)
    {
        foreach (var player in this._players)
        {
            player.Update(deltaTime);
        }

        this._cameraAngle += deltaTime * _cameraSpeed;
        this.UpdateCamera();
    }

    private void UpdateCamera()
    {
        var x = MathF.Cos(this._cameraAngle) * _cameraRadius;
        var z = MathF.Sin(this._cameraAngle) * _cameraRadius;
        this._camera.Position = _cameraTarget + new Vector3(x, _cameraHeight, z);
        this._camera.LookAt(_cameraTarget);
    }

    protected override void UpdateWindowTitle()
    {
        if (!this.Window.Exists)
        {
            return;
        }

        this.Window.Title = $"Imago BasicAnimation ({this.Renderer.BackendType}) - {this.Ticker.FramesPerSecond:0.0} FPS";
    }
}
