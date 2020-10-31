using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OctreeIndex {
    RightTopFront = 7, //111
    RightTopBack = 6, //110
    RightBottomFront = 5, //101
    RightLowerBack = 4, //100
    LeftUpperFront = 3, //011
    LeftUpperBack = 2, //010
    LeftLowerFront = 1, //001
    LeftLowerBack = 0 //000
}

public class Octree<T> {
    public Node<T> root { get { return rootNode; } }
    Node<T> rootNode;
    Vector3 treePosition;

    public Octree(Vector3 position, float size) {
        treePosition = position;
        rootNode = new Node<T>(Vector3.zero, size, 0);
    }

    public class Node<T> {
        public Vector3 position { get; private set; }
        public float size { get; private set; }
        public int level { get; private set; }
        public Node<T>[] subNodes { get; private set; }
        public T leaf;

        public Node(Vector3 position, float size, int level) {
            this.position = position;
            this.size = size;
            this.level = level;
        }

        public void Subdivide(int depth = 1) {
            if (subNodes != null || depth < 1) {
                return;
            }

            subNodes = new Node<T>[8];

            for (int i = 0; i < subNodes.Length; i++) {
                Vector3 newPosition = position;

                if ((i & 4) == 4) {
                    newPosition.x += size / 4;
                } else {
                    newPosition.x -= size / 4;
                }

                if ((i & 2) == 2) {
                    newPosition.y += size / 4;
                } else {
                    newPosition.y -= size / 4;
                }

                if ((i & 1) == 1) {
                    newPosition.z += size / 4;
                } else {
                    newPosition.z -= size / 4;
                }

                subNodes[i] = new Node<T>(newPosition, size / 2, level + 1);

                if (depth > 1) {
                    subNodes[i].Subdivide(depth - 1);
                }
            }
        }

        public void RemoveChildren() {
            subNodes = null;
        }

        public bool IsLeaf() {
            return subNodes == null;
        }

        public Node<T>[] GetLeafs() { // Not Working
            if (IsLeaf()) {
                return new Node<T>[] { this };
            }

            List<Node<T>> nodes = new List<Node<T>>();

            foreach (Node<T> node in subNodes) {
                if (node.IsLeaf()) {
                    nodes.Add(node);
                } else {
                    nodes.AddRange(node.GetLeafs());
                }
            }

            return nodes.ToArray();
        }

        public Node<T>[] GetNodes() {
            if (IsLeaf()) {
                return new Node<T>[] { this };
            }

            List<Node<T>> nodes = new List<Node<T>>();
            nodes.Add(this);

            foreach (Node<T> node in subNodes) {
                if (node.IsLeaf()) {
                    nodes.Add(node);
                } else {
                    nodes.AddRange(node.GetNodes());
                }
            }

            return nodes.ToArray();
        }
    }

    public int GetIndexOfNode(Vector3 lookupPosition, Vector3 nodePosition) {
        int index = 0;

        index += lookupPosition.x > nodePosition.x ? 4 : 0;
        index += lookupPosition.y > nodePosition.y ? 2 : 0;
        index += lookupPosition.z > nodePosition.z ? 1 : 0;

        return index;
    }

    public Node<T> GetBoundingNode(Vector3 position, Node<T> node = null) {
        if (node == null) {
            node = rootNode;
        }

        if (node.IsLeaf()) {
            return node;
        } else {
            int index = GetIndexOfNode(position, node.position + treePosition);
            return GetBoundingNode(position, node.subNodes[index]);
        }
    }
}
