using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentControl : MonoBehaviour
{
    public Vector3 destination;
    public SpawnPoint origin;
    public bool respawn = true;

    NavMeshAgent agent;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        SetAgentDestination();
    }

    public void SetAgentDestination()
    {
        agent.SetDestination(destination);
    }


    void Update() {
        CheckDestinationReached();
    }

    void CheckDestinationReached() {
        if (
            respawn &&
            !agent.pathPending &&
            (agent.remainingDistance <= agent.stoppingDistance) &&
            (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
        )
        {
            this.origin.Respawn(agent.gameObject); 
        }
    }

}
