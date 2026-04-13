// Standard surface shader - default texture sampling

SurfaceOutput Surface(SurfaceInput surfaceInput)
{
    SurfaceOutput o;
    vec4 tex = texture(sampler2D(SurfaceTexture, SurfaceSampler), surfaceInput.UV);
    o.Albedo = tex.rgb * surfaceInput.Color.rgb;
    o.Alpha = tex.a * surfaceInput.Color.a;
    o.Emission = vec3(0.0);
    return o;
}
