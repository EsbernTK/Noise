using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public enum NoiseTypes { FBM, Billow, Ridge, DomainWarp, Erosion }

[System.Serializable]
public class NoiseSettings
{

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
[ExecuteInEditMode]
public class EditorComputeTextureCreator : MonoBehaviour
{

    public static EditorComputeTextureCreator textureCreator;
    [Range(8, 2048)]
    public int resolution = 512;
    [Range(0f, 1f)]
    public float noiseAmplitude = 0.5f;

    public Vector3 noiseScale = new Vector3(1, 1, 1);
    public Vector3 noiseOffset = new Vector3(1, 1, 1);


    public List<NoiseSettings> noiseSettings;

    private RenderTexture texture;

    public ComputeShader shader;
    public Shader geometryShader;

    public Terrain terrain;
    public bool applyToTerrain = false;
    public bool applySplatMap = false;

    private int _Kernel;
    private int _SecondKernel;

    public Vector2 scale = new Vector2(0, 0);
    public float sum;

    public float[] output1;
    public float[,] output3;
    public float[] output2;
    


    NoiseSettings curSettings;
    void CreateTexture()
    {
        texture = new RenderTexture(resolution, resolution, 24);
        texture.useMipMap = false;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Trilinear;
        texture.anisoLevel = 9;
        texture.enableRandomWrite = true;
        texture.Create();
        texture.name = "Procedural Texture";
    }
    void CreateTexture(Vector2 resolution)
    {
        texture = new RenderTexture((int)resolution.x, (int)resolution.y, 24);
        texture.useMipMap = false;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Trilinear;
        texture.anisoLevel = 9;
        texture.enableRandomWrite = true;
        texture.Create();
        texture.name = "Procedural Texture";
    }
    void OnEnable()
    {
        textureCreator = this;
        _Kernel = shader.FindKernel("CSMain");
        _SecondKernel = shader.FindKernel("CSSecond");
        pointMaterial = new Material(geometryShader);

    }

