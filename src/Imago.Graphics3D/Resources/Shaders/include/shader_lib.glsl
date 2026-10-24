
/**
 * Gets the world-space position of the current vertex.
 *
 * If USE_JOINTS is defined, applies bone transformations weighted by Joints and Weights.
 * Otherwise, transforms the position by the world matrix only.
 * @return World-space position as a vec4 (homogeneous coordinates).
 */
vec4 GetVertexWorldPos()
{
#ifdef USE_JOINTS
    mat4 boneTransformation  = bones.Bones[Offsets.z + Joints[0]] * Weights[0];
    boneTransformation      += bones.Bones[Offsets.z + Joints[1]] * Weights[1];
    boneTransformation      += bones.Bones[Offsets.z + Joints[2]] * Weights[2];
    boneTransformation      += bones.Bones[Offsets.z + Joints[3]] * Weights[3];

    return transform.World[Offsets.x] * boneTransformation * vec4(Position, 1.0);
#else
    return transform.World[Offsets.x] * vec4(Position, 1.0);
#endif
}

/**
 * Gets the normal of the current vertex in world space.
 */
vec3 GetVertexNormal()
{
    // For non uniform scaling, we need to transform the normal to world space
    // return inverse(transpose(mat3(transform.World[Offsets.x]))) * Normal;

    // But for uniform scaling, we can skip the inverse
    return normalize(mat3(transform.World[Offsets.x]) * Normal);
}

/**
 * Gets the picking ID of the current instance.
 */
uint GetPickingID()
{
    return Offsets.w;
}

/**
 * Gets the instance data of the current vertex.
 */
InstanceData GetInstanceData()
{
    return instance.Data[Offsets.y];
}

/**
 * Applies gamma correction (linear to sRGB) to a color.
 *
 * @param color The color to be gamma corrected.
 * @return The gamma corrected color.
 */
vec3 GammaCorrect(vec3 color)
{
    return pow(color, vec3(1.0 / 2.2));
}
