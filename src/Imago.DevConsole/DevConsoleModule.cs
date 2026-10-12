using Microsoft.Extensions.DependencyInjection;

namespace Imago.DevConsole;

/// <summary>
/// Adds the developer console layer to the stage of the application.
/// </summary>
public sealed class DevConsoleModule : Module
{
    /// <inheritdoc />
    public override void Register(IServiceCollection services)
    {
        services.AddSingleton<ConsoleLayer>();
    }

    /// <inheritdoc />
    public override void Boot(Application app)
    {
        app.Stage.AddLayer(app.Services.GetRequiredService<ConsoleLayer>());
    }
}
