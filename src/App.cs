using LifeSim.Rendering;
using System.Numerics;
using System.IO;

namespace LifeSim
{
    class App
    {
        static void Main()
        {
            new App();
        }

        private Camera _camera;
        private Window _window;
        private Renderer _renderer;
        private float _cameraAngle = 0f;
        private float _cameraDist = 3f;
        private float _cameraRotationSpeed = 0.2f;

        private Shader _shader;

        private Texture _texture;
        private Texture _textureDuck;
        private Texture _textureBrick;

        private Material _material;
        private Material _materialDuck;
        private Material _materialBrick;

        private Mesh _cubeMesh;
        private Mesh _duckMesh;

        private Scene _scene;

        private Renderable _cube;

        public App()
        {
            this._window = new Window();
            this._renderer = new Renderer(this._window);


            this._window.nativeWindow.Resized += this.OnResize;


            this._camera = this._renderer.MakeCamera();
            this._camera.lookAt = Vector3.Zero;

            string vertexCode = File.ReadAllText("res/vertex.vert");
            string fragmentCode = File.ReadAllText("res/fragment.frag");
            this._shader = this._renderer.MakeShader(vertexCode, fragmentCode);

            this._texture = this._renderer.MakeTexture("res/uvs.jpg");
            this._textureDuck = this._renderer.MakeTexture("res/DuckCM.png");
            this._textureBrick = this._renderer.MakeTexture("res/wooden_planks.png");
            
            this._material = this._renderer.MakeMaterial(this._shader, this._texture);
            this._materialDuck = this._renderer.MakeMaterial(this._shader, this._textureDuck);
            this._materialBrick = this._renderer.MakeMaterial(this._shader, this._textureBrick);

            //this._cubeMesh = this._renderer.MakeMesh(Cube.GetVertices(), Cube.GetIndices());

            var loader = new GLTFLoader();
            this._cubeMesh = loader.Load(this._renderer, "res/BoxTextured.glb");
            this._duckMesh = loader.Load(this._renderer, "res/Duck.glb");

            this._cube = new Renderable(this._cubeMesh, this._material);
            this._scene = new Scene(this._camera);
            this._scene.Add(this._cube);

            var floor = new Renderable(this._cubeMesh, this._materialBrick);
            floor.transform.Scale = new Vector3(5f, 0.2f, 5f);
            floor.transform.Position = new Vector3(0f, -0.5f, 0f);
            this._scene.Add(floor);

            var duck = new Renderable(this._duckMesh, this._materialBrick);
            float scale = .006f;
            duck.transform.Scale = new Vector3(scale);
            duck.transform.Position = new Vector3(1f, -0.3f, 1f);
            this._scene.Add(duck);

            var duck2 = new Renderable(this._duckMesh, this._materialDuck);
            duck2.transform.Scale = new Vector3(scale);
            duck2.transform.Position = new Vector3(-1f, -0.3f, 1f);
            this._scene.Add(duck2);


            this.OnResize();
            this._window.RunMainLoop(this.Update);
            this.Dispose();
        }

        public void Dispose()
        {
            this._texture.Dispose();
            this._shader.Dispose();
            this._material.Dispose();
            this._cubeMesh.Dispose();
            this._duckMesh.Dispose();
        }

        public void OnResize() 
        {
            this._camera.aspect = (float) this._window.width / (float) this._window.height;
            this._renderer.Resize(this._window.width, this._window.height);
        }

        protected void Update(float deltaTime)
        {
            this._cameraAngle += this._cameraRotationSpeed * deltaTime;
            var x = System.MathF.Cos(this._cameraAngle) * this._cameraDist;
            var z = System.MathF.Sin(this._cameraAngle) * this._cameraDist;
            this._camera.position = new Vector3(x, 1.5f, z);

            var fps = (1f / deltaTime).ToString("0.00");
            var dt = deltaTime.ToString("0.00");
            this._window.title = "Hello world" + " (" + this._renderer.backendType.ToString() + ") deltaTime = " + dt + " FPS = " + fps;

            if (Input.GetKey(Veldrid.Key.W) || Input.GetKey(Veldrid.Key.Up)) {
                this._cameraDist -= 2f * deltaTime;
            }
            if (Input.GetKey(Veldrid.Key.S) || Input.GetKey(Veldrid.Key.Down)) {
                this._cameraDist += 2f * deltaTime;
            }
            if (Input.GetKey(Veldrid.Key.D) || Input.GetKey(Veldrid.Key.Right)) {
                this._cameraRotationSpeed += 2f * deltaTime;
            }
            if (Input.GetKey(Veldrid.Key.A) || Input.GetKey(Veldrid.Key.Left)) {
                this._cameraRotationSpeed -= 2f * deltaTime;
            }
            if (Input.GetKey(Veldrid.Key.Q)) {
                this._cube.transform.Scale += Vector3.One * deltaTime;
            }
            if (Input.GetKey(Veldrid.Key.E)) {
                this._cube.transform.Scale -= Vector3.One * deltaTime;
            }

            this._renderer.Render(this._scene);
        }
    }

}
