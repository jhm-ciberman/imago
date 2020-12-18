using System.Numerics;
using LifeSim.Rendering;
using Veldrid;

namespace LifeSim
{
    public static class Cube
    {
        
        public static Mesh.VertData[] GetVertices()
        {
            Mesh.VertData[] vertices = new Mesh.VertData[]
            {
                // Top
                new Mesh.VertData(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), RgbaFloat.Orange),
                new Mesh.VertData(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), RgbaFloat.Orange),
                new Mesh.VertData(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1), RgbaFloat.Orange),
                new Mesh.VertData(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1), RgbaFloat.Orange),
                // Bottom                                                             
                new Mesh.VertData(new Vector3(-0.5f,-0.5f, +0.5f),  new Vector2(0, 0), RgbaFloat.Green),
                new Mesh.VertData(new Vector3(+0.5f,-0.5f, +0.5f),  new Vector2(1, 0), RgbaFloat.Green),
                new Mesh.VertData(new Vector3(+0.5f,-0.5f, -0.5f),  new Vector2(1, 1), RgbaFloat.Green),
                new Mesh.VertData(new Vector3(-0.5f,-0.5f, -0.5f),  new Vector2(0, 1), RgbaFloat.Green),
                // Left                                                               
                new Mesh.VertData(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), RgbaFloat.Red),
                new Mesh.VertData(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0), RgbaFloat.Red),
                new Mesh.VertData(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1), RgbaFloat.Red),
                new Mesh.VertData(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1), RgbaFloat.Red),
                // Right                                                              
                new Mesh.VertData(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0), RgbaFloat.Pink),
                new Mesh.VertData(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), RgbaFloat.Pink),
                new Mesh.VertData(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1), RgbaFloat.Pink),
                new Mesh.VertData(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1), RgbaFloat.Pink),
                // Back                                                               
                new Mesh.VertData(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0), RgbaFloat.Yellow),
                new Mesh.VertData(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0), RgbaFloat.Yellow),
                new Mesh.VertData(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1), RgbaFloat.Yellow),
                new Mesh.VertData(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1), RgbaFloat.Yellow),
                // Front                                                              
                new Mesh.VertData(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0), RgbaFloat.Cyan),
                new Mesh.VertData(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0), RgbaFloat.Cyan),
                new Mesh.VertData(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1), RgbaFloat.Cyan),
                new Mesh.VertData(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1), RgbaFloat.Cyan),
            };

            return vertices;
        }

        public static ushort[] GetIndices()
        {
            ushort[] indices =
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };

            return indices;
        }
    }
}