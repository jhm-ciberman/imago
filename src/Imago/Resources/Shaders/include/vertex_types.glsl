// Vertex shader type definitions

#ifndef VERTEX_TYPES_GLSL
#define VERTEX_TYPES_GLSL

/**
 * Input data available to vertex shaders.
 */
struct VertexInput
{
    vec3 LocalPosition;     // Position in model space
    vec3 WorldPosition;     // Position in world space (after model transform)
    vec3 Normal;            // Normal in world space
    vec2 UV;                // Texture coordinates
    vec4 Color;             // Per-instance color
    vec4 Highlight;         // Per-instance highlight
};

/**
 * Output from vertex shaders that can be modified.
 */
struct VertexOutput
{
    vec3 Position;          // World position (can be displaced)
    vec3 Normal;            // World normal (can be modified)
};

#endif
