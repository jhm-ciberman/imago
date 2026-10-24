// x = TransformDataBuffer offset
// y = InstanceDataBuffer offset
// z = BonesDataBuffer offset
// w = PickingID
layout(location = 0) in uvec4 Offsets;

layout(location = 1) in vec3 Position;
layout(location = 2) in vec3 Normal;
layout(location = 3) in vec2 TextureCoords;

#ifdef USE_JOINTS
layout(location = 4) in uvec4 Joints;
layout(location = 5) in vec4 Weights;
#endif

#ifdef USE_LIGHT
layout(location = 4) in vec2 Light;
#endif
