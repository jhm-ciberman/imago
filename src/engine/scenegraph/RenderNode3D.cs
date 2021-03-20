using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.SceneGraph
{
    public class RenderNode3D : Node3D, IRenderable
    {
        public System.UInt32 pickingID = 0;
        
        private GPUMesh? _mesh;
        public GPUMesh? mesh
        {
            get => this._mesh;
            set
            {
                this._mesh = value;
                this._OnTransformDirty();
            }
        }
        
        private SurfaceMaterial? _material;
        public SurfaceMaterial? material
        {
            get => this._material;
            set
            {
                this._material = value;
                this.resourceLayout = value?.GetObjectResourceLayout(this);
            }
        }
        
        public Vector4 albedoColor = new Vector4(1f, 1f, 1f, 0f);

        public Vector4 textureST = new Vector4(1f, 1f, 0f, 0f);

        public ResourceLayout? resourceLayout { get; private set; }

        public VertexLayoutKind vertexLayoutKind => this.mesh != null ? this.mesh.vertexLayoutKind : VertexLayoutKind.Regular;

        private BoundingBox _boundingBox;

        private Vector3 _worldSpaceCenter;
        public Vector3 worldSpaceCenter => this._worldSpaceCenter;

        public RenderNode3D()
        {
            this.mesh = null;
            this.material = null;
        }

        public RenderNode3D(GPUMesh mesh, SurfaceMaterial material)
        {
            this.mesh = mesh;
            this.material = material;
            this.resourceLayout = material.GetObjectResourceLayout(this);
        }

        public RenderNode3D(GPUMesh mesh)
        {
            this._mesh = mesh;
            this.material = null;
        }

        public virtual string[] GetShaderKeywords()
        {
            return System.Array.Empty<string>();
        }

        

        public bool Cull(ref BoundingFrustum frustum)
        {
            return (frustum.Contains(ref this._boundingBox) != ContainmentType.Disjoint);
        }

        protected override void _AfterMatrixUpdate()
        {
            if (this.mesh == null) return;
            this._boundingBox = BoundingBox.Transform(this.mesh.boundingBox, this.worldMatrix);
            this._worldSpaceCenter = this._boundingBox.GetCenter();
        }
    }
}