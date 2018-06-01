using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBlocks : MonoBehaviour {

    public GameObject prefab;

    public int tiling;
    public float scale;
    public float xVal;
       

    private void Start()
    {
        scale = 1f / tiling;
        for(int i = 0; i < tiling; i++)
        {
            for(int j = 0; j < tiling; j++)
            {
                InstantiatePrefabs((float)i/(float)tiling, (float)j / (float)tiling);
            }

            
        }
 


    }

    public void InstantiatePrefabs(float indexValueX, float indexValueY)
    {
        float xStepper, yStepper;

        GameObject clone;
        clone = Instantiate(prefab, transform.position, transform.rotation);
        clone.transform.parent = transform;
        clone.GetComponent<HouseGenerator>().Generate(new Vector3(1,0.01f, 1), 1 + Random.Range(-0.1f,0.1f));
        
       

        
        xStepper = -1 + indexValueX * 2;
        yStepper = -1 + indexValueY * 2;

        clone.transform.position = transform.position + new Vector3((-transform.localScale.x / 2) * xStepper - clone.transform.lossyScale.x / 2, 
                                                                    (-transform.localScale.y / 2) * yStepper - clone.transform.lossyScale.y / 2, 
                                                                      transform.localScale.z / 2 + clone.transform.lossyScale.z /2);

      
      //  clone.GetComponent<Renderer>().material.color = new Color(Random.Range(0, 1f), 0, 0);
    }


  
}
