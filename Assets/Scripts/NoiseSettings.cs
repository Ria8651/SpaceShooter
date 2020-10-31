using UnityEngine;

[System.Serializable]
public class NoiseSettings {

    [Range(0, 1000)]
    public float groundLevel = 100;
    public NoiseFilter[] noiseFilters;

    [System.Serializable]
    public class NoiseFilter {
        public enum NoiseType { Simple, Ridged }
        public NoiseType noiseType;
        public bool enabled;

        public float strength = 1;
        [Range(1, 8)]
        public int numLayers = 1;
        public float baseRoughness = 0.04f;
        public float roughness = 2;
        public float persistance = 0.5f;
        public Vector3 centre;
        public float minValue;
    }
}