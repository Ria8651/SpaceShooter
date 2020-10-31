using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    [Range(1, 64)]
    public int chunkSize = 16;
    [Range(1, 16)]
    public int viewDistance = 7;
    [Range(0, 16)]
    public int extraSurrogateChunks;
    public int seed;
    public NoiseSettings settings;
    public PlanetData data;
    public Material material;
    public Transform chunkTargetTransform;
    public bool drawChunkDebuging;

    Vector3Int centerChunk;

    Dictionary<Vector3Int, Chunk> surrogateChunks;
    Queue<Chunk> unloadedChunks;

    Queue<Vector3Int> chunksToLoad;
    List<Vector3Int> testedChunks;

    void OnValidate() {
        // Initialize();
    }

    void Start() {
        Initialize();
    }

    void Initialize() {
        if (data.chunkDictionary == null || data.chunkDictionary.Count == 0) {
            data.chunkDictionary = new Dictionary<Vector3Int, float[]>();
        }

        if (surrogateChunks != null) {
            foreach (Chunk surrogateChunk in surrogateChunks.Values) {
                Destroy(surrogateChunk.gameObject);
            }
        }

        surrogateChunks = new Dictionary<Vector3Int, Chunk>();

        if (unloadedChunks == null || unloadedChunks.Count == 0) {
            unloadedChunks = new Queue<Chunk>();
        }

        UpdatePlanet();
    }

    void Update() {
        UpdatePlanet();
    }

    void UpdatePlanet() {
        Vector3Int chunkId = GetChunkFromPosition(chunkTargetTransform.position - transform.position);

        if (chunkId != centerChunk) {
            centerChunk = chunkId;

            StopCoroutine("UpdatePlanetCoroutine");
            StartCoroutine("UpdatePlanetCoroutine");
        }
    }

    IEnumerator UpdatePlanetCoroutine() {
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        int maxTime = 16;
        watch.Start();

        chunksToLoad = new Queue<Vector3Int>();
        chunksToLoad.Enqueue(centerChunk);

        testedChunks = new List<Vector3Int>();
        testedChunks.Add(centerChunk);

        while (chunksToLoad.Count > 0) {
            Vector3Int chunkId = chunksToLoad.Dequeue();

            LoadChunk(chunkId);

            if (drawChunkDebuging) {
                surrogateChunks[chunkId].colour = Color.Lerp(Color.green, Color.red, GetChunkDistance(centerChunk, chunkId) / 7f);
            }

            foreach (Vector3Int orthogonal in GetOrthogonals(chunkId)) {
                int distance = GetChunkDistance(centerChunk, orthogonal);
                if (distance > viewDistance) {
                    continue;
                }

                // if (surrogateChunks.ContainsKey(orthogonal)) {
                //     if (surrogateChunks[orthogonal].isLoaded) {
                //         continue;
                //     }
                // }

                if (testedChunks.Contains(orthogonal)) {
                    continue;
                }

                chunksToLoad.Enqueue(orthogonal);
                testedChunks.Add(orthogonal);
            }

            if (watch.ElapsedMilliseconds > maxTime) {
                watch.Reset();
                yield return null;
                watch.Start();
            }
        }

        foreach (var surrogateChunk in surrogateChunks.Keys) {
            if (GetChunkDistance(centerChunk, surrogateChunk) > viewDistance) {
                UnloadChunk(surrogateChunk);

                if (watch.ElapsedMilliseconds > maxTime){
                    watch.Reset();
                    yield return null;
                    watch.Start();
                }
            }
        }
    }

    void LoadChunk(Vector3Int chunkId) {
        if (data.chunkDictionary.ContainsKey(chunkId)) {
            Chunk chunk = null;

            if (surrogateChunks.ContainsKey(chunkId)) {
                chunk = surrogateChunks[chunkId];

                UpdateLOD(chunkId, false);

                if (!chunk.isLoaded) {
                    if (chunk.containsMaterial) {
                        chunk.Load();
                    }
                }
            } else {
                if (unloadedChunks.Count > 0) {
                    chunk = unloadedChunks.Dequeue();

                    surrogateChunks.Remove(chunk.chunkId);
                    surrogateChunks.Add(chunkId, chunk);

                    chunk.Load();
                }

                if (chunk == null) {
                    if (surrogateChunks.Count < (viewDistance * 2 + 1) * (viewDistance * 2 + 1) * (viewDistance * 2 + 1) + extraSurrogateChunks) {
                        GameObject chunkGO = new GameObject("New Chunk");
                        chunkGO.transform.parent = transform;
                        chunkGO.AddComponent<MeshFilter>();
                        chunkGO.AddComponent<MeshRenderer>().sharedMaterial = material;
                        chunkGO.AddComponent<MeshCollider>();

                        Chunk surrogateChunk = chunkGO.AddComponent<Chunk>();
                        surrogateChunk.Load();

                        surrogateChunks.Add(chunkId, surrogateChunk);

                        chunk = surrogateChunk;
                    } else {
                        foreach (var surrogateChunk in surrogateChunks.Values) {
                            chunk = surrogateChunk;

                            surrogateChunks.Remove(chunk.chunkId);
                            surrogateChunks.Add(chunkId, chunk);

                            break;
                        }
                    }
                }

                chunk.Initialize(chunkId, chunkSize);
                UpdateChunk(chunkId);
            }
        } else {
            GenerateChunk(chunkId);
        }
    }

    void GenerateChunk(Vector3Int chunkId) {
        float[] values = NoiseGenerator.GenerateValuesOld(chunkId, chunkSize, settings, seed);
        data.chunkDictionary.Add(chunkId, values);

        LoadChunk(chunkId);
    }

    public void UnloadChunk(Vector3Int chunkId) {
        if (surrogateChunks.ContainsKey(chunkId)) {
            Chunk chunk = surrogateChunks[chunkId];

            if (chunk.isLoaded) {
                chunk.UnLoad();

                unloadedChunks.Enqueue(chunk);
            }
        } else {
            Debug.LogError("Error while unloading chunk: chunk is not loaded!");
        }
    }

    public void UpdateChunk(Vector3Int chunkId) {
        if (surrogateChunks.ContainsKey(chunkId)) {
            Chunk chunk = surrogateChunks[chunkId];

            if (CheckForMaterial(data.chunkDictionary[chunkId])) {
                chunk.containsMaterial = false;
                chunk.UnLoad();

                unloadedChunks.Enqueue(chunk);
            } else {
                chunk.containsMaterial = true;

                UpdateLOD(chunkId, true);
            }
        } else {
            Debug.LogError("Error while updateing chunk: chunk is not loaded!");
        }
    }

    public void UpdateLOD(Vector3Int chunkId, bool forceUpdate) {
        if (surrogateChunks.ContainsKey(chunkId)) {
            Chunk chunk = surrogateChunks[chunkId];

            Vector3Int playerChunk = centerChunk;
            int distance = GetChunkDistance(playerChunk, chunkId);

            int currentLOD = chunk.LOD;
            int LOD;

            LOD = 1;
            // if (distance <= viewDistance - 4) {
            // } else if (distance <= viewDistance - 2) {
            //     LOD = 2;
            // } else {
            //     LOD = 4;
            // }

            if (LOD < currentLOD || forceUpdate) {
                chunk.UpdateChunk(data.chunkDictionary[chunkId], 1);
            }
        } else {
            Debug.LogError("Error while updateing chunk: chunk is not loaded!");
        }
    }

    public Vector3Int[] GetOrthogonals(Vector3Int chunkId) {
        Vector3Int[] orthogonals = new Vector3Int[6];

        orthogonals[0] = chunkId + new Vector3Int(0, 0, -1);
        orthogonals[1] = chunkId + new Vector3Int(0, 0, 1);
        orthogonals[2] = chunkId + new Vector3Int(0, -1, 0);
        orthogonals[3] = chunkId + new Vector3Int(0, 1, 0);
        orthogonals[4] = chunkId + new Vector3Int(-1, 0, 0);
        orthogonals[5] = chunkId + new Vector3Int(1, 0, 0);

        return orthogonals;
    }

    public float[] GetChunkData(Vector3Int chunkId) {
        return data.chunkDictionary[chunkId];
    }

    public void SetChunkData(Vector3Int chunkId, float[] chunkData) {
        data.chunkDictionary[chunkId] = chunkData;

        if (surrogateChunks.ContainsKey(chunkId)) {
            UpdateChunk(chunkId);
        } else {
            LoadChunk(chunkId);
        }
    }

    bool CheckForMaterial(float[] values) {
        bool isEmpty = true;
        bool isFull = true;
        foreach (float value in values) {
            if (value >= 0.5f) {
                isEmpty = false;

                if (!isFull) {
                    break;
                }
            } else {
                isFull = false;

                if (!isEmpty) {
                    break;
                }
            }
        }

        return isEmpty || isFull;
    }

    public Vector3Int GetChunkFromPosition(Vector3 position) {
        return Vector3Int.FloorToInt(position / chunkSize);
    }

    static int GetChunkDistance(Vector3Int p1, Vector3Int p2) {
        int x = Mathf.Abs(p1.x - p2.x);
        int y = Mathf.Abs(p1.y - p2.y);
        int z = Mathf.Abs(p1.z - p2.z);

        return Mathf.Max(x, Mathf.Max(y, z));
    }
}
