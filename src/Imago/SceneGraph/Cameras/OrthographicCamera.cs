using System.Numerics;
using CommunityToolkit.Diagnostics;

namespace Imago.SceneGraph.Cameras;

/// <summary>
/// Represents an orthographic camera that provides parallel projection without perspective distortion.
/// </summary>
public class OrthographicCamera : Camera
{
    private Matrix4x4 _projectionMatrix;
    private float _width = 10f;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrthographicCamera"/> class.
    /// </summary>
    /// <param name="viewport">The viewport to use. If null, uses the main viewport.</param>
    public OrthographicCamera(Viewport? viewport = null) : base(viewport)
    {
    }

    /// <summary>
    /// Gets or sets the width of the camera.
    /// </summary>
    public float Width
    {
        get => this._width;
        set
        {
            Guard.IsGreaterThanOrEqualTo(value, 0f);

            if (this._width != value)
            {
                this._width = value;
                this.ProjectionMatrixIsDirty = true;
            }
        }
    }

    /// <summary>
    /// Gets or sets the height of the camera.
    /// </summary>
    public float Height
    {
        get => this._width / this.Viewport.AspectRatio;
        set => this.Width = value * this.Viewport.AspectRatio;
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
                this._projectionMatrix = Matrix4x4.CreateOrthographic(this.Width, this.Height, this.NearPlane, this.FarPlane);
                this.ProjectionMatrixIsDirty = false;
            }

            return this._projectionMatrix;
        }
    }

    /// <inheritdoc/>
    public override Matrix4x4 GetShadowCascadeViewProjectionMatrix(float near, float far)
    {
        return Matrix4x4.CreateOrthographic(this.Width, this.Height, near, far);
    }

    /// <inheritdoc/>
    public override int MaxShadowCascades => 1;
}
