using System;
using System.Numerics;

namespace Imago.SceneGraph.Cameras;

/// <summary>
/// Represents a perspective camera that provides perspective projection with depth and field of view.
/// </summary>
public class PerspectiveCamera : Camera
{
    private Matrix4x4 _projectionMatrix;
    private float _fieldOfView = 60 * MathF.PI / 180f;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerspectiveCamera"/> class.
    /// </summary>
    /// <param name="viewport">The viewport to use. If null, uses the main viewport.</param>
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
                throw new ArgumentOutOfRangeException(nameof(value), "Field of view must be greater than 0.");

            if (value > MathF.PI)
                throw new ArgumentOutOfRangeException(nameof(value), "Field of view must be less than 2 pi radians.");

            if (this._fieldOfView != value)
            {
                this._fieldOfView = value;
                this.ProjectionMatrixIsDirty = true;
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
            if (this.ProjectionMatrixIsDirty)
            {
                this._projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(this._fieldOfView, this.Viewport.AspectRatio, this.NearPlane, this.FarPlane);
                this.ProjectionMatrixIsDirty = false;
            }

            return this._projectionMatrix;
        }
    }

    /// <inheritdoc/>
    public override Matrix4x4 GetShadowCascadeViewProjectionMatrix(float near, float far)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(this.FieldOfView, this.Viewport.AspectRatio, near, far);
    }


}
