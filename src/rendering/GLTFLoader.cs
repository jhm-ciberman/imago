using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class GLTFLoader
    {
        private GPURenderer _renderer;
        private SharpGLTF.Schema2.ModelRoot _model;

        public GLTFLoader(GPURenderer renderer, string path)
        {
            this._renderer = renderer;
            this._model = SharpGLTF.Schema2.ModelRoot.Load(path);

            System.Console.WriteLine("Loading file " + path);
        }

        public Scene3D LoadScene(Material? defaultMaterial, int sceneIndex = 0)
        {
            return this._Scene(this._model.LogicalScenes[sceneIndex], defaultMaterial);
        }

        public GPUMesh LoadMesh(int meshIndex = 0)
        {
            return this._Primitive(this._model.LogicalMeshes[meshIndex].Primitives[0]);
        }

        public GPUMesh LoadMesh(string name)
        {
            foreach (var mesh in this._model.LogicalMeshes) {
                if (mesh.Name == name) {
                    return this._Primitive(mesh.Primitives[0]);
                }
            }
            throw new System.Exception("Mesh not found"); // Todo: better error
        }

        public Animation LoadAnimation(int animationIndex = 0)
        {
            return this._Animation(this._model.LogicalAnimations[animationIndex]);
        }

        public Skin LoadSkin(int skinIndex, Material defaultMaterial)
        {
            return this._Skin(this._model.LogicalSkins[skinIndex], defaultMaterial);
        }

        private Animation _Animation(SharpGLTF.Schema2.Animation animation)
        {
            var anim = new Animation(animation.Name);
            //animation.FindRotationSampler(node)
            
            return anim;
        }

        private Scene3D _Scene(SharpGLTF.Schema2.Scene scene, Material? defaultMaterial)
        {
            var scene3D = new Scene3D();
            foreach (var node in scene.VisualChildren) {
                scene3D.Add(this._Node(node, defaultMaterial));
            }
            return scene3D;
        }

        private Node3D _Node(SharpGLTF.Schema2.Node node, Material? defaultMaterial)
        {
            System.Console.WriteLine("Node: " + node.Name);
            Node3D node3D;
            if (node.Mesh != null && defaultMaterial != null) {
                node3D = this._Mesh(node.Mesh, defaultMaterial);
            } else {
                node3D = new Node3D();
            }

            var t = node.LocalTransform;
            node3D.transform.Rotation = t.Rotation;
            node3D.transform.Scale    = t.Scale;
            node3D.transform.Position = t.Translation;

            foreach (var child in node.VisualChildren)
            {
                var child3D = this._Node(child, defaultMaterial);
                node3D.Add(child3D);
            }
            return node3D;
        }

        private Renderable3D _Mesh(SharpGLTF.Schema2.Mesh mesh, Material defaultMaterial)
        {
            var gpuMesh = this._Primitive(mesh.Primitives[0]);
            return new Renderable3D(gpuMesh, defaultMaterial);
        }

        private Skin _Skin(SharpGLTF.Schema2.Skin skin, Material defaultMaterial)
        {
            Node3D root = this._Node(skin.Skeleton ?? skin.GetJoint(0).Joint, defaultMaterial);
            var invBindMats = skin.GetInverseBindMatricesAccessor().AsMatrix4x4Array();
            return new Skin(root, invBindMats);
        }

        private GPUMesh _Primitive(SharpGLTF.Schema2.MeshPrimitive primitive)
        {
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
            var normalAccessor   = accessors.GetValueOrDefault("NORMAL");
            var jointsAccessor   = accessors.GetValueOrDefault("JOINTS_0");
            var weightsAccessor  = accessors.GetValueOrDefault("WEIGHTS_0");

            Debug.Assert(positionAccessor != null);
            var positions = positionAccessor.AsVector3Array();
            var texCoords = texCoordAccessor?.AsVector2Array();
            var normals = normalAccessor?.AsVector3Array();

            if (weightsAccessor != null && jointsAccessor != null) {
                var joints = jointsAccessor.AsVector4Array();
                var weights = weightsAccessor.AsVector4Array();

                var mesh = new LifeSim.SkinnedMeshData(positions, indices, texCoords, normals, joints, weights);
                var gpuMesh = this._renderer.MakeMesh(mesh);
                return gpuMesh;
            } else {
                var mesh = new LifeSim.MeshData(positions, indices, texCoords, normals);
                var gpuMesh = this._renderer.MakeMesh(mesh);
                return gpuMesh;
            }
        }
    }
}