using System;
using Veldrid;

namespace LifeSim.Rendering
{
    public interface IMaterial : IDisposable
    {
        Pass pass { get; }
        ResourceSet GetResourceSet();
    }
}