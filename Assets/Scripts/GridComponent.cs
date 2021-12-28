using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridComponent : MonoBehaviour
{

    public Grid grid;
    private Vector3 origin;
    public bool isVisible = false;

    HashSet<GameObject> gapSeekers = new HashSet<GameObject>();
    HashSet<GameObject> followers = new HashSet<GameObject>();

    [Min(0.1f)]
    public float cellSize = 10f;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        origin = -new Vector3(rend.bounds.size.x / 2, 0, rend.bounds.size.z / 2);
        grid = new Grid(Mathf.FloorToInt(rend.bounds.size.x / cellSize), Mathf.FloorToInt(rend.bounds.size.z / cellSize), cellSize, origin, isVisible);
        grid.UpdateOccupancy();
    }

    void FixedUpdate()
    {
        grid.UpdateOccupancy();
    }

    public void AddGapSeeker(GameObject seeker)
    {
        gapSeekers.Add(seeker);
    }
    public void RemoveGapSeeker(GameObject seeker)
    {
        gapSeekers.Remove(seeker);
    }

    public void AddFollower(GameObject follower)
    {
        gapSeekers.Add(follower);
    }
    public void RemoveFollower(GameObject follower)
    {
        gapSeekers.Remove(follower);
    }
}
