#pragma pack_matrix(row_major)


ps_input_cube main(in float4 pos : POSITION,
    uniform float4x4 gl_ModelViewProjectionMatrix,
    uniform float3 v3CameraPos,    // The camera's current position
    uniform float3 v3LightDir,    // Direction vector to the light source
    uniform float3 v3InvWavelength,    // 1 / pow(wavelength, 4) for RGB
    uniform float fCameraHeight,    // The camera's current height
    uniform float fCameraHeight2,    // fCameraHeight^2
    uniform float fOuterRadius,    // The outer (atmosphere) radius
    uniform float fOuterRadius2,    // fOuterRadius^2
    uniform float fInnerRadius,    // The inner (planetary) radius
    uniform float fInnerRadius2,    // fInnerRadius^2
    uniform float fKrESun,    // Kr * ESun
    uniform float fKmESun,    // Km * ESun
    uniform float fKr4PI,    // Kr * 4 * PI
    uniform float fKm4PI,    // Km * 4 * PI
    uniform float fScale,    // 1 / (fOuterRadius - fInnerRadius)
    uniform float fScaleOverScaleDepth) // fScale / fScaleDepth
{
    // Get the ray from the camera to the vertex and its length (which
    // is the far point of the ray passing through the atmosphere)
    float3 v3Pos = pos.xyz;
    float3 v3Ray = v3Pos - v3CameraPos;
    float fFar = length(v3Ray);
    v3Ray /= fFar;

    // Calculate the closest intersection of the ray with
    // the outer atmosphere (point A in Figure 16-3)
    float fNear = getNearIntersection(v3CameraPos, v3Ray, fCameraHeight2, fOuterRadius2);
    // Calculate the ray's start and end positions in the atmosphere,
    // then calculate its scattering offset
    float3 v3Start = v3CameraPos + v3Ray * fNear;
    fFar -= fNear;

    float fStartAngle = dot(v3Ray, v3Start) / fOuterRadius;
    float fStartDepth = exp(-fInvScaleDepth);
    float fStartOffset = fStartDepth * scale(fStartAngle);

    // Initialize the scattering loop variables
    float fSampleLength = fFar / fSamples;
    float fScaledLength = fSampleLength * fScale;
    float3 v3SampleRay = v3Ray * fSampleLength;
    float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;

    // Now loop through the sample points
    float3 v3FrontColor = float3(0.0, 0.0, 0.0);
    for (int i = 0; i < nSamples; i++) {
        float fHeight = length(v3SamplePoint);
        float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
        float fLightAngle = dot(v3LightDir, v3SamplePoint) / fHeight;
        float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
        float fScatter = (fStartOffset + fDepth * (scale(fLightAngle) Ð scale(fCameraAngle)));
        float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
        v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
        v3SamplePoint += v3SampleRay;
    }

    // Finally, scale the Mie and Rayleigh colors
	float4x4 viewNoTranslate = mView;
    viewNoTranslate._m30_m31_m32 = 0;
	
    ps_input_cube OUT;
    OUT.pos = mul(mul(pos, viewNoTranslate), mProjection);
    OUT.c0.rgb = v3FrontColor * (v3InvWavelength * fKrESun);
    OUT.c1.rgb = v3FrontColor * fKmESun;
    OUT.t0 = v3CameraPos - v3Pos;

    return OUT;
}
