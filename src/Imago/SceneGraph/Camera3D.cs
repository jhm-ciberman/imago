using System;
using System.Numerics;
using Support;
using Veldrid.Utilities;

namespace Imago.SceneGraph;

public class Camera3D
{
    /// <summary>
    /// Gets or sets the clear color of the camera.
    /// </summary>
    public ColorF? ClearColor { get; set; } = new ColorF(0.84f, 0.84f, 0.86f, 1.0f);

    private bool _viewMatrixIsDirty = true;
    private bool _projectionMatrixIsDirty = true;
    private Matrix4x4 _viewMatrix;
    private Matrix4x4 _projectionMatrix;


    private Vector3 _position = Vector3.Zero;

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
                this._viewMatrixIsDirty = true;
            }
        }
    }

    private Quaternion _rotation  = Quaternion.Identity;

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
                this._viewMatrixIsDirty = true;
            }
        }
    }

    private float _fieldOfView = 60 * System.MathF.PI / 180f;

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
                throw new ArgumentOutOfRangeException(nameof(value), "Field of view must be less than 180 degrees.");
            }

            if (this._fieldOfView != value)
            {
                this._fieldOfView = value;
                this._projectionMatrixIsDirty = true;
            }
        }
    }

    private float _nearPlane = 0.1f;

    /// <summary>
    /// Gets or sets the near plane of the camera.
    /// </summary>
    public float NearPlane
    {
        get => this._nearPlane;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Near plane must be greater than 0.");
            }

            if (this._nearPlane != value)
            {
                this._nearPlane = value;
                this._projectionMatrixIsDirty = true;
            }
        }
    }

    private float _farPlane = 300f;

    /// <summary>
    /// Gets or sets the far plane of the camera.
    /// </summary>
    public float FarPlane
    {
        get => this._farPlane;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Far plane must be greater than 0.");
            }

            if (this._farPlane != value)
            {
                this._farPlane = value;
                this._projectionMatrixIsDirty = true;
            }
        }
    }

    private Viewport _viewport = new Viewport(0, 0, 1, 1);

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
                this._viewport.Resized -= this.Viewport_Resized;
                this._viewport = value;
                this._projectionMatrixIsDirty = true;
                this._viewport.Resized += this.Viewport_Resized;
            }
        }
    }

    private void Viewport_Resized(object? sender, EventArgs e)
    {
        this._projectionMatrixIsDirty = true;
    }

    /// <summary>
    /// Gets or sets the camera that will be used to apply frustum culling. By default, this is the camera itself.
    /// </summary>
    public Camera3D FrustumCullingCamera { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera3D"/> class.
    /// </summary>
    /// <param name="viewport">The viewport.</param>
    public Camera3D(Viewport viewport)
    {
        this.Viewport = viewport;
        this.FrustumCullingCamera = this;
    }

    /// <summary>
    /// Gets the projection matrix for the camera.
    /// </summary>
    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            if (this._projectionMatrixIsDirty)
            {
                this._projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(this._fieldOfView, this.Viewport.AspectRatio, this._nearPlane, this._farPlane);
                this._projectionMatrixIsDirty = false;
            }

            return this._projectionMatrix;
        }
    }


    /// <summary>
    /// Gets the view matrix for the camera.
    /// </summary>
    public Matrix4x4 ViewMatrix
    {
        get
        {
            if (this._viewMatrixIsDirty)
            {
                Vector3 forward = Vector3.Transform(Vector3.UnitZ, this._rotation);
                Vector3 up = Vector3.Transform(Vector3.UnitY, this._rotation);
                this._viewMatrix = Matrix4x4.CreateLookAt(this.Position, this.Position + forward, up);
                this._viewMatrixIsDirty = false;
            }

            return this._viewMatrix;
        }
    }


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
}
