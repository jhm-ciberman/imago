using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class GLTFLoader
    {
        public Mesh Load(Renderer renderer)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load("res/BoxTextured.glb");

            var meshes = model.LogicalMeshes;
            var primitives = meshes[0].Primitives;
            var primitive = primitives[0];

            var indicesList = primitive.GetIndices();
            ushort[] indices = new ushort[indicesList.Count];
            for(var i = 0; i < indicesList.Count; i++) {
                indices[i] = (ushort) indicesList[i];
            }

            foreach (var s in primitive.VertexAccessors.Keys) {
                System.Console.WriteLine(s);
            }

            var verticesList = primitive.GetVertices("POSITION").AsVector3Array();
            var uvList = primitive.GetVertices("TEXCOORD_0").AsVector2Array();
            Mesh.VertData[] vertices = new Mesh.VertData[verticesList.Count];
            for(var i = 0; i < verticesList.Count; i++) {
                var pos = verticesList[i];
                var uv = uvList[i];
                vertices[i] = new Mesh.VertData(pos, uv);
            }

            System.Console.WriteLine("Meshes count: " + meshes.Count);
            System.Console.WriteLine("Primitives count: " + primitives.Count);
            System.Console.WriteLine("Primitive type: " + primitive.DrawPrimitiveType);
            return renderer.MakeMesh(vertices, indices);
        }
    }

}