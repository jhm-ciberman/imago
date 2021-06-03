namespace LifeSim.Engine.Rendering
{
    [System.Flags]
    public enum PassKind
    {
        Forward    = 0,
        Shadowmap  = 1,
        Sprites    = 2,
        Fullscreen = 3,
    }
}