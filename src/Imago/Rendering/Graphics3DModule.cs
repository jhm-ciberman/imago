using Microsoft.Extensions.DependencyInjection;

namespace Imago.Rendering;

/// <summary>
/// Adds the 3D renderer, so the application can render 3D scenes.
/// </summary>
public sealed class Graphics3DModule : Module
{
    /// <inheritdoc />
    public override void Register(IServiceCollection services)
    {
        services.AddSingleton<Forward3DRenderer>();
    }

    /// <inheritdoc />
    public override void Boot(Application app)
    {
        app.Services.GetRequiredService<Forward3DRenderer>();
    }
}
