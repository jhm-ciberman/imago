// Vertex shader template for forward pass
// User must implement: VertexOutput Vertex(VertexInput vertexInput)

#version 450

#include "@imago/include/vertex_layout.glsl"
#include "@imago/include/vertex_uniforms.glsl"
#include "@imago/include/vertex_types.glsl"
#include "@imago/include/globals.glsl"
#include "@imago/include/shader_lib.glsl"

layout(set = RENDER_PASS_UNIFORM_SET, binding = 0, std140) uniform CameraDataBuffer
{
    mat4 ViewProjection;
    mat4 ShadowMapMatrices[4];
} pass;

#define SHADOWMAP_CASCADE_COUNT 4

layout(location = 0) out vec2 fsin_TexCoords;
layout(location = 1) out vec3 fsin_Normal;
layout(location = 2) out vec4 fsin_AlbedoColor;
layout(location = 3) out vec4 fsin_HighlightColor;
layout(location = 4) out vec3 fsin_WSPos;
layout(location = 5) out vec4 fsin_ShadowMapCoords[SHADOWMAP_CASCADE_COUNT];
#ifdef USE_LIGHT
layout(location = 6 + SHADOWMAP_CASCADE_COUNT) out vec2 fsin_LightLevel;
#endif

VertexInput BuildVertexInput()
{
    VertexInput i;
    vec4 worldPos = GetVertexWorldPos();
    InstanceData instanceData = GetInstanceData();

    i.LocalPosition = Position;
    i.WorldPosition = worldPos.xyz;
    i.Normal = GetVertexNormal();
    i.UV = TextureCoords * instanceData.TextureST.xy + instanceData.TextureST.zw;
    i.Color = instanceData.AlbedoColor;
    i.Highlight = instanceData.HighlightColor;
    return i;
}

// ========== USER CODE ==========
{{USER_CODE}}
// ===============================

void main()
{
    VertexInput vertexInput = BuildVertexInput();
    VertexOutput v = Vertex(vertexInput);

    gl_Position = pass.ViewProjection * vec4(v.Position, 1.0);

    fsin_TexCoords = vertexInput.UV;
    fsin_Normal = v.Normal;
    fsin_WSPos = v.Position;
    fsin_AlbedoColor = vertexInput.Color;
    fsin_HighlightColor = vertexInput.Highlight;

#ifdef USE_LIGHT
    fsin_LightLevel = Light;
#endif

    for (uint i = 0; i < SHADOWMAP_CASCADE_COUNT; i++)
    {
        fsin_ShadowMapCoords[i] = pass.ShadowMapMatrices[i] * vec4(v.Position, 1.0);
    }
}
