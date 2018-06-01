using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EditorOpenerScript))]
public class EditorOpenerInspector : Editor {
    
    public NodeEditor editor;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Open Noise Window", GUILayout.Width(255)))
        {
            if (editor == null)
                editor = (NodeEditor)EditorWindow.GetWindow(typeof(NodeEditor));
            else editor.Show();
        }
    }
}
