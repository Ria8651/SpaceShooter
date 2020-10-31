using UnityEngine;

public static class NoiseGenerator {
    public static float[] GenerateValues(Vector3 position, float size, int chunkSize, NoiseSettings settings, int seed) {
        int pointCount = (int)Mathf.Pow(chunkSize, 3);

        float[] values = new float[pointCount];
        Noise noise = new Noise(seed);

        for (int i = 0; i < pointCount; i++) {
            Vector3 point = NaiveSurfaceNets.PositionFromId(i, chunkSize);
            point -= (chunkSize / 2f - 0.5f) * Vector3.one;
            float scale = size / chunkSize;
            point *= scale;
            point += position;

            float v = Evaluate(point, settings, noise);

            // float height = -(chunkId.y * chunkSize) + -point.y + settings.groundLevel;
            float height = -point.magnitude + settings.groundLevel;

            float value = height + v;
            value = Mathf.Clamp(value, 0, 1);

            values[i] = value;
        }

        return values;
    }

    public static float[] GenerateValuesOld(Vector3 position, int chunkSize, NoiseSettings settings, int seed) {
        int pointCount = (int)Mathf.Pow(chunkSize, 3);

        float[] values = new float[pointCount];

        Noise noise = new Noise(seed);

        for (int i = 0; i < pointCount; i++) {
            Vector3 point = CubeMarching.PositionFromId(i, chunkSize + 1);

            float v = Evaluate(point + position * chunkSize, settings, noise);

            // float height = -(chunkId.y * chunkSize) + -point.y + settings.groundLevel;
            float height = -(position * chunkSize + point).magnitude + settings.groundLevel;

            float value = v + height;
            value = Mathf.Clamp(value, 0, 1);

            values[i] = value;
        }

        return values;
    }

    static float Evaluate(Vector3 point, NoiseSettings settings, Noise noise) {
        float value = 0;

        foreach (NoiseSettings.NoiseFilter noiseFilter in settings.noiseFilters) {
            if (noiseFilter.enabled) {
                float noiseValue = 0;
                float frequency = noiseFilter.baseRoughness;
                float amplitue = 1;
                float v;

                switch (noiseFilter.noiseType) {
                    case NoiseSettings.NoiseFilter.NoiseType.Simple:
                        for (int i = 0; i < noiseFilter.numLayers; i++) {
                            v = noise.Evaluate(point * frequency + noiseFilter.centre);
                            noiseValue += (v + 1) / 2 * amplitue;
                            frequency *= noiseFilter.roughness;
                            amplitue *= noiseFilter.persistance;
                        }

                        noiseValue = Mathf.Max(0, noiseValue - noiseFilter.minValue);
                        value += noiseValue * noiseFilter.strength;

                        break;
                    case NoiseSettings.NoiseFilter.NoiseType.Ridged:
                        float weight = 1;

                        for (int i = 0; i < noiseFilter.numLayers; i++) {
                            v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + noiseFilter.centre));
                            v *= v;
                            v *= weight;
                            weight = Mathf.Clamp01(v * 0.8f);

                            noiseValue += v * amplitue;
                            frequency *= noiseFilter.roughness;
                            amplitue *= noiseFilter.persistance;
                        }

                        noiseValue = Mathf.Max(0, noiseValue - noiseFilter.minValue);
                        value += noiseValue * noiseFilter.strength;

                        break;
                }
            }
        }

        return value;
    }
}
