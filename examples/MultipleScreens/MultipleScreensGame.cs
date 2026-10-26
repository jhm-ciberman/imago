using System.Numerics;
using Imago;
using Imago.Graphics3D;
using Imago.Graphics3D.Materials;
using Imago.Graphics3D.Meshes;
using Imago.Graphics3D.SceneGraph;
using Imago.Graphics3D.SceneGraph.Cameras;
using Imago.Graphics3D.SceneGraph.Nodes;
using Imago.Support;
using Imago.Support.Drawing;
using Imago.Textures;
using Key = NeoVeldrid.Key;

namespace MultipleScreens;

// Press Space to go to the next screen: cylinder -> cube -> sphere -> cube -> ...
internal sealed class MultipleScreensGame : Game
{
    private static void Main()
    {
        Application.Configure<MultipleScreensGame>()
            .WithTitle("Imago MultipleScreens (press Space)")
            .WithModules([new Graphics3DModule()])
            .WithScreens(screens => screens
                .Map<CubeViewModel, CubeScreen>()
                .Map<SphereViewModel, SphereScreen>()
            )
            .Run();
    }

    public MultipleScreensGame()
    {
        this.Application.Screens.Show(new CylinderScreen());
    }
}

// A screen without a view model. You create it yourself and show it with Screens.Show().
internal sealed class CylinderScreen : Screen
{
    private readonly ShapeScene _scene = new ShapeScene(
        new PrimitiveMeshBuilder().AddCylinder(0.5f, 1f).BuildMesh(),
        new ColorF(0.30f, 0.70f, 0.30f, 1f)
    );

    private float _angle;

    public CylinderScreen()
    {
        this.AddLayer(this._scene);
    }

    public override void Update(float deltaTime)
    {
        this._angle += deltaTime;
        this._scene.Spin(this._angle);

        if (Application.Instance.Input.WasKeyPressedThisFrame(Key.Space))
        {
            Application.Instance.Screens.Navigate<CubeViewModel>();
        }
    }
}

// A screen with a view model. You navigate to the view model, and Imago creates the screen mapped to it.
// The view model holds the state and the logic. The screen only shows it.
internal sealed class CubeViewModel : IUpdatable
{
    private readonly IScreenNavigator _navigator;

    public float Angle { get; private set; }

    public CubeViewModel(IScreenNavigator navigator)
    {
        this._navigator = navigator;
    }

    public void Update(float deltaTime)
    {
        this.Angle += deltaTime;

        if (Application.Instance.Input.WasKeyPressedThisFrame(Key.Space))
        {
            this._navigator.Navigate<SphereViewModel>();
        }
    }
}

internal sealed class CubeScreen : Screen
{
    private readonly CubeViewModel _vm;

    private readonly ShapeScene _scene = new ShapeScene(
        new PrimitiveMeshBuilder().AddBox(1f, 1f, 1f).BuildMesh(),
        new ColorF(0.90f, 0.40f, 0.20f, 1f)
    );

    public CubeScreen(CubeViewModel vm)
    {
        this._vm = vm;
        this.AddLayer(this._scene);
    }

    public override void Update(float deltaTime)
    {
        this._scene.Spin(this._vm.Angle);
    }
}

internal sealed class SphereViewModel : IUpdatable
{
    private readonly IScreenNavigator _navigator;

    public float Angle { get; private set; }

    public SphereViewModel(IScreenNavigator navigator)
    {
        this._navigator = navigator;
    }

    public void Update(float deltaTime)
    {
        this.Angle += deltaTime;

        if (Application.Instance.Input.WasKeyPressedThisFrame(Key.Space))
        {
            this._navigator.Navigate<CubeViewModel>();
        }
    }
}

internal sealed class SphereScreen : Screen
{
    private readonly SphereViewModel _vm;

    private readonly ShapeScene _scene = new ShapeScene(
        new PrimitiveMeshBuilder().AddSphere(0.5f).BuildMesh(),
        new ColorF(0.20f, 0.50f, 0.90f, 1f)
    );

    public SphereScreen(SphereViewModel vm)
    {
        this._vm = vm;
        this.AddLayer(this._scene);
    }

    public override void Update(float deltaTime)
    {
        this._scene.Spin(this._vm.Angle);
    }
}

// A 3D scene with one shape on a floor. Every screen in this example uses one.
internal sealed class ShapeScene : Scene3DLayer
{
    private readonly RenderNode3D _shape;

    public ShapeScene(Mesh mesh, ColorF color)
    {
        var material = Material.Create<StandardMaterial>();
        material.Texture = Texture.White;

        var camera = new PerspectiveCamera
        {
            Position = new Vector3(4f, 4f, 4f),
        };
        camera.LookAt(new Vector3(0f, 0.5f, 0f));
        this.Camera = camera;

        var root = new Node3D { Name = "Root" };
        this.Root = root;

        var floor = new RenderNode3D(new PrimitiveMeshBuilder().AddPlane(10f, 10f).BuildMesh())
        {
            Name = "Floor",
            Material = material,
            AlbedoColor = new ColorF(0.40f, 0.50f, 0.35f, 1f),
        };
        root.AddChild(floor);

        this._shape = new RenderNode3D(mesh)
        {
            Name = "Shape",
            Material = material,
            Position = new Vector3(0f, 0.5f, 0f),
            AlbedoColor = color,
        };
        root.AddChild(this._shape);
    }

    public void Spin(float angle)
    {
        this._shape.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
    }
}
