using System;
using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering 
{
    public class Node3D
    {
        public Transform transform;

        public Node3D()
        {
            this.transform = new Transform(this);
        }

        public void Add(Node3D node)
        {
            this.transform.Add(node.transform);
        }
    }
}