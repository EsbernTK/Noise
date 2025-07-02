using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[Serializable]
public class NodeEditor : EditorWindow {

    [SerializeField]
    private List<BaseNode> nodes = new List<BaseNode>();
    private WindowEditorNodeSaver saver;
    private Vector2 mousePos;
    private BaseNode selectedNode;
    private bool makeTransitionMode = false;
    public static NodeEditor editor;
    private string saverName = "aaaNoise";
    private string pathName = "Assets/NoiseAssets/";
    [SerializeField]
    public EditorComputeTextureCreator myComputeTextureCreator;
    public List<BaseNode> GetNodes() { return nodes; }
    public void SetNodes(List<BaseNode> other) { nodes = other; }
    Vector2 scrollPos;
    void Awake()
    {
        pathName = pathName + saverName + ".asset";
        Debug.Log("Loading asset at: " + pathName);
        try
        {
            saver = (WindowEditorNodeSaver)AssetDatabase.LoadAssetAtPath(pathName, typeof(WindowEditorNodeSaver));
            nodes = saver.nodes;
            myComputeTextureCreator = saver.myComputeTextureCreator;
        }
        catch { }
        if (!saver)
        {
            Debug.Log("creating asset");
            saver = new WindowEditorNodeSaver();
            saver.name = saverName;
            saver.assetAmount = 0;
            saver.nodes = new List<BaseNode>();
            
            AssetDatabase.CreateAsset(saver, pathName);
        }
    }
    [MenuItem("Window/Node Editor")]
    static void ShowEditor()
    {
        editor = EditorWindow.GetWindow<NodeEditor>();
        //editor.position = new Rect(new Vector2(0f, 0f), editor.minSize);
    }
    void OnGUI()
    {
        //scrollPos = EditorGUILayout.BeginScrollView(scrollPos,true, true);
        if(editor == null)
        {
            editor = EditorWindow.GetWindow<NodeEditor>();
        }
        Event e = Event.current;
        mousePos = e.mousePosition;
        if (e.button == 1 && !makeTransitionMode)
        {
            if (e.type == EventType.MouseDown)
            {
                bool clickedOnWindow = false;
                BaseNode tempSelectedNode = null;

                foreach (BaseNode node in nodes)
                {
                    if (node.windowRect.Contains(mousePos))
                    {
                        tempSelectedNode = node;
                        clickedOnWindow = true;
                        break;
                    }
                }
                if (!clickedOnWindow)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Add Noise Node"), false, ContextCallback, "noiseNode");
                    menu.AddItem(new GUIContent("Add Output Node"), false, ContextCallback, "outputNode");
                    menu.AddItem(new GUIContent("Add Calculation Node"), false, ContextCallback, "calculationNode");
                    menu.AddItem(new GUIContent("Add Wave Node"), false, ContextCallback, "waveNode");
                    menu.ShowAsContext();
                    e.Use();
                }
                else
                {
                    GenericMenu menu = new GenericMenu();
                    if (tempSelectedNode.GetType() != typeof(OutputNode))
                        menu.AddItem(new GUIContent("Make Transition"), false, ContextCallback, "makeTransition");
                    if(tempSelectedNode.GetType() == typeof(NoiseNode))
                        menu.AddItem(new GUIContent("Call Method"), false, ContextCallback, "callMethod");
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, "deleteNode");
                    menu.ShowAsContext();
                    e.Use();
                }
            }
        }
        else if (e.button == 0 && e.type == EventType.MouseDown && makeTransitionMode)
        {
            bool clickedOnWindow = false;
            BaseNode tempSelectedNode = null;

            foreach (BaseNode node in nodes)
            {
                if (node.windowRect.Contains(mousePos))
                {
                    tempSelectedNode = node;
                    clickedOnWindow = true;
                    break;
                }
            }
            if (clickedOnWindow && !tempSelectedNode.Equals(selectedNode))
            {
                tempSelectedNode.SetInput((BaseInputNode)selectedNode, mousePos);
                makeTransitionMode = false;
                selectedNode = null;
            }
            if (!clickedOnWindow)
            {
                makeTransitionMode = false;
                selectedNode = null;
            }
        }
        else if (e.button == 0 && e.type == EventType.MouseDown && !makeTransitionMode)
        {
            bool clickedOnWindow = false;
            BaseNode tempSelectedNode = null;

            foreach (BaseNode node in nodes)
            {
                if (node.windowRect.Contains(mousePos))
                {
                    tempSelectedNode = node;
                    clickedOnWindow = true;
                    break;
                }
            }
            if (clickedOnWindow)
            {
                BaseInputNode nodeToChange = tempSelectedNode.ClickedOnInput(mousePos);
                if(nodeToChange != null)
                {
                    makeTransitionMode = true;
                    selectedNode = nodeToChange;
                }
            }
        }
        

        scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos, new Rect(0, 0, 1000, 1000));

        if (makeTransitionMode && selectedNode != null)
        {
            Rect mouseRect = new Rect(scrollPos.x + mousePos.x, scrollPos.y + mousePos.y, 10, 10);
            DrawNodeCurve(selectedNode.outputRect, mouseRect);
            Repaint();
        }
        for(int i = nodes.Count-1; i >= 0; i--)
        {
            if(nodes[i] == null)
            {
                nodes.Remove(nodes[i]);
            }
        }
        foreach(BaseNode n in nodes)
        {
            if(n != null)
                n.DrawCurves();
        }
        
        BeginWindows();
        for(int i = 0; i < nodes.Count; i++)
        {
            //Debug.Log("Drawing node: " + nodes[i].windowTitle + " at index: " + i + " with rect: " + nodes[i].windowRect);
            Rect tempRect = GUILayout.Window(i, nodes[i].windowRect, DrawNodeWindow, nodes[i].windowTitle);
            Rect nodeRect = nodes[i].windowRect;
            nodeRect.x = tempRect.x;
            nodeRect.y = tempRect.y;
            nodes[i].windowRect = nodeRect;
            nodes[i].id = i;
        }
        EndWindows();
        GUI.EndScrollView();
    }

    void DrawNodeWindow(int id)
    {
        nodes[id].DrawWindow();
        GUI.DragWindow();
    }

    void ContextCallback(object obj)
    {
        string clb = obj.ToString();
        if (saver == null)
        {
            Debug.LogError("saver is null");
            return;
        }

        BaseNode noiseNode = null;

        if (clb.Equals("noiseNode"))
        {
            noiseNode = ScriptableObject.CreateInstance<NoiseNode>();
            //noiseNode.windowRect = new Rect(mousePos, new Vector2(200, 150));
        }
        else if (clb.Equals("calculationNode"))
        {
            noiseNode = ScriptableObject.CreateInstance<CalculationNode>();
            //noiseNode.windowRect = new Rect(mousePos, new Vector2(200, 150));
        }
        else if (clb.Equals("outputNode"))
        {
            noiseNode = ScriptableObject.CreateInstance<OutputNode>();
            //noiseNode.windowRect = new Rect(mousePos, new Vector2(200, 200));
        }
        else if (clb.Equals("waveNode"))
        {
            noiseNode = ScriptableObject.CreateInstance<WaveNode>();
            //noiseNode.windowRect = new Rect(mousePos, new Vector2(200, 200));
        }
        else if (clb.Equals("makeTransition"))
        {
            bool clickedOnWindow = false;
            BaseNode tempSelectedNode = null;

            foreach (BaseNode node in nodes)
            {
                if (node.windowRect.Contains(mousePos))
                {
                    tempSelectedNode = node;
                    clickedOnWindow = true;
                    break;
                }
            }
            if (clickedOnWindow)
            {
                selectedNode = tempSelectedNode;
                makeTransitionMode = true;

            }
            AssetDatabase.SaveAssets();
        }
        else if (clb.Equals("callMethod"))
        {
            foreach (BaseNode node in nodes)
            {
                if (node.windowRect.Contains(mousePos))
                {
                    ((NoiseNode)(node)).TheMethod(0);
                    break;
                }
            }
        }
        else if (clb.Equals("deleteNode"))
        {
            bool clickedOnWindow = false;
            BaseNode tempSelectedNode = null;
            int index = 0;
            foreach (BaseNode node in nodes)
            {
                if (node.windowRect.Contains(mousePos))
                {
                    index = nodes.IndexOf(node);
                    tempSelectedNode = node;
                    clickedOnWindow = true;
                    break;
                }
            }
            if (clickedOnWindow)
            {
                //nodes = nodes.Where(x => x.index != index).ToList();
                //nodes.Remove(tempSelectedNode);
                nodes.Remove(tempSelectedNode);
                saver.nodes.Remove(tempSelectedNode);
                //nodes.RemoveAt(index);
                foreach (BaseNode node in nodes)
                {
                    node.NodeDeleted(tempSelectedNode);
                    //node.NodeDeleted(index);
                }
                DestroyImmediate(tempSelectedNode, true);
                AssetDatabase.SaveAssets();
                //AssetDatabase.ImportAsset(pathName);
            }
        }



        if(noiseNode != null)
        {
            nodes.Add(noiseNode);
            noiseNode.name = "zzz" + noiseNode.name + saver.assetAmount.ToString();
            noiseNode.index = nodes.Count - 1;
            saver.assetAmount += 1;
            saver.nodes.Add(noiseNode);
            AssetDatabase.AddObjectToAsset(noiseNode, pathName);
            AssetDatabase.SaveAssets();
        }


    }
    public static void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowCol = new Color(0, 0, 0, 0.06f);
        for(int i = 0; i < 3; i++)
        {
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        }
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }
    void OnDestroy()
    {
        foreach(BaseNode n in nodes)
        {
            if(n.GetType() == typeof(NoiseNode) || n.GetType() == typeof(CalculationNode))
            {
                BaseInputNode temp =  (BaseInputNode)n;
                temp.outputIsCalculated = false;
            }
        }
        AssetDatabase.SaveAssets();
    }
    void OnDisable()
    {
        foreach (BaseNode n in nodes)
        {
            if (n.GetType() == typeof(BaseInputNode))
            {
                BaseInputNode temp = (BaseInputNode)n;
                temp.outputIsCalculated = false;
            }
        }
        AssetDatabase.SaveAssets();
    }
}
