using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class GLTFLoader
    {
        public Mesh Load(Renderer renderer, string path)
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
            Vector3[] normals = this._ComputeNormals(verticesList, indices);

            Mesh.VertData[] vertices = new Mesh.VertData[verticesList.Count];
            for(var i = 0; i < verticesList.Count; i++) {
                vertices[i] = new Mesh.VertData(verticesList[i],  normals[i], uvList[i]);
            }

            System.Console.WriteLine("Meshes count: " + meshes.Count);
            System.Console.WriteLine("Primitives count: " + primitives.Count);
            System.Console.WriteLine("Primitive type: " + primitive.DrawPrimitiveType);
            return renderer.MakeMesh(vertices, indices);
        }

        private Vector3[] _ComputeNormals(IList<Vector3> positions, ushort[] indices)
        {
            Vector3[] normals = new Vector3[positions.Count];

            for (var i = 0; i < indices.Length; i += 3) {
                var index1 = indices[i + 0];
                var index2 = indices[i + 1];
                var index3 = indices[i + 2];

                var p1 = positions[index1];
                var p2 = positions[index2];
                var p3 = positions[index3];

                var normal = Vector3.Cross((p3 - p2), (p1 - p2));

                normals[index1] = normal;
                normals[index2] = normal;
                normals[index3] = normal;
            }

            return normals;
        }

    }

}