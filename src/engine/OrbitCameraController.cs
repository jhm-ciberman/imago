using System.Numerics;
using LifeSim.Engine;
using LifeSim.Engine.SceneGraph;

namespace LifeSim
{
    internal class OrbitCameraController
    {
        public float direction = 0f;
        public float distance = 3f;
        public float rotationSpeed = 0.2f;
        public Vector3 target = Vector3.Zero;
        public Camera3D camera;

        public OrbitCameraController(Camera3D camera)
        {
            this.camera = camera;
        }

        public void UpdateKeyboard(float deltaTime)
        {
            if (Input.GetKey(Veldrid.Key.W) || Input.GetKey(Veldrid.Key.Up)) {
                this.distance -= 2f * deltaTime;
            }
            if (Input.GetKey(Veldrid.Key.S) || Input.GetKey(Veldrid.Key.Down)) {
                this.distance += 2f * deltaTime;
            }
            if (Input.GetKey(Veldrid.Key.D) || Input.GetKey(Veldrid.Key.Right)) {
                this.rotationSpeed += 2f * deltaTime;
            }
            if (Input.GetKey(Veldrid.Key.A) || Input.GetKey(Veldrid.Key.Left)) {
                this.rotationSpeed -= 2f * deltaTime;
            }
        }

        public void Update(float deltaTime)
        {
            this.direction += this.rotationSpeed * deltaTime;
            var x = System.MathF.Cos(this.direction) * this.distance;
            var z = System.MathF.Sin(this.direction) * this.distance;
            this.camera.position = this.target + new Vector3(x, 1.5f, z);

            this.camera.LookAt(this.target);
        }
    }
}