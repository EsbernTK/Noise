using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowEditorNodeSaver : ScriptableObject {

    public List<BaseNode> nodes;
    public int assetAmount;
    public EditorComputeTextureCreator myComputeTextureCreator;
    public ComputeShader myShader;

    void OnEnable()
    {
        myComputeTextureCreator = (EditorComputeTextureCreator) GameObject.Find("NoiseGenerator").GetComponent(typeof( EditorComputeTextureCreator));
        //nodes = new List<BaseNode>();
    }

}
