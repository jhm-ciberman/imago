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
        private float _cameraRotationSpeed = 1f;

        private Texture _texture;
        private Shader _shader;
        private Material _material;
        private Mesh _mesh;
        private Scene _scene;

        public App()
        {
            this._window = new Window();
            this._renderer = new Renderer(this._window);


            this._window.nativeWindow.Resized += this.OnResize;


            this._camera = this._renderer.MakeCamera();
            this._camera.lookAt = Vector3.Zero;

            string vertexCode = File.ReadAllText("res/vertex.vert");
            string fragmentCode = File.ReadAllText("res/fragment.frag");
            this._texture = this._renderer.MakeTexture("res/uvs.jpg");
            this._shader = this._renderer.MakeShader(vertexCode, fragmentCode);
            this._material = this._renderer.MakeMaterial(this._shader, this._texture);
            this._mesh = this._renderer.MakeMesh(Cube.GetVertices(), Cube.GetIndices());

            var renderable = new Renderable(this._mesh, this._material);
            this._scene = new Scene(this._camera);
            this._scene.Add(renderable);

            this.OnResize();
            this._window.RunMainLoop(this.Update);
            this.Dispose();
        }

        public void Dispose()
        {
            this._texture.Dispose();
            this._shader.Dispose();
            this._material.Dispose();
            this._mesh.Dispose();
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

            this._renderer.Render(this._scene);
        }
    }

}
