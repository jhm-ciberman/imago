using System.Collections.Generic;
using System.Numerics;
using LifeSim.Anim;
using LifeSim.Rendering;
using LifeSim.SceneGraph;

namespace LifeSim.GLTF
{
    public class GLTFNode
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public GPUMesh? mesh;
        public Skin? skin;
        public Material? material;

        private List<GLTFNode> _children = new List<GLTFNode>();
        public IReadOnlyList<GLTFNode> children => this._children;
        
        public GLTFNode(string name)
        {
            this.name = name;
        }

        public void Add(GLTFNode node)
        {
            this._children.Add(node);
        }        
    }
}