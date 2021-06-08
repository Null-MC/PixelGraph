#define EPSLION 0.0001

struct complexFloat3 {
	float3 real;
	float3 imag;
};

complexFloat3 complexAdd(complexFloat3 a, complexFloat3 b) {
	complexFloat3 c;
	c.real = a.real + b.real;
	c.imag = a.imag + b.imag;
	return c;
}

complexFloat3 complexSub(complexFloat3 a, complexFloat3 b) {
	complexFloat3 c;
	c.real = a.real - b.real;
	c.imag = a.imag - b.imag;
	return c;
}

complexFloat3 complexSub(complexFloat3 a, float3 b) {
	complexFloat3 c;
	c.real = a.real - b;
	c.imag = a.imag;
	return c;
}

complexFloat3 complexSub(float3 a, complexFloat3 b) {
	complexFloat3 c;
	c.real = a - b.real;
	c.imag = -b.imag;
	return c;
}

complexFloat3 complexMul(complexFloat3 a, complexFloat3 b) {
	complexFloat3 c;
	c.real = a.real * b.real - a.imag * b.imag;
	c.imag = a.imag * b.real + a.real * b.imag;
	return c;
}

complexFloat3 complexMul(float3 x, complexFloat3 a) {
	complexFloat3 c;
	c.real = x * a.real;
	c.imag = x * a.imag;
	return c;
}

complexFloat3 complexMul(complexFloat3 x, float3 a) {
	complexFloat3 c;
	c.real = x.real * a;
	c.imag = x.imag * a;
	return c;
}

complexFloat3 complexConjugate(complexFloat3 z) {
	complexFloat3 c;
	c.real = z.real;
	c.imag = -z.imag;
	return c;
}

complexFloat3 complexDiv(complexFloat3 a, complexFloat3 b) {
	complexFloat3 c;
	float3 r, den;
	if (abs(b.real).x >= abs(b.imag).x && abs(b.real).y >= abs(b.imag).y && abs(b.real).z >= abs(b.imag).z) {
		r = b.imag / (b.real + EPSLION);
		den = (b.real + r * b.imag) + EPSLION;
		c.real = (a.real + r * a.imag) * (1.0f / den);
		c.imag = (a.imag - r * a.real) * (1.0f / den);
	}
	else {
		r = b.real / (b.imag + EPSLION);
		den = (b.imag + r * b.real) + EPSLION;
		c.real = (a.real * r + a.imag) * (1.0f / den);
		c.imag = (a.imag * r - a.real) * (1.0f / den);
	}
	return c;
}

float3 complexAbs(complexFloat3 z) {
	return sqrt(z.real * z.real + z.imag * z.imag);
}

bool allEqual(float3 x, float3 y) {
	if (x.x == y.x) return true;
	if (x.y == y.y) return true;
	if (x.z == y.z) return true;
	return false;
}

complexFloat3 complexSqrt(complexFloat3 z) {
	complexFloat3 c;
	float3 w, r;

	if (allEqual(z.real, float3(0.0f, 0.0f, 0.0f)) && allEqual(z.imag, float3(0.0f, 0.0f, 0.0f))) {
		c.real = float3(0.0f, 0.0f, 0.0f);
		c.imag = float3(0.0f, 0.0f, 0.0f);
		return c;
	}

	float3 x = abs(z.real);
	float3 y = abs(z.imag);

	if (x.x >= y.x && x.y >= y.y && x.z >= y.z) {
		r = y / (x + EPSLION);
		w = sqrt(x) * sqrt(0.5 * (1.0f + sqrt(1.0f + r * r)));
	}
	else {
		r = x / (y + EPSLION);
		w = sqrt(y) * sqrt(0.5 * (r + sqrt(1.0f + r * r)));
	}

	if (z.real.x >= 0.0f && z.real.y >= 0.0f && z.real.z >= 0.0f) {
		c.real = w;
		c.imag = z.imag / (2.0f * w + EPSLION);
	}
	else {
		c.imag = (z.real.x >= 0.0f && z.real.y >= 0.0f && z.real.z >= 0.0f) ? w : -w;
		c.real = z.imag / (2.0f * c.imag + EPSLION);
	}

	return c;
}

complexFloat3 complexExp(complexFloat3 z) {
	complexFloat3 c;
	c.imag = cos(z.imag);
	c.real = sin(z.imag);
	return complexMul(exp(z.real), c);
}