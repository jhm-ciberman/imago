using System.Numerics;
using LifeSim.Support.Drawing;
using LifeSim.Support.Tweening;

namespace LifeSim.Imago.SceneGraph.Cameras;

/// <summary>
/// The InterpolationCamera can be used to smoothly transition between two cameras.
/// A display camera is used to display the scene. This is the camera that is actually drawn on screen.
/// The target camera is the camera that the interpolator is currently transitioning to and the previous camera is the camera that was previously used.
/// The InterpolationCamera can handle both instant and smooth transitions and orthographic and perspective cameras.
/// </summary>
public class InterpolationCamera : Camera
{
    /// <summary>
    /// Gets the camera that the interpolator is currently transitioning to.
    /// </summary>
    public Camera? TargetCamera { get; private set; }

    /// <summary>
    /// Gets the camera that the interpolator is currently transitioning from.
    /// </summary>
    public Camera? PreviousCamera { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InterpolationCamera"/> class.
    /// </summary>
    /// <param name="initialCamera">The initial camera.</param>
    /// <param name="viewport">The viewport to use.</param>
    public InterpolationCamera(Camera? initialCamera = null, Viewport? viewport = null) : base(viewport)
    {
        this.TargetCamera = initialCamera;
        this.PreviousCamera = initialCamera;
    }

    public float Progress { get; private set; } = 1f;

    public float TransitionDuration { get; private set; } = 0.5f;

    private Matrix4x4 _projectionMatrix;

    public override Matrix4x4 ProjectionMatrix => this._projectionMatrix;

    private int _maxShadowCascades = 4;

    public override int MaxShadowCascades => this._maxShadowCascades;

    /// <summary>
    /// Transitions to the given camera with a smooth transition.
    /// </summary>
    /// <param name="camera">The camera to transition to.</param>
    /// <param name="duration">The duration of the transition in seconds.</param>
    public void TransitionToCamera(Camera camera, float duration = 0.5f)
    {
        if (duration == 0f || this.TargetCamera == null)
        {
            this.ChangeCamera(camera);
            return;
        }

        this.PreviousCamera = this.TargetCamera;
        this.TargetCamera = camera;
        this.TransitionDuration = duration;
        this.Progress = 0f;
    }

    /// <summary>
    /// Changes the camera instantly.
    /// </summary>
    /// <param name="camera">The camera to change to.</param>
    public void ChangeCamera(Camera camera)
    {
        this.TargetCamera = camera;
        this.PreviousCamera = camera;
        this.Progress = 1f;
        this.TransitionDuration = 0f;
        this.Position = camera.Position;
        this.Rotation = camera.Rotation;
        this._projectionMatrix = camera.ProjectionMatrix;
        this.ClearColor = camera.ClearColor;
        this._maxShadowCascades = camera.MaxShadowCascades;
    }

    public override Matrix4x4 GetShadowCascadeViewProjectionMatrix(float near, float far)
    {
        if (this.TargetCamera == null)
            return Matrix4x4.Identity;

        return this.TargetCamera.GetShadowCascadeViewProjectionMatrix(near, far);
    }

    public virtual void Update(float deltaTime)
    {
        if (this.TargetCamera == null) return;

        if (this.TargetCamera == this.PreviousCamera || this.Progress >= 1f)
        {
            this.Position = this.TargetCamera.Position;
            this.Rotation = this.TargetCamera.Rotation;
            this._projectionMatrix = this.TargetCamera.ProjectionMatrix;
            this.NearPlane = this.TargetCamera.NearPlane;
            this.FarPlane = this.TargetCamera.FarPlane;
            this.ClearColor = this.TargetCamera.ClearColor;
            return;
        }

        if (this.PreviousCamera == null) return;

        // Update progress
        this.Progress += deltaTime / this.TransitionDuration;
        if (this.Progress >= 1f)
        {
            this.Progress = 1f;
            this.PreviousCamera = this.TargetCamera;
        }

        var p = Easing.Quadratic.Out(this.Progress);
        this.Position = Vector3.Lerp(this.PreviousCamera.Position, this.TargetCamera.Position, p);
        this.Rotation = Quaternion.Slerp(this.PreviousCamera.Rotation, this.TargetCamera.Rotation, p);
        this._projectionMatrix = Matrix4x4.Lerp(this.PreviousCamera.ProjectionMatrix, this.TargetCamera.ProjectionMatrix, p);
        this.NearPlane = float.Lerp(this.PreviousCamera.NearPlane, this.TargetCamera.NearPlane, p);
        this.FarPlane = float.Lerp(this.PreviousCamera.FarPlane, this.TargetCamera.FarPlane, p);

        var prevColor = this.PreviousCamera.ClearColor;
        var targetColor = this.TargetCamera.ClearColor;
        if (prevColor != null && targetColor != null)
            this.ClearColor = ColorF.Lerp(prevColor.Value, targetColor.Value, p);

        this._maxShadowCascades = p >= 0.5f ? this.TargetCamera.MaxShadowCascades : this.PreviousCamera.MaxShadowCascades;
    }
}
