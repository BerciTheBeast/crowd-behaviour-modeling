using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AgentBehaviourType
{
    Default,
    GapSeeking,
    Following,
    StopAndGo,
    Overtagking
}

public class AgentControl : MonoBehaviour
{
    private GridComponent gridComponent;
    private Grid grid;
    private NavMeshAgent agent;

    public Vector3 destination;
    public SpawnPoint origin;
    
    public bool respawn = true;
    [Min(1)]
    public int seeds = 3;
    [Min(1)]
    public int gapSearchArea = 5;
    [Min(1.0f)]
    public float visionRadius = 2.5f;
    public int visionAngle = 120;
    [Min(0f)]
    public float stoppingDistance = 0.5f;

    public AgentBehaviourType behaviour = AgentBehaviourType.Default;


    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        gridComponent = (GridComponent)GameObject.Find("Plane").GetComponent<GridComponent>();
        grid = gridComponent.grid;
        SetAgentDestination();
    }

    public void SetAgentDestination()
    {
        agent.SetDestination(destination);
    }


    void Update() {
        CheckDestinationReached();
        GapSeeking();
    }

    void CheckDestinationReached()
    {
        if (respawn && agent.remainingDistance <= agent.stoppingDistance)
        {
            this.origin.Respawn(agent.gameObject); 
        }
    }


    void GapSeeking()
    {
        List<Gap> gaps = grid.GapDetection(agent.gameObject.transform.position, gapSearchArea, seeds);
        gaps = GapSelection(gaps);
    }

    public List<Gap> GapSelection(List<Gap> gaps)
    {
        Vector3 agentPos = agent.gameObject.transform.position;

        // 1. Filter gaps that are in agent's field of vision.
        for (int i = gaps.Count - 1; i >= 0; i--)
        {
            Gap gap = gaps[i];
            Vector2 gapCenter = gap.GetCenter();
            Vector3 p = grid.GetWorldPosition((int)gapCenter[0], (int)gapCenter[1]) - agentPos;

            if (p.magnitude > visionRadius || Vector3.Angle(agent.gameObject.transform.forward, p) > (visionAngle / 2))
            {
                gaps.RemoveAt(i);
            }
        }

        // 2. Filter gaps that are too small for our agent.
        for (int i = gaps.Count - 1; i >= 0; i--)
        {
            Gap gap = gaps[i];
            Vector3 p1 = grid.GetWorldPosition(gap.p1[0], gap.p1[1]);
            Vector3 p2 = grid.GetWorldPosition(gap.p2[0], gap.p2[1]);
            float gapWidth = Mathf.Abs(p1[0] - p2[0]);
            float gapHeight = Mathf.Abs(p2[0] - p2[0]);

            if (Mathf.Min(gapWidth, gapHeight) < 2 * agent.radius)
            {
                gaps.RemoveAt(i);
            }
            
        }

        return gaps;
    }

}
