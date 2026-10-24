// Global uniforms available to all shaders
// Provides Time and CameraPosition for effects

#ifndef GLOBALS_GLSL
#define GLOBALS_GLSL

layout(set = RENDER_PASS_UNIFORM_SET, binding = 4, std140) uniform GlobalDataBuffer
{
    vec3 CameraPosition;
    float Time;
};

#endif
