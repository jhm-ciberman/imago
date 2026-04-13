#version 450

// -----------------------------------------------
// Vertex Shader Input
// -----------------------------------------------

#ifdef ENABLE_ALPHA_TEST
layout(location = 0) in vec2 fsin_TexCoords;
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
// Shader Code
// -----------------------------------------------

void main()
{
#if defined(ENABLE_ALPHA_TEST) && !defined(ENABLE_WIREFRAME)
    float alpha = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_TexCoords).a;

    if (alpha < 0.5) {
        discard;
    }
#endif
}
