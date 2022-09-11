// https://www.chaosgroup.com/blog/understanding-metalness
// R=670 nm
// G=550 nm
// B=430 nm

static const float3 ior_n_iron = float3(2.4680, 2.1890, 1.6310);
static const float3 ior_k_iron = float3(3.4040, 3.1710, 2.8060);

static const float3 ior_n_gold = float3(0.17410, 0.42108, 1.4762);
static const float3 ior_k_gold = float3(3.6123, 2.3459, 1.8135);

static const float3 ior_n_aluminum = float3(1.5196, 0.96565, 0.53124);
static const float3 ior_k_aluminum = float3(7.6635, 6.4581, 5.0699);

static const float3 ior_n_chrome = float3(3.6400, 2.9008, 1.9906);
static const float3 ior_k_chrome = float3(4.3511, 4.2311, 3.7505);

static const float3 ior_n_copper = float3(0.24052, 0.67693, 1.3379);
static const float3 ior_k_copper = float3(3.8090, 2.6248, 2.2981);

static const float3 ior_n_lead = float3(2.5298, 2.5444, 1.8967);
static const float3 ior_k_lead = float3(4.1709, 4.1823, 4.1552);

static const float3 ior_n_platinum = float3(2.4390, 2.0847, 1.7998);
static const float3 ior_k_platinum = float3(4.3677, 3.7153, 3.0211);

static const float3 ior_n_silver = float3(0.16262, 0.14512, 0.13550);
static const float3 ior_k_silver = float3(4.0728, 3.1900, 2.1997);


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


void get_hcm_ior(const in float f0, const in float3 albedo, out float3 n, out float3 k) {
	int i = int(f0 * 255.0f - 229.5f);

	if (i < 0) {
		// dielectric
		n = f0_to_ior(f0);
		k = 0.0f;
	}
	else if (i >= 8) {
		// albedo-only conductor
		n = f0_to_ior(albedo);
		k = albedo;
	}
	else {
		// HCM conductor
		i = min(i, 8);
		n = ior_n[i];
		k = ior_k[i];
	}
}
