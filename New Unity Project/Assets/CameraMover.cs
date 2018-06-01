using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour {

    public Transform lookAt;
    public Transform position;

    [Range(0,1f)]
    public float lerpSpeed = 0.4f;
    [Range(0, 1f)]
    public float looklerpSpeed = 0.4f;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.position = Vector3.Lerp(transform.position, position.position, lerpSpeed);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookAt.position - transform.position), looklerpSpeed);


    }
}
