using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedNoise : MonoBehaviour {

    public float speed = 0.5f;

    public bool activated;

    ComputeTextureCreator thisComputeCreator;
	// Use this for initialization
	void Start () {
        thisComputeCreator = GetComponent<ComputeTextureCreator>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        
	}
}
