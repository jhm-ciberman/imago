using System;
using System.Numerics;

namespace Imago.SceneGraph;

public class PerspectiveCamera : Camera
{
    private Matrix4x4 _projectionMatrix;
    private float _fieldOfView = 60 * System.MathF.PI / 180f;

    public PerspectiveCamera(Viewport? viewport = null) : base(viewport)
    {
    }

    /// <summary>
    /// Gets or sets the field of view of the camera.
    /// </summary>
    public float FieldOfView
    {
        get => this._fieldOfView;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Field of view must be greater than 0.");
            }

            if (value > MathF.PI)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Field of view must be less than 2 pi radians.");
            }

            if (this._fieldOfView != value)
            {
                this._fieldOfView = value;
                this._projectionMatrixIsDirty = true;
            }
        }
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
                this._projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(this._fieldOfView, this.Viewport.AspectRatio, this.NearPlane, this.FarPlane);
                this._projectionMatrixIsDirty = false;
            }

            return this._projectionMatrix;
        }
    }

    public override Matrix4x4 GetShadowCascadeViewProjectionMatrix(float near, float far)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(this.FieldOfView, this.Viewport.AspectRatio, near, far);
    }


}
