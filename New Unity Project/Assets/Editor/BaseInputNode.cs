using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class FloatArray2D
{
    
    public float[][] array;
    public FloatArray2D(float[][] array)
    {
        this.array = array;
    }
    
    public int Length()
    {
        return array.Length;
    }
    public float[] this[int key]
    {
        get
        {
            return array[key];
        }
        set
        {
            SetValue(key, value);
        }
    }
    public float this[int key, int key1]
    {
        get
        {
            return array[key][key1];
        }
        set
        {
            SetValue(key,key1, value);
        }
    }
    private void SetValue(int key, float[] value)
    {
        array[key] = value;
    }
    private void SetValue(int key,int key1, float value)
    {
        array[key][key1] = value;
    }
    
}

public class BaseInputNode : BaseNode {
    public enum CalcType
    {
        Adition,
        Subtraction,
        Division,
        Multiplication,
        Clamp,
        ReScale,
        SingleNumber
    }
    public Vector2 resolution;
    public List<BaseNode> outputNodes;
    public Vector2 numberScale;
    public bool outputIsCalculated;
    public EditorComputeTextureCreator textureCreator;
    float[][] output;
    public int priority = 0;
    public BaseInputNode()
    {
        outputNodes = new List<BaseNode>();
        hasInput = true;
        numberScale = new Vector2(0, 1);
    }
    public override void DrawWindow()
    {
        
        base.DrawWindow();
        if (textureCreator == null)
        {
            textureCreator = EditorComputeTextureCreator.textureCreator;
        }
    }
    public override void DrawCurves()
    {
    }
    public string GetResult()
    {
        return "None";
    }

    public virtual List<BaseInputNode> FindEnd()
    {
        return new List<BaseInputNode>();
    }
    

