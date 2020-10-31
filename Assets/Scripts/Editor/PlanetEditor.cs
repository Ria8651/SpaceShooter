using UnityEngine;
using UnityEditor;

namespace OctreeChunk {

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor {
    public override void OnInspectorGUI() {
        Planet planet = (Planet)target;

        if (GUILayout.Button("Update Planet")) {
            planet.UpdatePlanet();
        }

        if (GUILayout.Button("Reset Planet")) {
            planet.ResetPlanet();
        }

        if (GUILayout.Button("Update Data")) {
            planet.ResetData();
            planet.UpdateData();
        }

        if (GUILayout.Button("Debug Dictionary")) {
            planet.DebugDictionary();
        }

        base.OnInspectorGUI();
    }
}
}
