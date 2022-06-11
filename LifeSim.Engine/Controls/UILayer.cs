using System;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class UILayer
{
    public Viewport Viewport { get; }

    private Control? _content;

    public float Zoom { get; set; } = 1f;

    public Matrix4x4 ViewProjectionMatrix
    {
        get
        {
            Vector2 size = this.Viewport.Size / this.Zoom;
            return Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, -10f, 100f);
        }
    }
    public UILayer(Viewport viewport)
    {
        this.Viewport = viewport;
        this.Viewport.Resized += (sender, e) => this.TriggerLayoutUpdate();
    }

    public T? FindByName<T>(string name) where T : Control
    {
        if (this._content != null)
        {
            return this._content.FindByName<T>(name);
        }

        return null;
    }

    public void TriggerLayoutUpdate()
    {
        if (this._content is null) return;

        Vector2 size = this.Viewport.Size / this.Zoom;
        this._content.Measure(size);
        this._content.Arrange(new Rect(0, 0, size.X, size.Y));
    }

    public Control? Content
    {
        get => this._content;
        set
        {
            if (this._content != value)
            {
                if (this._content != null)
                {
                    this._content.Parent = null;
                }

                this._content = value;

                if (this._content != null)
                {
                    this._content.Root = this;
                }

                this.TriggerLayoutUpdate();
            }
        }
    }



    internal void Draw(SpriteBatcher spriteBatcher)
    {
        if (this._content is null) return;
        this._content.Draw(spriteBatcher);
    }

    internal void Update(float deltaTime)
    {
        if (this._content is null) return;
        this._content.Update(deltaTime);
    }
}