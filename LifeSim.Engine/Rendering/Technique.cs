using System;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class Technique
{
    public int ResourceCount { get; }

    public ResourceLayout ResourceLayout => this.ForwardShader.MaterialResourceLayout;

    public Shader ForwardShader { get; private set; }

    public Shader ShadowMapShader { get; private set; }

    public Technique(Shader forwardShader, Shader shadowMapShader)
    {
        if (forwardShader.MaterialResourceLayout != shadowMapShader.MaterialResourceLayout)
        {
            throw new ArgumentException("Forward and shadowmap shaders must use the same resource layout.");
        }

        this.ForwardShader = forwardShader;
        this.ShadowMapShader = shadowMapShader;

        this.ResourceCount = forwardShader.Textures.Length * 2;
    }
}