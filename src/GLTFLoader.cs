using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class GLTFLoader
    {
        public LifeSim.Mesh Load(string path)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(path);

            var meshes = model.LogicalMeshes;
            
            var primitives = meshes[0].Primitives;
            
            var primitive = primitives[0];
            System.Console.WriteLine("Loading file " + path);
            System.Console.WriteLine("Meshes count: " + meshes.Count);
            System.Console.WriteLine("Primitives count: " + primitives.Count);
            System.Console.WriteLine("Primitive type: " + primitive.DrawPrimitiveType);

            var indicesList = primitive.GetIndices();
            ushort[] indices = new ushort[indicesList.Count];
            for(var i = 0; i < indicesList.Count; i++) {
                indices[i] = (ushort) indicesList[i];
            }
            var accessors = primitive.VertexAccessors;
            foreach (var s in accessors.Keys) {
                System.Console.WriteLine(s);
            }

            var positionAccessor = accessors.GetValueOrDefault("POSITION");
            var texCoordAccessor = accessors.GetValueOrDefault("TEXCOORD_0");
            var normalAccessor = accessors.GetValueOrDefault("NORMAL");


            var positions = positionAccessor.AsVector3Array();
            var texCoords = texCoordAccessor?.AsVector2Array();
            var normals = normalAccessor?.AsVector3Array();

            var skins = model.LogicalSkins;
            if (skins.Count > 0) {
                var skin = skins[0];
                System.Console.WriteLine("Skin: " + skin.Name);
                System.Console.WriteLine("Skin Joints: " + skin.JointsCount);
                var jointsAccessor = accessors.GetValueOrDefault("JOINTS_0");
                var weightsAccessor = accessors.GetValueOrDefault("WEIGHTS_0");
                var joints = jointsAccessor?.AsVector4Array();
                var weights = weightsAccessor?.AsVector4Array();
                var invBindMats = skin.GetInverseBindMatricesAccessor().AsMatrix4x4Array();

                return new LifeSim.SkinnedMesh(positions, indices, texCoords, normals, joints, weights, invBindMats);
            }

            //var animations = model.LogicalAnimations;
            //if (animations.Count > 0) {
            //    var animation = animations[0];
            //}


            return new LifeSim.Mesh(positions, indices, texCoords, normals);
        }
    }
}