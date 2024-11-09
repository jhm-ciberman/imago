using FontStashSharp.Interfaces;
using LifeSim.Imago.Textures;
using Veldrid;

namespace LifeSim.Imago.Rendering.Sprites;

internal class FontStashAdapter : ITexture2DManager, IFontStashRenderer2
{
    private readonly GraphicsDevice _gd;

    private readonly DrawingContext _batcher;

    ITexture2DManager IFontStashRenderer2.TextureManager => this;

    public FontStashAdapter(GraphicsDevice gd, DrawingContext batcher)
    {
        this._gd = gd;
        this._batcher = batcher;
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
        var v3 = new SpriteVertex(bottomLeft.Position, bottomLeft.TextureCoordinate, bottomLeft.Color.PackedValue);
        var v4 = new SpriteVertex(bottomRight.Position, bottomRight.TextureCoordinate, bottomRight.Color.PackedValue);
        this._batcher.DrawQuad((ITexture)texture, ref v1, ref v2, ref v3, ref v4);
    }
}
