using System.Collections.Generic;
using System.Diagnostics;

namespace LifeSim.GLTF
{
    class GLTFPrimitive
    {
        private Dictionary<string, int> _attributes;
        private GLTFAccessor? _indicesAccessor;
        private GLTFModel _model;

        public GLTFPrimitive(GLTFModel model, int? indices, Dictionary<string, int> attributes)
        {
            this._model = model;
            this._indicesAccessor = indices.HasValue ? this._model.GetAccessor(indices.Value) : null;
            this._attributes = attributes;
        }

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

            if (weightsAccessor != null && jointsAccessor != null) {
                var joints = jointsAccessor.AsVector4Array();
                var weights = weightsAccessor.AsVector4Array();
                return new LifeSim.SkinnedMeshData(positions, indices, texCoords, normals, joints, weights);
            } else {
                var mesh = new LifeSim.MeshData(positions, indices, texCoords, normals);
                return mesh;
            }
        }


        private GLTFAccessor? _GetAttributeAccessor(string name)
        {
            if (this._attributes.TryGetValue(name, out int attributeId)) {
                return this._model.GetAccessor(attributeId);
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