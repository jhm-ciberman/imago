using System.Numerics;
using LifeSim.Imago.Textures;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents a cursor with a texture and hotspot for GUI display.
/// </summary>
public class Cursor
{
    /// <summary>
    /// Gets the texture path for the cursor.
    /// </summary>
    public string TexturePath { get; }

    /// <summary>
    /// Gets the hotspot offset for the cursor. This is the point in the cursor texture that represents the actual cursor position.
    /// </summary>
    public Vector2 Hotspot { get; }

    /// <summary>
    /// Gets the loaded texture for the cursor.
    /// </summary>
    public ImageTexture Texture { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Cursor"/> class.
    /// </summary>
    /// <param name="texturePath">The path to the cursor texture.</param>
    /// <param name="hotspot">The hotspot offset for the cursor.</param>
    public Cursor(string texturePath, Vector2 hotspot)
    {
        this.TexturePath = texturePath;
        this.Hotspot = hotspot;
        this.Texture = new ImageTexture(this.TexturePath, srgb: false);
    }
}
