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
    public Vector3 startingPosition;
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
    private float seekingEnd;

    // bounding coefficients
    [Range(0.000001f, 1f)]
    public float alpha = 0.5f;
    [Min(0f)]
    public float beta = 0.75f;
    [Min(1f)]
    public float lambda = 2.3f;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        gridComponent = (GridComponent)GameObject.Find("Plane").GetComponent<GridComponent>();
        grid = gridComponent.grid;
        startingPosition = agent.gameObject.transform.position;
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
        if (behaviour == AgentBehaviourType.Default && respawn && agent.remainingDistance <= agent.stoppingDistance)
        {
            this.origin.Respawn(agent.gameObject);
        }
    }


    void GapSeekingBehaviour()
    {
        if (behaviour == AgentBehaviourType.Default && Random.value < GetGapSeekingProbability())
        {
            List<Gap> gaps = grid.GapDetection(agent.gameObject.transform.position, gapSearchArea, seeds);
            Gap selectedGap = GapSelection(gaps);

            if (selectedGap != null)
            {
                DrawGap(selectedGap);
                GapSeeking(selectedGap);
            }
        } else if (ShouldEndSeeking())
        {
            EndSeeking();
        }
    }

    public Gap GapSelection(List<Gap> gaps)
    {
        Vector3 agentPos = agent.gameObject.transform.position;
        Vector3 agentToDestination = destination - agentPos;
        agentToDestination.y = 0;


        // 1. Filter gaps that are in agent's field of vision.
        for (int i = gaps.Count - 1; i >= 0; i--)
        {
            Gap gap = gaps[i];
            Vector3 gapCenter = (grid.GetWorldPosition((int)gap.p1.x, (int)gap.p1.y) + grid.GetWorldPosition((int)gap.p2.x, (int)gap.p2.y)) / 2;
            gap.agentToCenter = gapCenter - agentPos;
            gap.agentToCenter.y = 0;

            if (gap.agentToCenter.magnitude > visionRadius || Vector3.Angle(agent.gameObject.transform.forward, gap.agentToCenter) > (visionAngle / 2))
            {
                gaps.RemoveAt(i);
            }
        }

        // 2. Filter gaps that are too small for our agent.
        for (int i = gaps.Count - 1; i >= 0; i--)
        {
            Gap gap = gaps[i];
            Vector3 p1 = grid.GetWorldPosition((int)gap.p1.x, (int)gap.p1.y);
            Vector3 p2 = grid.GetWorldPosition((int)gap.p2.x, (int)gap.p2.y);
            float gapWidth = Mathf.Abs(p1.x - p2.x);
            float gapHeight = Mathf.Abs(p1.z - p2.z);
            Vector3 agentScale = agent.gameObject.transform.localScale;

            print(agent.radius * Mathf.Max(agentScale.x, agentScale.z));
            if (Mathf.Min(gapWidth, gapHeight) < 2 * (agent.radius * Mathf.Max(agentScale.x, agentScale.z)))
            {
                gaps.RemoveAt(i);
            }
        }

        // 3. Filter gaps would lead agent away from its destination.
        for (int i = gaps.Count - 1; i >= 0; i--)
        {
            if (Vector3.Angle(agentToDestination, gaps[i].agentToCenter) > destinationTresholdAngle)
            {
                gaps.RemoveAt(i);
            }
        }

        // TODO: 4. Filter out gaps that the agent is not closest too, and are searched by other agents.

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
            return selectedGap;
        }

        return null;
    }

    public void GapSeeking(Gap gap)
    {
        behaviour = AgentBehaviourType.GapSeeking;
        // Perform gap seeking.
        // Get limiter objects
        Debug.Log("Get limiter objects");
        List<GameObject> limiterList = grid.DetectLimiters(gap).FindAll(obj => obj != gameObject);

        // Calculate gap speed.
        Debug.Log("Calculate gap speed for limiter count: " + limiterList.Count);
        Vector3 gapSpeed = new Vector3(0, 0, 0);
        if (limiterList.Count > 0)
        {
            foreach (GameObject obj in limiterList)
            {
                gapSpeed += obj.GetComponent<NavMeshAgent>().velocity;
            }
            gapSpeed = gapSpeed / limiterList.Count;
        }
        Debug.Log("Gap speed: " + gapSpeed);

        // Determine seeking time.
        float gapSeekerSpeed = GetGapSeekerSpeed(GetGapArea(gap));
        float agentToGapCenterMagnitude = gap.agentToCenter.magnitude;
        float seekingTime = agentToGapCenterMagnitude / gapSeekerSpeed;

        // Calculate gap position in the future.
        Vector3 gapCenter = (grid.GetWorldPosition((int)gap.p1.x, (int)gap.p1.y) + grid.GetWorldPosition((int)gap.p2.x, (int)gap.p2.y)) / 2;
        Vector3 gapDestination = gapCenter + gapSpeed * seekingTime;

        // Set new agent destination
        seekingEnd = Time.time + seekingTime;
        agent.SetDestination(gapDestination);
    }

    private float GetGapArea(Gap gap)
    {
        Vector3 topLeft = grid.GetWorldPosition((int)gap.p1.x, (int)gap.p1.y);
        Vector3 bottomRight = grid.GetWorldPosition((int)gap.p2.x, (int)gap.p2.y);
        float width = Mathf.Abs(bottomRight.x - topLeft.x);
        float height = Mathf.Abs(topLeft.z - bottomRight.z);
        return width * height;
    }

    private float GetGapSeekerSpeed(float gapArea)
    {
        float smin = 4 * Mathf.Pow(agent.radius, 2);
        return this.agent.speed / (1 + Mathf.Exp(-beta * (gapArea - alpha * smin)));
    }

    private bool ShouldEndSeeking()
    {
        if (seekingEnd < Time.time || agent.remainingDistance <= agent.stoppingDistance)
        {
            return true;
        }
        return false;
    }

    private void EndSeeking()
    {
        agent.SetDestination(destination);
        behaviour = AgentBehaviourType.Default;
    }

    private float GetGapSeekingProbability()
    {
        Vector3 agentToDestination = destination - agent.gameObject.transform.position;
        agentToDestination.y = 0;
        Vector3 startToAgent = agent.gameObject.transform.position - startingPosition;
        startToAgent.y = 0;
        float probability = lambda * agentToDestination.magnitude / startToAgent.magnitude;
        // Debug.Log("Dist to start, end, ratio (end/start): " + startToAgent.magnitude + ", " + agentToDestination.magnitude + ", " + agentToDestination.magnitude / startToAgent.magnitude);
        // Debug.Log("Gap seeking probability: " + Mathf.Clamp(probability, 0, 1));
        return Mathf.Clamp(probability, 0, 1);
    }

     public void DrawGap(Gap gap)
    {
        Vector3 p1 = grid.GetWorldPosition((int)gap.p1.x, (int)gap.p1.y);
        Vector3 p2 = grid.GetWorldPosition((int)gap.p2.x, (int)gap.p2.y);

        Debug.DrawLine(new Vector3(p1.x, 0.1f, p1.z), new Vector3(p2.x, 0.1f, p1.z), Color.green, 2.5f);
        Debug.DrawLine(new Vector3(p2.x, 0.1f, p1.z), new Vector3(p2.x, 0.1f, p2.z), Color.green, 2.5f);
        Debug.DrawLine(new Vector3(p2.x, 0.1f, p2.z), new Vector3(p1.x, 0.1f, p2.z), Color.green, 2.5f);
        Debug.DrawLine(new Vector3(p1.x, 0.1f, p2.z), new Vector3(p1.x, 0.1f, p1.z), Color.green, 2.5f);
    }
}
