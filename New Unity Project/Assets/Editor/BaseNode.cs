using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class BaseNode : ScriptableObject {
    public Rect windowRect;
    public Rect outputRect;
    public bool hasInput = false;
    public string windowTitle = "";
    public int index;
    public int id;

    public virtual void DrawWindow()
    {
        windowTitle = EditorGUILayout.TextField("Title", windowTitle);
    }
    public abstract void DrawCurves();
    public virtual void SetInput(BaseInputNode input, Vector2 pos)
    {

    }
    public virtual void NodeDeleted(BaseNode node){ }
    //public virtual void NodeDeleted(int node) { }

    public virtual bool IsInInfiniteLoop(BaseNode node) { return false; }
    public virtual BaseInputNode ClickedOnInput(Vector2 pos)
    {
        return null;
    }

    public virtual float[][] GiveOutput()
    {
        return new float[0][];
    }
    public virtual float[] GiveOutput(Vector2 res)
    {
        return new float[0];
    }
}
