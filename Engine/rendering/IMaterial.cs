using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public interface IMaterial : IDisposable
    {
        Pass pass { get; }
        ResourceLayout resourceLayout { get; }
        ResourceSet resourceSet { get; }
    }
}