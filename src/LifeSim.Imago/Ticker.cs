using System;
using System.Diagnostics;
using System.Threading;

namespace LifeSim.Imago;

public class TickEventArgs : EventArgs
{
    public double DeltaTime { get; private set; } = 0;
    public double ElapsedTime { get; private set; } = 0;

    public TickEventArgs()
    {
    }

    internal void SetValues(double deltaTime, double elapsedTime)
    {
        this.DeltaTime = deltaTime;
        this.ElapsedTime = elapsedTime;
    }
}

public class Ticker
{
    private double _elapsedTime;
    private double _deltaTime;
    private double _framesPerSecond;
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private readonly TickEventArgs _tickEventArgs = new TickEventArgs();

    /// <summary>
    /// Occurs when a tick is performed.
    /// </summary>
    public event EventHandler<TickEventArgs>? Tick;

    /// <summary>
    /// Gets the time passed since the last frame in seconds.
    /// </summary>
    public double DeltaTime => this._deltaTime;

    /// <summary>
    /// Gets the total time passed since the start of the application in seconds.
    /// </summary>
    public double ElapsedTime => this._elapsedTime;

    /// <summary>
    /// Gets the number of frames per second.
    /// </summary>
    public double FramesPerSecond => this._framesPerSecond;

    /// <summary>
    /// Gets or sets the minimum time that should pass between frames.
    /// </summary>
    public float TargetFrameTime { get; set; } = 1f / 60f;

    /// <summary>
    /// Gets or sets the target frames per second. If set to <see cref="float.PositiveInfinity"/>, the ticker will run as fast as possible.
    /// </summary>
    public float TargetFramesPerSecond
    {
        get => this.TargetFrameTime > 0 ? 1f / this.TargetFrameTime : float.PositiveInfinity;
        set => this.TargetFrameTime = float.IsPositiveInfinity(value) ? 1f / value : 0;
    }

    /// <summary>
    /// Gets a value indicating whether the ticker is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ticker"/> class.
    /// </summary>
    /// <param name="targetFramesPerSecond">The target frames per second. If set to <see cref="float.PositiveInfinity"/>, the ticker will run as fast as possible.</param>
    public Ticker(float targetFramesPerSecond = float.PositiveInfinity)
    {
        this.TargetFramesPerSecond = targetFramesPerSecond;
    }

    /// <summary>
    /// Stops the ticker.
    /// </summary>
    public void Stop()
    {
        this.IsRunning = false;
    }

    /// <summary>
    /// Starts the ticker.
    /// </summary>
    public void Start()
    {
        if (this.IsRunning) return;
        this.IsRunning = true;
        this._stopwatch.Restart();

        double previousElapsed = 0.0;

        while (this.IsRunning)
        {
            double currentTime = this._stopwatch.Elapsed.TotalSeconds;

            // Wait if we haven't reached the target frame time yet.
            while (this.TargetFrameTime > 0 && (currentTime - previousElapsed) < this.TargetFrameTime)
            {
                Thread.Sleep(0);
                currentTime = this._stopwatch.Elapsed.TotalSeconds;
            }

            this._elapsedTime = currentTime;
            this._deltaTime = this._elapsedTime - previousElapsed;
            previousElapsed = this._elapsedTime;

            if (this._deltaTime > 0.0)
            {
                this._framesPerSecond = 1.0 / this._deltaTime;
            }

            // Fire the tick event
            this._tickEventArgs.SetValues(this._deltaTime, this._elapsedTime);
            Tick?.Invoke(this, this._tickEventArgs);
        }
    }
}
