using System;
using System.Collections.Generic;
using System.Numerics;
using FontStashSharp;
using Imago.Assets.Textures;
using Imago.Support.Drawing;
using Imago.Support.Numerics;
using NeoVeldrid;
using Texture = Imago.Assets.Textures.Texture;

namespace Imago.Rendering.Sprites;

/// <summary>
/// Draws batches of 2D sprites, text, and other graphical elements.
/// </summary>
public class SpriteBatch
{
    private readonly SpritesPass _pass;

    private readonly FontStashAdapter _fontStashAdapter;

    private readonly Stack<Rect> _clipRectStack = new Stack<Rect>();

    private readonly Stack<float> _opacityStack = new Stack<float>();

    private CommandList _commandList = null!;

    private IRenderTexture _target = null!;

    private bool _hasBegun;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBatch"/> class.
    /// </summary>
    /// <param name="gd">The graphics device.</param>
    /// <param name="pass">The sprites pass that records the drawn quads.</param>
    internal SpriteBatch(GraphicsDevice gd, SpritesPass pass)
    {
        this._pass = pass;
        this._fontStashAdapter = new FontStashAdapter(gd, this);
    }

    /// <summary>
    /// Sets the command list and the render texture that the next batches draw into.
    /// </summary>
    /// <param name="commandList">The command list to record on.</param>
    /// <param name="target">The render texture to draw into.</param>
    internal void SetRenderTarget(CommandList commandList, IRenderTexture target)
    {
        this._commandList = commandList;
        this._target = target;
    }

    /// <summary>
    /// Begins a new batch. Positions are in pixels of the render target, with the origin at the top left corner.
    /// </summary>
    /// <param name="transform">The transform applied to everything drawn in the batch, or null for no transform.</param>
    /// <exception cref="InvalidOperationException">Thrown if the batch has already begun.</exception>
    public void Begin(Matrix3x2? transform = null)
    {
        if (this._hasBegun)
        {
            throw new InvalidOperationException("Begin cannot be called again until End has been called.");
        }

        var projection = Matrix4x4.CreateOrthographicOffCenter(0f, this._target.Width, this._target.Height, 0f, -10f, 100f);

        this._pass.Begin(this._commandList, this._target);
        this._pass.SetViewProjectionMatrix(new Matrix4x4(transform ?? Matrix3x2.Identity) * projection);
        this._opacityStack.Clear();
        this._opacityStack.Push(1f);
        this._clipRectStack.Clear();
        this._hasBegun = true;
    }

    /// <summary>
    /// Ends the batch and draws the remaining sprites.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called.</exception>
    public void End()
    {
        if (!this._hasBegun)
        {
            throw new InvalidOperationException("Begin must be called before calling End.");
        }

        this._pass.End();
        this._hasBegun = false;
    }

    /// <summary>
    /// Draws a texture at the specified position.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called.</exception>
    public void DrawTexture(ITexture texture, Vector2 position, Vector2 size)
    {
        this.DrawTexture(texture, position, size, Vector2.Zero, Vector2.One, Color.White);
    }

    /// <summary>
    /// Draws a texture at the specified position.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called.</exception>
    public void DrawTexture(ITextureRegion texture, Vector2 position, Vector2 size)
    {
        this.DrawTexture(texture.Texture, position, size, texture.TransformUV(Vector2.Zero), texture.TransformUV(Vector2.One), Color.White);
    }

    /// <summary>
    /// Draws a texture at the specified position with a transformation matrix.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    /// <param name="uvTopLeft">The top left UV coordinate of the texture.</param>
    /// <param name="uvBottomRight">The bottom right UV coordinate of the texture.</param>
    /// <param name="transform">A transformation matrix to apply to the texture.</param>
    /// <param name="color">The color to draw the texture with.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called.</exception>
    public void DrawTexture(ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color)
    {
        this.EnsureBegun();
        this._pass.Draw(texture, SpriteQuad.FromRectangle(position, size, uvTopLeft, uvBottomRight, in transform, this.ApplyOpacity(color)));
    }

    /// <summary>
    /// Draws a texture at the specified position.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    /// <param name="uvTopLeft">The top left UV coordinate of the texture.</param>
    /// <param name="uvBottomRight">The bottom right UV coordinate of the texture.</param>
    /// <param name="color">The color to draw the texture with.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called.</exception>
    public void DrawTexture(ITexture texture, Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color)
    {
        this.EnsureBegun();
        this._pass.Draw(texture, SpriteQuad.FromRectangle(position, size, uvTopLeft, uvBottomRight, this.ApplyOpacity(color)));
    }

