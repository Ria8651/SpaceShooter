using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour {

    public bool isLoaded { get; private set; }
    public bool containsMaterial;
    public Vector3Int chunkId;
    public int size = 16;
    public NoiseSettings settings;
    public int LOD;

    public Color colour;

    MeshFilter meshFilter;
    MeshCollider meshCollider;

    public void Initialize(Vector3Int chunkId, int size) {
        this.chunkId = chunkId;
        this.size = size;
    }

    public void UpdateChunk(float[] values, int LOD) {
        transform.localPosition = new Vector3(chunkId.x * size, chunkId.y * size, chunkId.z * size);

        if (meshFilter == null) {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (meshCollider == null) {
            meshCollider = GetComponent<MeshCollider>();
        }

        if (meshFilter.sharedMesh == null) {
            meshFilter.sharedMesh = new Mesh();
        }

        if (!isLoaded) {
            return;
        }

        this.LOD = LOD;

        // foreach (float value in values) {
        //     Debug.Log(value);
        // }
        CubeMarching.GenerateMesh(meshFilter.sharedMesh, values, 0.5f, size + 1, 1);

        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    public void Load() {
        gameObject.SetActive(true);
        isLoaded = true;
    }

    public void UnLoad() {
        gameObject.SetActive(false);
        isLoaded = false;
    }

    void OnDrawGizmos() {
        Gizmos.color = colour;
        Gizmos.DrawWireCube((Vector3)chunkId * size + new Vector3(size / 2, size / 2, size / 2), new Vector3(size, size, size));
    }
}
