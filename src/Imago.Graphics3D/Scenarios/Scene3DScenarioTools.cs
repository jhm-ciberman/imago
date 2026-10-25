using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Imago.Graphics3D.SceneGraph;
using Imago.Graphics3D.SceneGraph.Cameras;
using Imago.Scenarios;
using NeoVeldrid;

namespace Imago.Graphics3D.Scenarios;

/// <summary>
/// The 3D scene tools of a running <see cref="Scenario"/>. They aim the scenario's mouse at points in the world,
/// so a script does not have to work out screen positions.
/// </summary>
/// <remarks>
/// The tools use the camera of the first visible 3D scene. Screen positions are in window pixels.
/// </remarks>
public sealed class Scene3DScenarioTools
{
    private readonly Scenario _scenario;

    internal Scene3DScenarioTools(Scenario scenario)
    {
        this._scenario = scenario;
    }

    /// <summary>
    /// Gets the camera of the first visible 3D scene.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no visible 3D scene has a camera.</exception>
    public Camera Camera =>
        this._scenario.App.Stage.FindVisibleLayers<Scene3DLayer>().FirstOrDefault()?.Camera
        ?? throw new InvalidOperationException("No visible 3D scene has a camera.");

    /// <summary>
    /// Gets the screen position of the given point in the world.
    /// </summary>
    /// <param name="point">The point in world space.</param>
    /// <returns>The position in window pixels.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the point is behind the camera.</exception>
    public Vector2 ToScreen(Vector3 point)
    {
        var camera = this.Camera;
        if (!camera.IsInFront(point))
        {
            throw new InvalidOperationException($"The point {point} is behind the camera.");
        }

        return camera.WorldToViewport(point);
    }

    /// <summary>
    /// Moves the scenario's mouse over the given point in the world.
    /// </summary>
    /// <param name="point">The point in world space.</param>
    /// <returns>A task that completes once the game has seen the mouse over the point.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the point is behind the camera.</exception>
    public Task Hover(Vector3 point)
    {
        this._scenario.Mouse.Move(this.ToScreen(point));
        return this._scenario.NextFrame();
    }

    /// <summary>
    /// Clicks the given point in the world with the scenario's mouse.
    /// </summary>
    /// <param name="point">The point in world space.</param>
    /// <param name="button">The button to click with.</param>
    /// <returns>A task that completes once the button has been released.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the point is behind the camera.</exception>
    public Task Click(Vector3 point, MouseButton button = MouseButton.Left)
    {
        return this._scenario.Mouse.Click(this.ToScreen(point), button);
    }

    /// <summary>
    /// Drags the scenario's mouse from one point in the world to another.
    /// </summary>
    /// <param name="from">The point to press at, in world space.</param>
    /// <param name="to">The point to release at, in world space.</param>
    /// <param name="button">The button to drag with.</param>
    /// <returns>A task that completes once the button has been released.</returns>
    /// <exception cref="InvalidOperationException">Thrown if either point is behind the camera.</exception>
    public Task Drag(Vector3 from, Vector3 to, MouseButton button = MouseButton.Left)
    {
        return this._scenario.Mouse.Drag(this.ToScreen(from), this.ToScreen(to), button);
    }
}
