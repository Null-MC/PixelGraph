#define NORMAL_COMPRESSED_PROJECT 2
#define NORMAL_COMPRESSED_ANGLE 3
#define NORMAL_UNIFORM_ANGLE 4
#define NORMAL_OCTAHEDRON 5

#define NORMAL_FORMAT 0

#pragma pack_matrix(row_major)


//input range: 0 to 1
//output range: -1 to 1
float3 decodeNormal(const in float3 normal) {
	#if NORMAL_FORMAT == COMPRESSED_PROJECT
		float3 result = normal * 2.f - 1.f;
		float2 normalizedNormal2 = normalize(result.xy);
		result.xy *= max(abs(normalizedNormal2.x), abs(normalizedNormal2.y));
		result.z = sqrt(max(1.0 - dot(result.xy, result.xy), 0.0));
		return result;
	#elif NORMAL_FORMAT == COMPRESSED_ANGLE || NORMAL_FORMAT == UNIFORM_ANGLE
		float3 result = normal * 2.f - 1.f;
		#if NORMAL_FORMAT == UNIFORM_ANGLE
			float2 absNormal3 = abs(result.xy);
			if (absNormal3.x > absNormal3.y) {
				result.y = tan(result.y * (PI / 4.f) / absNormal3.x) * absNormal3.x;
			}
			else {
				result.x = tan(result.x * (PI / 4.f) / absNormal3.y) * absNormal3.y;
			}
		#endif
		float2 normalizedNormal2 = normalize(result.xy);
		result.xy *= max(abs(normalizedNormal2.x), abs(normalizedNormal2.y));
		float angle = length(result.xy) * (PI * 0.5f);
		return float3(sin(angle) * normalizedNormal2, cos(angle));
	#elif NORMAL_FORMAT == OCTAHEDRON
		float3 result = normal * 2.f - 1.f;
		result.xy *= mat2(0.5, -0.5, 0.5, 0.5);
		result.z = 1.0 - abs(result.x) - abs(result.y);
		return normalize(result);
	#else
		float3 result = float3(normal.xy * 2.f - 1.f, normal.z);
		return normalize(result);
	#endif
}

//input range: -1 to 1
//output range: 0 to 1
//float2 encodeNormal(const in float3 normal3) {
//	float2 normal2;
//
//	#if NORMAL_FORMAT == LABPBR_NOCLAMP || NORMAL_FORMAT == LABPBR_CLAMP
//		normal2 = normal3.xy;
//	#elif NORMAL_FORMAT == COMPRESSED_PROJECT
//		float2 normalizedNormal2 = normalize(normal3.xy);
//		normal2 = normal3.xy / max(abs(normalizedNormal2.x), abs(normalizedNormal2.y));
//	#elif NORMAL_FORMAT == COMPRESSED_ANGLE || NORMAL_FORMAT == UNIFORM_ANGLE
//		float2 normalizedNormal2 = normalize(normal3.xy);
//		float radius = acos(normal3.z) / (3.14159265359 * 0.5);
//		normal2 = normalizedNormal2 * radius / max(abs(normalizedNormal2.x), abs(normalizedNormal2.y));
//		#if NORMAL_FORMAT == UNIFORM_ANGLE
//			float2 absNormal2 = abs(normal2);
//			if (absNormal2.x > absNormal2.y) {
//				normal2.y = atan(normal2.y / absNormal2.x) * (4.0 / 3.14159265359) * absNormal2.x;
//			}
//			else {
//				normal2.x = atan(normal2.x / absNormal2.y) * (4.0 / 3.14159265359) * absNormal2.y;
//			}
//		#endif
//	#elif NORMAL_FORMAT == OCTAHEDRON
//		normal2 = normal3.xy / (abs(normal3.x) + abs(normal3.y) + abs(normal3.z));
//		normal2 *= mat2(1.0, 1.0, -1.0, 1.0);
//	#else
//		#error Invalid normal format.
//	#endif
//
//	return normal2 * 0.5 + 0.5;
//}