    /// <summary>
    /// Draws a texture using a 9-patch scaling algorithm. This method is useful for drawing scalable UI elements.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position to draw the texture at.</param>
    /// <param name="size">The size of the texture to draw.</param>
    /// <param name="patchMargin">The margin of the unstretchable area of the texture. Any area inside this margin will be stretched.</param>
    /// <param name="color">The color to draw the texture with.</param>
    /// <param name="scale">The scale to apply to the texture.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called.</exception>
    public void DrawNinePatch(ITextureRegion texture, Vector2 position, Vector2 size, Thickness patchMargin, Color color, float scale)
    {
        this.EnsureBegun();

        var sizeTL = new Vector2(patchMargin.Left, patchMargin.Top);
        var sizeBR = new Vector2(patchMargin.Right, patchMargin.Bottom);
        var sizeTR = new Vector2(patchMargin.Right, patchMargin.Top);
        var sizeBL = new Vector2(patchMargin.Left, patchMargin.Bottom);

        var texelUV = Vector2.One / texture.Texture.Size;
        var uvExtTL = texture.TopLeft;
        var uvExtBR = texture.BottomRight;
        var uvIntTL = uvExtTL + sizeTL * texelUV;
        var uvIntBR = uvExtBR - sizeBR * texelUV;

        // Scale if the size is smaller (after calculating UVs)
        var minimumRequiredSize = (sizeTL + sizeBR) * scale;
        if (minimumRequiredSize.X > 0 && minimumRequiredSize.Y > 0)
        {
            if (size.X < minimumRequiredSize.X || size.Y < minimumRequiredSize.Y)
            {
                scale *= float.Min(size.X / minimumRequiredSize.X, size.Y / minimumRequiredSize.Y);
            }
        }

        sizeTL *= scale;
        sizeBR *= scale;
        sizeTR *= scale;
        sizeBL *= scale;
        var sizeSegmentCenter = size - sizeTL - sizeBR;

        color = this.ApplyOpacity(color);
        Span<SpriteQuad> quads = stackalloc SpriteQuad[9];
        int count = 0;

        if (sizeSegmentCenter.X > 0 && sizeSegmentCenter.Y > 0)
        {
            quads[count++] = SpriteQuad.FromRectangle(position + sizeTL, sizeSegmentCenter, uvIntTL, uvIntBR, color);
        }

        var posTL = new Vector2(0f, 0f);
        var posTR = new Vector2(size.X - sizeTR.X, 0f);
        var posBR = size - sizeBR;
        var posBL = new Vector2(0f, size.Y - sizeBL.Y);

        // Corner Top Left
        quads[count++] = SpriteQuad.FromRectangle(position + posTL, sizeTL,
            uvExtTL, uvIntTL, color);

        // Corner Top Right
        quads[count++] = SpriteQuad.FromRectangle(position + posTR, sizeTR,
            new Vector2(uvIntBR.X, uvExtTL.Y), new Vector2(uvExtBR.X, uvIntTL.Y), color);

        // Corner Bottom Left
        quads[count++] = SpriteQuad.FromRectangle(position + posBL, sizeBL,
            new Vector2(uvExtTL.X, uvIntBR.Y), new Vector2(uvIntTL.X, uvExtBR.Y), color);

        // Corner Bottom Right
        quads[count++] = SpriteQuad.FromRectangle(position + posBR, sizeBR,
            uvIntBR, uvExtBR, color);

        var sizeTop = new Vector2(size.X - sizeTL.X - sizeTR.X, sizeTL.Y);
        var sizeBottom = new Vector2(size.X - sizeBL.X - sizeBR.X, sizeBL.Y);
        var sizeLeft = new Vector2(sizeTL.X, size.Y - sizeTL.Y - sizeBL.Y);
        var sizeRight = new Vector2(sizeTR.X, size.Y - sizeTR.Y - sizeBR.Y);

        // Lateral Top
        quads[count++] = SpriteQuad.FromRectangle(position + new Vector2(sizeTL.X, 0f), sizeTop,
            new Vector2(uvIntTL.X, uvExtTL.Y), new Vector2(uvIntBR.X, uvIntTL.Y), color);

        // Lateral Bottom
        quads[count++] = SpriteQuad.FromRectangle(position + new Vector2(sizeBL.X, size.Y - sizeBL.Y), sizeBottom,
            new Vector2(uvIntTL.X, uvIntBR.Y), new Vector2(uvIntBR.X, uvExtBR.Y), color);

        // Lateral Left
        quads[count++] = SpriteQuad.FromRectangle(position + new Vector2(0f, sizeTL.Y), sizeLeft,
            new Vector2(uvExtTL.X, uvIntTL.Y), new Vector2(uvIntTL.X, uvIntBR.Y), color);

        // Lateral Right
        quads[count++] = SpriteQuad.FromRectangle(position + new Vector2(size.X - sizeTR.X, sizeTR.Y), sizeRight,
            new Vector2(uvIntBR.X, uvIntTL.Y), new Vector2(uvExtBR.X, uvIntBR.Y), color);

        this._pass.Draw(texture.Texture, quads[..count]);
    }

