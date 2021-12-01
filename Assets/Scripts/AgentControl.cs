using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentControl : MonoBehaviour
{
    private GridComponent gridComponent;
    private NavMeshAgent agent;

    public Vector3 destination;
    public SpawnPoint origin;
    
    [Min(1)]
    public int gapDistance = 5;
    public bool respawn = true;
    [Min(0f)]
    public float stoppingDistance = 0.5f;


    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        gridComponent = (GridComponent)GameObject.Find("Plane").GetComponent<GridComponent>();
        SetAgentDestination();
    }

    public void SetAgentDestination()
    {
        agent.SetDestination(destination);
    }


    void Update() {
        CheckDestinationReached();
        DetectGaps();
    }

    void CheckDestinationReached()
    {
        if (respawn && agent.remainingDistance <= agent.stoppingDistance)
        {
            this.origin.Respawn(agent.gameObject); 
        }
    }

    void DetectGaps()
    {
        gridComponent.grid.FindNearbyGaps(agent.gameObject.transform.position, gapDistance);
    }

}
