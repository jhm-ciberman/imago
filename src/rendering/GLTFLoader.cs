using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class GLTFLoader
    {
        private GPURenderer _renderer;
        public GLTFLoader(GPURenderer renderer)
        {
            this._renderer = renderer;
        }
        
        public GPUMesh Load(string path)
        {
            var model = SharpGLTF.Schema2.ModelRoot.Load(path);
            //var model = glTFLoader.Interface.LoadModel(path);

            var meshes = model.LogicalMeshes;
            
            var primitives = meshes[0].Primitives;
            
            var primitive = primitives[0];
            System.Console.WriteLine("Loading file " + path);
            System.Console.WriteLine("Meshes count: " + meshes.Count);
            System.Console.WriteLine("Primitives count: " + primitives.Count);
            System.Console.WriteLine("Primitive type: " + primitive.DrawPrimitiveType);

            var indicesList = primitive.IndexAccessor.AsIndicesArray();
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

            if (positionAccessor == null) throw new System.Exception("No position in mesh");
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
                Debug.Assert(jointsAccessor != null);
                Debug.Assert(weightsAccessor != null);
                var joints = jointsAccessor.AsVector4Array();
                var weights = weightsAccessor.AsVector4Array();
                var invBindMats = skin.GetInverseBindMatricesAccessor().AsMatrix4x4Array();

                var mesh = new LifeSim.SkinnedMeshData(positions, indices, texCoords, normals, joints, weights, invBindMats);
                var gpuMesh = this._renderer.MakeMesh(mesh);
                return gpuMesh;
            } else {
                var mesh = new LifeSim.MeshData(positions, indices, texCoords, normals);
                var gpuMesh = this._renderer.MakeMesh(mesh);
                return gpuMesh;
            }

            

            //var animations = model.LogicalAnimations;
            //if (animations.Count > 0) {
            //    var animation = animations[0];
            //    model.DefaultScene.
            //    animation.FindRotationSampler();
            //}



        }
    }
}