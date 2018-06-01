using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
public class ListWrapper
{
    public List<BaseNode> nodes = new List<BaseNode>();
}
[System.Serializable]
public class NoiseNode : BaseInputNode
{
    [SerializeField]
    private NoiseTypes noiseType;
    public enum NoiseType
    {
        WhiteNoise,
        Perlin,
        DomainWarp
    }
    public ComputeTextureCreator myComputeTextureCreator;
    private string result = "";
    private int buttonAmount;
    [SerializeField]
    //private List<List<BaseInputNode>> inputNodes = new List<List<BaseInputNode>>();
    public List<ListWrapper> inputNodes = new List<ListWrapper>();

    [SerializeField]
    private Vector3 noiseAreaScale = Vector3.one;
    [SerializeField]
    private Rect[] inputNodeRects;
    [SerializeField]
    private float[] hSliderVal;
    [SerializeField]
    private float[] minVal;
    private float[] minMinVal;
    [SerializeField]
    private float[] maxVal;

    private float minScale;
    private float maxScale;
    private string[] variableNames = new string[] { "strength", "offsetX", "offsetY","frequency", "octaves", "lacunarity", "persistence", "weight" };
    [SerializeField]
    public bool extendedOptions;
    [SerializeField]
    public bool advancedOptions;
    public bool dampening;
    public bool things;

    

    public float Length;
    bool _isLengthDecimal;
    bool _isLengthMinus;
    
    private float[][] output;

