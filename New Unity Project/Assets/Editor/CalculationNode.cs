using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class CalculationNode : BaseInputNode {
    
    public enum InputType
    {
        Node,
        Number
    }
    [SerializeField]
    public CalcType calcType = new CalcType();
    CalcType pastCalcType = new CalcType();
    private float[][] output;

    public Vector2 pastRes;

    public string[] variableNames = new string[] {"Var 0","Var 1"};
    [SerializeField]
    public List<BaseInputNode> inputNodes = new List<BaseInputNode>();
    float[] inputNumbers = new float[] { 1, 1, 1 };
    private Rect[] inputNodeRects;
    public float[] viewArray11;
    public float[] viewArray21;
    public float[] viewArray31;
    public Vector2 clampValues;
    bool calculateOutput;

    InputType[] inputType;
    
    public CalculationNode()
    {
        windowTitle = "Calculation Node";
        inputNodeRects = new Rect[variableNames.Length];
        pastCalcType = calcType;
        for(int i = 0; i < inputNodeRects.Length; i++)
        {
            inputNodes.Add(null);
        }
        inputType = new InputType[] { InputType.Node, InputType.Node };

    }
    public float[][] CalculateOutput()
    {
        output = new float[0][];
        switch (calcType)
        {
            case CalcType.Adition:
                if (HasInputs())
                {
                    output = inputNodes[0] + inputNodes[1];
                    outputIsCalculated = true;
                }
                else
                    output = null;
                break;
            case CalcType.Subtraction:
                if (HasInputs())
                {
                    output = inputNodes[0] - inputNodes[1];
                    outputIsCalculated = true;
                }
                else
                    output = null;
                break;
            case CalcType.Division:
                if (HasInputs())
                {
                    output = inputNodes[0] / inputNodes[1];
                    outputIsCalculated = true;
                }
                else
                    output = null;
                break;
            case CalcType.Multiplication:
                if (HasInputs())
                {
                    output = inputNodes[0] * inputNodes[1];
                    outputIsCalculated = true;
                }
                else
                    output = null;
                break;
            case CalcType.SingleNumber:
                createArray(out output, (int)resolution.x, (int)(resolution.y),inputNumbers[0]);
                outputIsCalculated = true;
                break;
            case CalcType.Clamp:
                if(inputNodes[0] != null)
                {
                    output = ClampArray(clampValues, inputNodes[0].GiveOutput());
                    outputIsCalculated = true;
                }
                else
                    output = null;
                break;
            case CalcType.ReScale:
                if (inputNodes[0] != null)
                {
                    output = ReScaleArray(inputNodes[0].numberScale, numberScale, inputNodes[0].GiveOutput());
                    outputIsCalculated = true;
                }
                else
                    output = null;
                break;
            default:
                output = null;
                break;
        }
        if (output != null && output.Length > 0)
        {
            viewArray11 = output[0];
            viewArray21 = output[1];
            viewArray31 = output[2];
        }
        CalculateScale();
        return output;
    }

    public void CalculateScale()
    {
        
        switch (calcType)
        {
            case CalcType.Adition:
                if (HasInputs())
                {
                    numberScale = inputNodes[0].numberScale + inputNodes[1].numberScale;
                }
                break;
            case CalcType.Subtraction:
                if (HasInputs())
                {
                    numberScale = inputNodes[0].numberScale - inputNodes[1].numberScale;

                }
                break;
            case CalcType.Division:
                if (HasInputs())
                {
                    numberScale = DivideVector( inputNodes[0].numberScale, inputNodes[1].numberScale);
                   
                }
                break;
            case CalcType.Multiplication:
                if (HasInputs())
                {
                    numberScale= MultiplyVector( inputNodes[0].numberScale , inputNodes[1].numberScale);
                }
                break;
            case CalcType.SingleNumber:
                numberScale = new Vector2(inputNumbers[0], inputNumbers[0] + 0.00001f);
                break;
            case CalcType.Clamp:
                if (inputNodes[0] != null)
                {
                    numberScale = inputNodes[0].numberScale;
                    numberScale.x = numberScale.x < clampValues.x ? clampValues.x : numberScale.x;
                    numberScale.y = numberScale.y > clampValues.y ? clampValues.y : numberScale.y;
                }
                break;
            case CalcType.ReScale:
                break;
            default:
                break;
        }
        if(!HasInputs() && (calcType != CalcType.ReScale && calcType != CalcType.Clamp && calcType != CalcType.SingleNumber))
        {
            foreach(BaseInputNode n in inputNodes)
            {
                if(n != null)
                {
                    numberScale = n.numberScale;
                }
            }
        }
    }

    Vector2 DivideVector(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x / b.x, a.y / b.y);
    }
    Vector2 MultiplyVector(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }
    public override void DrawWindow()
    {
        base.DrawWindow();
        float windowHeight = 150;
        Event e = Event.current;
        EditorGUI.BeginChangeCheck();
        calcType = (CalcType)EditorGUILayout.EnumPopup("Calculation Type : ", calcType);
        resolution = EditorGUILayout.Vector2Field("Resolution", resolution);
        if (calcType == CalcType.Clamp)
        {
            clampValues = EditorGUILayout.Vector2Field("Clamp Values", clampValues);
            windowHeight += 50;
        }
        if (calcType == CalcType.ReScale)
        {
            numberScale = EditorGUILayout.Vector2Field("Number Scale", numberScale);
            windowHeight += 50;
        }
        for (int i = 0; i < variableNames.Length; i++)
        {
            GUILayout.BeginHorizontal();
            if (inputType[i] == InputType.Node)
            {
                GUILayout.Label(variableNames[i]);
                if (e.type == EventType.Repaint)
                {
                    inputNodeRects[i] = GUILayoutUtility.GetLastRect();
                }
            }
            else
            {
                inputNumbers[i] = EditorGUILayout.FloatField(inputNumbers[i]);
                if(inputNodes[i] != null)
                {
                    inputNodes[i].outputNodes.Remove(this);
                    inputNodes[i] = null;
                    variableNames[i] = "Var " + i;
                }
            }
            inputType[i] = (InputType)EditorGUILayout.EnumPopup("", inputType[i], GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }
        
        windowRect = new Rect(windowRect.x, windowRect.y, 200, windowHeight);
        if (e.type == EventType.Repaint)
        {
            outputRect = GUILayoutUtility.GetLastRect();
            outputRect = new Rect(windowRect.x + windowRect.width / 1.1f, windowRect.y + outputRect.y, 1, 1);

        }
        if (EditorGUI.EndChangeCheck())
        {
            outputIsCalculated = false;
            iHaveBeenRecalculated();
            EditorUtility.SetDirty(this);
        }
        calculateOutput = GUILayout.Button("Calculate Output");
        if(!outputIsCalculated && calculateOutput)
        {
            output =  CalculateOutput();
        }
    }
    public override void DrawCurves()
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            if (inputNodes[i] != null)
            {
                Rect rect = windowRect;
                rect.x += inputNodeRects[i].x;
                rect.y += inputNodeRects[i].y + inputNodeRects[i].height / 2;
                rect.width = 1;
                rect.height = 1;
                NodeEditor.DrawNodeCurve(inputNodes[i].outputRect, rect);
            }
        }
    }
    public override void NodeDeleted(BaseNode node)
    {
        for (int i = 0; i < inputNodeRects.Length; i++)
        {
            
            if (node.Equals(inputNodes[i]))
            {
                inputNodes[i] = null;
                variableNames[i] = "Var " + i;
                outputIsCalculated = false;
                break;
            }
            
        }
        foreach (BaseNode n in outputNodes)
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
            if (inputNodeRects[i].Contains(pos) && inputNodes[i] != null)
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
                retVal = (BaseInputNode)inputNodes[i];
                inputNodes[i] = null;
                variableNames[i] = "Var " + i;
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
                    inputNodes[i] = input;
                    variableNames[i] = input.windowTitle;
                    if (!input.outputNodes.Contains(this))
                        input.outputNodes.Add(this);
                    output = CalculateOutput();
                    
                    outputIsCalculated = false;
                    
                    iHaveBeenRecalculated();
                    
                }
                else { EditorUtility.DisplayDialog("Infinite Loop Error", "An infinite loop would be created by making that connection", "Okay"); }
            }
        }
        AssetDatabase.SaveAssets();
    }
    public override bool HasInputs()
    {
        if (calcType != CalcType.Clamp)
        {
            for (int i = 0; i < inputNodes.Count; i++)
            {
                if (inputNodes[i] == null && inputType[i] != InputType.Number)
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            for (int i = 0; i < inputNodes.Count; i++)
            {
                if (inputNodes[i] != null)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public override bool IsInInfiniteLoop(BaseNode node)
    {
        bool isInInfiniteLoop = false;
        if (hasInput)
        {
            foreach (BaseInputNode list in inputNodes)
            {
                if (list != null)
                {
                    if (list.Equals(node))
                    {
                        isInInfiniteLoop = true;
                        Debug.Log("IS IN INFINITE LOOP 1");
                        return isInInfiniteLoop;
                    }
                    else
                    {
                        isInInfiniteLoop = list.IsInInfiniteLoop(node);
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
        if (!outputIsCalculated)
        {
            output = CalculateOutput();
        }
        if(output != null)
        {
            return output;
        }
        else
        {
            foreach (BaseInputNode n in inputNodes)
            {
                if (n != null)
                {
                    return n.GiveOutput();
                }
            }
        }
        createArray(out output, 256, 256, 0.5f);
        return output;
    }
    void createArray(out float[][] array, int width, int depth, float val)
    {
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
    public override List<BaseInputNode> FindEnd()
    {
        List<BaseInputNode> output = new List<BaseInputNode>();
        if (HasInputs())
        {
            foreach (BaseInputNode node in inputNodes)
            {
                if (node != null)
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
