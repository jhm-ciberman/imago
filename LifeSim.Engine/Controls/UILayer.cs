using System;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class UILayer
{
    public Viewport Viewport { get; }

    private Control? _content;

    public Matrix4x4 ViewProjectionMatrix => Matrix4x4.CreateOrthographicOffCenter(0, this.Viewport.Width, this.Viewport.Height, 0, -10f, 100f);

    public UILayer(Viewport viewport)
    {
        this.Viewport = viewport;
        this.Viewport.OnResized += this.OnViewportResized;
        this.OnViewportResized(this.Viewport);
    }

    public T? FindByName<T>(string name) where T : Control
    {
        if (this._content != null)
        {
            return this._content.FindByName<T>(name);
        }

        return null;
    }

    private void OnViewportResized(Viewport viewport)
    {
        this.TriggerLayoutUpdate();
    }

    public void TriggerLayoutUpdate()
    {
        if (this._content is null) return;
        this._content.Measure(this.Viewport.Size);
        this._content.Arrange(new Rect(0, 0, this.Viewport.Width, this.Viewport.Height));
    }

    public Control? Content
    {
        get => this._content;
        set
        {
            this._content = value;
            this.TriggerLayoutUpdate();
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