using System.Numerics;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Gltf;

/// <summary>
/// Defines a contract for reading various data types from a glTF buffer view.
/// </summary>
internal interface IGltfBufferView
{
    /// <summary>
    /// Reads an array of <see cref="Vector2"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Vector2"/> elements to read.</param>
    /// <returns>An array of <see cref="Vector2"/>.</returns>
    public Vector2[] ReadVector2Array(int offset, int count);
    /// <summary>
    /// Reads an array of <see cref="Vector3"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Vector3"/> elements to read.</param>
    /// <returns>An array of <see cref="Vector3"/>.</returns>
    public Vector3[] ReadVector3Array(int offset, int count);
    /// <summary>
    /// Reads an array of <see cref="Vector4"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Vector4"/> elements to read.</param>
    /// <returns>An array of <see cref="Vector4"/>.</returns>
    public Vector4[] ReadVector4Array(int offset, int count);
    /// <summary>
    /// Reads an array of unsigned 16-bit integers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="ushort"/> elements to read.</param>
    /// <returns>An array of <see cref="ushort"/>.</returns>
    public ushort[] ReadUShortArray(int offset, int count);
    /// <summary>
    /// Reads an array of <see cref="Vector4UShort"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Vector4UShort"/> elements to read.</param>
    /// <returns>An array of <see cref="Vector4UShort"/>.</returns>
    public Vector4UShort[] ReadUShort4Array(int offset, int count);
    /// <summary>
    /// Reads an array of unsigned 32-bit integers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="uint"/> elements to read.</param>
    /// <returns>An array of <see cref="uint"/>.</returns>
    public uint[] ReadUIntArray(int offset, int count);
    /// <summary>
    /// Reads an array of bytes from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="byte"/> elements to read.</param>
    /// <returns>An array of <see cref="byte"/>.</returns>
    public byte[] ReadByteArray(int offset, int count);
    /// <summary>
    /// Reads an array of single-precision floating-point numbers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="float"/> elements to read.</param>
    /// <returns>An array of <see cref="float"/>.</returns>
    public float[] ReadFloatArray(int offset, int count);
    /// <summary>
    /// Reads an array of <see cref="Matrix4x4"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Matrix4x4"/> elements to read.</param>
    /// <returns>An array of <see cref="Matrix4x4"/>.</returns>
    public Matrix4x4[] ReadMatrix4x4Array(int offset, int count);
    /// <summary>
    /// Reads an array of <see cref="Quaternion"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Quaternion"/> elements to read.</param>
    /// <returns>An array of <see cref="Quaternion"/>.</returns>
    public Quaternion[] ReadQuaternionArray(int offset, int count);
    /// <summary>
    /// Reads an array of signed 8-bit integers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="sbyte"/> elements to read.</param>
    /// <returns>An array of <see cref="sbyte"/>.</returns>
    public sbyte[] ReadSByteArray(int offset, int count);
    /// <summary>
    /// Reads an array of signed 16-bit integers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="short"/> elements to read.</param>
    /// <returns>An array of <see cref="short"/>.</returns>
    public short[] ReadShortArray(int offset, int count);
}
