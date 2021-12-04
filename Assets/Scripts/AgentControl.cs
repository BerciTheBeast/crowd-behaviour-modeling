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
    public float visionAngle = 120.0f;
    [Min(0f)]
    public float stoppingDistance = 0.5f;

    public float destinationTresholdAngle = 78.0f;

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
        GapSeekingBehaviour();
    }

    void CheckDestinationReached()
    {
        if (respawn && agent.remainingDistance <= agent.stoppingDistance)
        {
            this.origin.Respawn(agent.gameObject); 
        }
    }


    void GapSeekingBehaviour()
    {
        List<Gap> gaps = grid.GapDetection(agent.gameObject.transform.position, gapSearchArea, seeds);
        Gap selectedGap = GapSelection(gaps);
        print(selectedGap);

        if (selectedGap != null)
        {
            // Perform gap seeking.
        }
    }

    public Gap GapSelection(List<Gap> gaps)
    {
        Vector3 agentPos = agent.gameObject.transform.position;

        Vector3 agentToDestination = destination - agentPos;
        agentToDestination.y = 0;

        print("Gaps: " + gaps.Count);


        // 1. Filter gaps that are in agent's field of vision.
        for (int i = gaps.Count - 1; i >= 0; i--)
        {
            Gap gap = gaps[i];
            Vector2 gapCenter = gap.GetCenter();
            Vector3 gapCenterWorld = (grid.GetWorldPosition((int)gap.p1.x, (int)gap.p1.y) + grid.GetWorldPosition((int)gap.p2.x, (int)gap.p2.y)) / 2;
            gap.agentToCenter = gapCenterWorld - agentPos;
            gap.agentToCenter.y = 0;

            if (gap.agentToCenter.magnitude > visionRadius || Vector3.Angle(agent.gameObject.transform.forward, gap.agentToCenter) > (visionAngle / 2))
            {
                gaps.RemoveAt(i);
            }
        }
        print("Gaps after 1: " + gaps.Count);

        // 2. Filter gaps that are too small for our agent.
        for (int i = gaps.Count - 1; i >= 0; i--)
        {
            Gap gap = gaps[i];
            Vector3 p1 = grid.GetWorldPosition((int)gap.p1[0], (int)gap.p1[1]);
            Vector3 p2 = grid.GetWorldPosition((int)gap.p2[0], (int)gap.p2[1]);
            float gapWidth = Mathf.Abs(p1[0] - p2[0]);
            float gapHeight = Mathf.Abs(p1[0] - p2[0]);

            if (Mathf.Min(gapWidth, gapHeight) < 2 * agent.radius)
            {
                gaps.RemoveAt(i);
            }
        }
        print("Gaps after 2: " + gaps.Count);

        // 3. Filter gaps would lead agent away from its destination.
        for (int i = gaps.Count - 1; i >= 0; i--)
        {
            if (Vector3.Angle(agentToDestination, gaps[i].agentToCenter) > destinationTresholdAngle)
            {
                gaps.RemoveAt(i);
            }
        }
        print("Gaps after 3: " + gaps.Count);

        // 4. Filter out gaps that the agent is not closest too, and are searched by other agents.


        // Finally select the gap that has the minimum angle between the gap and the destination.
        if (gaps.Count > 0) {
            Gap selectedGap = gaps[0];
            float minAngle = Vector3.Angle(agentToDestination, selectedGap.agentToCenter);

            foreach (Gap gap in gaps)
            {
                float angle = Vector3.Angle(agentToDestination, gap.agentToCenter);
                if (angle < minAngle)
                {
                    selectedGap = gap;
                    minAngle = angle;
                }
            }
            grid.DetectLimiters(selectedGap).Where(obj => obj != gameObject).ToList();
            return selectedGap;
        }

        return null;
    }

}
