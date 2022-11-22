/*  A Unity tool used in conjunction with FastNoiseLite HLSL. https://github.com/Auburn/FastNoise
 *  
 *  Created by: Jacob Fletcher
 * 
 *  This tool provides a simple, one method call, way to provide FastNoise parameters and receive an array of noise data.
 *  This data can be used to generate heightmaps, colormaps, or any other thing that may require an array of noise data.
 */

using UnityEngine;

namespace PicoGames.FastNoise
{
    public class FastNoiseGenerator : MonoBehaviour
    {
        [SerializeField] private ComputeShader csFastNoise;

        private int kernel;
        private ComputeBuffer noiseParamsBuffer;
        private ComputeBuffer noiseArrayBuffer;

        private void Awake()
        {
            kernel = csFastNoise.FindKernel("CSNoiseArray");
        }

        private void OnDestroy()
        {
            noiseArrayBuffer?.Release();
            noiseParamsBuffer?.Release();
        }

        public float[] GetNoiseArray(int arrayWidth, Vector2 noiseOrigin, Vector2 sampleOrigin, float sampleWidth, params FastNoiseParams[] noiseParams)
        {
            SetNoiseBuffers(noiseParams);
            SetPatchParams(arrayWidth, noiseOrigin, sampleOrigin, sampleWidth);

            float[] noiseArray = new float[arrayWidth * arrayWidth];
            noiseArrayBuffer?.Release();
            noiseArrayBuffer = new ComputeBuffer(noiseArray.Length, sizeof(float));
            noiseArrayBuffer.SetData(noiseArray);
            csFastNoise.SetBuffer(kernel, "ResultBuffer", noiseArrayBuffer);

            csFastNoise.GetKernelThreadGroupSizes(kernel, out var xTG, out var yTG, out _);
            csFastNoise.Dispatch(kernel, Mathf.CeilToInt(arrayWidth / (float)xTG), Mathf.CeilToInt(arrayWidth / (float)yTG), 1);

            noiseArrayBuffer.GetData(noiseArray);

            return noiseArray;
        }

        private void SetNoiseBuffers(params FastNoiseParams[] noiseParams)
        {
            if (noiseParamsBuffer == null || noiseParamsBuffer.count != noiseParams.Length)
            {
                noiseParamsBuffer?.Release();
                noiseParamsBuffer = new ComputeBuffer(noiseParams.Length, FastNoiseLayer.FastNoiseParams.GetStride());
            }
                
            noiseParamsBuffer.SetData(noiseParams);
            csFastNoise.SetBuffer(kernel, "NoiseParamsBuffer", noiseParamsBuffer);
        }

        private void SetPatchParams(int arrayWidth, Vector2 noiseOrigin, Vector2 sampleOrigin, float sampleWidth)
        {
            csFastNoise.SetInt("_ArrayWidth", arrayWidth);
            csFastNoise.SetFloat("_SampleWidth", sampleWidth);
            csFastNoise.SetFloats("_SampleOrigin", sampleOrigin.x, sampleOrigin.y);
            csFastNoise.SetFloats("_NoiseOrigin", noiseOrigin.x, noiseOrigin.y);
        }
    }

    #region FNL Enums

    public enum NoiseType
    {
        OpenSimplex2,
        OpenSimplex2S,
        Cellular,
        Perlin,
        ValueCubic,
        Value
    }

    public enum RotationType3D
    {
        None,
        ImproveXYPlanes,
        ImproveXZPlanes
    }

    public enum FractalType
    {
        None,
        FBm,
        Ridged,
        PingPong,
        DomainWarpProgressive,
        DomainWarpIndependent
    }

    public enum CellularDistanceFunction
    {
        Euclidean,
        EuclideanSq,
        Manhattan,
        Hybrid
    }

    public enum CellularReturnType
    {
        CellValue,
        Distance,
        Distance2,
        Distance2Add,
        Distance2Sub,
        Distance2Mul,
        Distance2Div
    }

    public enum DomainWarpType
    {
        OpenSimplex2,
        OpenSimplex2Reduced,
        BasicGrid
    }

    #endregion

    [System.Serializable]
    public struct FastNoiseParams
    {
        public int seed;
        [Range(0, 1f)]
        public float weight;
        public Vector2 offset;
        public NoiseType noiseType;
        [Min(0)]
        public float frequency;
        public FractalType fractalType;
        [Range(1, 20)]
        public int octaves;
        [Min(0)]
        public float lacunarity;
        [Min(0)]
        public float gain;

        public static int Stride()
        {
            return sizeof(float) * 6 + sizeof(int) * 4;
        }
    }
}