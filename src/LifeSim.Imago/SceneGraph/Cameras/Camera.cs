using System;
using System.Numerics;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.Utilities;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph.Cameras;

/// <summary>
/// Represents an abstract base class for all camera types in the scene.
/// </summary>
public abstract class Camera
{
    private Matrix4x4 _viewMatrix;
    private Vector3 _position = Vector3.Zero;
    private Quaternion _rotation  = Quaternion.Identity;
    private bool _viewMatrixIsDirty = true;
    private bool _projectionMatrixIsDirty = true;
    private float _nearPlane = 0.1f;
    private float _farPlane = 300f;

    /// <summary>
    /// Gets or sets a value indicating whether the view matrix needs to be recalculated.
    /// </summary>
    protected bool ViewMatrixIsDirty
    {
        get => this._viewMatrixIsDirty;
        set => this._viewMatrixIsDirty = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the projection matrix needs to be recalculated.
    /// </summary>
    protected bool ProjectionMatrixIsDirty
    {
        get => this._projectionMatrixIsDirty;
        set => this._projectionMatrixIsDirty = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera"/> class.
    /// </summary>
    /// <param name="viewport">The viewport.</param>
    public Camera(Viewport? viewport = null)
    {
        this.Viewport = viewport ?? Renderer.Instance.MainViewport;
        this.FrustumCullingCamera = this;
    }

    /// <summary>
    /// Gets or sets the clear color of the camera.
    /// </summary>
    public ColorF? ClearColor { get; set; } = new ColorF(0.84f, 0.84f, 0.86f, 1.0f);


    /// <summary>
    /// Gets or sets the camera that will be used to apply frustum culling. By default, this is the camera itself.
    /// </summary>
    public Camera FrustumCullingCamera { get; set; }

    /// <summary>
    /// Gets or sets the position of the camera.
    /// </summary>
    public Vector3 Position
    {
        get => this._position;
        set
        {
            if (this._position != value)
            {
                this._position = value;
                this.ViewMatrixIsDirty = true;
            }
        }
    }


    /// <summary>
    /// Gets or sets the rotation of the camera.
    /// </summary>
    public Quaternion Rotation
    {
        get => this._rotation;
        set
        {
            if (this._rotation != value)
            {
                this._rotation = value;
                this.ViewMatrixIsDirty = true;
            }
        }
    }

    private Viewport _viewport = new Viewport(Vector2.One);

    /// <summary>
    /// Gets or sets the viewport of the camera.
    /// </summary>
    public Viewport Viewport
    {
        get => this._viewport;
        set
        {
            if (this._viewport != value)
            {
                this._viewport.SizeChanged -= this.Viewport_SizeChanged;
                this._viewport = value;
                this.ProjectionMatrixIsDirty = true;
                this._viewport.SizeChanged += this.Viewport_SizeChanged;
            }
        }
    }

    private void Viewport_SizeChanged(object? sender, EventArgs e)
    {
        this.ProjectionMatrixIsDirty = true;
    }

    /// <summary>
    /// Gets the view matrix for the camera.
    /// </summary>
    public Matrix4x4 ViewMatrix
    {
        get
        {
            if (this.ViewMatrixIsDirty)
            {
                Vector3 forward = Vector3.Transform(Vector3.UnitZ, this._rotation);
                Vector3 up = Vector3.Transform(Vector3.UnitY, this._rotation);
                this._viewMatrix = Matrix4x4.CreateLookAt(this.Position, this.Position + forward, up);
                this.ViewMatrixIsDirty = false;
            }

            return this._viewMatrix;
        }
    }

    /// <summary>
    /// Gets or sets the near plane of the camera.
    /// </summary>
    public float NearPlane
    {
        get => this._nearPlane;
        set
        {
            if (this._nearPlane != value)
            {
                this._nearPlane = value;
                this.ProjectionMatrixIsDirty = true;
            }
        }
    }



    /// <summary>
    /// Gets or sets the far plane of the camera.
    /// </summary>
    public float FarPlane
    {
        get => this._farPlane;
        set
        {
            if (this._farPlane != value)
            {
                this._farPlane = value;
                this.ProjectionMatrixIsDirty = true;
            }
        }
    }

    /// <summary>
    /// Gets the projection matrix for the camera.
    /// </summary>
    public abstract Matrix4x4 ProjectionMatrix { get; }

    /// <summary>
    /// Gets the view projection matrix.
    /// </summary>
    public Matrix4x4 ViewProjectionMatrix => this.ViewMatrix * this.ProjectionMatrix;

    /// <summary>
    /// Gets the up vector for the camera.
    /// </summary>
    public Vector3 Up => new Vector3(this.ViewMatrix.M12, this.ViewMatrix.M22, this.ViewMatrix.M32);

    /// <summary>
    /// Gets the right vector for the camera.
    /// </summary>
    public Vector3 Right => new Vector3(this.ViewMatrix.M11, this.ViewMatrix.M21, this.ViewMatrix.M31);

    /// <summary>
    /// Gets the forward vector for the camera.
    /// </summary>
    public Vector3 Forward => new Vector3(this.ViewMatrix.M13, this.ViewMatrix.M23, this.ViewMatrix.M33);

    /// <summary>
    /// Rotates the camera to look at the specified target.
    /// </summary>
    /// <param name="target">The target to look at.</param>
    public void LookAt(Vector3 target)
    {
        Matrix4x4 worldMat = Matrix4x4.CreateWorld(this.Position, this.Position - target, Vector3.UnitY);
        this.Rotation = Quaternion.CreateFromRotationMatrix(worldMat);
    }

    /// <summary>
    /// Unproject a point from screen space to world space.
    /// </summary>
    /// <param name="point">The point in screen space.</param>
    /// <returns>The point in world space.</returns>
    public Vector3 Unproject(Vector3 point)
    {
        Matrix4x4.Invert(this.ViewProjectionMatrix, out Matrix4x4 inverseViewProjection);
        Vector4 v = Vector4.Transform(point, inverseViewProjection);
        v /= v.W;
        return new Vector3(v.X, v.Y, v.Z);
    }

    /// <summary>
    /// Projects a point from world space to screen space.
    /// </summary>
    /// <param name="point">The point in world space.</param>
    /// <returns>The point in screen space.</returns>
    public Vector3 Project(Vector3 point)
    {
        Vector4 v = Vector4.Transform(point, this.ViewProjectionMatrix);
        v /= v.W;
        return new Vector3(v.X, v.Y, v.Z);
    }

    /// <summary>
    /// Creates a ray from the camera's position to the given point in screen space.
    /// </summary>
    /// <param name="point">The point in screen space from the top left (0,0) to bottom right (1,1).</param>
    /// <returns>The ray.</returns>
    public Ray ViewportRay(Vector2 point)
    {
        point = point * 2 - Vector2.One;
        point.Y = -point.Y;
        Vector3 nearPoint = this.Unproject(new Vector3(point.X, point.Y, 0));
        Vector3 farPoint = this.Unproject(new Vector3(point.X, point.Y, 1));
        var ray = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));
        return ray;
    }

    /// <summary>
    /// Returns the view projection matrix for a shadow cascade with the specified near and far planes.
    /// </summary>
    /// <param name="near">The near plane.</param>
    /// <param name="far">The far plane.</param>
    /// <returns>The view projection matrix.</returns>
    public abstract Matrix4x4 GetShadowCascadeViewProjectionMatrix(float near, float far);

    /// <summary>
    /// Gets the maximum number of shadow cascades supported by the camera.
    /// </summary>
    public virtual int MaxShadowCascades => int.MaxValue;
}