    public static float[][] CombineArrays(float[][] inputFormat, float[][] outputFormat, bool interpolate)
    {
        float[][] output = outputFormat;
        float jumpX = (float)inputFormat.Length / (float) outputFormat.Length;
        float jumpY = (float)inputFormat[0].Length / (float)outputFormat[0].Length;
        if (!interpolate)
        {
            for (int x = 0; x < outputFormat.Length; x++)
            {
                for (int y = 0; y < outputFormat[0].Length; y++)
                {
                    output[x][y] = inputFormat[(int)(x * jumpX)][(int)(y * jumpY)];
                }
            }
        }
        else if (interpolate)
        {
            float[][] xOutput = new float[outputFormat.Length][];

            for (int x = 0; x < outputFormat.Length; x++)
            {
                xOutput[x] = new float[inputFormat[0].Length];
                for (int y = 0; y < xOutput[0].Length; y++)
                {
                    float xVal = 0.0f;
                    if (jumpX > 1)
                    {
                        //Debug.Log("Jump X is " + jumpX);
                        float start = (float)x * jumpX;
                        float startFrac = 1 - (start - (int)start);
                        float end = (x + 1) * jumpX;
                        float endFrac = end - (int)end;
                        float divVal = 0;
                        xVal = startFrac * inputFormat[(int)start][(int)(y)];
                        divVal += startFrac;
                        if (end < inputFormat.Length - 1)
                        {
                            xVal += endFrac * inputFormat[(int)end][(int)(y)];
                            divVal += endFrac;
                        }
                        else
                        {
                            end = inputFormat.Length - 1;
                            //divVal = end - start;
                        }
                        for (int i = Mathf.RoundToInt(start + startFrac); i < (int)end; i++)
                        {
                            xVal += inputFormat[i][(int)(y)];
                            divVal += 1;
                        }
                        xVal /= divVal;
                    }
                    else
                    {
                        //Debug.Log("Jump X is " + jumpX);
                        float start = (float)x * jumpX;
                        float startFrac = start - (int)start;
                        if ((int)start + 1 < inputFormat.Length)
                            xVal = (1 - startFrac) * inputFormat[(int)start][(int)(y)] + (startFrac) * inputFormat[(int)(start) + 1][(int)(y)];
                        else
                            xVal = inputFormat[inputFormat.Length - 1][(int)(y)];
                    }
                    float fracX = ((float)x * jumpX - (int)((float)x * jumpX));
                    if(fracX != 0f)
                        //output[x][y] = (xVal*fracX + yVal*fracY)/(fracX+fracY);
                        xOutput[x][y] = (xVal);
                    else
                        xOutput[x][y] = inputFormat[(int)(x*jumpX)][(int)(y)];
                    //output[x][y] = (xVal*(1+fracX) + yVal *(1 + fracY)) / (2 + fracY + fracX);
                    //output[x][y] = xVal;
                }
            }
            
            int oneDivJumpX = Mathf.RoundToInt(1 / jumpX);
            for (int x = 0; x < outputFormat.Length; x++)
            {
                for (int y = 0; y < outputFormat[0].Length; y++)
                {

                    float yVal = 0.0f;
                    if (jumpY > 1)
                    {
                        //Debug.Log("Jump Y is " + jumpY);
                        float start = (float)y * jumpY;
                        float startFrac = 1 - (start - (int)start);
                        float end = (y + 1) * jumpY;
                        float endFrac = end - (int)end;
                        float divVal = 0;
                        yVal = startFrac * xOutput[(int)(x)][(int)start];
                        divVal += startFrac;
                        if (end < inputFormat[0].Length - 1)
                        {
                            //yVal += endFrac * inputFormat[(int)(x * jumpX)][(int)end];
                            yVal += endFrac * xOutput[(int)(x)][(int)end];
                            divVal += endFrac;
                        }
                        else
                        {
                            end = inputFormat[0].Length - 1;
                            // divVal = end - start;
                        }
                        for (int i = Mathf.RoundToInt(start + startFrac); i <= (int)end; i++)
                        {
                            yVal += xOutput[(int)(x)][i];
                            divVal += 1;
                        }
                        yVal /= divVal;
                    }
                    else
                    {
                        //Debug.Log("Jump Y is " + jumpY);
                        float start = (float)y * (float)jumpY;
                        float startFrac = start - (int)start;
                        if ((int)start + 1 < xOutput[0].Length)
                            yVal = (1 - startFrac) * xOutput[(int)(x)][(int)start] + (startFrac) * xOutput[(int)(x)][(int)start + 1];
                        else
                            yVal = (1 - startFrac) * xOutput[(int)(x)][(int)start] + startFrac * xOutput[(int)(x)][xOutput[0].Length - 1];
                            //yVal = startFrac * xOutput[(int)(x)][xOutput[0].Length - 1];
                    }
                    float fracX = ((float)x * jumpX - (int)((float)x * jumpX));
                    float fracY = ((float)y * jumpY - (int)((float)y * jumpY));
                    if (fracY != 0f)
                        //output[x][y] = (xVal*fracX + yVal*fracY)/(fracX+fracY);
                        output[x][y] = (yVal);
                    else
                        output[x][y] = xOutput[(int)(x)][(int)(y*jumpY)];
                    //output[x][y] = (xVal*(1+fracX) + yVal *(1 + fracY)) / (2 + fracY + fracX);
                    //output[x][y] = xVal;
                }
            }
            /*for (int x = 0; x < outputFormat.Length; x++)
            {
                for (int y = 0; y < outputFormat[0].Length; y++)
                {

                    float yVal = 0.0f;
                    float xVal = 0.0f;
                    if (jumpX > 1)
                    {
                        //Debug.Log("Jump X is " + jumpX);
                        float start = (float)x * jumpX;
                        float startFrac = 1 - (start - (int)start);
                        float end = (x + 1) * jumpX;
                        float endFrac = end - (int)end;
                        float divVal = 0;
                        xVal = startFrac * inputFormat[(int)start][(int)(y * jumpY)];
                        divVal += startFrac;
                        if (end < inputFormat.Length - 1)
                        {
                            xVal += endFrac * inputFormat[(int)end][(int)(y * jumpY)];
                            divVal += endFrac;
                        }
                        else
                        {
                            end = inputFormat.Length - 1;
                            //divVal = end - start;
                        }
                        for (int i = Mathf.RoundToInt(start + startFrac); i < (int)end; i++)
                        {
                            xVal += inputFormat[i][(int)(y * jumpY)];
                            divVal += 1;
                        }
                        xVal /= divVal;
                    }
                    else
                    {
                        //Debug.Log("Jump X is " + jumpX);
                        float start = (float)x * jumpX;
                        float startFrac = start - (int)start;
                        if ((int)start + 1 < inputFormat.Length)
                            xVal = (1 - startFrac) * inputFormat[(int)start][(int)(y * jumpY)] + (startFrac) * inputFormat[(int)(start) + 1][(int)(y * jumpY)];
                        else
                            xVal = inputFormat[inputFormat.Length - 1][(int)(y * jumpY)];
                    }

                    if (jumpY > 1)
                    {
                        //Debug.Log("Jump Y is " + jumpY);
                        float start = (float)y * jumpY;
                        float startFrac = 1 - (start - (int)start);
                        float end = (y + 1) * jumpY;
                        float endFrac = end - (int)end;
                        float divVal = 0;
                        yVal = startFrac * inputFormat[(int)(x * jumpX)][(int)start];
                        divVal += startFrac;
                        if (end < inputFormat[0].Length - 1)
                        {
                            yVal += endFrac * inputFormat[(int)(x * jumpX)][(int)end];
                            yVal += endFrac * inputFormat[(int)(x * jumpX)][(int)end];
                            divVal += endFrac;
                        }
                        else
                        {
                            end = inputFormat[0].Length - 1;
                            // divVal = end - start;
                        }
                        for (int i = Mathf.RoundToInt(start + startFrac); i <= (int)end; i++)
                        {
                            yVal += inputFormat[(int)(x * jumpX)][i];
                            divVal += 1;
                        }
                        yVal /= divVal;
                    }
                    else
                    {
                        //Debug.Log("Jump Y is " + jumpY);
                        float start = (float)y * jumpY;
                        float startFrac = start - (int)start;
                        if ((int)start + 1 < inputFormat[0].Length)
                            yVal = (1 - startFrac) * inputFormat[(int)(x * jumpX)][(int)start] + (startFrac) * inputFormat[(int)(x * jumpX)][(int)(start) + 1];
                        else
                            yVal = inputFormat[(int)(x * jumpX)][inputFormat[0].Length - 1];
                    }
                    float fracX = ((float)x * jumpX - (int)((float)x * jumpX));
                    float fracY = ((float)y * jumpY - (int)((float)y * jumpY));
                    if (fracX != 0f || fracY != 0f)
                        //output[x][y] = (xVal*fracX + yVal*fracY)/(fracX+fracY);
                        output[x][y] = (xVal + yVal);
                    else
                        output[x][y] = inputFormat[(int)(x * jumpX)][(int)(y * jumpY)];
                    //output[x][y] = (xVal*(1+fracX) + yVal *(1 + fracY)) / (2 + fracY + fracX);
                    //output[x][y] = xVal;
                }
            }*/
        }

        return output;
    }
    public static float[][] CombineArrays(float[][] inputFormat, Vector2 outputFormat, bool interpolate)
    {
        float[][] output = new float[(int)outputFormat.x][];
        for (int x = 0; x < output.Length; x++)
        {
            output[x] = new float[(int)outputFormat.y];
            for (int y = 0; y < output[0].Length; y++)
            {
                output[x][y] = 0;
            }
        }
        float jumpX = (float)inputFormat.Length / (int)outputFormat.x;
        float jumpY = (float)inputFormat[0].Length / (int)outputFormat.y;
        if (!interpolate)
        {
            for (int x = 0; x < output.Length; x++)
            {
                for (int y = 0; y < output[0].Length; y++)
                {
                    output[x][y] = inputFormat[(int)(x * jumpX)][(int)(y * jumpY)];
                }
            }
        }
        else if (interpolate)
        {
            float[][] xOutput = new float[(int)outputFormat.x][];

            for (int x = 0; x < (int)outputFormat.x; x++)
            {
                xOutput[x] = new float[inputFormat[0].Length];
                for (int y = 0; y < inputFormat[0].Length; y++)
                {
                    float xVal = 0.0f;
                    if (jumpX > 1)
                    {
                        //Debug.Log("Jump X is " + jumpX);
                        float start = (float)x * jumpX;
                        float startFrac = 1 - (start - (int)start);
                        float end = (x + 1) * jumpX;
                        float endFrac = end - (int)end;
                        float divVal = 0;
                        xVal = startFrac * inputFormat[(int)start][y];
                        divVal += startFrac;
                        if (end < inputFormat.Length - 1)
                        {
                            xVal += endFrac * inputFormat[(int)end][y];
                            divVal += endFrac;
                        }
                        else
                        {
                            end = inputFormat.Length - 1;
                            //divVal = end - start;
                        }
                        for (int i = Mathf.RoundToInt(start + startFrac); i < (int)end; i++)
                        {
                            xVal += inputFormat[i][y];
                            divVal += 1;
                        }
                        xVal /= divVal;
                    }
                    else
                    {
                        //Debug.Log("Jump X is " + jumpX);
                        float start = (float)x * jumpX;
                        float startFrac = start - (int)start;
                        if ((int)start + 1 < inputFormat.Length)
                            xVal = (1 - startFrac) * inputFormat[(int)start][(int)(y)] + (startFrac) * inputFormat[(int)(start) + 1][(int)(y)];
                        else
                            xVal = inputFormat[inputFormat.Length - 1][(int)(y)];
                    }
                    float fracX = ((float)x * jumpX - (int)((float)x * jumpX));
                    float fracY = ((float)y * jumpY - (int)((float)y * jumpY));
                    if (fracX != 0f)
                        //output[x][y] = (xVal*fracX + yVal*fracY)/(fracX+fracY);
                        xOutput[x][y] = (xVal);
                    else
                        xOutput[x][y] = inputFormat[(int)(x * jumpX)][(int)(y)];
                    //output[x][y] = (xVal*(1+fracX) + yVal *(1 + fracY)) / (2 + fracY + fracX);
                    //output[x][y] = xVal;
                }
            }
            int oneDivJumpX = Mathf.RoundToInt(1 / jumpX);
            for (int x = 0; x < output.Length; x++)
            {
                for (int y = 0; y < output[0].Length; y++)
                {

                    float yVal = 0.0f;
                    if (jumpY > 1)
                    {
                        //Debug.Log("Jump Y is " + jumpY);
                        float start = (float)y * jumpY;
                        float startFrac = 1 - (start - (int)start);
                        float end = (y + 1) * jumpY;
                        float endFrac = end - (int)end;
                        float divVal = 0;
                        yVal = startFrac * xOutput[(int)(x)][(int)start];
                        divVal += startFrac;
                        if ((int)end < xOutput[0].Length - 1)
                        {
                            //yVal += endFrac * inputFormat[(int)(x * jumpX)][(int)end];
                            yVal += endFrac * xOutput[(int)(x)][(int)end];
                            divVal += endFrac;
                        }
                        else
                        {
                            end = xOutput[0].Length - 1;
                            // divVal = end - start;
                        }
                        for (int i = Mathf.RoundToInt(start + startFrac); i <= (int)end; i++)
                        {
                            yVal += xOutput[(int)(x)][i];
                            divVal += 1;
                        }
                        yVal /= divVal;
                    }
                    else
                    {
                        //Debug.Log("Jump Y is " + jumpY);
                        float start = (float)y * (float)jumpY;
                        float startFrac = start - (int)start;
                        if ((int)start + 1 < xOutput[0].Length)
                            yVal = (1 - startFrac) * xOutput[(int)(x)][(int)start] + (startFrac) * xOutput[(int)(x)][(int)start + 1];
                        else
                            yVal = (1 - startFrac) * xOutput[(int)(x)][(int)start] + startFrac * xOutput[(int)(x)][xOutput[0].Length - 1];
                        //yVal = startFrac * xOutput[(int)(x)][xOutput[0].Length - 1];
                    }
                    float fracX = ((float)x * jumpX - (int)((float)x * jumpX));
                    float fracY = ((float)y * jumpY - (int)((float)y * jumpY));
                    if (fracY != 0f)
                        output[x][y] = (yVal);
                    else
                        output[x][y] = xOutput[(int)(x)][(int)(y * jumpY)];
                }
            }
        }
        return output;
    }
    public float[][] CombineArrays(float[][] Array1, float[][] Array2, float[][] outputFormat, bool interpolate)
    {
        
        return outputFormat;
    }
    
