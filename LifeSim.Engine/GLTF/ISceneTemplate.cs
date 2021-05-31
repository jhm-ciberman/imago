using System.Collections.Generic;

namespace LifeSim.Engine.GLTF
{
    public interface ISceneTemplate
    {
        IReadOnlyList<GLTFNode> children { get; }

        GLTFNode? FindNodeByName(string name);
    }
}