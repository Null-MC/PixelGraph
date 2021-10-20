static const float pi = 3.14159265f;


float3 fresnelNonPolarized(float voh, complexFloat3 n1, complexFloat3 n2) {
    complexFloat3 eta = complexDiv(n1, n2);
    float3 cosThetaI = float3(voh, voh, voh);
    float sinThetaI = sqrt(saturate(1.0 - voh * voh));
    complexFloat3 sinThetaT;
    sinThetaT.real = eta.real * sinThetaI;
    sinThetaT.imag = eta.imag * sinThetaI;
    complexFloat3 cosThetaT = complexSqrt(complexSub(1.0f, complexMul(sinThetaT, sinThetaT)));

    float3 Rs = pow(complexAbs(
        complexDiv(complexSub(complexMul(n1, cosThetaI), complexMul(n2, cosThetaT)), complexAdd(complexMul(n1, cosThetaI), complexMul(n2, cosThetaT)))
    ), 2.0f);

    float3 Rp = pow(complexAbs(
        complexDiv(complexSub(complexMul(n1, cosThetaT), complexMul(n2, cosThetaI)), complexAdd(complexMul(n1, cosThetaT), complexMul(n2, cosThetaI)))
    ), 2.0f);

    return saturate((Rs + Rp) * 0.5f);
}

float schlickFresnel(in float cosTheta, in float f0) {
    return f0 + (1.0f - f0) * pow(1.0f - cosTheta, 5.0f);
}

float smithGGXMasking(float ndotv, float a2) {
    float denomC = sqrt(a2 + (1.0f - a2) * ndotv * ndotv) + ndotv;

    return saturate(2.0f * ndotv / denomC);
}

float smithGGXMaskingShadowing(float ndotv, float ndotl, float a2) {
    float a = 2.0f * ndotl * ndotv;
    float denomA = ndotv * sqrt(a2 + (ndotv - ndotv * a2) * ndotv);
    float denomB = ndotl * sqrt(a2 + (ndotl - ndotl * a2) * ndotl);

    return saturate(a / (denomA + denomB));
}

float D_GGX(float ndoth, float roughnessSquared) {
    roughnessSquared = roughnessSquared < 1e-5 ? 0.0f : roughnessSquared;
    float p = (ndoth * roughnessSquared - ndoth) * ndoth + 1.0f;
    return roughnessSquared / (pi * p * p);
}

float3 specularBRDF(float nDotL, float nDotV, float nDotH, float vDotH, float f0, float roughnessSquared) {
    nDotV = abs(nDotV);

    complexFloat3 n1;
    n1.real = 1.00029f;
    n1.imag = 0.0f;
	
    complexFloat3 n2;
    n2.real = f0_to_ior(f0);
    n2.imag = 0.0f;

    float G = smithGGXMaskingShadowing(nDotV, nDotL, roughnessSquared);
    float D = D_GGX(nDotH, roughnessSquared);
    float3 F = fresnelNonPolarized(vDotH, n1, n2);

    float3 numerator = G * D * F;
    float denominator = 4.0f * vDotH;

    return max(numerator / denominator, 0.0f);
}