    public float ScaleLength(Vector2 scale)
    {
        return scale.y - scale.x;
    }
    public Vector2 ReScaleAmount(Vector2 oldScale, Vector2 newScale)
    {
        Vector2 output = new Vector2(0, 0);
        float oldLen = ScaleLength(oldScale);
        float newLen = ScaleLength(newScale);
        output.x = newLen / oldLen;
        output.y = newScale.x - (oldScale.x * output.x);
        return output;

    }
    public float[][] ReScaleArray(Vector2 oldScale, Vector2 newScale, float[][] array)
    {
        Vector2 reScale = ReScaleAmount(oldScale, newScale);
        float[][] output = new float[array.Length][];
        for (int x = 0; x < array.Length; x++)
        {
            output[x] = new float[array[0].Length];
            for (int y = 0; y < array[0].Length; y++)
            {
                output[x][y] = array[x][y] * reScale.x + reScale.y;
            }
        }
        return output;
    }

    public float[][] ClampArray(Vector2 limits, float[][] array)
    {
        float[][] output = new float[array.Length][];
        for (int x = 0; x < array.Length; x++)
        {
            output[x] = new float[array[0].Length];
            for (int y = 0; y < array[0].Length; y++)
            {
                if (array[x][y] < limits.x)
                    output[x][y] = limits.x;
                else if (array[x][y] > limits.y)
                    output[x][y] = limits.y;
                else
                    output[x][y] = array[x][y];
            }
        }
        return output;
    }

