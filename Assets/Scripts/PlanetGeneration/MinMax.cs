using UnityEngine;

namespace OldNoiseGen {
public class MinMax {

    public float min { get; private set; }
    public float max { get; private set; }

    public MinMax() {
        min = float.MinValue;
        max = float.MaxValue;
    }

    public void AddValue(float v) {
        if (v > max) {
            max = v;
        } else if (v < min) {
            min = v;
        }
    }
}
}