    public void FillTexture()
    {
        if (texture.width != resolution)
        {
            CreateTexture();
        }
        GenerateAndApplyToTerrain();
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
    void GenerateAndApplyToTerrain()
    {
        Debug.Log("Generating and Applying to terrain");
        //Vector3 vectransform = new Vector3(noiseScale.x * noiseOffset.x, noiseScale.y * noiseOffset.y, noiseScale.z* noiseOffset.z);
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f * noiseScale.x, -0.5f * noiseScale.y) + noiseOffset);
        Vector3 point10 = transform.TransformPoint(new Vector3(0.5f * noiseScale.x, -0.5f * noiseScale.y) + noiseOffset);
        Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f * noiseScale.x, 0.5f * noiseScale.y) + noiseOffset);
        Vector3 point11 = transform.TransformPoint(new Vector3(0.5f * noiseScale.x, 0.5f * noiseScale.y) + noiseOffset);

        Vector3[] posArray = { point00, point01, point10, point11 };


        ComputeBuffer positionBuffer = new ComputeBuffer(posArray.Length, 12);
        positionBuffer.SetData(posArray);


        shader.SetBuffer(_SecondKernel, "worldPositions", positionBuffer);
        shader.Dispatch(_SecondKernel, posArray.Length, 1, 1);

        shader.SetTexture(_SecondKernel, "Result", texture);
        shader.Dispatch(_Kernel, resolution / 8, resolution / 8, 1);

        TerrainData terrainData = terrain.terrainData;
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
        shader.SetBuffer(_SecondKernel, "noiseInfoBuffer", noiseInfoBuffer);

        shader.SetBuffer(_SecondKernel, "SecondResult", buffer);

        shader.Dispatch(_SecondKernel, resolution / 8, resolution / 8, 1);
        buffer.GetData(terrainHeightsOutput);
        output1 = SectionOfArray(terrainHeightsOutput, 0, 256);

        pointMaterial.SetBuffer("buf_points", buffer);

        if (applyToTerrain)
        {
            float[,] noiseHeightData = new float[resolution, resolution];
            float[,,] splatmapData = new float[resolution, resolution, terrainData.alphamapLayers];
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    noiseHeightData[x, y] = ((terrainHeightsOutput[(y) + resolution * x])) * noiseAmplitude;// * noiseSettings.amplitude;
                }
            }


            //Debug.Log(noiseHeightData[5, 5]);
            terrainData.SetHeights(0, 0, noiseHeightData);
            //GameObject _terrain = Terrain.CreateTerrainGameObject(terrainData);
            terrain.ApplyDelayedHeightmapModification();

            if (applySplatMap)
            {
                ApplySplatMap(terrainData);
                applySplatMap = false;
            }

        }

        noiseInfoBuffer.Dispose();
        buffer.Dispose();
        positionBuffer.Dispose();

        // Clean up
    }

    float GetSplatForPoint(int x, int y, int f, TerrainData terrainData)
    {
        // Normalise x/y coordinates to range 0-1 
        float y_01 = (float)y / (float)terrainData.alphamapHeight;
        float x_01 = (float)x / (float)terrainData.alphamapWidth;

        // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
        float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapResolution), Mathf.RoundToInt(x_01 * terrainData.heightmapResolution));

        // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
        Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

        // Calculate the steepness of the terrain
        float steepness = terrainData.GetSteepness(y_01, x_01);

        // Setup an array to record the mix of texture weights at this point
        float[] splatWeights = new float[terrainData.alphamapLayers];

        // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

        // Texture[0] has constant influence
        splatWeights[3] = 0.5f;

        // Texture[1] is stronger at lower altitudes
        splatWeights[2] = Mathf.Clamp01((terrainData.heightmapResolution - height));

        // Texture[2] stronger on flatter terrain
        // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
        // Subtract result from 1.0 to give greater weighting to flat surfaces
        splatWeights[2] = 1.0f - Mathf.Clamp01(steepness * steepness / (terrainData.heightmapResolution / 5.0f));

        // Texture[3] increases with height but only on surfaces facing positive Z axis 
        splatWeights[0] = height * Mathf.Clamp01(normal.z);

        // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
        float z = splatWeights.Sum();
        float[] splatArrayForPoint = new float[terrainData.alphamapLayers];

        float splatForPoint = splatWeights[f];
        // Loop through each terrain texture
        return splatForPoint;
    }

    void ApplySplatMap(TerrainData terrainData)
    {
        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y / (float)terrainData.alphamapHeight;
                float x_01 = (float)x / (float)terrainData.alphamapWidth;

                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapResolution), Mathf.RoundToInt(x_01 * terrainData.heightmapResolution));

                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

                // Calculate the steepness of the terrain
                float steepness = terrainData.GetSteepness(y_01, x_01);

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];

                // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

                // Texture[0] has constant influence
                splatWeights[0] = 0.5f;

                // Texture[1] is stronger at lower altitudes
                splatWeights[2] = Mathf.Clamp01((terrainData.heightmapResolution - height));

                // Texture[2] stronger on flatter terrain
                // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
                // Subtract result from 1.0 to give greater weighting to flat surfaces
                splatWeights[2] = 1.0f - Mathf.Clamp01(steepness * steepness / (terrainData.heightmapResolution / 5.0f));

                // Texture[3] increases with height but only on surfaces facing positive Z axis 
                splatWeights[3] = height * Mathf.Clamp01(normal.z);

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {

                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;

                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }
        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);

    }
    Material pointMaterial;
    void OnRenderObject()
    {
        pointMaterial.SetPass(0);
        pointMaterial.SetVector("_worldPos", transform.position);
        Graphics.DrawProceduralNow(MeshTopology.Points, resolution * resolution);
    }
    public float[][] GenerateNoise(NoiseSettings settings, Vector2 resolution)
    {
        
        int resolutionX = (int)resolution.x;
        int resolutionY = (int)resolution.y;
       /* if (texture.width != resolutionY || texture.height != resolutionX)
        {
            CreateTexture(resolution);
        }*/
        CreateTexture(resolution);
        curSettings = settings;
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f * noiseScale.x, -0.5f * noiseScale.y) + noiseOffset);
        Vector3 point10 = transform.TransformPoint(new Vector3(0.5f * noiseScale.x, -0.5f * noiseScale.y) + noiseOffset);
        Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f * noiseScale.x, 0.5f * noiseScale.y) + noiseOffset);
        Vector3 point11 = transform.TransformPoint(new Vector3(0.5f * noiseScale.x, 0.5f * noiseScale.y) + noiseOffset);

        Vector3[] posArray = { point00, point01, point10, point11 };


        ComputeBuffer positionBuffer = new ComputeBuffer(posArray.Length, 12);
        positionBuffer.SetData(posArray);


        shader.SetBuffer(_SecondKernel, "worldPositions", positionBuffer);
        shader.Dispatch(_SecondKernel, posArray.Length, 1, 1);

        shader.SetTexture(_SecondKernel, "Result", texture);
        shader.Dispatch(_Kernel, resolutionX / 8, resolutionY / 8, 1);

        TerrainData terrainData = terrain.terrainData;
        //terrainData.SetDetailResolution(resolution, 16);


        float[] terrainHeightsInput = new float[resolutionX * resolutionY];
        float[] terrainHeightsOutput = new float[resolutionX * resolutionY];
        NoiseInfo[] noiseInputs = new NoiseInfo[1];
        //Copy noise values from noiseSettings to noiseInputs

        for (int i = 0; i < noiseInputs.Length; i++)
        {
            noiseInputs[i].noiseType = (int)settings.noiseType;
            noiseInputs[i].weight = settings.weight;
            noiseInputs[i].frequency = settings.frequency;
            noiseInputs[i].lacunarity = settings.lacunarity;
            noiseInputs[i].persistence = settings.persistence;
            noiseInputs[i].octaves = settings.octaves;
            noiseInputs[i].amplitude = settings.damping ? settings.strength / settings.frequency : settings.strength;
        }
        ComputeBuffer buffer = new ComputeBuffer(terrainHeightsInput.Length, sizeof(float));
        ComputeBuffer noiseInfoBuffer = new ComputeBuffer(noiseInputs.Length, (sizeof(float) * 5) + sizeof(int) * 2);
        //Set noise information into buffer
        noiseInfoBuffer.SetData(noiseInputs);
        shader.SetBuffer(_SecondKernel, "noiseInfoBuffer", noiseInfoBuffer);

        shader.SetBuffer(_SecondKernel, "SecondResult", buffer);

        shader.Dispatch(_SecondKernel, resolutionX / 8, resolutionY / 8, 1);
        buffer.GetData(terrainHeightsOutput);

        //output2 = SectionOfArray( terrainHeightsOutput, 0, 256);
        float[][] tempOutput = new float[resolutionX][];
        //output2 = SectionOfArray(terrainHeightsOutput, 0, 2048);
        scale = new Vector2(terrainHeightsOutput[0], terrainHeightsOutput[0]);
        float sum = 0;
        foreach (float n in terrainHeightsOutput)
        {
            sum += Mathf.Abs(n);
            if (n > scale.y) scale.y = n;
            if (n < scale.x) scale.x = n;
        }
        sum /= terrainHeightsOutput.Length;
        this.sum = sum;
        for (int x = 0; x < resolutionX; x++)
        {
            tempOutput[x] = new float[resolutionY];
            for (int y = 0; y < resolutionY; y++)
            {
                tempOutput[x][y] = ((terrainHeightsOutput[(x) + resolutionX * y])); //* settings.strength;// * noiseSettings.amplitude;
            }

        }



        noiseInfoBuffer.Dispose();
        buffer.Dispose();
        positionBuffer.Dispose();
        return tempOutput;
    }
    public float[][] GenerateNoise(NoiseSettings settings, Vector2 resolution,Vector2 scale, Vector3 offset)
    {

        int resolutionX = (int)resolution.x;
        int resolutionY = (int)resolution.y;
        /* if (texture.width != resolutionY || texture.height != resolutionX)
         {
             CreateTexture(resolution);
         }*/
        CreateTexture(resolution);
        curSettings = settings;
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f * scale.x, -0.5f * scale.y) + offset);
        Vector3 point10 = transform.TransformPoint(new Vector3(0.5f * scale.x, -0.5f * scale.y) + offset);
        Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f * scale.x, 0.5f * scale.y) + offset);
        Vector3 point11 = transform.TransformPoint(new Vector3(0.5f * scale.x, 0.5f * scale.y) + offset);

        Vector3[] posArray = { point00, point01, point10, point11 };


        ComputeBuffer positionBuffer = new ComputeBuffer(posArray.Length, 12);
        positionBuffer.SetData(posArray);


        shader.SetBuffer(_SecondKernel, "worldPositions", positionBuffer);
        shader.Dispatch(_SecondKernel, posArray.Length, 1, 1);

        shader.SetTexture(_SecondKernel, "Result", texture);
        shader.Dispatch(_Kernel, resolutionX / 8, resolutionY / 8, 1);

        TerrainData terrainData = terrain.terrainData;
        //terrainData.SetDetailResolution(resolution, 16);


        float[] terrainHeightsInput = new float[resolutionX * resolutionY];
        float[] terrainHeightsOutput = new float[resolutionX * resolutionY];
        NoiseInfo[] noiseInputs = new NoiseInfo[1];
        //Copy noise values from noiseSettings to noiseInputs

        for (int i = 0; i < noiseInputs.Length; i++)
        {
            noiseInputs[i].noiseType = (int)settings.noiseType;
            noiseInputs[i].weight = settings.weight;
            noiseInputs[i].frequency = settings.frequency;
            noiseInputs[i].lacunarity = settings.lacunarity;
            noiseInputs[i].persistence = settings.persistence;
            noiseInputs[i].octaves = settings.octaves;
            noiseInputs[i].amplitude = settings.damping ? settings.strength / settings.frequency : settings.strength;
        }
        ComputeBuffer buffer = new ComputeBuffer(terrainHeightsInput.Length, sizeof(float));
        ComputeBuffer noiseInfoBuffer = new ComputeBuffer(noiseInputs.Length, (sizeof(float) * 5) + sizeof(int) * 2);
        //Set noise information into buffer
        noiseInfoBuffer.SetData(noiseInputs);
        shader.SetBuffer(_SecondKernel, "noiseInfoBuffer", noiseInfoBuffer);

        shader.SetBuffer(_SecondKernel, "SecondResult", buffer);

        shader.Dispatch(_SecondKernel, resolutionX / 8, resolutionY / 8, 1);
        buffer.GetData(terrainHeightsOutput);

        //output2 = SectionOfArray( terrainHeightsOutput, 0, 256);
        float[][] tempOutput = new float[resolutionX][];
        //output2 = SectionOfArray(terrainHeightsOutput, 0, 2048);
        scale = new Vector2(terrainHeightsOutput[0], terrainHeightsOutput[0]);
        float sum = 0;
        foreach (float n in terrainHeightsOutput)
        {
            sum += Mathf.Abs(n);
            if (n > scale.y) scale.y = n;
            if (n < scale.x) scale.x = n;
        }
        sum /= terrainHeightsOutput.Length;
        this.sum = sum;
        for (int x = 0; x < resolutionX; x++)
        {
            tempOutput[x] = new float[resolutionY];
            for (int y = 0; y < resolutionY; y++)
            {
                tempOutput[x][y] = ((terrainHeightsOutput[(x) + resolutionX * y])); //* settings.strength;// * noiseSettings.amplitude;
            }

        }



        noiseInfoBuffer.Dispose();
        buffer.Dispose();
        positionBuffer.Dispose();
        return tempOutput;
    }

    public float[,] GetCurve(float interval)
    {
        NoiseSettings tempSettings = curSettings;
        
        float[,] output = new float[3, (int)(1/interval)];

        for(int i = output.GetLength(1); i >= 1; i --)
        {
            tempSettings.strength = (float)i/10f;
            GenerateNoise(tempSettings, new Vector2(512, 512));
            output[0, output.GetLength(1) - i] = sum;
            output[1, output.GetLength(1) - i] = scale.x;
            output[2, output.GetLength(1) - i] = scale.y;
            
            
        }
        return output;
    }
    public void ApplyToTerrain(float[][] terrainInfo)
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.SetHeights(0, 0, ConvertToOtherArrayType(terrainInfo));
        terrain.ApplyDelayedHeightmapModification();
    }
    public float[,] ConvertToOtherArrayType(float[][] array)
    {
        float[,] output = new float[array.Length, array[0].Length];
        for (int x = 0; x < array.Length; x++)
        {
            for (int y = 0; y < array[0].Length; y++)
            {
                output[x, y] = array[x][y];
            }
        }
        return output;
    }
    public float[] SectionOfArray(float[] array, int start, int finish)
    {
        float[] output = new float[finish - start];
        for(int i = start; i < finish; i++)
        {
            output[i - start] = array[i];
        }
        return output;
    }
}