    /// <summary>
    /// Draws the given text at the specified position.
    /// </summary>
    /// <param name="font">The font to use.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="position">The position to draw the text at.</param>
    /// <param name="color">The color to draw the text with.</param>
    /// <param name="effect">The effect to apply to the text.</param>
    /// <param name="effectAmount">The amount of the effect to apply.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called.</exception>
    public void DrawText(SpriteFontBase font, string text, Vector2 position, Color color, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
    {
        this.EnsureBegun();

        // TODO: Use a LRU cache for text strings. Use Span<char> for the "text" parameter and only call .ToString() if the string is not in the cache.
        // Idk if this is worth it, but it could be a nice optimization.

        var fsColor = new FSColor(color.R, color.G, color.B, color.A);
        var style = TextStyle.None;
        font.DrawText(this._fontStashAdapter, text, position, fsColor, 0, Vector2.Zero, Vector2.One, 0, 0, 0, style, effect, effectAmount);
    }

    /// <summary>
    /// Draws a quad with the specified vertices.
    /// </summary>
    /// <param name="texture">The texture to use.</param>
    /// <param name="topLeft">The top left vertex of the quad.</param>
    /// <param name="topRight">The top right vertex of the quad.</param>
    /// <param name="bottomLeft">The bottom left vertex of the quad.</param>
    /// <param name="bottomRight">The bottom right vertex of the quad.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called.</exception>
    public void DrawQuad(ITexture texture, ref SpriteVertex topLeft, ref SpriteVertex topRight, ref SpriteVertex bottomLeft, ref SpriteVertex bottomRight)
    {
        this.EnsureBegun();

        if (this._opacityStack.Peek() < 1f)
        {
            this.ApplyOpacity(ref topLeft.Color);
            this.ApplyOpacity(ref topRight.Color);
            this.ApplyOpacity(ref bottomLeft.Color);
            this.ApplyOpacity(ref bottomRight.Color);
        }

        this._pass.Draw(texture, new SpriteQuad(topLeft, topRight, bottomRight, bottomLeft));
    }

    /// <summary>
    /// Draws a solid color rectangle at the specified position.
    /// </summary>
    /// <param name="position">The position to draw the rectangle at.</param>
    /// <param name="size">The size of the rectangle to draw.</param>
    /// <param name="color">The color to draw the rectangle with.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called.</exception>
    public void DrawRectangle(Vector2 position, Vector2 size, Color color)
    {
        this.DrawTexture(Texture.White, position, size, Vector2.Zero, Vector2.One, color);
    }

    /// <summary>
    /// Begins drawing with a new opacity value.
    /// </summary>
    /// <param name="opacity">The opacity to use.</param>
    public void PushOpacity(float opacity)
    {
        opacity *= this._opacityStack.Peek();
        this._opacityStack.Push(opacity);
    }

    /// <summary>
    /// Ends drawing with the current opacity value.
    /// </summary>
    public void PopOpacity()
    {
        this._opacityStack.Pop();
    }

    /// <summary>
    /// Begins drawing with a new scissor rectangle mask applied. This method will clip all drawing to the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to clip to.</param>
    public void PushScissorRectangle(Rect rect)
    {
        this._clipRectStack.Push(rect);
        this._pass.SetScissorRectangle(rect);
    }

    /// <summary>
    /// Ends drawing with the current scissor rectangle mask applied.
    /// </summary>
    public void PopScissorRectangle()
    {
        if (this._clipRectStack.Count == 0) return;

        this._clipRectStack.Pop();
        if (this._clipRectStack.Count > 0)
        {
            this._pass.SetScissorRectangle(this._clipRectStack.Peek());
        }
        else
        {
            this._pass.ClearScissorRectangle();
        }
    }

    private void EnsureBegun()
    {
        if (!this._hasBegun)
        {
            throw new InvalidOperationException("Begin must be called before calling Draw.");
        }
    }

    private Color ApplyOpacity(Color color)
    {
        return new Color(color.R, color.G, color.B, (byte)(color.A * this._opacityStack.Peek()));
    }

    private void ApplyOpacity(ref uint color)
    {
        color = color & 0x00FFFFFF | (uint)((color >> 24) * this._opacityStack.Peek()) << 24;
    }
}
