using Veldrid.ImageSharp;
using LifeSim.Rendering;
using System.Numerics;

namespace LifeSim
{
    class Program
    {
        private const string vertexCode = @"
#version 450

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 projection;
};

layout(set = 0, binding = 1) uniform ViewBuffer
{
    mat4 view;
};

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec4 color;

//layout(location = 2) in mat4 model;
//layout(location = 2) in mat4 model;
//layout(location = 2) in mat4 model;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = projection * view * vec4(position, 1);
    fsin_Color = color;
}";

        private const string fragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

        static void Main()
        {
            Window window = new Window();
            Renderer renderer = new Renderer(window);

            var shader = renderer.MakeShader(vertexCode, fragmentCode);
            Material material = renderer.MakeMaterial(shader);
            Mesh mesh = renderer.MakeMesh(Cube.GetVertices(), Cube.GetIndices());
            Camera camera = new Camera();

            float cameraAngle = 0f;
            
            ImageSharpTexture texture = new ImageSharpTexture("res/uvs.jpg", true);
            System.Console.WriteLine(texture.Width + " " + texture.Height);

            window.window.KeyDown += (e) => {
                switch (e.Key) {
                    case Veldrid.Key.Up:
                    case Veldrid.Key.W: 
                        camera.position += new Vector3(0f, 0f, .1f); break;
                    case Veldrid.Key.Down:
                    case Veldrid.Key.S: 
                        camera.position += new Vector3(0f, 0f, -.1f);  break;
                    case Veldrid.Key.Left:
                    case Veldrid.Key.A: 
                        camera.position += new Vector3(-.1f, 0f, 0f);  break;
                    case Veldrid.Key.Right:
                    case Veldrid.Key.D: 
                        camera.position += new Vector3(.1f, 0f, 0f); break;
                }
            };
            camera.aspect = (float) window.window.Width / (float) window.window.Height;
            window.window.Resized += () => {
                camera.aspect = (float) window.window.Width / (float) window.window.Height;
            };

            camera.lookAt = Vector3.Zero;
            while (window.exists)
            {
                cameraAngle += 0.01f;
                var dist = 3f;
                camera.position = new Vector3(System.MathF.Cos(cameraAngle) * dist, 2f, System.MathF.Sin(cameraAngle) * dist);
                window.PumpEvents();
                renderer.DrawBegin();
                renderer.DrawMesh(mesh, material, camera);
                renderer.DrawEnd();
            }

            renderer.Dispose();

            shader.Dispose();
            mesh.Dispose();
        }
    }

}