    public NoiseSettings[] createNoiseSettingArray(NoiseTypes noiseType, float[][] strength, float[][] frequency, float[][] octaves, float[][] lacunarity, float[][] persistence, float[][] wheight, bool dampening)
    {
        NoiseSettings[] output = new NoiseSettings[strength.Length * strength[0].Length];
        for (int x = 0; x < strength.Length; x++)
        {
            for (int y = 0; y < strength[0].Length; y++)
            {
                NoiseSettings temp = new NoiseSettings();
                temp.noiseType = noiseType;
                temp.strength = strength[x][y];
                temp.frequency = frequency[x][y];
                temp.octaves = (int)octaves[x][y];
                temp.lacunarity = lacunarity[x][y];
                temp.persistence = persistence[x][y];
                temp.weight = wheight[x][y];
                temp.damping = dampening;
                output[y + x * strength[0].Length] = temp;
            }
        }
        return output;
    }

    public virtual void iHaveBeenRecalculated()
    {
        if (outputNodes.Count != 0)
        {
            foreach (BaseInputNode n in outputNodes)
            {
                n.outputIsCalculated = false;
                n.iHaveBeenRecalculated();
            }
        }
    }


    public virtual bool HasInputs()
    {
        return false;
    }

    public static float[][] operator +(BaseInputNode a1, BaseInputNode b1)
    {
        float[][] a = a1.GiveOutput();
        float[][] b = b1.GiveOutput();
        float[][] output = new float[0][];
        if (a != null && b != null)
        {
            output = new float[a.Length][];
            if (a.Length == b.Length && a[0].Length == b[0].Length)
            {
                for (int x = 0; x < a.Length; x++)
                {
                    output[x] = new float[a[0].Length];
                    for (int y = 0; y < a[0].Length; y++)
                    {
                        output[x][y] = a[x][y] + b[x][y];
                    }
                }
            }
            else
            {
                float[][] tempB = CombineArrays(b, new Vector2(a.Length,a[0].Length), true);
                for (int x = 0; x < a.Length; x++)
                {
                    output[x] = new float[a[0].Length];
                    for (int y = 0; y < a[0].Length; y++)
                    {
                        output[x][y] = a[x][y] + tempB[x][y];
                    }
                }
            }
        }
        return output;
    }
    public static float[][] operator -(BaseInputNode a1, BaseInputNode b1)
    {
        float[][] a = a1.GiveOutput();
        float[][] b = b1.GiveOutput();
        float[][] output = new float[a.Length][];
        if (a.Length == b.Length && a[0].Length == b[0].Length)
        {
            for (int x = 0; x < a.Length; x++)
            {
                output[x] = new float[a[0].Length];
                for (int y = 0; y < a[0].Length; y++)
                {
                    output[x][y] = a[x][y] - b[x][y];
                }
            }
        }
        else
        {
            float[][] tempB = CombineArrays(b, new Vector2(a.Length, a[0].Length), true);
            for (int x = 0; x < a.Length; x++)
            {
                output[x] = new float[a[0].Length];
                for (int y = 0; y < a[0].Length; y++)
                {
                    output[x][y] = a[x][y] - tempB[x][y];
                }
            }
        }
        return output;

    }
    public static float[][] operator *(BaseInputNode a1, BaseInputNode b1)
    {
        float[][] a = a1.GiveOutput();
        float[][] b = b1.GiveOutput();
        float[][] output = new float[a.Length][];
        if (a.Length == b.Length && a[0].Length == b[0].Length)
        {
            for (int x = 0; x < a.Length; x++)
            {
                output[x] = new float[a[0].Length];
                for (int y = 0; y < a[0].Length; y++)
                {
                    output[x][y] = a[x][y] * b[x][y];
                }
            }
        }
        else
        {
            float[][] tempB = CombineArrays(b, new Vector2(a.Length, a[0].Length), true);
            for (int x = 0; x < a.Length; x++)
            {
                output[x] = new float[a[0].Length];
                for (int y = 0; y < a[0].Length; y++)
                {
                    output[x][y] = a[x][y] * tempB[x][y];
                }
            }
        }
        return output;
    }
    public static float[][] operator /(BaseInputNode a1, BaseInputNode b1)
    {
        float[][] a = a1.GiveOutput();
        float[][] b = b1.GiveOutput();
        float[][] output = new float[a.Length][];

        if (a.Length == b.Length && a[0].Length == b[0].Length)
        {
            for (int x = 0; x < a.Length; x++)
            {
                output[x] = new float[a[0].Length];
                for (int y = 0; y < a[0].Length; y++)
                {
                    if (b[x][y] == 0f)
                    {
                        if (a[x][y] == 0f)
                            output[x][y] = 1;
                        else
                            output[x][y] = 0;
                    }
                    else
                        output[x][y] = a[x][y] / b[x][y];
                }
            }
        }
        else
        {
            float[][] tempB = CombineArrays(b, new Vector2(a.Length, a[0].Length), true);
            for (int x = 0; x < a.Length; x++)
            {
                output[x] = new float[a[0].Length];
                for (int y = 0; y < a[0].Length; y++)
                {
                    if (tempB[x][y] == 0f)
                    {
                        if (a[x][y] == 0f)
                            output[x][y] = 1;
                        else
                            output[x][y] = 0;
                    }
                    else
                        output[x][y] = a[x][y] / tempB[x][y];
                }
            }
        }
        return output;
    }
    public static float[][] operator /(float a1, BaseInputNode b1)
    {
        
        float[][] b = b1.GiveOutput();
        float[][] output = new float[b.Length][];
        for (int x = 0; x < b.Length; x++)
        {
            output[x] = new float[b[0].Length];
            for (int y = 0; y < b[0].Length; y++)
            {
                if (b[x][y] == 0f)
                {
                    output[x][y] = 0;
                }
                else
                    output[x][y] = a1 / b[x][y];
            }
        }
        return output;
    }
    public static float[][] operator /(BaseInputNode a1, float b1)
    {
        float[][] a = a1.GiveOutput();
        float[][] output = new float[a.Length][];
        for (int x = 0; x < a.Length; x++)
        {
            output[x] = new float[a[0].Length];
            for (int y = 0; y < a[0].Length; y++)
            {
                if (b1 == 0f)
                {
                    output[x][y] = 0;
                }
                else
                    output[x][y] = a[x][y] / b1;
            }
        }
        return output;
    }

