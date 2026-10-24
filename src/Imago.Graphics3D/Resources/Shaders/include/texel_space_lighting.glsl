#ifndef CIBERMAN_TSL_INCLUDED
#define CIBERMAN_TSL_INCLUDED

vec2 GetDeltaST(vec2 originalUV)
{
    vec2 textureSize = textureSize(sampler2D(SurfaceTexture, SurfaceSampler), 0);
    vec2 texelSize = 1.0 / textureSize;
    // Gets the UV coordinate of the center of the texel (using the BaseMap texel size as reference)
    vec2 baseMapTexelCenter = floor(originalUV * textureSize) / textureSize + (texelSize / 2.0);

    // Calculate how much the texture UV coords need to
    // shift to be at the center of the nearest texel.
    vec2 uvToConvert = (baseMapTexelCenter - originalUV);

    // Calculate how much the texture coords vary over fragment space.
    // This essentially defines a 2x2 matrix that gets
    // texture space (UV) deltas from fragment space (ST) deltas
    // Note: I call fragment space (S,T) to disambiguate.
    vec2 dUVdS = dFdx(originalUV);
    vec2 dUVdT = dFdy(originalUV);
    //mat2 m = mat2(
    //    dUVdT[1], -dUVdT[0],
    //    -dUVdS[1], dUVdS[0]);

    mat2 m = mat2(
        -dUVdT[1], dUVdT[0],
        -dUVdS[1], dUVdS[0]);

    // Invert the fragment from texture matrix
    m = m / determinant(m);

    // Convert the UV delta to a fragment space delta
    return uvToConvert * m;
}

// If you are writting your own shader, you can use this macro to "snap" any value to the nearest texel
// You first need to call GetDeltaST with the original UV texture coordinate of your albedo texture and then
// call this macro with that value and the vec2 value you want to snap (for example the UV of the shadowmap or any custom
// value like a custom "fire intensity" that is calculated in your vertex shader)
#define SNAP_TO_TEXELS(stDelta, valueToSnap) (valueToSnap + dFdx(valueToSnap) * stDelta.x - dFdy(valueToSnap) * stDelta.y);

#endif
