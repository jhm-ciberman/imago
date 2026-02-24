using Imago.Support.Drawing;

namespace Imago.Controls.Drawing;

/// <summary>
/// A static class with predefined backgrounds.
/// </summary>
public static class Backgrounds
{
    /// <summary>
    /// Gets a brush that paints the entire area with the color white.
    /// </summary>
    public static IBackground White { get; } = new ColorBackground(Color.White);

    /// <summary>
    /// Gets a brush that paints the entire area with the color gray.
    /// </summary>
    public static IBackground Gray { get; } = new ColorBackground(Color.Gray);

    /// <summary>
    /// Gets a brush that paints the entire area with the color light gray.
    /// </summary>
    public static IBackground LightGray { get; } = new ColorBackground(Color.LightGray);

    /// <summary>
    /// Gets a brush that paints the entire area with the color dark gray.
    /// </summary>
    public static IBackground DarkGray { get; } = new ColorBackground(Color.DarkGray);

    /// <summary>
    /// Gets a brush that paints the entire area with the color cool gray.
    /// </summary>
    public static IBackground CoolGray { get; } = new ColorBackground(Color.CoolGray);

    /// <summary>
    /// Gets a brush that paints the entire area with the color black.
    /// </summary>
    public static IBackground Black { get; } = new ColorBackground(Color.Black);

    /// <summary>
    /// Gets a brush that paints the entire area with the color red.
    /// </summary>
    public static IBackground Red { get; } = new ColorBackground(Color.Red);

    /// <summary>
    /// Gets a brush that paints the entire area with the color green.
    /// </summary>
    public static IBackground Green { get; } = new ColorBackground(Color.Green);

    /// <summary>
    /// Gets a brush that paints the entire area with the color blue.
    /// </summary>
    public static IBackground Blue { get; } = new ColorBackground(Color.Blue);

    /// <summary>
    /// Gets a brush that paints the entire area with the color yellow.
    /// </summary>
    public static IBackground Yellow { get; } = new ColorBackground(Color.Yellow);

    /// <summary>
    /// Gets a brush that paints the entire area with the color cyan.
    /// </summary>
    public static IBackground Cyan { get; } = new ColorBackground(Color.Cyan);

    /// <summary>
    /// Gets a brush that paints the entire area with the color magenta.
    /// </summary>
    public static IBackground Magenta { get; } = new ColorBackground(Color.Magenta);

    /// <summary>
    /// Gets a brush that paints the entire area with the color transparent.
    /// </summary>
    public static IBackground Transparent { get; } = new ColorBackground(Color.Transparent);

    /// <summary>
    /// Gets a brush that paints the entire area with the color orange.
    /// </summary>
    public static IBackground Orange { get; } = new ColorBackground(Color.Orange);

    /// <summary>
    /// Gets a brush that paints the entire area with the color purple.
    /// </summary>
    public static IBackground Purple { get; } = new ColorBackground(Color.Purple);

    /// <summary>
    /// Gets a brush that paints the entire area with the color brown.
    /// </summary>
    public static IBackground Brown { get; } = new ColorBackground(Color.Brown);

    /// <summary>
    /// Gets a brush that paints the entire area with the color pink.
    /// </summary>
    public static IBackground Pink { get; } = new ColorBackground(Color.Pink);

    /// <summary>
    /// Gets a brush that paints the entire area with the color indigo.
    /// </summary>
    public static IBackground Indigo { get; } = new ColorBackground(Color.Indigo);

    /// <summary>
    /// Gets a brush that paints the entire area with the color violet.
    /// </summary>
    public static IBackground Violet { get; } = new ColorBackground(Color.Violet);

    /// <summary>
    /// Gets a brush that paints the entire area with the color ghost white.
    /// </summary>
    public static IBackground GhostWhite { get; } = new ColorBackground(Color.GhostWhite);
}
