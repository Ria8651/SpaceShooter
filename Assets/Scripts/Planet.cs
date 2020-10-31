using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctreeChunk {
public class Planet : MonoBehaviour {
    public float size = 500;
    public float[] distances = new float[] { 1, 0.5f, 0.25f, 0.15f, 0.1f };
    public Transform targetTransform;
    public Material material;
    public int chunkSize = 32;
    public NoiseSettings settings;
    public bool drawChunkDebug;
    public bool drawDataDebug;

    Octree<MeshFilter> chunks;
    float[][] data;
    Dictionary<Octree<MeshFilter>.Node<MeshFilter>, int[]> dataChunkRefrenceThingie;
    // Queue<MeshFilter> unusedChunks;

    float[] testData;

    void OnDrawGizmos() {
        UpdatePlanet();

        if (drawChunkDebug) {
            DrawChunkDebug();
        }

        if (drawDataDebug) {
            DrawDataDebug();
        }
    }

    public void ResetData() {
        int dataChunkArraySize = (int)Mathf.Pow(2, distances.Length);
        int dataChunkArrayCount = (int)Mathf.Pow(dataChunkArraySize, 3);
        data = new float[dataChunkArrayCount][];
    }

    public void UpdateData() {
        if (data == null) {
            ResetData();
        }

        for (int i = 0; i < data.Length; i++) {
            Vector3 position = GetDataChunkPosition(i);
            int dataChunkArraySize = (int)Mathf.Pow(2, distances.Length);
            float dataChunkSize = size / dataChunkArraySize;

            data[i] = NoiseGenerator.GenerateValues(position, dataChunkSize, chunkSize, settings, 0);
        }

        // testData = NoiseGenerator.GenerateValues(new Vector3(0, 0, 0), size, chunkSize, settings, 0);
    }

    public void ResetPlanet() {
        DestoryAllChunks();

        chunks = new Octree<MeshFilter>(transform.position, size);

        dataChunkRefrenceThingie = new Dictionary<Octree<MeshFilter>.Node<MeshFilter>, int[]>();

        int[] initalChunks = new int[data.Length];
        for (int id = 0; id < data.Length; id++) {
            initalChunks[id] = id;
        }

        dataChunkRefrenceThingie.Add(chunks.root, initalChunks);

        CreateChunk(chunks.root);
    }

    public void UpdatePlanet() {
        if (chunks == null) {
            ResetPlanet();
        }

        if (data == null) {
            UpdateData();
        }

        foreach (var node in chunks.root.GetNodes()) {
            if (node.IsLeaf()) {
                if (node.level < distances.Length) {
                    if (Vector3.Distance(node.position + transform.position, targetTransform.position) < distances[node.level] * size) {
                        SubdivideChunk(node);
                    }
                }
            } else {
                if (Vector3.Distance(node.position + transform.position, targetTransform.position) >= distances[node.level] * size) {
                    RemoveChunkChildren(node);
                }
            }
        }
    }

    void SubdivideChunk(Octree<MeshFilter>.Node<MeshFilter> node) {
        DestroyChunk(node);

        node.Subdivide();

        int[] currentDataChunks = dataChunkRefrenceThingie[node];
        List<int>[] newDataChunks = new List<int>[8];

        foreach(int dataChunk in currentDataChunks) {
            Vector3 position = GetDataChunkPosition(dataChunk);

            int index = chunks.GetIndexOfNode(position, node.position);

            if (newDataChunks[index] == null) {
                newDataChunks[index] = new List<int>();
            }

            newDataChunks[index].Add(dataChunk);
        }

        dataChunkRefrenceThingie.Remove(node);

        for (int i = 0; i < 8; i++) {
            dataChunkRefrenceThingie.Add(node.subNodes[i], newDataChunks[i].ToArray());
        }

        foreach (var subNode in node.subNodes) {
            CreateChunk(subNode);
        }
    }

    void RemoveChunkChildren(Octree<MeshFilter>.Node<MeshFilter> node) {
        List<int> newDataChunks = new List<int>();

        foreach (var subNode in node.GetNodes()) {
            if (dataChunkRefrenceThingie.ContainsKey(subNode)) {
                newDataChunks.AddRange(dataChunkRefrenceThingie[subNode]);
                dataChunkRefrenceThingie.Remove(subNode);
            }

            DestroyChunk(subNode);
        }

        node.RemoveChildren();

        newDataChunks.Sort();
        dataChunkRefrenceThingie.Add(node, newDataChunks.ToArray());

        CreateChunk(node);
    }

    void CreateChunk(Octree<MeshFilter>.Node<MeshFilter> node) {
        GameObject chunk = new GameObject();
        chunk.transform.parent = transform;

        Vector3 position = node.position - Vector3.one * node.size / 2;
        chunk.transform.localPosition = position;

        MeshFilter filter = chunk.AddComponent<MeshFilter>();
        chunk.AddComponent<MeshRenderer>().material = material;
        node.leaf = filter;

        UpdateChunk(node);
    }

    void UpdateChunk(Octree<MeshFilter>.Node<MeshFilter> node) {
        if (node.IsLeaf() == false) {
            Debug.LogError("Can't update chunk - Chunk is not leaf!");
            return;
        }

        int[] dataChunks = dataChunkRefrenceThingie[node];

        int localDataChunkArraySize = (int)Mathf.Pow(dataChunks.Length, 1f/3f);
        int dataChunkArraySize = (int)Mathf.Pow(2, distances.Length);
        int chunkVolume = chunkSize * chunkSize * chunkSize;

        Vector3Int offset = NaiveSurfaceNets.PositionFromId(dataChunks[0], dataChunkArraySize);

        float[] sampledValues = new float[chunkVolume];
        foreach (int dataChunk in dataChunks) {
            float[] subValues = data[dataChunk];
            for (int i = 0; i < subValues.Length; i++) {
                Vector3Int valueIntPosition = NaiveSurfaceNets.PositionFromId(i, chunkSize);
                valueIntPosition += (NaiveSurfaceNets.PositionFromId(dataChunk, dataChunkArraySize) - offset) * chunkSize;

                if (valueIntPosition.x % localDataChunkArraySize == 0) {
                    if (valueIntPosition.y % localDataChunkArraySize == 0) {
                        if (valueIntPosition.z % localDataChunkArraySize == 0) {
                            Vector3Int insertionPosition = valueIntPosition / localDataChunkArraySize;
                            int insertionId = NaiveSurfaceNets.IdFromPosition(insertionPosition, chunkSize);

                            sampledValues[insertionId] = subValues[i];
                        }
                    }
                }
            }
        }

        float[] values = sampledValues;

        float scale = node.size / chunkSize;
        node.leaf.sharedMesh = CubeMarching.GenerateMesh(values, 0.5f, scale);
    }

    void DestroyChunk(Octree<MeshFilter>.Node<MeshFilter> node) {
        if (node.leaf != null) {
            // node.leaf.gameObject.SetActive(false);

            DestroyImmediate(node.leaf.gameObject);
        }
    }

    void DestoryAllChunks() {
        if (chunks != null) {
            foreach (var filter in chunks.root.GetLeafs()) {
                if (filter.leaf != null) {
                    DestroyImmediate(filter.leaf.gameObject);
                }
            }
        }

        GameObject[] children = new GameObject[transform.childCount];

        int i = 0;
        foreach (Transform child in transform) {
            children[i] = child.gameObject;
            i += 1;
        }

        foreach (GameObject child in children) {
            DestroyImmediate(child);
        }
    }

    public void DebugDictionary() {
        foreach (int[] dataChunks in dataChunkRefrenceThingie.Values) {
            string output = "";
            foreach (int dataChunk in dataChunks) {
                output += dataChunk.ToString() + ", ";
            }
            Debug.Log(output);
        }
    }

    // void Update() {
    //     UpdateOctree();
    // }

    Vector3 GetDataChunkPosition(int id) {
        int dataChunkArraySize = (int)Mathf.Pow(2, distances.Length);
        float dataChunkSize = size / dataChunkArraySize;

        Vector3 position = NaiveSurfaceNets.PositionFromId(id, dataChunkArraySize);
        position *= dataChunkSize;
        position += Vector3.one * dataChunkSize / 2;
        position -= Vector3.one * size / 2;

        return position;
    }

    void DrawChunkDebug() {
        foreach (var subNode in chunks.root.GetNodes()) {
            Gizmos.color = Color.Lerp(new Color(0, 1, 0, 1), new Color(1, 0, 0, 0.2f), subNode.level / (float)distances.Length);
            Gizmos.DrawWireCube(transform.position + subNode.position, Vector3.one * subNode.size);
        }
    }

    void DrawDataDebug() {
        for (int i = 0; i < data.Length; i++) {
            int dataChunkArraySize = (int)Mathf.Pow(2, distances.Length);
            float dataChunkSize = size / dataChunkArraySize;

            Vector3 position = GetDataChunkPosition(i);

            Gizmos.color = new Color(0, 0.25f, 0.75f, 0.2f);
            Gizmos.DrawWireCube(transform.position + position, Vector3.one * dataChunkSize);

            // int pointCount = (int)Mathf.Pow(chunkSize, 3);
            //
            // for (int i = 0; i < pointCount; i++) {
            //     Vector3 position = NaiveSurfaceNets.PositionFromId(i, chunkSize);
            //     position -= (chunkSize / 2f - 0.5f) * Vector3.one;
            //     float scale = subNode.size / chunkSize;
            //     position *= scale;
            //     position += subNode.position + transform.position;
            //
            //     Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1);
            //     Gizmos.DrawSphere(position, 1);
            // }
        }
    }
}
}
