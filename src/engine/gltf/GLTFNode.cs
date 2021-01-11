using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.GLTF
{
    public class GLTFNode
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public GPUMesh? mesh = null;
        public Skin? skin = null;
        public SurfaceMaterial? material = null;
        public GLTFNode? parent = null;

        private List<GLTFNode> _children = new List<GLTFNode>();
        public IReadOnlyList<GLTFNode> children => this._children;
        
        public GLTFNode(string name)
        {
            this.name = name;
        }

        public void Add(GLTFNode node)
        {
            node.parent = this;
            this._children.Add(node);
        } 
    
        public string GetFullPathName()
        {
            if (this.parent != null) {
                return this.parent.GetFullPathName() + "/" + this.name;
            } 
            return "/" + this.name;
        }
    }
}