using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;

namespace LifeSim
{
    public class PickingList
    {
        private enum TargetType
        {
            None,
            Renderable,
        }

        private readonly List<Renderable3D> _list = new List<Renderable3D>();

        public PickingList()
        {

        }


        public void Add(Renderable3D renderable)
        {
            renderable.pickingID = (uint) (this._list.Count + 1); 
            this._list.Add(renderable); 
        }

        public void AddAllRecursive(Node3D container)
        {
            if (container is Renderable3D renderable) {
                this.Add(renderable);
            }

            foreach (var node in container.children) {
                this.AddAllRecursive(node);    
            }
        }

        public Renderable3D? FindByIndex(uint index)
        {
            index--;
            if (this._list.Count < index) {
                return null;
            }
            return this._list[(int) index];
        }

    }
}