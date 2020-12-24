using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using LifeSim.Rendering;
using glTFLoader;

namespace LifeSim.GLTF
{
    public class GLTFLoader
    {
        private string _path;
        private GPURenderer _renderer;
        private GLTFModel _model;
        
        public GLTFLoader(GPURenderer renderer, string path)
        {
            this._path = path;
            this._renderer = renderer;
            this._model = new GLTFModel(path);
        }

        //public int meshesCount => this._model.Meshes.Length;

        //public Scene3D LoadScene(Material? defaultMaterial, int sceneIndex = 0)
        //{
        //    return this._Scene(this._model.Scenes[sceneIndex], defaultMaterial);
        //}

        //public SharpGLTF.Runtime.SceneTemplate LoadSceneTemplate(int sceneIndex = 0)
        //{
        //    return SharpGLTF.Runtime.SceneTemplate.Create(this._model.Scenes[sceneIndex], true);
        //}

        //public GPUMesh LoadMesh(int meshIndex = 0)
        //{
        //    return this._Primitive(this._model.Meshes[meshIndex].Primitives[0]);
        //}

        //public GPUMesh LoadMesh(string name)
        //{
        //    foreach (var mesh in this._model.Meshes) {
        //        if (mesh.Name == name) {
        //            return this._Primitive(mesh.Primitives[0]);
        //        }
        //    }
        //    throw new System.Exception("Mesh not found"); // Todo: better error
        //}

        //public Animation LoadAnimation(int animationIndex = 0)
        //{
        //    return this._Animation(this._model.LogicalAnimations[animationIndex]);
        //}

        //public Skin LoadSkin(int skinIndex, Material defaultMaterial)
        //{
        //    return this._Skin(this._model.LogicalSkins[skinIndex], defaultMaterial);
        //}

        ///private Animation _Animation(SharpGLTF.Schema2.Animation animation)
        ///{
        ///    var anim = new Animation(animation.Name);
        ///    //animation.FindRotationSampler(node)
        ///    
        ///    return anim;
        ///}

        //private Scene3D _Scene(SharpGLTF.Schema2.Scene scene, Material? defaultMaterial)
        //{
        //    var scene3D = new Scene3D();
        //    foreach (var node in scene.VisualChildren) {
        //        scene3D.Add(this._Node(node, defaultMaterial));
        //    }
        //    return scene3D;
        //}

        /*
        private Node3D _Node(SharpGLTF.Schema2.Node node, Material? defaultMaterial)
        {
            Node3D node3D;
            if (node.Mesh != null && defaultMaterial != null) {
                node3D = this._Mesh(node.Mesh, defaultMaterial);
            } else {
                node3D = new Node3D();
            }

            var t = node.LocalTransform;
            node3D.rotation = t.Rotation;
            node3D.scale    = t.Scale;
            node3D.position = t.Translation;

            foreach (var child in node.VisualChildren)
            {
                var child3D = this._Node(child, defaultMaterial);
                node3D.Add(child3D);
            }
            return node3D;
        }
        */

        public GPUMesh LoadMesh(int meshIndex = 0)
        {
            return this._renderer.MakeMesh(this._model.GetPrimitive(meshIndex).MakeMesh());
        }


        /*
        private Skin _Skin(SharpGLTF.Schema2.Skin skin, Material defaultMaterial)
        {
            Node3D root = this._Node(skin.Skeleton ?? skin.GetJoint(0).Joint, defaultMaterial);
            var invBindMats = skin.GetInverseBindMatricesAccessor().AsMatrix4x4Array();
            return new Skin(root, invBindMats);
        }
        */
    }
}