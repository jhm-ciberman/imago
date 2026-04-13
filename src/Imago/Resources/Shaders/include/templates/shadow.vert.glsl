// Vertex shader template for shadow pass
// User must implement: VertexOutput Vertex(VertexInput vertexInput)

#version 450

#include "@imago/include/vertex_layout.glsl"
#include "@imago/include/vertex_uniforms.glsl"
#include "@imago/include/vertex_types.glsl"
#include "@imago/include/shader_lib.glsl"

layout(set = RENDER_PASS_UNIFORM_SET, binding = 0, std140) uniform ShadowMapDataBuffer
{
    mat4 ShadowMapMatrix;
    vec4 ShadowBias;
    vec4 LightDirection;
};

layout(set = RENDER_PASS_UNIFORM_SET, binding = 1, std140) uniform GlobalDataBuffer
{
    vec3 CameraPosition;
    float Time;
};

#ifdef ENABLE_ALPHA_TEST
layout(location = 0) out vec2 fsin_TexCoords;
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

vec3 ApplyShadowBias(vec3 positionWS, vec3 normalWS)
{
    float depthBias = ShadowBias.x;
    float normalOffset = ShadowBias.y;
    vec3 lightDir = LightDirection.xyz;

    float invNdotL = 1.0 - clamp(dot(lightDir, normalWS), 0.0, 1.0);
    float scale = invNdotL * normalOffset;

    return positionWS + lightDir * depthBias + normalWS * scale;
}

// ========== USER CODE ==========
{{USER_CODE}}
// ===============================

void main()
{
    VertexInput vertexInput = BuildVertexInput();
    VertexOutput v = Vertex(vertexInput);

    vec3 biasedPos = ApplyShadowBias(v.Position, v.Normal);
    gl_Position = ShadowMapMatrix * vec4(biasedPos, 1.0);

#ifdef ENABLE_ALPHA_TEST
    fsin_TexCoords = vertexInput.UV;
#endif
}
