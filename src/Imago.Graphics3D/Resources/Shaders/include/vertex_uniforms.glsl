#define MAX_BONES 64

// I think D3D crashed if this was set to 1024. I think it was a bug. But this fixes it.
// TODO: Future self, please document this better.
#ifdef D3D11
#define MAX_INSTANCES_PER_BUFFER 2
#else
#define MAX_INSTANCES_PER_BUFFER 1024
#endif

#define RENDER_PASS_UNIFORM_SET 0
#define TRANSFORMS_UNIFORM_SET 1
#define MATERIAL_UNIFORM_SET 2
#define INSTANCE_UNIFORM_SET 3
#define SKELETON_UNIFORM_SET 4


layout(set = TRANSFORMS_UNIFORM_SET, binding = 0, std140) uniform TransformDataBuffer {
    mat4 World[MAX_INSTANCES_PER_BUFFER];
} transform;

struct InstanceData
{
    vec4 AlbedoColor; // rgb = color, a = alpha
    vec4 TextureST; // x = width, y = height, z = offsetX, w = offsetY
    vec4 HighlightColor; // rgb = highlight color, a = highlight intensity
};

layout(set = INSTANCE_UNIFORM_SET, binding = 0, std140) uniform InstanceDataBuffer {
    InstanceData Data[MAX_INSTANCES_PER_BUFFER];
} instance;

#ifdef USE_JOINTS
layout(set = SKELETON_UNIFORM_SET, binding = 0, std140) uniform BonesDataBuffer {
    mat4 Bones[MAX_INSTANCES_PER_BUFFER];
} bones;
#endif
