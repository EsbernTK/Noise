using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class OutputNode : BaseInputNode {
    private string result = "";
    private int buttonAmount;
    public BaseInputNode inputNode;
    private Rect[] inputNodeRects;
    private float[] minVal;
    private float[] maxVal;
    private string[] variableNames = new string[] {"Output"};
    public float[][] output;
    private bool realTimeCalculations;
    public float Length;
    bool _isLengthDecimal;
    bool _isLengthMinus;
    public bool somethingChanged;
    private bool calculateOutput;
    private bool applyToTerrain;
    private bool realTimeTerrain;
    private bool hasBeenAppliedToTerrain;
    public List<BaseInputNode> PriorityQueue = new List<BaseInputNode>();
    public List<float> instructionBuffer = new List<float>();

    public OutputNode()
    {
        buttonAmount = variableNames.Length;
        windowTitle = "Output Node";
        realTimeCalculations = false;
        somethingChanged = false;
        calculateOutput = false;
        inputNodeRects = new Rect[buttonAmount];
        minVal = new float[buttonAmount];
        maxVal = new float[buttonAmount];
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            minVal[i] = 0;
            maxVal[i] = 10;
        }
        textureCreator = EditorComputeTextureCreator.textureCreator;
    }
    public override void DrawWindow()
    {
        base.DrawWindow();
        Event e = Event.current;
        EditorGUI.BeginChangeCheck();
        realTimeCalculations = GUILayout.Toggle(realTimeCalculations, "Calculate in real time");
        calculateOutput = GUILayout.Button("Calculate Output");
        realTimeTerrain = GUILayout.Toggle(realTimeTerrain, "Apply automatically");
        applyToTerrain = GUILayout.Button("Apply To Terrain");
        if (somethingChanged)
        {
            if (calculateOutput || realTimeCalculations)
            {
                
                GiveOutput();
            }
        }
        
        if((applyToTerrain || realTimeTerrain) && !somethingChanged && !hasBeenAppliedToTerrain)
        {
            ApplyToTerrain();
        }
        GUILayout.Label("Result : " + result);
        if (e.type == EventType.Repaint)
        {
            outputRect = GUILayoutUtility.GetLastRect();
            inputNodeRects[0] = outputRect;
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
    }
    public override void DrawCurves()
    {
        if (inputNode != null)
        {
            Rect rect = windowRect;
            rect.x += inputNodeRects[0].x;
            rect.y += inputNodeRects[0].y + inputNodeRects[0].height / 2;
            rect.width = 1;
            rect.height = 1;
            NodeEditor.DrawNodeCurve(inputNode.outputRect, rect);
        }
        
    }
    public override void NodeDeleted(BaseNode node)
    {
        if (node.Equals(inputNode))
        {
            inputNode = null;
            AssetDatabase.SaveAssets();
        }
    }
    public override BaseInputNode ClickedOnInput(Vector2 pos)
    {
        BaseInputNode retVal = null;
        selectedConnection = null;
        pos.x -= windowRect.x;
        pos.y -= windowRect.y;
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodeRects[i].Contains(pos))
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
                retVal = inputNode;
                inputNode = null;
                somethingChanged = true;
                AssetDatabase.SaveAssets();
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
                inputNode = input;
                if (!input.outputNodes.Contains(this))
                    input.outputNodes.Add(this);
                somethingChanged = true;
                iHaveBeenRecalculated();
                AssetDatabase.SaveAssets();
            }
        }
        
    }
    public override float[][] GiveOutput()
    {
        somethingChanged = false;
        hasBeenAppliedToTerrain = false;
        CreatePriorityQueue();

        if (inputNode != null)
        {
            output = inputNode.GiveOutput();
        }
        if (output != null)
        {
            //EditorComputeTextureCreator.textureCreator.output2 = output[512];
            //EditorComputeTextureCreator.textureCreator.output3 = output[1];
        }
        AssetDatabase.SaveAssets();
        return output;
    }
    public override void iHaveBeenRecalculated()
    {
        somethingChanged = true;
        hasBeenAppliedToTerrain = false;
        outputIsCalculated = false;
    }
    public void ApplyToTerrain()
    {
        if (output == null)
            GiveOutput();
        if (output != null)
        {
            int heightmapRes = textureCreator.terrain.terrainData.heightmapResolution - 1;
            if (output.Length != heightmapRes || output[0].Length != heightmapRes)
            {
                output = CombineArrays(output, new Vector2(heightmapRes, heightmapRes), true);
            }
            textureCreator.ApplyToTerrain(output);
            hasBeenAppliedToTerrain = true;
        }
    }
    void OnDestroy()
    {
        somethingChanged = true;
        AssetDatabase.SaveAssets();
    }

    void CreatePriorityQueue()
    {
        if(inputNode != null)
        {
            PriorityQueue = inputNode.FindEnd();
            PriorityQueue.OrderByDescending(x => x.priority);
            Debug.Log(PriorityQueue.Count);
            int i = 0; 
            foreach (BaseInputNode node in PriorityQueue)
            {
                
                switch (node.GetType().ToString())
                {
                    case "NoiseNode":
                        node.windowTitle = string.Format("N{0}", i);
                        break;
                    case "WaveNode":
                        node.windowTitle = string.Format("W{0}", i);
                        break;
                    case "CalculationNode":
                        node.windowTitle = string.Format("C{0}", i);
                        break;

                }
                i++;
            }
            CreateInstructionBuffer();
            CreateCalculationString();
        }
    }

    public override string CreateCalculationString()
    {
        return inputNode.CreateCalculationString();
    }
    void CreateInstructionBuffer()
    {
        instructionBuffer = new List<float>();
        for (int i = 0; i < PriorityQueue.Count; i++)
        {
            BaseInputNode node = PriorityQueue[i];
            if (node.priority == 0)
            {
                instructionBuffer.Add(-1f);
                instructionBuffer.Add(i);
            }
            else if (node.HasInputs())
            {
                if(node.GetType() == typeof(NoiseNode))
                {
                    NoiseNode noiseNode = (NoiseNode)node;
                    instructionBuffer.Add(-3);
                    instructionBuffer.Add(i);
                    for (int j = 0; j < noiseNode.inputNodes.Count; j++)
                    {
                        foreach(BaseInputNode node1 in noiseNode.inputNodes[j].nodes)
                        {
                            instructionBuffer.Add(PriorityQueue.IndexOf(node1) + j / 100);
                        }
                    }
                    instructionBuffer.Add(i);
                }
                if (node.GetType() == typeof(CalculationNode))
                {
                    CalculationNode calcNode = (CalculationNode)node;
                    switch (calcNode.calcType)
                    {
                        case CalcType.Adition:
                            instructionBuffer.Add(-2.1f);
                            instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[0]));
                            instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[1]));
                            break;
                        case CalcType.Subtraction:
                            instructionBuffer.Add(-2.2f);
                            instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[0]));
                            instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[1]));
                            break;
                        case CalcType.Division:
                            instructionBuffer.Add(-2.3f);
                            instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[0]));
                            instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[1]));
                            break;
                        case CalcType.Multiplication:
                            instructionBuffer.Add(-2.4f);
                            instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[0]));
                            instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[1]));
                            break;
                        case CalcType.SingleNumber:
                            instructionBuffer.Add(-2.5f);
                            break;
                        case CalcType.Clamp:
                            instructionBuffer.Add(-2.6f);
                            if(calcNode.inputNodes[0] != null)
                                instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[0]));
                            else if(calcNode.inputNodes[1] != null)
                                instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[1]));
                            instructionBuffer.Add(calcNode.clampValues.x);
                            instructionBuffer.Add(calcNode.clampValues.y);
                            break;
                        case CalcType.ReScale:
                            instructionBuffer.Add(-2.7f);
                            if (calcNode.inputNodes[0] != null)
                                instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[0]));
                            else if (calcNode.inputNodes[1] != null)
                                instructionBuffer.Add(PriorityQueue.IndexOf(calcNode.inputNodes[1]));
                            instructionBuffer.Add(calcNode.numberScale.x);
                            instructionBuffer.Add(calcNode.numberScale.y);
                            break;



                    }

                }
            }
        }
        
    }
}
