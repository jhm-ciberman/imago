using System.Collections.Generic;
using System.Diagnostics;

namespace LifeSim.Engine.Gltf;

internal class GLTFPrimitive
{
    private readonly Dictionary<string, int> _attributes;
    private readonly GltfAccessor? _indicesAccessor;
    private readonly GltfLoader _model;

    public GLTFPrimitive(GltfLoader model, int? indices, Dictionary<string, int> attributes)
    {
        this._model = model;
        this._indicesAccessor = indices.HasValue ? this._model.GetAccessor(indices.Value) : null;
        this._attributes = attributes;
    }

    private readonly bool _loadSkinned = true;

    public IMeshData MakeMeshData()
    {
        var positionAccessor = this.GetAttributeAccessor("POSITION");
        Debug.Assert(positionAccessor != null);
        var positions = positionAccessor.AsVector3Array();

        var texCoordAccessor = this.GetAttributeAccessor("TEXCOORD_0");
        var texCoords = texCoordAccessor?.AsVector2Array();

        var normalAccessor   = this.GetAttributeAccessor("NORMAL");
        var normals = normalAccessor?.AsVector3Array();

        var jointsAccessor   = this.GetAttributeAccessor("JOINTS_0");
        var weightsAccessor  = this.GetAttributeAccessor("WEIGHTS_0");


        var indices = this._indicesAccessor == null ? MakeFakeIndices(positions.Length) : this._indicesAccessor.AsIndicesArray();

        if (this._loadSkinned && weightsAccessor != null && jointsAccessor != null)
        {
            var joints = jointsAccessor.AsUShort4Array();
            var weights = weightsAccessor.AsVector4Array();
            return new SkinnedMeshData(indices, positions, normals, texCoords, joints, weights);
        }
        else
        {
            return new BasicMeshData(indices, positions, normals, texCoords);
        }
    }


    private GltfAccessor? GetAttributeAccessor(string name)
    {
        if (this._attributes.TryGetValue(name, out int attributeId))
        {
            return this._model.GetAccessor(attributeId);
        }
        return null;
    }

    private static ushort[] MakeFakeIndices(int count)
    {
        var arr = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            arr[i] = (ushort)i;
        }
        return arr;
    }
}