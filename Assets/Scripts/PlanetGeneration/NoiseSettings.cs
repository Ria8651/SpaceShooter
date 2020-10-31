using UnityEngine;

namespace OldNoiseGen {
[System.Serializable]
public class NoiseSettings {

    public enum FilterType { Simple, Ridged };
    public FilterType filterType;

    [ConditionalHide("filterType", 0)]
    public SimpleNoiseSettings simpleNoiseSettings;
    [ConditionalHide("filterType", 1)]
    public RidgedNoiseSettings ridgedNoiseSettings;

    [System.Serializable]
    public class SimpleNoiseSettings {
        public float strength = 1;
        [Range(1, 8)]
        public int numLayers = 1;
        public float baseRoughness = 1;
        public float roughness = 2;
        public float persistance = 0.5f;
        public Vector3 centre;
        public float minValue;
    }

    [System.Serializable]
    public class RidgedNoiseSettings : SimpleNoiseSettings {
        public float weightMultiplyer = 0.8f;
    }
}
}