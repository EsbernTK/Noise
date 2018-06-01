using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NoiseCurveWindow : EditorWindow {

    AnimationCurve curveX;
    AnimationCurve curveY;
    AnimationCurve curveZ;
    float[,] curveValues;
    
    [MenuItem("Window/Noise Curve Window")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(NoiseCurveWindow));
        window.position = new Rect(0, 0, 400, 199);
        window.Show();
        
    }
    void Awake()
    {
        curveX = new AnimationCurve();
        curveY = new AnimationCurve();
        curveZ = new AnimationCurve();
        Debug.Log("Awaking window");
        curveValues = EditorComputeTextureCreator.textureCreator.GetCurve(0.025f);
        for(int i = 1; i <= curveValues.GetLength(1); i++)
        {
            curveX.AddKey(new Keyframe((float)i / curveValues.GetLength(1), curveValues[0, i-1]));
            curveY.AddKey(new Keyframe((float)i / curveValues.GetLength(1), curveValues[2, i-1]));
            curveZ.AddKey(new Keyframe((float)i / curveValues.GetLength(1), curveValues[1, i-1]));
        }
    }
    void OnGUI()
    {
        curveX = EditorGUI.CurveField(
                new Rect(3, 3, position.width - 6, 50),
                "Mean", curveX);
        curveY = EditorGUI.CurveField(
                new Rect(3, 56, position.width - 6, 50),
                "Upper bound", curveY);
        curveZ = EditorGUI.CurveField(
                new Rect(3, 109, position.width - 6, 50),
                "Lower bound", curveZ);
    }
}
