using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Imago.Assets.Materials;

/// <summary>
/// Base class for materials with custom GPU parameters.
/// </summary>
/// <typeparam name="TParams">
/// The parameter struct type. Must be unmanaged and match the GLSL MaterialParams uniform block layout.
/// Use <see cref="System.Runtime.InteropServices.StructLayoutAttribute"/> with Sequential layout
/// and follow std140 alignment rules (vec4 = 16 bytes, float = 4 bytes, etc.).
/// </typeparam>
public abstract class Material<TParams> : Material
    where TParams : unmanaged
{
    private TParams _params;
    private bool _paramsDirty = true;
    private readonly DeviceBuffer _paramsBuffer;

    /// <summary>
    /// Gets a reference to the material parameters struct.
    /// Modify fields through <see cref="SetParam{T}"/> to ensure dirty tracking.
    /// </summary>
    protected ref TParams Params => ref this._params;

    /// <inheritdoc/>
    private protected sealed override DeviceBuffer? ParamsBuffer => this._paramsBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Material{TParams}"/> class.
    /// </summary>
    /// <param name="shaders">The compiled shaders for all render passes.</param>
    protected Material(ShaderSet shaders) : base(shaders)
    {
        uint size = (uint)Unsafe.SizeOf<TParams>();
        this._paramsBuffer = this.CreateParamsBuffer(size);
    }

    /// <summary>
    /// Sets a parameter field value with automatic dirty tracking.
    /// If the value changes, the parameters will be uploaded to the GPU on the next render.
    /// </summary>
    /// <typeparam name="T">The type of the parameter field.</typeparam>
    /// <param name="field">Reference to the field in the Params struct.</param>
    /// <param name="value">The new value to set.</param>
    protected void SetParam<T>(ref T field, T value) where T : unmanaged
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            this._paramsDirty = true;
            this.NotifyResourcesDirty();
        }
    }

    /// <inheritdoc/>
    private protected sealed override void UpdateParamsBuffer(CommandList cl)
    {
        if (this._paramsDirty)
        {
            this._paramsDirty = false;
            cl.UpdateBuffer(this._paramsBuffer, 0, ref this._params);
        }
    }
}
