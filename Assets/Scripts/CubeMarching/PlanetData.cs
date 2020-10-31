using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Planet Data", menuName = "Planet Data")]
public class PlanetData : ScriptableObject {
    
    public Dictionary<Vector3Int, float[]> chunkDictionary = new Dictionary<Vector3Int, float[]>();

}