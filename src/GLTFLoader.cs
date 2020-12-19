using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class GLTFLoader
    {
        public MeshData Load(string path)
        {

            var model = SharpGLTF.Schema2.ModelRoot.Load(path);

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
            var normalList = primitive.GetVertices("NORMAL");

            System.Console.WriteLine("Loading file " + path);
            System.Console.WriteLine("Meshes count: " + meshes.Count);
            System.Console.WriteLine("Primitives count: " + primitives.Count);
            System.Console.WriteLine("Primitive type: " + primitive.DrawPrimitiveType);

            if (normalList != null) {
                return new MeshData(verticesList, uvList, indices, normalList.AsVector3Array());
            } else {
                return new MeshData(verticesList, uvList, indices);
            }
        }


    }

}