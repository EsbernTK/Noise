using UnityEngine;

[ExecuteInEditMode]
public class ProceduralCubes : MonoBehaviour
{
	public ComputeShader cs;

    private Material mat;

    private ComputeBuffer cubes;
    public Shader shader;

    public int iterations = 1;
    private int instances = 27;

	void OnDisable ()
	{
        if (cubes != null)
        {
            cubes.Release();
            cubes.Dispose();
            cubes = null;
        }
	}
	
	private void CreateResources ()
	{
        if (mat == null)
        {
            mat = new Material(shader);
            mat.hideFlags = HideFlags.HideAndDontSave;
        }

        if (cubes == null)
        {
            cubes = new ComputeBuffer( instances , 12 * 36 + 12, ComputeBufferType.Append);
          

        }


	}

    Vector3[] verts;
    void Start()
    {
        if (cubes != null)
        {

            cubes.Release();
            cubes.Dispose();
            cubes = null;
        }
        

       instances = iterations * 3;
       instances = instances * instances * instances;
        
       // verts = new Vector3[instances * 37];
        

        CreateResources();
    
        cs.SetFloat("iterations", (float)iterations);
        cs.SetBuffer(0, "cubes", cubes);
        cs.Dispatch(0, iterations, iterations, iterations);
        mat.SetBuffer("cubeBuffer", cubes);
       // cubes.GetData(verts);
       // Debug.Log(verts.Length);
       // foreach (Vector3 v in verts)
            //Debug.Log(v);

      
    }


    void Update()
    {
        if (!SystemInfo.supportsComputeShaders)
            return;
    }

    void OnPostRender()
    {
        mat.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Triangles,  36, instances);

    }

    void OnApplicationQuit()
    {
        cubes.Release();
        cubes.Dispose();
        cubes = null;
    }



}

