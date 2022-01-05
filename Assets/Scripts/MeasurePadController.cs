using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    IDictionary<GameObject, List<float>> agentSpeeds = new Dictionary<GameObject, List<float>>();
    IDictionary<GameObject, List<int>> agentCounts = new Dictionary<GameObject, List<int>>();

    // Start is called before the first frame update
    void Start()
    {
        VerticeList = new List<Vector3>(GetComponent<MeshFilter>().sharedMesh.vertices); //get vertice points from the mesh of the object
        area = transform.localScale.x * transform.localScale.z;
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
        int count = GetAgentsOnPad().Count;
        foreach (KeyValuePair<GameObject, List<float>> entry in agentSpeeds)
        {
            entry.Value.Add(GetGameObjectVelocity(entry.Key));
        }
        foreach (KeyValuePair<GameObject, List<int>> entry in agentCounts)
        {
            entry.Value.Add(count);
        }
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
        if (!other.gameObject.name.Contains("Capsule"))
        {
            return;
        }
        // What to when something enters
        agentSpeeds.Add(other.gameObject, new List<float>());
        agentSpeeds[other.gameObject].Add(GetGameObjectVelocity(other.gameObject));

        agentCounts.Add(other.gameObject, new List<int>());
        agentCounts[other.gameObject].Add(GetAgentsOnPad().Count);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.name.Contains("Capsule"))
        {
            return;
        }
        // What to when something exits
        float avgDensity = (float)agentCounts[other.gameObject].Sum() / (float)agentCounts[other.gameObject].Count;
        float avgSpeed = agentSpeeds[other.gameObject].Sum() / agentSpeeds[other.gameObject].Count;
        Debug.Log("Agent exited with avg density: " + avgDensity);
        Debug.Log("Agent exited with avg speed: " + avgSpeed);
        agentCounts.Remove(other.gameObject);
        agentSpeeds.Remove(other.gameObject);
    }

    private float GetGameObjectVelocity(GameObject obj)
    {
        return obj.GetComponent<NavMeshAgent>().velocity.magnitude;
    }
}