    public static float[][] operator -(float a1, BaseInputNode b1)
    {

        float[][] b = b1.GiveOutput();
        float[][] output = new float[b.Length][];
        for (int x = 0; x < b.Length; x++)
        {
            output[x] = new float[b[0].Length];
            for (int y = 0; y < b[0].Length; y++)
            {
                output[x][y] = a1 - b[x][y];
            }
        }
        return output;
    }
    public static float[][] operator -(BaseInputNode a1, float b1)
    {
        float[][] a = a1.GiveOutput();
        float[][] output = new float[a.Length][];
        for (int x = 0; x < a.Length; x++)
        {
            output[x] = new float[a[0].Length];
            for (int y = 0; y < a[0].Length; y++)
            {
                output[x][y] = a[x][y] - b1;
            }
        }
        return output;
    }
    public static float[][] operator +(float a1, BaseInputNode b1)
    {

        float[][] b = b1.GiveOutput();
        float[][] output = new float[b.Length][];
        for (int x = 0; x < b.Length; x++)
        {
            output[x] = new float[b[0].Length];
            for (int y = 0; y < b[0].Length; y++)
            {
                output[x][y] = a1 + b[x][y];
            }
        }
        return output;
    }
    public static float[][] operator +(BaseInputNode a1, float b1)
    {
        float[][] a = a1.GiveOutput();
        float[][] output = new float[a.Length][];
        for (int x = 0; x < a.Length; x++)
        {
            output[x] = new float[a[0].Length];
            for (int y = 0; y < a[0].Length; y++)
            {
                output[x][y] = a[x][y] + b1;
            }
        }
        return output;
    }
    public static float[][] operator *(float a1, BaseInputNode b1)
    {

        float[][] b = b1.GiveOutput();
        float[][] output = new float[b.Length][];
        for (int x = 0; x < b.Length; x++)
        {
            output[x] = new float[b[0].Length];
            for (int y = 0; y < b[0].Length; y++)
            {
                output[x][y] = a1 * b[x][y];
            }
        }
        return output;
    }
    public static float[][] operator *(BaseInputNode a1, float b1)
    {
        float[][] a = a1.GiveOutput();
        float[][] output = new float[a.Length][];
        for (int x = 0; x < a.Length; x++)
        {
            output[x] = new float[a[0].Length];
            for (int y = 0; y < a[0].Length; y++)
            {
                output[x][y] = a[x][y] * b1;
            }
        }
        return output;
    }

