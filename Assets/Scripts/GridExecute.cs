using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridExecute : MonoBehaviour
{

    private Grid grid;
    private Vector3 origin;
    [Min(0.1f)]
    public float cellSize = 10f;

    // Start is called before the first frame update
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        origin = -new Vector3(rend.bounds.size.x / 2, 0, rend.bounds.size.z / 2);
        Debug.Log("size" + rend.bounds.size);
        Debug.Log("origin" + origin);
        grid = new Grid(Mathf.FloorToInt(rend.bounds.size.x / cellSize), Mathf.FloorToInt(rend.bounds.size.z / cellSize), cellSize, origin);
        // grid.UpdateOccupancy();
    }

    // Update is called once per frame
    void Update()
    {
        grid.UpdateOccupancy();
    }

}
