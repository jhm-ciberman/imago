// Shadow map sampling utilities
// Requires: SHADOWMAP_CASCADE_COUNT, ShadowMapDistances, fsin_ShadowMapCoords,
//           ShadowMapTexture, ShadowMapSampler, texel_space_lighting.glsl

/**
 * Smooth rectangle transition for shadow edge fading.
 */
float InsideRectangleSmooth(vec2 p, vec2 bottom_left, vec2 top_right, float transition_area)
{
    vec2 s = smoothstep(bottom_left, bottom_left + vec2(transition_area), p) -
             smoothstep(top_right - vec2(transition_area), top_right, p);
    return(s.x * s.y);
}

/**
 * Determines which shadow cascade to use based on distance from camera.
 *
 * @param distanceToCamera The fragment's distance from the camera.
 * @return The cascade index (0 to SHADOWMAP_CASCADE_COUNT-1).
 */
int GetCascadeIndex(float distanceToCamera)
{
    for (int i = 0; i < SHADOWMAP_CASCADE_COUNT - 1; i++)
    {
        if (distanceToCamera < ShadowMapDistances[i])
        {
            return i;
        }
    }
    return SHADOWMAP_CASCADE_COUNT - 1;
}

/**
 * Gets the distance from the fragment to the camera, along the camera view direction.
 * The value is in world units, the same as ShadowMapDistances.
 * With an orthographic camera the value is always 1.
 */
float GetDistanceToCamera()
{
    // gl_FragCoord.w is 1 / clip.w on every backend. SPIRV-Cross converts the HLSL SV_Position.w to match.
    return 1.0 / gl_FragCoord.w;
}

/**
 * Samples the shadow map and returns the shadow factor.
 *
 * @param distanceToCamera The fragment's distance from the camera.
 * @return Shadow factor where 0.0 = fully lit, 1.0 = fully shadowed.
 */
float SampleShadowMap(float distanceToCamera)
{
#ifdef ENABLE_RECEIVE_SHADOWS
    int cascadeIndex = GetCascadeIndex(distanceToCamera);
    vec4 shadowMapCoords = fsin_ShadowMapCoords[cascadeIndex];

    #ifdef ENABLE_PIXEL_PERFECT_SHADOWS
        // We only want to snap the shadowmap coordinates if the 2x2 neighbor pixels are the same
        // cascadeIndex. Otherwise, we will get artifacts near the transition zone between two cascades.
        if (dFdx(cascadeIndex) == 0.0 && dFdy(cascadeIndex) == 0.0) {
            // Apply the snap to texels function to the shadow map coordinates
            vec2 deltaST = GetDeltaST(fsin_TexCoords);
            shadowMapCoords.xyz = SNAP_TO_TEXELS(deltaST, shadowMapCoords.xyz);
        }
    #endif

    #ifdef ENABLE_SHADOW_CASCADES
        vec4 pShadow = vec4(shadowMapCoords.xy, cascadeIndex, shadowMapCoords.z);
        float shadow = texture(sampler2DArrayShadow(ShadowMapTexture, ShadowMapSampler), pShadow);
    #else
        vec3 pShadow = vec3(shadowMapCoords.xy, shadowMapCoords.z);
        float shadow = texture(sampler2DShadow(ShadowMapTexture, ShadowMapSampler), pShadow);
    #endif

    if(shadowMapCoords.z > 1.0)
        shadow = 0.0;

    #ifdef SHADOWMAP_FADE
        shadow = shadow * InsideRectangleSmooth(shadowMapCoords.xy, vec2(0.0, 0.0), vec2(1.0, 1.0), 0.05);
    #endif

    return shadow;

#else // ENABLE_RECEIVE_SHADOWS
    return 0.0;
#endif
}
