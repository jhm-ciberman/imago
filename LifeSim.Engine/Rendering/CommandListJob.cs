using System;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;


internal class CommandListJob : IDisposable
{
    public CommandList CommandList { get; }

    public string Name { get; }

    public Fence Fence { get; }
    public IRenderingPass[] Passes { get; }

    public CommandListJob(string name, ResourceFactory factory, params IRenderingPass[] passes)
    {
        this.Name = name;
        this.CommandList = factory.CreateCommandList();
        this.Fence = factory.CreateFence(signaled: false);
        this.Passes = passes;
    }

    public void Execute(Scene scene)
    {
        this.CommandList.Begin();
        foreach (var pass in this.Passes)
        {
            pass.Render(this.CommandList, scene);
        }
        this.CommandList.End();
    }

    public void SubmitCommands(GraphicsDevice gd)
    {
        gd.SubmitCommands(this.CommandList, this.Fence);
    }

    public void Dispose()
    {
        this.CommandList.Dispose();
    }
}
