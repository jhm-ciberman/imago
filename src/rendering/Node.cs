namespace LifeSim.Rendering {
    public class Node3D
    {
        public string? name;

        private Transform _transform = new Transform();
        public Transform transform => this._transform;

        public GPUMesh? _mesh = null;

        public Node3D(string name)
        {
            this.name = name;
        }

        public Node3D()
        {
            //
        }


    }
}