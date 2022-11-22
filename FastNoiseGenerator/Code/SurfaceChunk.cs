using PicoGames.FastNoise;
using UnityEngine;

namespace PicoGames
{
    [RequireComponent(typeof(FastNoiseGenerator))]
    public class SurfaceChunk : MonoBehaviour
    {
        [SerializeField] private int patchEdgeCount = 64;
        [SerializeField] private float patchEdgeSize = 1f;
        [SerializeField] private float heightScale = 150;
        [SerializeField] private Material surfaceMaterial;

        [SerializeField] private FastNoiseGenerator noiseGen;
        [SerializeField] private FastNoiseParams[] noiseParams;

        private SurfaceMesh surface;

        private void OnDestroy()
        {
            surface?.Release();
        }

        private void Awake()
        {
            noiseGen = GetComponent<FastNoiseGenerator>();
            surface = new SurfaceMesh(patchEdgeCount, patchEdgeSize);
        }

        private void Update()
        {
            UpdateSurfaceGeometry();
            Graphics.DrawMesh(surface.Mesh, transform.localToWorldMatrix, surfaceMaterial, 0);
        }

        private void UpdateSurfaceGeometry()
        {
            float sampleWidth = patchEdgeCount * patchEdgeSize;
            float[] heights = noiseGen.GetNoiseArray(patchEdgeCount + 1, new Vector2(transform.position.x, transform.position.z), Vector2.zero, sampleWidth, noiseParams);

            // Update Surface Edge Size & Update Heights Lambda
            surface.EdgeSize = patchEdgeSize;
            surface.SetHeights((x, y) => 
            {
                return heights[x + y * (patchEdgeCount + 1)] * heightScale; 
            });

            surface.Mesh.RecalculateBounds();
            surface.Mesh.RecalculateNormals();
        }
    }
}