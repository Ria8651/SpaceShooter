using UnityEngine;

namespace OldNoiseGen {
public class SimpleNoiseFilter : INoiseFilter {
    
    NoiseSettings.SimpleNoiseSettings settings;
    Noise noise = new Noise();
    
    public SimpleNoiseFilter(NoiseSettings.SimpleNoiseSettings settings) {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point) {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitue = 1;

        for (int i = 0; i < settings.numLayers; i++) {
            float v = noise.Evaluate(point * frequency + settings.centre);
            noiseValue += (v + 1) / 2 * amplitue; 
            frequency *= settings.roughness;
            amplitue *= settings.persistance;
        }

        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}
}