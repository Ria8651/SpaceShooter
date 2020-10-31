using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VertexPositions {
    RightUpperFront = 7, //111
    RightUpperBack = 6, //110
    RightLowerFront = 5, //101
    RightLowerBack = 4, //100
    LeftUpperFront = 3, //011
    LeftUpperBack = 2, //010
    LeftLowerFront = 1, //001
    LeftLowerBack = 0 //000
}

public enum Directions {
    Left = 0,
    Lower = 1,
    Back = 2,
    Right = 3,
    Upper = 4,
    Front = 5
}

public static class NaiveSurfaceNets {
    public static Mesh GenerateMesh(float[] values, float surfaceLevel, float scale) {
        Mesh mesh = new Mesh();

        int volume = values.Length;
        float cubeRoot = Mathf.Pow(volume, 1f / 3f);
        Debug.Log("Volume = " + volume + ", size = " + cubeRoot);

        int size = (int)cubeRoot;

        bool[] surface = GenerateSurface(values, surfaceLevel);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int vertId = 0; vertId < Mathf.Pow(size + 1, 3); vertId++) {
            Vector3Int vertIndex = PositionFromId(vertId, size + 1);

            // int[] localValues = new int[8];
            // for (int i = 0; i < 8; i++) {
            //     Vector3Int valueIdPosition = vertIndex;
            //
            //     int value = 1;
            //
            //     if ((i & 4) == 4) {
            //         valueIdPosition.x -= value;
            //     }
            //
            //     if ((i & 2) == 2) {
            //         valueIdPosition.y -= value;
            //     }
            //
            //     if ((i & 1) == 1) {
            //         valueIdPosition.z -= value;
            //     }
            //
            //     localValues[i] = IdFromPosition(valueIdPosition, size);
            // }

            vertices.Add((Vector3)vertIndex * scale);
        }

        for (int id = 0; id < volume; id++) {
            if (surface[id]) {
                Vector3Int cubeIndex = PositionFromId(id, size);

                int[] verticies = new int[8];
                for (int i = 0; i < 8; i++) {
                    Vector3Int vertIdPosition = cubeIndex;

                    int value = 1;

                    if ((i & 4) == 4) {
                        vertIdPosition.x += value;
                    }

                    if ((i & 2) == 2) {
                        vertIdPosition.y += value;
                    }

                    if ((i & 1) == 1) {
                        vertIdPosition.z += value;
                    }

                    verticies[i] = IdFromPosition(vertIdPosition, size + 1);
                }

                int[] face;
                for (int i = 0; i < 6; i++) {
                    Vector3Int orthogonal = cubeIndex + Tables.directions[i];

                    if (orthogonal.x >= 0 && orthogonal.x < size) {
                        if (orthogonal.y >= 0 && orthogonal.y < size) {
                            if (orthogonal.z >= 0 && orthogonal.z < size) {
                                int orthogonalId = IdFromPosition(orthogonal, size);
                                if (!surface[orthogonalId]) {
                                    face = new int[] {
                                        verticies[Tables.faces[i][0]],
                                        verticies[Tables.faces[i][1]],
                                        verticies[Tables.faces[i][2]],
                                        verticies[Tables.faces[i][3]]
                                    };

                                    triangles.AddRange(CreateFace(face));
                                }
                            }
                        }
                    }
                }

                // // Left
                // face = new int[] { verticies[0], verticies[1], verticies[2], verticies[3] };
                // triangles.AddRange(CreateFace(face));
                //
                // // Lowwer
                // face = new int[] { verticies[4], verticies[5], verticies[0], verticies[1] };
                // triangles.AddRange(CreateFace(face));
                //
                // // Back
                // face = new int[] { verticies[0], verticies[2], verticies[4], verticies[6] };
                // triangles.AddRange(CreateFace(face));
                //
                // // Right
                // face = new int[] { verticies[6], verticies[7], verticies[4], verticies[5] };
                // triangles.AddRange(CreateFace(face));
                //
                // // Upper
                // face = new int[] { verticies[2], verticies[3], verticies[6], verticies[7] };
                // triangles.AddRange(CreateFace(face));
                //
                // // Front
                // face = new int[] { verticies[5], verticies[7], verticies[1], verticies[3] };
                // triangles.AddRange(CreateFace(face));
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    static int[] CreateFace(int[] verticies) {
        int[] output = new int[6];

        output[0] = verticies[0];
        output[1] = verticies[1];
        output[2] = verticies[2];

        output[3] = verticies[2];
        output[4] = verticies[1];
        output[5] = verticies[3];

        return output;
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

    public static int IdFromPosition(Vector3Int position, int size) {
        return (int)((size * position.z + position.y) * size + position.x);
    }
}
