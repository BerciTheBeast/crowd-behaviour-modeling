using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    // Destination spawn point.
    public GameObject destinationSpawnPoint;
    SpawnPoint destination;

    //List of local vertices on the plane.
    List<Vector3> VerticeList = new List<Vector3>();
    List<Vector3> Corners = new List<Vector3>();
    Vector3 RandomPoint;
    List<Vector3> EdgeVectors = new List<Vector3>();

    // Public properties.
    public GameObject entity;
    public bool spawnable = true;
    [Min(0)]
    public int agentCount = 5;
    [Min(1)]
    public int spawnAtTime = 1;
    [Min(0f)]
    public float spawnCooldown = 1.0f;


    // Start is called before the first frame update.
    void Start()
    {
        VerticeList = new List<Vector3>(GetComponent<MeshFilter>().sharedMesh.vertices); //get vertice points from the mesh of the object
        CalculateCornerPoints();

        destination = (SpawnPoint) destinationSpawnPoint.GetComponent<SpawnPoint>();
        if (spawnable)
        {
            StartCoroutine(GenerateAgents());
        }
    }

    // Update is called on every frame.
    void Update()
    {
        // CalculateCornerPoints(); //To show corner points with transform change
    }

    void CalculateEdgeVectors(int VectorCorner)
    {
        EdgeVectors.Clear();

        EdgeVectors.Add(Corners[3] - Corners[VectorCorner]);
        EdgeVectors.Add(Corners[1] - Corners[VectorCorner]);
    }

    public Vector3 CalculateRandomPoint()
    {
        int randomCornerIdx = Random.Range(0, 2) == 0 ? 0 : 2; // There is two triangles in a plane, which triangle contains the random point is chosen.

        CalculateEdgeVectors(randomCornerIdx); // in case of transform changes edge vectors change too

        float u = Random.Range(0.0f, 1.0f);
        float v = Random.Range(0.0f, 1.0f);

        if (v + u > 1) // sum of coordinates should be smaller than 1 for the point be inside the triangle
        {
            v = 1 - v;
            u = 1 - u;
        }

        RandomPoint = Corners[randomCornerIdx] + u * EdgeVectors[0] + v * EdgeVectors[1];
        return RandomPoint;
    }

    public void CalculateCornerPoints()
    {
        Corners.Clear(); // in case of transform changes corner points are reset

        Debug.Log(VerticeList);
        Corners.Add(transform.TransformPoint(VerticeList[0])); // corner points are added to show  on the editor
        Corners.Add(transform.TransformPoint(VerticeList[10]));
        Corners.Add(transform.TransformPoint(VerticeList[110]));
        Corners.Add(transform.TransformPoint(VerticeList[120]));
    }

    IEnumerator GenerateAgents()
    {
        yield return new WaitForSeconds(0.5f);
        for (var i = 0; i < agentCount;)
        {
            for (var j = 0; j < spawnAtTime && i < agentCount; j++, i++)
            {
                GenerateNewAgent(); //go through each corner and set that to the line renderer's position
            }
            yield return new WaitForSeconds(spawnCooldown);
        }
    }

    public void GenerateNewAgent()
    {
        // Instantiate at position (0, 0, 0) and zero rotation.
        GameObject capsule = (GameObject)Instantiate(entity, CalculateRandomPoint(), Quaternion.identity);
        capsule.GetComponent<AgentControl>().destination = destination.CalculateRandomPoint();
        capsule.GetComponent<AgentControl>().origin = this;
    }

    public void Respawn(GameObject capsule)
    {
        capsule.transform.position = CalculateRandomPoint();
        capsule.GetComponent<AgentControl>().destination = destination.CalculateRandomPoint();
        capsule.GetComponent<AgentControl>().SetAgentDestination();
    }
}
