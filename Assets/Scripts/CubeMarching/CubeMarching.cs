using System.Collections.Generic;
using UnityEngine;

public static class CubeMarching {
    public static Mesh GenerateMesh(float[] values, float surfaceLevel, float scale) {
        Mesh mesh = new Mesh();

        int volume = values.Length;
        float cubeRoot = Mathf.Pow(volume, 1f / 3f);
        Debug.Log("Volume = " + volume + ", size = " + cubeRoot);

        int size = (int)cubeRoot;

        bool[] surface = GenerateSurface(values, surfaceLevel);

        List<Vector3> edges = new List<Vector3>();
        List<int> triangles = new List<int>();

        int LOD = 1;

        for (int id = 0; id < volume; id++) {
            Vector3Int cubeIndex = PositionFromId(id, size);

            if (cubeIndex.x < size - LOD && cubeIndex.y < size - LOD && cubeIndex.z < size - LOD) {
                if (cubeIndex.x % LOD == 0 && cubeIndex.y % LOD == 0 && cubeIndex.z % LOD == 0) {
                    bool[] cubeStates = {
                        surface[IdFormPosition(cubeIndex.x, cubeIndex.y, cubeIndex.z, size)],
                        surface[IdFormPosition(cubeIndex.x + LOD, cubeIndex.y, cubeIndex.z, size)],
                        surface[IdFormPosition(cubeIndex.x, cubeIndex.y + LOD, cubeIndex.z, size)],
                        surface[IdFormPosition(cubeIndex.x + LOD, cubeIndex.y + LOD, cubeIndex.z, size)],
                        surface[IdFormPosition(cubeIndex.x, cubeIndex.y, cubeIndex.z + LOD, size)],
                        surface[IdFormPosition(cubeIndex.x + LOD, cubeIndex.y, cubeIndex.z + LOD, size)],
                        surface[IdFormPosition(cubeIndex.x, cubeIndex.y + LOD, cubeIndex.z + LOD, size)],
                        surface[IdFormPosition(cubeIndex.x + LOD, cubeIndex.y + LOD, cubeIndex.z + LOD, size)]
                    };

                    bool isEmpty = true;
                    bool isFull = true;
                    for (int i = 0; i < 8; i++) {
                        if (cubeStates[i]) {
                            isEmpty = false;
                        } else if (!cubeStates[i]) {
                            isFull = false;
                        }
                    }

                    if (!isFull && !isEmpty) {
                        int[] edgeIndices = new int[12];
                        for (int i = 0; i < 12; i++) {
                            Vector3Int pointAPosition = (PositionFromId(Tables.pointAFromEdge[i], 2) * LOD) + cubeIndex;
                            Vector3Int pointBPosition = (PositionFromId(Tables.pointBFromEdge[i], 2) * LOD) + cubeIndex;

                            float pointAValue = values[IdFormPosition(pointAPosition.x, pointAPosition.y, pointAPosition.z, size)];
                            float pointBValue = values[IdFormPosition(pointBPosition.x, pointBPosition.y, pointBPosition.z, size)];

                            float t;
                            if (pointAValue == pointBValue) {
                                t = 0.5f;
                            } else {
                                t = (surfaceLevel - pointAValue) / (pointBValue - pointAValue);
                            }

                            Vector3 edge = Vector3.Lerp(pointAPosition, pointBPosition, t);

                            edges.Add(edge * scale);
                            edgeIndices[i] = edges.Count - 1;
                        }

                        int[] tris = EvaluateCube(cubeStates, edgeIndices);
                        for (int i = 0; i < tris.Length; i++) {
                            triangles.Add(tris[i]);
                        }
                    }
                }
            }
        }

        mesh.vertices = edges.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    public static void GenerateMesh(Mesh mesh, float[] values, float surfaceLevel, int size, int LOD) {
        int pointCount = size * size * size;

        bool[] surface = GenerateSurface(values, surfaceLevel);

        List<Vector3> edges = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int id = 0; id < pointCount; id++) {
            Vector3Int cubeIndex = PositionFromId(id, size);

            if (cubeIndex.x < size - LOD && cubeIndex.y < size - LOD && cubeIndex.z < size - LOD) {
                if (cubeIndex.x % LOD == 0 && cubeIndex.y % LOD == 0 && cubeIndex.z % LOD == 0) {
                    bool[] cubeStates = {
                        surface[IdFormPosition(cubeIndex.x, cubeIndex.y, cubeIndex.z, size)],
                        surface[IdFormPosition(cubeIndex.x + LOD, cubeIndex.y, cubeIndex.z, size)],
                        surface[IdFormPosition(cubeIndex.x, cubeIndex.y + LOD, cubeIndex.z, size)],
                        surface[IdFormPosition(cubeIndex.x + LOD, cubeIndex.y + LOD, cubeIndex.z, size)],
                        surface[IdFormPosition(cubeIndex.x, cubeIndex.y, cubeIndex.z + LOD, size)],
                        surface[IdFormPosition(cubeIndex.x + LOD, cubeIndex.y, cubeIndex.z + LOD, size)],
                        surface[IdFormPosition(cubeIndex.x, cubeIndex.y + LOD, cubeIndex.z + LOD, size)],
                        surface[IdFormPosition(cubeIndex.x + LOD, cubeIndex.y + LOD, cubeIndex.z + LOD, size)]
                    };

                    bool isEmpty = true;
                    bool isFull = true;
                    for (int i = 0; i < 8; i++) {
                        if (cubeStates[i]) {
                            isEmpty = false;
                        } else if (!cubeStates[i]) {
                            isFull = false;
                        }
                    }

                    if (!isFull && !isEmpty) {
                        int[] edgeIndices = new int[12];
                        for (int i = 0; i < 12; i++) {
                            Vector3Int pointAPosition = (PositionFromId(Tables.pointAFromEdge[i], 2) * LOD) + cubeIndex;
                            Vector3Int pointBPosition = (PositionFromId(Tables.pointBFromEdge[i], 2) * LOD) + cubeIndex;

                            float pointAValue = values[IdFormPosition(pointAPosition.x, pointAPosition.y, pointAPosition.z, size)];
                            float pointBValue = values[IdFormPosition(pointBPosition.x, pointBPosition.y, pointBPosition.z, size)];

                            float t;
                            if (pointAValue == pointBValue) {
                                t = 0.5f;
                            } else {
                                t = (surfaceLevel - pointAValue) / (pointBValue - pointAValue);
                            }

                            Vector3 edge = Vector3.Lerp(pointAPosition, pointBPosition, t);

                            edges.Add(edge);
                            edgeIndices[i] = edges.Count - 1;
                        }

                        int[] tris = EvaluateCube(cubeStates, edgeIndices);
                        for (int i = 0; i < tris.Length; i++) {
                            triangles.Add(tris[i]);
                        }
                    }
                }
            }
        }

        mesh.Clear();
        mesh.vertices = edges.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    static int[] EvaluateCube(bool[] states, int[] edges) {
        int index = 0;
        index += states[Tables.vertices[0]] ? 1 : 0;
        index += states[Tables.vertices[1]] ? 2 : 0;
        index += states[Tables.vertices[2]] ? 4 : 0;
        index += states[Tables.vertices[3]] ? 8 : 0;
        index += states[Tables.vertices[4]] ? 16 : 0;
        index += states[Tables.vertices[5]] ? 32 : 0;
        index += states[Tables.vertices[6]] ? 64 : 0;
        index += states[Tables.vertices[7]] ? 128 : 0;

        List<int> tris = new List<int>();
        for (int i = 0; i < 16; i++) {
            int edge = Tables.triTable[index, i];

            if (edge != -1) {
                tris.Add(edges[edge]);
            }
        }

        return tris.ToArray();
    }

    static bool[] GenerateSurface(float[] values, float surfaceLevel) {
        bool[] surface = new bool[values.Length];

        for (int i = 0; i < values.Length; i++) {
            if (values[i] < surfaceLevel) {
                surface[i] = false;
            } else {
                surface[i] = true;
            }
        }

        return surface;
    }

    public static Vector3Int PositionFromId(int id, int size) {
        int z = id / (size * size);
        id -= (z * size * size);
        int y = id / size;
        int x = id % size;
        return new Vector3Int(x, y, z);
    }

    public static int IdFormPosition(float x, float y, float z, int size) {
        return (int)((size * z + y) * size + x);
    }
}
