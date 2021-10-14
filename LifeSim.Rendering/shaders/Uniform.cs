using System;
using System.Runtime.InteropServices;

namespace LifeSim.Rendering
{
    public struct Uniform<T> : MaterialDefinition.IUniform where T : unmanaged
    {
        public string Name { get; private set; }
        private T _data;

        public unsafe Uniform(string name, T defaultValue)
        {
            this.Name = name;
            this._data = defaultValue;
        }

        public void CopyTo(Span<byte> dest)
        {
            MemoryMarshal.Write(dest, ref this._data);
        }
    }
}