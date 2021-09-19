using System.Collections.Generic;

namespace LifeSim.Engine.GLTF
{
    public interface ISceneTemplate
    {
        IReadOnlyList<GLTFNode> Children { get; }

        GLTFNode? FindNodeByName(string name);
    }
}