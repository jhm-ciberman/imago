using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public interface IMaterial : IDisposable
    {
        ResourceLayout resourceLayout { get; }
        ResourceSet resourceSet { get; }
    }
}