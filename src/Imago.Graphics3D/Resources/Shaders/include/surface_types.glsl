// Surface shader type definitions

#ifndef SURFACE_TYPES_GLSL
#define SURFACE_TYPES_GLSL

/**
 * Input data available to surface shaders.
 */
struct SurfaceInput
{
    vec2 UV;                // Texture coordinates
    vec3 WorldPosition;     // Fragment world position
    vec3 Normal;            // Interpolated normal (world space, normalized)
    vec3 ViewDir;           // Direction to camera (normalized)
    vec4 Color;             // Per-instance albedo color
    vec4 Highlight;         // Per-instance highlight (RGB + intensity)
    vec2 LightLevel;        // Baked lighting (sunlight, blocklight)
};

/**
 * Output from surface shaders that defines the material appearance.
 */
struct SurfaceOutput
{
    vec3 Albedo;            // Base color
    vec3 Emission;          // Self-illumination color
    float Alpha;            // Opacity
};

#endif
