// Fog calculations for forward pass
// Requires: FogColor, FogRange uniforms

/**
 * Applies distance-based fog to a color.
 *
 * @param color The input color before fog.
 * @param distanceToCamera The fragment's distance from the camera.
 * @return The color with fog applied.
 */
vec3 ApplyFog(vec3 color, float distanceToCamera)
{
    float fogFactor = clamp(1.0 - (distanceToCamera - FogRange.x) / (FogRange.y - FogRange.x), 0.0, 1.0);
    return mix(FogColor.rgb, color, fogFactor);
}
