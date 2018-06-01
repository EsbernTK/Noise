using System.Collections.Generic;
using UnityEngine;

//This game object invokes PlaneComputeShader (when attached via drag'n drop in the editor) using the PlaneBufferShader (also attached in the editor)
//to display a grid of points moving back and forth along the z axis.
public class useSimpleComputeShader : MonoBehaviour
{
    public Transform Quad;

    [Range(8, 2048)]
    public int resolution = 512;
    [Range(0f, 1f)]
    public float noiseAmplitude = 0.5f;

    [System.Serializable]
    public class NoiseSettings
    {
        public enum NoiseTypes { FBM, Billow, Ridge, DomainWarp, Erosion }

        public NoiseTypes noiseType = NoiseTypes.FBM;

        [Range(0f, 1f)]
        public float strength = 1f;

        public bool damping;

        [Range(1, 20)]
        public float frequency = 5;

        [Range(1, 8)]
        public int octaves = 1;

        [Range(1f, 4f)]
        public float lacunarity = 2f;

        [Range(0f, 4f)]
        public float persistence = 0.5f;

        [Range(0f, 1f)]
        public float weight = 0.5f;
    }

    public List<NoiseSettings> noiseSettings;

    public Shader shader;
    public ComputeShader computeShader;
    public ComputeShader noiseShader;
    private ComputeBuffer noiseBuffer;
    private ComputeBuffer outputBuffer;
    private ComputeBuffer constantBuffer;
    private int _kernel;
    private int _Kernel;
    private int _SecondKernel;
    private Material material;
    //private RenderTexture texture;
    private RenderTexture texture;

    public const int VertCount = 16384; //32*32*4*4 (Groups*ThreadsPerGroup)

    //We initialize the buffers and the material used to draw.
    void Start()
    {
        CreateBuffers();
        CreateMaterial();
        _kernel = computeShader.FindKernel("CSMain");
        _Kernel = noiseShader.FindKernel("CSMain");
        _SecondKernel = noiseShader.FindKernel("CSSecond");
    }

    //When this GameObject is disabled we must release the buffers or else Unity complains.
    private void OnDisable()
    {
        ReleaseBuffer();
    }
    void CreateTexture()
    {

        //texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);


        texture = new RenderTexture(resolution, resolution, 24);
        texture.useMipMap = false;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Trilinear;
        texture.anisoLevel = 9;
        texture.enableRandomWrite = true;
        texture.Create();
        texture.name = "Procedural Texture";
        Quad.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
    }
    //After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
    //this just draws the "mesh" as a set of points
    void OnPostRender()
    {
        Dispatch();

        material.SetPass(0);
        material.SetBuffer("buf_Points", outputBuffer);
        Graphics.DrawProcedural(MeshTopology.Lines , VertCount);
    }

    //To setup a ComputeBuffer we pass in the array length, as well as the size in bytes of a single element.
    //We fill the offset buffer with random numbers between 0 and 2*PI.
    void CreateBuffers()
    {
        noiseBuffer = new ComputeBuffer(VertCount, 4); //Contains a single float value (OffsetStruct)

        //float[] values = new float[VertCount];

        //for (int i = 0; i < VertCount; i++)
        //{
        //    values[i] = Random.value * 2 * Mathf.PI;
        //}

        //noiseBuffer.SetData(values);
        CreateTexture();
        noiseBuffer.SetData(GetNoiseData());

        constantBuffer = new ComputeBuffer(1, 4); //Contains a single element (time) which is a float

        outputBuffer = new ComputeBuffer(resolution*resolution, sizeof(float)*3); //Output buffer contains vertices (float3 = Vector3 -> 12 bytes)
    }
    struct NoiseInfo
    {
        public int noiseType;
        public float frequency;
        public int octaves;
        public float lacunarity;
        public float persistence;
        public float amplitude;
        public float weight;
    }
    float[] GetNoiseData()
    {
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
        Vector3 point10 = transform.TransformPoint(new Vector3(0.5f, -0.5f));
        Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
        Vector3 point11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));

        Vector3[] posArray = { point00, point01, point10, point11 };


        ComputeBuffer positionBuffer = new ComputeBuffer(posArray.Length, 12);
        positionBuffer.SetData(posArray);


        noiseShader.SetBuffer(_SecondKernel, "worldPositions", positionBuffer);
        noiseShader.Dispatch(_SecondKernel, posArray.Length, 1, 1);

        noiseShader.SetTexture(_SecondKernel, "Result", texture);
        noiseShader.Dispatch(_Kernel, resolution / 8, resolution / 8, 1);
        positionBuffer.Dispose();
        //TerrainData terrainData = terrain.terrainData;
        //terrainData.SetDetailResolution(resolution, 16);


        float[] terrainHeightsInput = new float[resolution * resolution];
        float[] terrainHeightsOutput = new float[resolution * resolution];
        NoiseInfo[] noiseInputs = new NoiseInfo[noiseSettings.Count];
        //Copy noise values from noiseSettings to noiseInputs

        for (int i = 0; i < noiseInputs.Length; i++)
        {
            noiseInputs[i].noiseType = (int)noiseSettings[i].noiseType;
            noiseInputs[i].weight = noiseSettings[i].weight;
            noiseInputs[i].frequency = noiseSettings[i].frequency;
            noiseInputs[i].lacunarity = noiseSettings[i].lacunarity;
            noiseInputs[i].persistence = noiseSettings[i].persistence;
            noiseInputs[i].octaves = noiseSettings[i].octaves;
            noiseInputs[i].amplitude = noiseSettings[i].damping ? noiseSettings[i].strength / noiseSettings[i].frequency : noiseSettings[i].strength;
        }

        ComputeBuffer buffer = new ComputeBuffer(terrainHeightsInput.Length, sizeof(float));
        ComputeBuffer noiseInfoBuffer = new ComputeBuffer(noiseInputs.Length, (sizeof(float) * 5) + sizeof(int) * 2);
        //Set noise information into buffer
        noiseInfoBuffer.SetData(noiseInputs);
        noiseShader.SetBuffer(_SecondKernel, "noiseInfoBuffer", noiseInfoBuffer);

        noiseShader.SetBuffer(_SecondKernel, "SecondResult", buffer);

        noiseShader.Dispatch(_SecondKernel, resolution / 8, resolution / 8, 1);
        buffer.GetData(terrainHeightsOutput);
        buffer.Dispose();
        noiseInfoBuffer.Dispose();
        return terrainHeightsOutput;
    }

    //For some reason I made this method to create a material from the attached shader.
    void CreateMaterial()
    {
        material = new Material(shader);
    }

    //Remember to release buffers and destroy the material when play has been stopped.
    void ReleaseBuffer()
    {
        constantBuffer.Release();
        noiseBuffer.Release();
        outputBuffer.Release();

        DestroyImmediate(material);
    }

    //The meat of this script, it sets the constant buffer (current time) and then sets all of the buffers for the compute shader.
    //We then dispatch 32x32x1 groups of threads of our CSMain kernel.
    void Dispatch()
    {
        constantBuffer.SetData(new[] { Time.time });

        computeShader.SetBuffer(_kernel, "cBuffer", constantBuffer);
        computeShader.SetBuffer(_kernel, "noises", noiseBuffer);
        computeShader.SetBuffer(_kernel, "output", outputBuffer);

        computeShader.Dispatch(_kernel, 32, 32, 1);
    }
}