    public float[][] testArray;
    public float[] viewArray1;
    public float[] viewArray2;
    public float[] viewArray3;
    public NoiseNode()
    {
        
        //EditorUtility.SetDirty(this);
        windowTitle = "Noise Node";
        buttonAmount = variableNames.Length + 1;
        inputNodeRects = new Rect[buttonAmount];
        hSliderVal = new float[variableNames.Length];
        minVal = new float[buttonAmount];
        maxVal = new float[buttonAmount];
        resolution = new Vector2(256, 256);

        minMinVal = new float[] { 0, 0, 0, 1, 1, 0, 0, 0 };
        minVal = new float[] { 0, 0, 0, 1, 1, 1, 0, 0 };
        maxVal = new float[] { 1, 10, 10, 20, 8, 4, 4, 1 };
        minScale = 0;
        maxScale = 1;
        output = testArray;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            //inputNodes.Add(new List<BaseInputNode>());
            inputNodes.Add(new ListWrapper());
            
            //minVal[i] = 0;
            //maxVal[i] = 10;
        }
        for(int i = 0; i < hSliderVal.Length; i++)
        {
            hSliderVal[i] = maxVal[i] / 2;
        }
        textureCreator = EditorComputeTextureCreator.textureCreator;
    }
    public Vector2 pastRes;
    public override void DrawWindow()
    {
        base.DrawWindow();
        Event e = Event.current;
        EditorGUI.BeginChangeCheck();
        
        noiseType = (NoiseTypes)EditorGUILayout.EnumPopup("Noise Type : ", noiseType);
        resolution = EditorGUILayout.Vector2Field("Resolution", resolution);
        noiseAreaScale = EditorGUILayout.Vector3Field("Noise Scale", noiseAreaScale);
        numberScale = EditorGUILayout.Vector2Field("Number Scale", numberScale);
        if (advancedOptions)
        {
            if (extendedOptions)
            {
                GUILayout.BeginHorizontal();
                minScale = (EditorGUILayout.FloatField(minScale, GUILayout.Width(40)));
                EditorGUILayout.MinMaxSlider(ref numberScale.x, ref numberScale.y, minScale, maxScale, GUILayout.Width(100));
                maxScale = (EditorGUILayout.FloatField(maxScale, GUILayout.Width(40)));
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.MinMaxSlider(ref numberScale.x, ref numberScale.y, minScale, maxScale);
            }
        }
        //if (scale.x >= scale.y - 0.1) scale.x = scale.y - 0.1f;
        if (numberScale.x < minScale) numberScale.x = minScale;
        if (numberScale.y < minScale) numberScale.y = minScale + 0.1f;
        if (inputNodes.Count > 0)
        {
            //input1Title = inputNode.GetResult();
        }
        advancedOptions = GUILayout.Toggle(advancedOptions, "AdvancedOptions");
        if (advancedOptions)
        {
            extendedOptions = GUILayout.Toggle(extendedOptions, "Extended Options");
            dampening = GUILayout.Toggle(dampening, "Dampening");
            /*string lengthText = GUILayout.TextField((_isLengthMinus ? "" : "") + Length.ToString() + (_isLengthDecimal ? "." : ""));
            _isLengthDecimal = lengthText.EndsWith(".");
            _isLengthMinus = lengthText[0].Equals('-');

            float newLength;
            if (float.TryParse(lengthText, out newLength))
            {
                Length = newLength;
            }*/

            if (extendedOptions)
            {
                windowRect = new Rect(windowRect.x, windowRect.y, 200, 700);
                for (int i = 0; i < variableNames.Length; i++)
                {
                    GUILayout.Label(variableNames[i]);
                    if (e.type == EventType.Repaint)
                    {
                        inputNodeRects[i] = GUILayoutUtility.GetLastRect();
                    }
                    GUILayout.BeginHorizontal();
                    minVal[i] = (EditorGUILayout.FloatField(minVal[i], GUILayout.Width(40)));
                    //GUILayout.Space(100);
                    hSliderVal[i] = GUILayout.HorizontalSlider(hSliderVal[i], minVal[i], maxVal[i], GUILayout.Width(100));
                    maxVal[i] = (EditorGUILayout.FloatField(maxVal[i], GUILayout.Width(40)));
                    //float.TryParse(GUILayout.TextField(maxVal[i].ToString(), GUILayout.Width(40)), out maxVal[i]);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(45);
                    hSliderVal[i] = (EditorGUILayout.FloatField(hSliderVal[i], GUILayout.Width(100)));
                    GUILayout.EndHorizontal();
                    if (hSliderVal[i] < minVal[i]) hSliderVal[i] = minVal[i];
                    else if (hSliderVal[i] > maxVal[i]) hSliderVal[i] = maxVal[i];
                    if (minVal[i] < minMinVal[i]) minVal[i] = minMinVal[i];
                }
            }
            else
            {
                windowRect = new Rect(windowRect.x, windowRect.y, 200, 550);
                //Debug.Log(variableNames.Length);
                //Debug.Log(inputNodeRects.Length);
                for (int i = 0; i < variableNames.Length; i++)
                {
                    GUILayout.Label(variableNames[i]);
                    if (e.type == EventType.Repaint)
                    {
                        inputNodeRects[i] = GUILayoutUtility.GetLastRect();
                    }
                    GUILayout.BeginHorizontal();
                    hSliderVal[i] = GUILayout.HorizontalSlider(hSliderVal[i], minVal[i], maxVal[i], GUILayout.Width(100));
                    hSliderVal[i] = EditorGUILayout.FloatField(hSliderVal[i], GUILayout.Width(50));
                    GUILayout.EndHorizontal();
                    if (hSliderVal[i] < minVal[i]) hSliderVal[i] = minVal[i];
                    else if (hSliderVal[i] > maxVal[i]) hSliderVal[i] = maxVal[i];
                }
            }
        }
        else windowRect = new Rect(windowRect.x, windowRect.y, 200, 200);
        GUILayout.Label("Result : " + result);
        if (e.type == EventType.Repaint)
        {
            outputRect = GUILayoutUtility.GetLastRect();
            inputNodeRects[inputNodeRects.Length - 1] = outputRect;
            outputRect = new Rect(windowRect.x + windowRect.width / 1.1f, windowRect.y + outputRect.y, 1, 1);
        }
        if (EditorGUI.EndChangeCheck())
        {
            outputIsCalculated = false;
            iHaveBeenRecalculated();
            EditorUtility.SetDirty(this);
        }
    }
    
    public override void DrawCurves()
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodes.Count > 0)
            {
                /*foreach (BaseInputNode node in inputNodes[i])
                {
                    Rect rect = windowRect;
                    rect.x += inputNodeRects[i].x;
                    rect.y += inputNodeRects[i].y + inputNodeRects[i].height / 2;
                    rect.width = 1;
                    rect.height = 1;
                    NodeEditor.DrawNodeCurve(node.outputRect, rect);
                }*/

                foreach (BaseNode node in inputNodes[i].nodes)
                {
                    Rect rect = windowRect;
                    rect.x += inputNodeRects[i].x;
                    rect.y += inputNodeRects[i].y + inputNodeRects[i].height / 2;
                    rect.width = 1;
                    rect.height = 1;
                    NodeEditor.DrawNodeCurve(node.outputRect, rect);
                }
            }
        }
    }
    /*public override void NodeDeleted(BaseNode node)
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            foreach (BaseNode n in inputNodes[i])
            {
                if (node.Equals(n))
                {
                    inputNodes[i].Remove((BaseInputNode)node);
                    break;
                }
            }
        }
        AssetDatabase.SaveAssets();
    }*/
    /*public override void NodeDeleted(int node)
    {
        if (index > node) index--;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            //foreach (int n in inputNodes[i])
            for (int j = inputNodes[i].Count - 1; j >= 0; j--) // in inputNodes[i])
            {
                int n = inputNodes[i][j];
                if (node.Equals(n))
                {
                    inputNodes[i].Remove(n);
                }
                if(n > node)
                {
                    inputNodes[i][j]--;
                }
            }
        }
        AssetDatabase.SaveAssets();
    }*/
    public override void NodeDeleted(BaseNode node)
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            foreach (BaseNode n in inputNodes[i].nodes)
            {
                if (node.Equals(n))
                {
                    inputNodes[i].nodes.Remove((BaseInputNode)node);
                    outputIsCalculated = false;
                    iHaveBeenRecalculated();
                    break;
                }
            }
        }
        foreach(BaseNode n in outputNodes)
        {
            if (node.Equals(n))
            {
                outputNodes.Remove(n);
                break;
            }
        }
        AssetDatabase.SaveAssets();
    }
    public override BaseInputNode ClickedOnInput(Vector2 pos)
    {
        BaseInputNode retVal = null;
        selectedConnection = null;
        pos.x -= windowRect.x;
        pos.y -= windowRect.y;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodeRects[i].Contains(pos) && inputNodes[i].nodes.Count > 0)
            {
                /*GenericMenu menu = new GenericMenu();
                for(int i = 0; i < inputNodes.Count; i++)
                {
                    menu.AddItem(new GUIContent(""+ i), false, RemoveConnection, i);
                }
                menu.ShowAsContext();
                Debug.Log("calling method");
                retVal = selectedConnection;
                inputNodes.Remove(retVal);*/

                /*
                retVal = inputNodes[i][0];
                inputNodes[i] = new List<BaseInputNode>();*/
                retVal = (BaseInputNode) inputNodes[i].nodes[0];
                inputNodes[i].nodes = new List<BaseNode>();
                outputIsCalculated = false;
                iHaveBeenRecalculated();
            }
        }
        return retVal;
    }
    BaseInputNode selectedConnection = null;
    public void RemoveConnection(object index)
    {
        int obj = (int)index;
        //selectedConnection = inputNodes[0][obj];
    }
    public override void SetInput(BaseInputNode input, Vector2 clickPos)
    {
        clickPos.x -= windowRect.x;
        clickPos.y -= windowRect.y;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodeRects[i].Contains(clickPos))
            {
                //inputNodes[i].Add(input);
                if (!input.IsInInfiniteLoop(this))
                {
                    inputNodes[i].nodes.Add(input);
                    if(!input.outputNodes.Contains(this))
                        input.outputNodes.Add(this);
                    outputIsCalculated = false;
                    iHaveBeenRecalculated();
                }
                else { EditorUtility.DisplayDialog("Infinite Loop Error", "An infinite loop would be created by making that connection", "Okay"); }
            }
        }
        AssetDatabase.SaveAssets();
    }
    public List<int> TheMethod(int iteration)
    {
        Debug.Log(windowTitle + " " +iteration);
        if (HasInputs())
        {
            foreach (ListWrapper node in inputNodes)
            {
                foreach (BaseInputNode n in node.nodes)
                {
                    if(n.GetType() == typeof(NoiseNode))
                    return ((NoiseNode)n).TheMethod(iteration + 1);
                }
            }
        }
        return new List<int>();
    }
    public override bool HasInputs()
    {
        foreach(ListWrapper n in inputNodes)
        {
            if(n.nodes.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    public override bool IsInInfiniteLoop(BaseNode node)
    {
        bool isInInfiniteLoop = false;
        if (hasInput)
        {
            foreach(ListWrapper list in inputNodes)
            {
                for(int i = 0; i < list.nodes.Count; i++)
                {
                    if (list.nodes[i].Equals(node))
                    {
                        isInInfiniteLoop = true;
                        Debug.Log("IS IN INFINITE LOOP 1");
                        return isInInfiniteLoop;
                    }
                    else
                    {
                        isInInfiniteLoop = list.nodes[i].IsInInfiniteLoop(node);
                        if (isInInfiniteLoop)
                        {
                            Debug.Log("IS IN INFINITE LOOP 2");
                            return isInInfiniteLoop;
                        }
                    }
                }
            }
            return isInInfiniteLoop;
        }
        else
        return isInInfiniteLoop;
    }

    public override float[][] GiveOutput()
    {
        if (!outputIsCalculated || output == null)
        {
            Vector2 tempRes = resolution;
            if (tempRes.x < 8) tempRes.x = 8;
            if (tempRes.y < 8) tempRes.y = 8;
            NoiseSettings tempSettings = new NoiseSettings();

            tempSettings.strength = hSliderVal[0];
            tempSettings.noiseType = noiseType;
            tempSettings.frequency = hSliderVal[3];
            tempSettings.octaves = (int)hSliderVal[4];
            tempSettings.lacunarity = hSliderVal[5];
            tempSettings.persistence = hSliderVal[6];
            tempSettings.weight = hSliderVal[7];
            tempSettings.damping = dampening;
            output = EditorComputeTextureCreator.textureCreator.GenerateNoise(tempSettings/*EditorComputeTextureCreator.textureCreator.noiseSettings[0]*/, tempRes/*new Vector2(512,512)*/, noiseAreaScale,new Vector3(hSliderVal[1],hSliderVal[2],0));
            outputIsCalculated = true;
            if(output.Length > (int)resolution.x || output[0].Length > (int)resolution.y)
            {
                output = CombineArrays(output, resolution, true);
            }
            output = ReScaleArray(GetScale(output), numberScale, output);
            viewArray1 = output[0];
            viewArray2 = output[1];
            viewArray3 = output[2];
            return output;
        }
        else
        {
            viewArray1 = output[0];
            viewArray2 = output[1];
            viewArray3 = output[2];
            return output;
        }
    }
    public override float[] GiveOutput(Vector2 res)
    {
        return base.GiveOutput(res);
    }
    public override List<BaseInputNode> FindEnd()
    {
        List<BaseInputNode> output = new List<BaseInputNode>();
        if (HasInputs())
        {
            foreach (ListWrapper wrapper in inputNodes)
            {
                foreach (BaseInputNode node in wrapper.nodes)
                {
                    node.priority = 0;
                    List<BaseInputNode> temp = node.FindEnd();
                    if (temp.Count > priority)
                        priority = temp.Count;
                    output.AddRange(temp);
                }
            }
        }

        output.Add(this);


        return output;
    }
}
