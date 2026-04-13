// Surface shader template for forward pass
// User must implement: SurfaceOutput Surface(SurfaceInput input)

#version 450

#define SHADOWMAP_CASCADE_COUNT 4

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 1) in vec3 fsin_Normal;
layout(location = 2) in vec4 fsin_AlbedoColor;
layout(location = 3) in vec4 fsin_HighlightColor;
layout(location = 4) in vec3 fsin_WSPos;
layout(location = 5) in vec4 fsin_ShadowMapCoords[SHADOWMAP_CASCADE_COUNT];
#ifdef USE_LIGHT
layout(location = 6 + SHADOWMAP_CASCADE_COUNT) in vec2 fsin_LightLevel;
#endif

#define RENDER_PASS_UNIFORM_SET 0
#define TRANSFORMS_UNIFORM_SET 1
#define MATERIAL_UNIFORM_SET 2
#define INSTANCE_UNIFORM_SET 3

layout(set = RENDER_PASS_UNIFORM_SET, binding = 1, std140) uniform LightDataBuffer
{
    vec4 AmbientColor;
    vec4 MainLightColor;
    vec4 ShadowColor;
    vec3 MainLightDirection;
    vec3 FogColor;
    vec2 FogRange;
    vec4 ShadowMapDistances;
};

#ifdef ENABLE_SHADOW_CASCADES
layout(set = RENDER_PASS_UNIFORM_SET, binding = 2) uniform texture2DArray ShadowMapTexture;
#else
layout(set = RENDER_PASS_UNIFORM_SET, binding = 2) uniform texture2D ShadowMapTexture;
#endif

layout(set = RENDER_PASS_UNIFORM_SET, binding = 3) uniform sampler ShadowMapSampler;

layout(set = MATERIAL_UNIFORM_SET, binding = 0) uniform texture2D SurfaceTexture;
layout(set = MATERIAL_UNIFORM_SET, binding = 1) uniform sampler SurfaceSampler;

#include "@imago/include/globals.glsl"
#include "@imago/include/surface_types.glsl"
#include "@imago/include/texel_space_lighting.glsl"
#include "@imago/include/shadow_sampling.glsl"
#include "@imago/include/lighting.glsl"
#include "@imago/include/fog.glsl"

layout(location = 0) out vec4 fsout_color;

SurfaceInput BuildSurfaceInput()
{
    SurfaceInput i;
    i.UV = fsin_TexCoords;
    i.WorldPosition = fsin_WSPos;
    i.Normal = normalize(fsin_Normal);
    i.ViewDir = normalize(CameraPosition - fsin_WSPos);
    i.Color = fsin_AlbedoColor;
    i.Highlight = fsin_HighlightColor;
#ifdef USE_LIGHT
    i.LightLevel = fsin_LightLevel;
#else
    i.LightLevel = vec2(1.0, 0.0);
#endif
    return i;
}

// ========== USER CODE ==========
{{USER_CODE}}
// ===============================

void main()
{
#ifdef ENABLE_WIREFRAME
    fsout_color = vec4(0.0, 0.0, 0.0, 1.0);
    return;
#endif

    SurfaceInput surfaceIn = BuildSurfaceInput();
    SurfaceOutput surface = Surface(surfaceIn);

#ifdef ENABLE_ALPHA_TEST
    if (surface.Alpha < 0.05)
    {
        discard;
    }
#endif

#ifndef ENABLE_TRANSPARENT
    surface.Alpha = 1.0;
#endif

    float sunlight = surfaceIn.LightLevel.x;
    float distanceToCamera = GetDistanceToCamera();
    float shadow = SampleShadowMap(distanceToCamera);

    float diffuse = LightingLambert(MainLightDirection, surfaceIn.Normal);
    float sunLightIntensity = AmbientColor.a;
    float shadowIntensity = ShadowColor.a;
    shadow = 1.0 - shadow * shadowIntensity;

    vec3 result = sunLightIntensity * AmbientColor.rgb + diffuse * shadow;
    result = result * surface.Albedo;
    result = result * sunlight * sunlight;
    result += surface.Emission;
    result = mix(result, surfaceIn.Highlight.rgb, surfaceIn.Highlight.a);

#ifdef ENABLE_COLOR_WRITE
    fsout_color = vec4(result, surface.Alpha);
#else
    #ifdef ENABLE_RECEIVE_SHADOWS
        fsout_color = vec4(0.0, 0.0, 0.0, 1.0 - shadow);
    #else
        fsout_color = vec4(0.0);
    #endif
#endif

#ifdef ENABLE_FOG
    fsout_color.rgb = ApplyFog(fsout_color.rgb, distanceToCamera);
#endif

    fsout_color.rgb = pow(fsout_color.rgb, vec3(1.0 / 2.2));
}
