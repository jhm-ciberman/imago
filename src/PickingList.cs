using System.Collections.Generic;
using LifeSim.SceneGraph;

namespace LifeSim
{
    public class PickingList
    {
        enum TargetType
        {
            None,
            Renderable,
        }

        struct PickTarget
        {
            public TargetType type;
            public Renderable3D target;
        }

        private List<Renderable3D> _list = new List<Renderable3D>();

        public PickingList()
        {

        }


        public void Add(Renderable3D renderable)
        {
            renderable.pickingID = (uint) (this._list.Count + 1); 
            this._list.Add(renderable); 
            
            /*new PickTarget {
                type = TargetType.Renderable,
                target = renderable,
            });*/
        }

        public void AddAllRecursive(Container3D container)
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