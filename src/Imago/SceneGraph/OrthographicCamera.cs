using System;
using System.Numerics;

namespace Imago.SceneGraph;

public class OrthographicCamera : Camera
{
    private Matrix4x4 _projectionMatrix;
    private float _size = 10f;

    public OrthographicCamera(Viewport? viewport = null) : base(viewport)
    {
    }

    /// <summary>
    /// Gets or sets the width of the camera.
    /// </summary>
    public float Width
    {
        get => this._size;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Width must be greater than 0.");
            }

            if (this._size != value)
            {
                this._size = value;
                this._projectionMatrixIsDirty = true;
            }
        }
    }

    /// <summary>
    /// Gets or sets the height of the camera.
    /// </summary>
    public float Height
    {
        get => this._size / this.Viewport.AspectRatio;
        set => this.Width = value * this.Viewport.AspectRatio;
    }

    /// <summary>
    /// Gets the projection matrix for the camera.
    /// </summary>
    public override Matrix4x4 ProjectionMatrix
    {
        get
        {
            if (this._projectionMatrixIsDirty)
            {
                this._projectionMatrix = Matrix4x4.CreateOrthographic(this.Width * this.Viewport.AspectRatio, this.Width, this.NearPlane, this.FarPlane);
                this._projectionMatrixIsDirty = false;
            }

            return this._projectionMatrix;
        }
    }

    public override Matrix4x4 GetShadowCascadeViewProjectionMatrix(float near, float far)
    {
        return Matrix4x4.CreateOrthographic(this.Width / this.Viewport.AspectRatio, this.Width, near, far);
    }

    public override int MaxShadowCascades => 1;
}
