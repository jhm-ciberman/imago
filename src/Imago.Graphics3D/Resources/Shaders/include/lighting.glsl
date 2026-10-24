// Lighting calculations for forward pass

/**
 * Calculates Lambert diffuse lighting.
 *
 * @param lightDir The direction of the light source (normalized).
 * @param normal The surface normal (normalized).
 * @return The diffuse lighting factor (0.0 to 1.0).
 */
float LightingLambert(vec3 lightDir, vec3 normal)
{
#ifdef ENABLE_HALF_LAMBERT
    // https://developer.valvesoftware.com/wiki/Half_Lambert
    float diffuse = dot(normal, lightDir) * 0.5 + 0.5;
    diffuse *= diffuse;
#else
    float diffuse = max(dot(normal, lightDir), 0.0);
#endif

    return diffuse;
}
