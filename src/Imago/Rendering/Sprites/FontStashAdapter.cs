using FontStashSharp.Interfaces;
using Imago.Textures;
using NeoVeldrid;

namespace Imago.Rendering.Sprites;

internal class FontStashAdapter : ITexture2DManager, IFontStashRenderer2
{
    private readonly GraphicsDevice _gd;

    private readonly SpriteBatch _spriteBatch;

    ITexture2DManager IFontStashRenderer2.TextureManager => this;

    public FontStashAdapter(GraphicsDevice gd, SpriteBatch spriteBatch)
    {
        this._gd = gd;
        this._spriteBatch = spriteBatch;
    }

    object ITexture2DManager.CreateTexture(int width, int height)
    {
        return new DirectTexture(this._gd, (uint)width, (uint)height);
    }

    void ITexture2DManager.SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
    {
        ((DirectTexture)texture).Update(data, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    System.Drawing.Point ITexture2DManager.GetTextureSize(object texture)
    {
        var texture2D = (DirectTexture)texture;
        return new System.Drawing.Point((int)texture2D.Width, (int)texture2D.Height);
    }

    void IFontStashRenderer2.DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
    {
        var v1 = new SpriteVertex(topLeft.Position, topLeft.TextureCoordinate, topLeft.Color.PackedValue);
        var v2 = new SpriteVertex(topRight.Position, topRight.TextureCoordinate, topRight.Color.PackedValue);
        var v3 = new SpriteVertex(bottomRight.Position, bottomRight.TextureCoordinate, bottomRight.Color.PackedValue);
        var v4 = new SpriteVertex(bottomLeft.Position, bottomLeft.TextureCoordinate, bottomLeft.Color.PackedValue);
        this._spriteBatch.DrawQuad((ITexture)texture, in v1, in v2, in v3, in v4);
    }
}
