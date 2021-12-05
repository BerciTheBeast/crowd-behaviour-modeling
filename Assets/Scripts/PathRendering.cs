using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathRendering : MonoBehaviour
{
    LineRenderer line; //to hold the line Renderer
    Transform target; //to hold the transform of the target
    NavMeshAgent agent; //to hold the agent of this gameObject
    public bool visible = true;

    void Start()
        {
            line = this.GetComponent<LineRenderer>(); //get the line renderer
            agent = this.GetComponent<NavMeshAgent>(); //get the agent
        }


    void Update()
    {
        if (!agent.isStopped && visible)
        {
            DrawPath(agent.path);
        }
    }

    void DrawPath(NavMeshPath path)
    {
        line.positionCount = path.corners.Length;
        line.SetPosition(0, agent.transform.position); //set the line's origin
        if (path.corners.Length < 2) //if the path has 1 or no corners, there is no need
            return;

        line.SetVertexCount(path.corners.Length); //set the array of positions to the amount of corners

        for (var i = 1; i < path.corners.Length; i++)
        {
            line.SetPosition(i, path.corners[i]); //go through each corner and set that to the line renderer's position
        }
    }
}
