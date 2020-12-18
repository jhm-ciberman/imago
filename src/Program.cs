using Veldrid.ImageSharp;
using LifeSim.Rendering;
using System.Numerics;

namespace LifeSim
{
    class Program
    {
        private const string vertexCode = @"
#version 450

layout(set = 0, binding = 0) uniform ProjectionBuffer {
    mat4 projection;
};

layout(set = 0, binding = 1) uniform ViewBuffer {
    mat4 view;
};

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texCoords;
layout(location = 2) in vec4 color;

layout(location = 0) out vec2 fsin_texCoords;
layout(location = 1) out vec4 fsin_color;

void main()
{
    gl_Position = projection * view * vec4(position, 1);
    fsin_color = color;
    fsin_texCoords = texCoords;
}";

        private const string fragmentCode = @"
#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 1) in vec4 fsin_color;
layout(set = 1, binding = 0) uniform texture2D surfaceTexture;
layout(set = 1, binding = 1) uniform sampler surfaceSampler;

layout(location = 0) out vec4 fsout_color;

void main()
{
    fsout_color = texture(sampler2D(surfaceTexture, surfaceSampler), fsin_texCoords);
}";

        static void Main()
        {
            Window window = new Window();
            Renderer renderer = new Renderer(window);
            window.title = "Hello world" + " (" + renderer.backendType.ToString() + ")";

            var texture = renderer.MakeTexture("res/uvs.jpg");
            var shader = renderer.MakeShader(vertexCode, fragmentCode);
            Material material = renderer.MakeMaterial(shader, texture);
            Mesh mesh = renderer.MakeMesh(Cube.GetVertices(), Cube.GetIndices());

            Camera camera = renderer.MakeCamera();

            float cameraAngle = 0f;
            
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
                var dist = 1.5f;
                camera.position = new Vector3(System.MathF.Cos(cameraAngle) * dist, 1.5f, System.MathF.Sin(cameraAngle) * dist);
                renderer.DrawBegin();
                renderer.DrawMesh(mesh, material, camera);
                renderer.DrawEnd();

                window.PumpEvents(); //For next frame
            }


            shader.Dispose();
            mesh.Dispose();
            renderer.Dispose();
        }
    }

}
