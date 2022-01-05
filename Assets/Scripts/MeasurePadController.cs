using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasurePadController : MonoBehaviour
{
    public string title;

    //List of local vertices on the plane.
    List<Vector3> VerticeList = new List<Vector3>();
    List<Vector3> Corners = new List<Vector3>();
    Vector3 RandomPoint;
    List<Vector3> EdgeVectors = new List<Vector3>();

    Renderer rend;
    private float nextActionTime = 0.0f;
    [Min(0.0f)]
    public float period = 0.1f;
    private float area;

    // Start is called before the first frame update
    void Start()
    {
        VerticeList = new List<Vector3>(GetComponent<MeshFilter>().sharedMesh.vertices); //get vertice points from the mesh of the object
        rend = GetComponent<Renderer>();
        area = rend.bounds.size.x * rend.bounds.size.z;
    }

    // Fixed update is called on physics update
    void FixedUpdate()
    {
        /*
        if (Time.time > nextActionTime)
        {
            Debug.Log("Density: " + GetDensity());
        }
        */
    }

    public List<GameObject> GetAgentsOnPad()
    {
        Vector3 pos = transform.position;
        pos.y = 0.1f;
        Collider[] hitColliders = Physics.OverlapBox(pos, new Vector3(transform.localScale.x / 2, 0, transform.localScale.z / 2), transform.rotation);

        HashSet<GameObject> limiters = new HashSet<GameObject>();
        foreach (Collider col in hitColliders)
        {
            limiters.Add(col.gameObject);
        }
        return limiters.Where(obj => obj.name.Contains("Capsule")).ToList();
    }

    public float GetDensity()
    {
        nextActionTime += period;
        // Compute desnity.
        List<GameObject> agents = GetAgentsOnPad();
        Debug.Log("Agent count: " + agents.Count);
        // TODO
        return 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        // What to when something enters
        // other.gameObject
        Debug.Log("Enter");
    }

    private void OnTriggerExit(Collider other)
    {
        // What to when something exits
        // other.gameObject
        Debug.Log("Exit");
    }
}
