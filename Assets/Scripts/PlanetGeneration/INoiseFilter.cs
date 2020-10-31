using UnityEngine;

namespace OldNoiseGen {
public interface INoiseFilter {
    
    float Evaluate(Vector3 point);
}
}