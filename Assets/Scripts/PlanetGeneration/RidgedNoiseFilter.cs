using UnityEngine;

namespace OldNoiseGen {
public class RidgedNoiseFilter : INoiseFilter {
    
    NoiseSettings.RidgedNoiseSettings settings;
    Noise noise = new Noise();
    
    public RidgedNoiseFilter(NoiseSettings.RidgedNoiseSettings settings) {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point) {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitue = 1;
        float weight = 1;

        for (int i = 0; i < settings.numLayers; i++) {
            float v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + settings.centre));
            v *= v;
            v *= weight;
            weight = Mathf.Clamp01(v * settings.weightMultiplyer);

            noiseValue += v * amplitue; 
            frequency *= settings.roughness;
            amplitue *= settings.persistance;
        }

        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}
}