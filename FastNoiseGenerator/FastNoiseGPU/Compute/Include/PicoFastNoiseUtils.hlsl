/* Custom tools used by CSFastNoise.compute to generate 1D array of noise values.
 * To be used in conjunction with FastNoiseLite HLSL. https://github.com/Auburn/FastNoise
 */

#include "FastNoiseLite.hlsl"

struct FastNoiseParams
{
	int seed;
	float weight;
	float2 offset;
	int noiseType;
	float frequency;
	int fractalType;
	int octaves;
	float lacunarity;
	float gain;
};

float GetNoiseFromParams01(float2 samplePos, float2 noiseOrigin, FastNoiseParams params) {

	if (params.weight <= 0)
		return 0;

	fnl_state noise = fnlCreateState(params.seed);
	noise.fractal_type = params.fractalType;
	noise.noise_type = params.noiseType;
	noise.octaves = params.octaves;
	noise.frequency = params.frequency;
	noise.lacunarity = params.lacunarity;
	noise.gain = params.gain;

	float noiseX = samplePos.x + params.offset.x + noiseOrigin.x;
	float noiseY = samplePos.y + params.offset.y + noiseOrigin.y;

	return (fnlGetNoise2D(noise, noiseX, noiseY) + 1.0) * 0.5 * params.weight;
}

float GetCompositeNoise01(float2 samplePos, float2 noiseOrigin, StructuredBuffer<FastNoiseParams> noiseParamsBuffer) {

	uint npbCount, npbStride = 0;
	noiseParamsBuffer.GetDimensions(npbCount, npbStride);

	if (npbCount <= 0)
		return 0;

	float h = 0;
	for (uint i = 0; i < npbCount; i++) {
		h += GetNoiseFromParams01(samplePos, noiseOrigin, noiseParamsBuffer[i]);
	}

	if (npbCount > 0)
		return clamp(h / npbCount, 0, 1);
	else
		return 0;
}