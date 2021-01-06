using System;
using Veldrid;

namespace LifeSim.Rendering
{
    public interface IMaterial : IDisposable
    {
        Pass pass { get; }
        ResourceLayout resourceLayout { get; }
        ResourceSet resourceSet { get; }
    }
}