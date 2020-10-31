using UnityEngine;

namespace OldNoiseGen {
public class Planet : MonoBehaviour {

    [Range(2, 256)]
    public int resolution = 10;
    public enum FaceRenderMask { All, Forward, Right, Up, Back, Left, Down };
    public FaceRenderMask faceRenderMask;

    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colourSettingsFoldout;

    ShapeGenarator shapeGenarator = new ShapeGenarator();
    ColourGenerator colourGenerator = new ColourGenerator();

    void OnValidate() {
        GeneratePlanet();
    }

    void Initilise() {
        shapeGenarator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colourSettings);

        if (meshFilters == null || meshFilters.Length == 0) {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.up, -Vector3.forward, -Vector3.right, -Vector3.up };

        for (int i = 0; i < 6; i++) {
            if (meshFilters[i] == null) {
                GameObject meshGameObject = new GameObject("Mesh");
                meshGameObject.transform.parent = transform;
                meshGameObject.transform.localPosition = Vector3.zero;

                meshGameObject.AddComponent<MeshRenderer>();
                meshFilters[i] = meshGameObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;

            terrainFaces[i] = new TerrainFace(shapeGenarator, meshFilters[i].sharedMesh, resolution, directions[i]);
            if (meshFilters[i].gameObject.GetComponent<MeshCollider>() == null) {
                meshFilters[i].gameObject.AddComponent<MeshCollider>();
            }
            meshFilters[i].gameObject.GetComponent<MeshCollider>().sharedMesh = meshFilters[i].sharedMesh;


            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }

    public void GeneratePlanet() {
        Initilise();
        GenerateMesh();
        GenerateColours();
    }

    public void OnShapeSettingsUpdated() {
        Initilise();
        GenerateMesh();
    }

    public void OnColourSettingsUpdated() {
        Initilise();
        GenerateColours();
    }

    void GenerateMesh() {
        for (int i = 0; i < 6; i++) {
            if (meshFilters[i].gameObject.activeSelf) {
                terrainFaces[i].ConstructMesh();
            }
        }

        colourGenerator.UpdateElevation(shapeGenarator.elevationMinMax);
    }

    void GenerateColours() {
        colourGenerator.UpdateColours();
    }
}
}
