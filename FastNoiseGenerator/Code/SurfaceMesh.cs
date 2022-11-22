/*  A simple way to create a smooth 2D non-skirted plane. Useful for terrain tiles/chunks.
 *  
 *  Created by: Jacob Fletcher
 */

using System;
using UnityEngine;

namespace PicoGames
{
    public class SurfaceMesh
    {
        private int edgeCount;
        private float edgeSize;

        private Mesh mesh;

        public Mesh Mesh { get { return mesh; } }
        public float EdgeSize { get { return edgeSize; } set { edgeSize = value; } }

        public SurfaceMesh(int edgeCount, float edgeSize)
            : this("New Mesh", edgeCount, edgeSize) { }

        public SurfaceMesh(string name, int edgeCount, float edgeSize)
        {
            this.edgeCount = edgeCount;
            this.edgeSize = edgeSize;

            mesh = new Mesh();
            mesh.name = name;

            InitializeMesh();
        }
        
        public void Release()
        {
            if (mesh != null)
                GameObject.Destroy(mesh);
        }

        public void SetHeights(Func<int, int, float> heightFunc)
        {
            int arrWidth = edgeCount + 1;
            Vector3[] mVertices = mesh.vertices;

            for (int x = 0, v = 0; x < arrWidth; x++)
            {
                for (int y = 0; y < arrWidth; y++, v++)
                {
                    mVertices[v].Set(x * edgeSize, heightFunc(x, y), y * edgeSize);
                }
            }

            mesh.vertices = mVertices;
        }

        private void InitializeMesh()
        {
            int arrWidth = edgeCount + 1;
            Vector3[] vertices = new Vector3[arrWidth * arrWidth];
            Vector2[] uv0s = new Vector2[vertices.Length];
            int[] triangles = new int[edgeCount * edgeCount * 6];

            for (int x = 0, v = 0, t = 0; x < arrWidth; x++)
            {
                for (int y = 0; y < arrWidth; y++, v++)
                {
                    vertices[v] = new Vector3(x * edgeSize, 0, y * edgeSize);           // LS Position
                    uv0s[v] = new Vector2(x / (float)edgeCount, y / (float)edgeCount);  // Unit Mapping

                    // Triangulate
                    if(y < edgeCount && x < edgeCount)
                    {
                        triangles[t + 0] = v;
                        triangles[t + 4] = triangles[t + 1] = v + 1;
                        triangles[t + 3] = triangles[t + 2] = v + arrWidth;
                        triangles[t + 5] = v + arrWidth + 1;

                        t += 6;
                    }
                }
            }

            mesh.indexFormat = (vertices.Length > 65000) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv0s;
        }
    }
}