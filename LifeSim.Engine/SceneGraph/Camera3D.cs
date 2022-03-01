using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid.Utilities;

namespace LifeSim.Engine.SceneGraph;

public class Camera3D
{
    public ColorF? ClearColor { get; set; } = new ColorF(0.84f, 0.84f, 0.86f, 1.0f);
    private bool _viewMatrixIsDirty = true;
    private bool _projectionMatrixIsDirty = true;
    private Matrix4x4 _viewMatrix;
    private Matrix4x4 _projectionMatrix;


    private Vector3 _position = Vector3.Zero;
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

    public float FieldOfView
    {
        get => this._fieldOfView;
        set
        {
            if (value < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(value), "Field of view must be greater than 0.");
            }

            if (value > System.MathF.PI)
            {
                throw new System.ArgumentOutOfRangeException(nameof(value), "Field of view must be less than 180 degrees.");
            }

            if (this._fieldOfView != value)
            {
                this._fieldOfView = value;
                this._projectionMatrixIsDirty = true;
            }
        }
    }

    private float _nearPlane = 0.1f;
    public float NearPlane
    {
        get => this._nearPlane;
        set
        {
            if (value < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(value), "Near plane must be greater than 0.");
            }

            if (this._nearPlane != value)
            {
                this._nearPlane = value;
                this._projectionMatrixIsDirty = true;
            }
        }
    }

    private float _farPlane = 300f;
    public float FarPlane
    {
        get => this._farPlane;
        set
        {
            if (value < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(value), "Far plane must be greater than 0.");
            }

            if (this._farPlane != value)
            {
                this._farPlane = value;
                this._projectionMatrixIsDirty = true;
            }
        }
    }

    public float AspectRatio => (float)this.Viewport.Width / (float)this.Viewport.Height;

    private Viewport _viewport = new Viewport(0, 0, 1, 1);
    public Viewport Viewport
    {
        get => this._viewport;
        set
        {
            if (this._viewport != value)
            {
                this._viewport.OnResized -= this.OnViewportResized;
                this._viewport = value;
                this._projectionMatrixIsDirty = true;
                this._viewport.OnResized += this.OnViewportResized;
            }
        }
    }

    private void OnViewportResized(Viewport viewport)
    {
        this._viewport = viewport;
        this._projectionMatrixIsDirty = true;
    }

    public Camera3D FrustumCullingCamera { get; set; }

    public BoundingFrustum FrustumForCulling => new BoundingFrustum(this.FrustumCullingCamera.ViewProjectionMatrix);

    public Camera3D(Viewport viewport)
    {
        this.Viewport = viewport;
        this.FrustumCullingCamera = this;
    }

    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            if (this._projectionMatrixIsDirty)
            {
                this._projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(this._fieldOfView, this.AspectRatio, this._nearPlane, this._farPlane);
                this._projectionMatrixIsDirty = false;
            }

            return this._projectionMatrix;
        }
    }



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


    public Matrix4x4 ViewProjectionMatrix => this.ViewMatrix * this.ProjectionMatrix;

    public Vector3 Up => new Vector3(this.ViewMatrix.M12, this.ViewMatrix.M22, this.ViewMatrix.M32);
    public Vector3 Right => new Vector3(this.ViewMatrix.M11, this.ViewMatrix.M21, this.ViewMatrix.M31);
    public Vector3 Forward => new Vector3(this.ViewMatrix.M13, this.ViewMatrix.M23, this.ViewMatrix.M33);

    public void LookAt(Vector3 destPoint)
    {
        Matrix4x4 worldMat = Matrix4x4.CreateWorld(this.Position, this.Position - destPoint, Vector3.UnitY);
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