    public virtual Vector2 GetScale(float[][] array)
    {
        Vector2 output = new Vector2(array[0][0], array[0][0]);
        foreach (float[] array1 in array)
        {
            foreach (float n in array1)
            {
                if (n > output.y) output.y = n;
                if (n < output.x) output.x = n;
            }
        }
        return output;
    }
    void OnDestroy()
    {
        outputIsCalculated = false;
        AssetDatabase.SaveAssets();
    }
    void OnDisable()
    {
        outputIsCalculated = false;
        AssetDatabase.SaveAssets();
    }

    void createArray(out float[][] array, int width, int depth)
    {
        float[][] temp = new float[width][];
        for (int x = 0; x < width; x++)
        {
            temp[x] = new float[depth];
        }
        array = temp;
    }
    void createArray(out float[][] array, int width, int depth, float val)
    {
        Debug.Log("creating array with val " + val);
        float[][] temp = new float[width][];
        for (int x = 0; x < width; x++)
        {
            temp[x] = new float[depth];
            for (int y = 0; y < depth; y++)
            {
                temp[x][y] = val;
            }
        }
        array = temp;
    }
    float[][] createArray(Vector2 resolution, float val)
    {
        int width = (int)resolution.x;
        int depth = (int)resolution.y;
        float[][] temp = new float[width][];
        for (int x = 0; x < width; x++)
        {
            temp[x] = new float[depth];
            for (int y = 0; y < depth; y++)
            {
                temp[x][y] = val;
            }
        }
        return temp;
    }
    void createArray(out float[][] array, int width, int depth, Vector2 vals)
    {
        Debug.Log("creating array with val " + vals);
        float[][] temp = new float[width][];
        for (int x = 0; x < width - 1; x += 2)
        {
            temp[x] = new float[depth];
            temp[x + 1] = new float[depth];
            for (int y = 0; y < depth - 1; y += 2)
            {
                temp[x][y] = vals.x;
                temp[x + 1][y] = vals.y;
                temp[x][y + 1] = vals.y;
                temp[x + 1][y + 1] = vals.x;
            }
        }
        array = temp;
    }
    void createArray(out float[][] array, int width, int depth, float val, bool consecutiveNumbers)
    {
        Debug.Log("creating array with val " + val);
        float[][] temp = new float[width][];
        for (int x = 0; x < width; x++)
        {
            temp[x] = new float[depth];
            for (int y = 0; y < depth; y++)
            {
                if (consecutiveNumbers)
                    temp[x][y] = y + val;
                else
                    temp[x][y] = y + x * val;
            }
        }
        array = temp;
    }

    public virtual string CreateCalculationString()
    {
        return null;
    }
}
