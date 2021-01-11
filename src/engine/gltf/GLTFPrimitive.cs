using System.Collections.Generic;
using System.Diagnostics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.GLTF
{
    class GLTFPrimitive
    {
        private Dictionary<string, int> _attributes;
        private GLTFAccessor? _indicesAccessor;
        private GLTFLoader _model;

        public GLTFPrimitive(GLTFLoader model, int? indices, Dictionary<string, int> attributes)
        {
            this._model = model;
            this._indicesAccessor = indices.HasValue ? this._model._GetAccessor(indices.Value) : null;
            this._attributes = attributes;
        }

        private bool _loadSkinned = true;

        public MeshData MakeMesh()
        {
            var positionAccessor = this._GetAttributeAccessor("POSITION");
            Debug.Assert(positionAccessor != null);
            var positions = positionAccessor.AsVector3Array();

            var texCoordAccessor = this._GetAttributeAccessor("TEXCOORD_0");
            var texCoords = texCoordAccessor?.AsVector2Array();

            var normalAccessor   = this._GetAttributeAccessor("NORMAL");
            var normals = normalAccessor?.AsVector3Array();
            
            var jointsAccessor   = this._GetAttributeAccessor("JOINTS_0");
            var weightsAccessor  = this._GetAttributeAccessor("WEIGHTS_0");


            var indices = this._indicesAccessor == null ? this._MakeFakeIndices(positions.Length) : this._indicesAccessor.AsIndicesArray();

            if (this._loadSkinned && weightsAccessor != null && jointsAccessor != null) {
                var joints = jointsAccessor.AsUShort4Array();
                var weights = weightsAccessor.AsVector4Array();
                return new SkinnedMeshData(positions, indices, texCoords, normals, joints, weights);
            } else {
                var mesh = new MeshData(positions, indices, texCoords, normals);
                return mesh;
            }
        }


        private GLTFAccessor? _GetAttributeAccessor(string name)
        {
            if (this._attributes.TryGetValue(name, out int attributeId)) {
                return this._model._GetAccessor(attributeId);
            }
            return null;
        }

        private ushort[] _MakeFakeIndices(int count)
        {
            var arr = new ushort[count];
            for (int i = 0; i < count; i++) {
                arr[i] = (ushort) i;
            }
            return arr;
        }
    }
}