// Vertex shader template for picking pass
// User must implement: VertexOutput Vertex(VertexInput vertexInput)

#version 450

#include "@imago/include/vertex_layout.glsl"
#include "@imago/include/vertex_uniforms.glsl"
#include "@imago/include/vertex_types.glsl"
#include "@imago/include/shader_lib.glsl"

layout(set = RENDER_PASS_UNIFORM_SET, binding = 0, std140) uniform CameraDataBuffer
{
    mat4 ViewProjection;
} pass;

layout(set = RENDER_PASS_UNIFORM_SET, binding = 1, std140) uniform GlobalDataBuffer
{
    vec3 CameraPosition;
    float Time;
};

layout(location = 0) out flat uint fsin_PickingID;
#ifdef ENABLE_ALPHA_TEST
layout(location = 1) out vec2 fsin_TexCoords;
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
    fsin_PickingID = GetPickingID();

#ifdef ENABLE_ALPHA_TEST
    fsin_TexCoords = vertexInput.UV;
#endif
}
