// https://www.chaosgroup.com/blog/understanding-metalness
// R=0.65 micrometers
// G=0.55 micrometers
// B=0.45 micrometers

static const float3 ior_n_iron = float3(2.9114f, 2.9497f, 2.5845f);
static const float3 ior_k_iron = float3(3.0893f, 2.9318f, 2.767f);

static const float3 ior_n_gold = float3(0.18299f, 0.42108f, 1.3734f);
static const float3 ior_k_gold = float3(3.4242f, 2.3459f, 1.7704f);

static const float3 ior_n_aluminum = float3(1.3456f, 0.96521f, 0.61722f);
static const float3 ior_k_aluminum = float3(7.4746f, 6.3995f, 5.3031f);

static const float3 ior_n_chrome = float3(3.1071f, 3.1812f, 2.323f);
static const float3 ior_k_chrome = float3(3.3314f, 3.3291f, 3.135f);

static const float3 ior_n_copper = float3(0.27105f, 0.67693f, 1.3164f);
static const float3 ior_k_copper = float3(3.6092f, 2.6248f, 2.2921f);

static const float3 ior_n_lead = float3(1.91f, 1.83f, 1.44f);
static const float3 ior_k_lead = float3(3.51f, 3.4f, 3.18f);

static const float3 ior_n_platinum = float3(2.3757f, 2.0847f, 1.8453f);
static const float3 ior_k_platinum = float3(4.2655f, 3.7153f, 3.1365f);

static const float3 ior_n_silver = float3(0.15943f, 0.14512f, 0.13547f);
static const float3 ior_k_silver = float3(3.9291f, 3.19f, 2.3808f);


static const float3 ior_n[8] = {
	ior_n_iron,
	ior_n_gold,
	ior_n_aluminum,
	ior_n_chrome,
	ior_n_copper,
	ior_n_lead,
	ior_n_platinum,
	ior_n_silver,
};

static const float3 ior_k[8] = {
	ior_k_iron,
	ior_k_gold,
	ior_k_aluminum,
	ior_k_chrome,
	ior_k_copper,
	ior_k_lead,
	ior_k_platinum,
	ior_k_silver,
};

//float3 get_hcm_f0(const in float f0) {
//	if (f0 > 0.900 && f0 < 0.902) return float3(0.56f, 0.57f, 0.58f); // 230: Iron
//	if (f0 > 0.902 && f0 <= 0.906) return float3(1.0f, 0.71f, 0.29f); // 231: Gold
//	if (f0 > 0.906 && f0 <= 0.910) return float3(0.91f, 0.92f, 0.92f); // 232: Aluminum
//	if (f0 > 0.910 && f0 <= 0.914) return float3(0.550, 0.556, 0.554); // 233: Chrome
//	if (f0 > 0.914 && f0 <= 0.918) return float3(0.955, 0.637, 0.538); // 234: Copper
//	if (f0 > 0.918 && f0 <= 0.922) return float3(0.0, 0.0, 0.0); // 235: Lead - WARN: MISSING
//	if (f0 > 0.922 && f0 <= 0.926) return float3(0.83, 0.81, 0.78); // 236: Platinum
//	if (f0 > 0.926 && f0 <= 0.930) return float3(0.97, 0.96, 0.91); // 237: Silver
//	return 1.0f;
//}

void get_hcm_ior(const in float f0, out float3 n, out float3 k) {
	int i = int(f0 * 255.0f - 229.5f);

	if (i < 0) {
		// dielectric
		n = f0_to_ior(f0);
		k = 0.0f;
	}
	else if (i >= 8) {
		// albedo-only conductor
		n = f0_to_ior(1.0f);
		k = 0.0f;

		// TODO: use albedo for K
	}
	else {
		// HCM conductor
		i = min(i, 8);
		n = ior_n[i];
		k = ior_k[i];
	}
}
