#version 450

// -----------------------------------------------
// Vertex Shader Input
// -----------------------------------------------

layout(location = 0) in flat uint fsin_PickingID;
#ifdef ENABLE_ALPHA_TEST
layout(location = 1) in vec2 fsin_TexCoords;
#endif

// -----------------------------------------------
// Uniforms
// -----------------------------------------------

#define RENDER_PASS_UNIFORM_SET 0
#define TRANSFORMS_UNIFORM_SET 1
#define MATERIAL_UNIFORM_SET 2
#define INSTANCE_UNIFORM_SET 3

#ifdef ENABLE_ALPHA_TEST
layout(set = MATERIAL_UNIFORM_SET, binding = 0) uniform texture2D SurfaceTexture;
layout(set = MATERIAL_UNIFORM_SET, binding = 1) uniform sampler SurfaceSampler;
#endif

// -----------------------------------------------
// Fragment Shader Output
// -----------------------------------------------

layout(location = 0) out uvec4 fsout_pickID;

// -----------------------------------------------
// Shader Code
// -----------------------------------------------

void main()
{
#ifdef ENABLE_ALPHA_TEST
    float alpha = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_TexCoords).a;

    if (alpha < 0.1) {
        discard;
    }
#endif

    fsout_pickID = uvec4(fsin_PickingID, 0, 0, 0);
}
