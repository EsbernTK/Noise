using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseGenerator : MonoBehaviour {

    public Vector3 floorDimensions;
    public float wallHeight;

    public bool inGenerator = true;

    public float minHeightBeforeSupport = 0.1f;
    public float minHeightBeforeAngledSupport = 0.1f;
    public Vector3 supportDimensions = new Vector3(0.1f, 0, 0.1f);


    GameObject floor;
    private Vector3[] wallPositions;
    Vector3[] wallScales;
    // Use this for initialization
    void Start()
    {
        Application.targetFrameRate = 300;

        if (!inGenerator)
        {
            supportDimensions = floorDimensions / 10f;
            GenerateFloor();
            GenerateWalls(floor);
            GenerateRoof();
            GenerateSupport();
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void Generate(Vector3 floorDim, float wallHeight)
    {
        floorDimensions = floorDim;
        this.wallHeight = wallHeight;
        supportDimensions = floorDim / 10f;
        GenerateFloor();
        GenerateWalls(floor);
        GenerateRoof();
        GenerateSupport();
    }

    void GenerateFloor()
    {
        floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.position = transform.position;
        floor.transform.localScale = floorDimensions;
        floor.transform.parent = transform;
    }

 
    void GenerateWalls(GameObject floor)
    {
        Vector3 floorPos = floor.transform.position;
        wallPositions = new Vector3[]{ new Vector3(0, wallHeight/2, floorDimensions.z/2) , new Vector3(floorDimensions.x / 2, wallHeight / 2, 0) , new Vector3(0, wallHeight / 2, -floorDimensions.z / 2), new Vector3(-floorDimensions.x / 2, wallHeight / 2, 0) };
         wallScales = new Vector3[] { new Vector3(floorDimensions.x, wallHeight, 0.1f/10), new Vector3(0.1f / 10, wallHeight, floorDimensions.z) };

        for (int i = 0; i < 4; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = floor.transform.position + wallPositions[i];
            go.transform.localScale = wallScales[i%2];
            go.transform.parent = floor.transform;
        }
    }

    void GenerateRoof()
    {
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.transform.position = transform.position + new Vector3(0,wallHeight,0);
        roof.transform.localScale = floorDimensions;
        roof.transform.parent = transform;
    }

  

    void GenerateSupport()
    {
        float supportHeight = 0;

        Vector3[] supportPositions = { new Vector3(floorDimensions.x,0, floorDimensions.z), new Vector3(floorDimensions.x, 0, -floorDimensions.z) ,
                new Vector3(-floorDimensions.x, 0, -floorDimensions.z),  new Vector3(-floorDimensions.x, 0, floorDimensions.z) };

        for (int i = 0; i < 4; i++)
        {
            Ray downRay = new Ray(floor.transform.position + supportPositions[i] / 2, -transform.up);
            Debug.DrawRay(floor.transform.position + supportPositions[i] / 2, -transform.up, Color.red,5f);

            RaycastHit hit;
            if (Physics.Raycast(downRay, out hit))
            {
                supportHeight = hit.distance;
            }

            if (supportHeight < minHeightBeforeSupport)
            {
                GenerateVerticalBeam(supportHeight, supportPositions[i]);
            }
            else
            {
                Ray angledRay = new Ray(floor.transform.position + supportPositions[i] / 2, -transform.up + (-transform.forward));
                Debug.DrawRay(floor.transform.position + supportPositions[i] / 2, -transform.up + (-transform.forward), Color.blue, 5f);
                if (Physics.Raycast(angledRay, out hit))
                {
                    supportHeight = hit.distance;
                }
                GenerateAngledBeams(supportHeight, supportPositions[i]);
            }

        }
    }


    void GenerateVerticalBeam(float supportHeight, Vector3 beamPos)
    {
        Vector3 floorPosition = floor.transform.position;

        float supportY = -supportHeight/2;

        Vector3[] supportPositions = { new Vector3(floorDimensions.x,supportY, floorDimensions.z), new Vector3(floorDimensions.x, supportY, -floorDimensions.z) ,
                new Vector3(-floorDimensions.x, supportY, -floorDimensions.z),  new Vector3(-floorDimensions.x, supportY, floorDimensions.z) };

       
        GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
        support.transform.parent = transform;

        support.transform.position = transform.position + beamPos/2.5f + new Vector3(0, supportY,0);
        support.transform.localScale = supportDimensions + new Vector3(0, supportHeight, 0);

    }

    void GenerateAngledBeams(float supportLength, Vector3 beamPos)
    {
        Vector3 floorPosition = floor.transform.position;

        float supportY = -supportLength / 2;

        Vector3[] supportPositions = { new Vector3(floorDimensions.x,supportY, floorDimensions.z), new Vector3(floorDimensions.x, supportY, -floorDimensions.z) ,
                new Vector3(-floorDimensions.x, supportY, -floorDimensions.z),  new Vector3(-floorDimensions.x, supportY, floorDimensions.z) };


        Vector3 beamDirection = transform.up +(-transform.forward);

        Quaternion beamOrientation = Quaternion.LookRotation(beamDirection);

        GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
        support.transform.parent = transform;
        support.transform.rotation = beamOrientation;

        Vector3 properbeamDirection = (-transform.up) - (transform.forward);


        support.transform.position = (transform.position + beamPos / 2.5f) + properbeamDirection.normalized*supportLength/2;

        support.transform.localScale = supportDimensions + new Vector3(0, supportLength, 0);
    }

    void GenerateTerrace()
    {

    }

    void GeneratePathWay()
    {

    }
}
