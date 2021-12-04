using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private int[,] gridArray;
    private TextMesh[,] debugTextArray;

    public Grid(int width, int height, float cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[width, height];
        debugTextArray = new TextMesh[width, height];

    }

    private Vector3 GetWorldPosition(int x, int y) {
        return new Vector3(x, 0, y) * cellSize + originPosition;
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }

    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    public int GetValue(int x, int y)
    {

        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        } else
        {
            return 0;
        }
    }

    public int GetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }

    public void UpdateOccupancy() {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Collider[] hitColliders = GetPositionColliders(x, y);

                if (hitColliders.GetLength(0) > 0) {
                    SetValue(x, y, 1);
                } else
                {
                    SetValue(x, y, 0);
                }
            }
        }
    }

    public Collider[] GetPositionColliders(int x, int y)
    {
        Vector3 pos = GetWorldPosition(x, y) + new Vector3(cellSize / 2, 0, cellSize / 2);
        pos.y = 0.1f;
        Collider[] hitColliders = Physics.OverlapBox(pos, new Vector3(cellSize / 2, 0, cellSize / 2), Quaternion.identity);
        return hitColliders;
    }

    public List<Gap> DetectGaps(Vector3 pos, int searchDist, int seeds)
    {        
        int x, y;
        GetXY(pos, out x, out y);

        List<Vector2> explorationArea = new List<Vector2>();
        string state = "";
        for (int i = Mathf.Max(0, x - searchDist); i < Mathf.Min(width, x + searchDist); i ++)
        {
            for (int j = Mathf.Max(0, y - searchDist); j < Mathf.Min(height, y + searchDist); j ++)
            {
                state += GetValue(i, j).ToString();
                if (GetValue(i, j) == 0) explorationArea.Add(new Vector2(i, j));
            }
            Debug.Log(state);
            state = "";
        }
        Debug.Log("-------------------");

        List<Vector2> seedPoints = explorationArea.OrderBy(arg => System.Guid.NewGuid()).Take(seeds).ToList();
        List<Gap> detectedGaps = new List<Gap>();
        int seedCount = 0;
        Debug.Log("Seedpoints: " + seedPoints.Count);
        foreach (Vector2 seed in seedPoints)
        {
            Gap gap = new Gap(seed, seed);

            // Expand gap to the left - X coordinate of P1.
            while (true)
            {
                if (explorationArea.Contains(new Vector2(gap.p1[0] - 1, gap.p1[1])))
                    gap.p1[0]--;
                else
                    break;
            }

            // Expand gap up - Y coordinate of P1.
            while (true)
            {   
                if (CheckExpand(gap.p1[0], seed[0], gap.p1[1] + 1, explorationArea, true))
                    gap.p1[1]++;
                else
                    break;
            }

            // Expand gap right - X coordinate of P2.
            while (true)
            {
                if (CheckExpand(gap.p2[1], gap.p1[1], gap.p2[0] + 1, explorationArea, false))
                    gap.p2[0]++;
                else
                    break;
            }

            // Expand gap down - Y coordinate of P2.
            while (true)
            {
                if (CheckExpand(gap.p1[0], gap.p2[0], gap.p2[1] - 1, explorationArea, true))
                    gap.p2[1]--;
                else
                    break;
            }

            detectedGaps.Add(gap);
        }
        Debug.Log("gaps: " + detectedGaps.Count);

        return detectedGaps;
    }

    public bool CheckExpand(float start, float stop, float len, List<Vector2> explorationArea, bool row)
    {
        for (float i = start; i <= stop; i++)
        {
            if (row) {
                if (!explorationArea.Contains(new Vector2(i, len)))
                {
                    return false;
                }
            }
            else {
                if (!explorationArea.Contains(new Vector2(len, i)))
                {
                    return false;
                }
            }
        }
        return true;
    }
    /*
    public List<GameObject> detectLimiters(Gap gap)
    {

        HashSet<GameObject> limiters = new HashSet<GameObject>();

        // if top side in bounds
        if (gap.p1.y + 1 < height)
        {
            // compute start and stop index within bounds & loop
            int start = (int)Mathf.Max(gap.p1.x, 0);
            int stop = (int)Mathf.MIN(gap.p2.x, width - 1);
            for (int x = start; x <= stop; x++)
            {
                // detect colliders
                Collider[] hitColliders = GetPositionColliders(x, gap.p1.y + 1);
                // extract game objects and add to set
                foreach (Collider col in hitColliders)
                {
                    limiters.Add(col.GameObject)
                }
            }

        }
        // if right side in bounds
        if (gap.p2.x + 1 < width)
        {

        }
        // if bottom side in bounds
        if (gap.p2.y - 1 >= 0)
        {

        }
        // if left side in bounds
        if (gap.p1.x - 1 >= 0)
        {

        }

    }
    */
}
