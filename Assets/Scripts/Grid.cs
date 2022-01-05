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
    public List<PossibleGap> possibleGaps;
   
    public Grid(int width, int height, float cellSize, Vector3 originPosition, bool isVisible = false)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[width, height];
        debugTextArray = new TextMesh[width, height];
        possibleGaps = new List<PossibleGap>();

        if (isVisible)
        {
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 600f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 600f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 600f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 600f);
        }

    }

    public Vector3 GetWorldPosition(int x, int y) {
        return new Vector3(x, 0, y) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
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

    public Gap getSearchArea(Vector3 pos, int searchDist)
    {
        int x, y;
        GetXY(pos, out x, out y);
        return new Gap(
            new Vector2(Mathf.Max(0, x - searchDist), Mathf.Min(height, y + searchDist)),
            new Vector2(Mathf.Min(width, x + searchDist), Mathf.Max(0, y - searchDist))
        );
    }

    public List<Gap> OvertakeGapDetection(Vector3 pos, Vector3 direction, int searchDepth, int searchWidth, int seeds, out Gap searchArea) {
        int x, y, x_dir, y_dir;

        GetXY(pos, out x, out y);
        GetXY(direction, out x_dir, out y_dir);
        Vector2 pos_vec = new Vector2(x, y);
        Vector2 dest_vec = new Vector2(x_dir, y_dir);

        Vector2 direction_vec = dest_vec - pos_vec;
        Vector2 direction_vec_norm = direction_vec;

        float des_vec_angle = Vec2Angle(direction_vec_norm);

        if (des_vec_angle < 135 && des_vec_angle >= 45)
        {
            direction_vec_norm = new Vector2(1, 0);

        }
        else if (des_vec_angle < 225 && des_vec_angle >= 135)
        {
            direction_vec_norm = new Vector2(0, 1);

            
        }
        else if (des_vec_angle < 315 && des_vec_angle >= 225)
        {
            direction_vec_norm = new Vector2(-1, 0);

            
        } 
        else 
        {
            direction_vec_norm = new Vector2(0, -1);

        }
        direction_vec_norm.Normalize();

        Vector2 dir_vec_perp = new Vector2(direction_vec_norm.y, -direction_vec_norm.x);
        dir_vec_perp.Normalize();
        Vector2 pt1 = pos_vec + direction_vec_norm * searchDepth - dir_vec_perp * (searchWidth / 2);
        Vector2 pt2 = pos_vec + dir_vec_perp * (searchWidth / 2);


        int i_start, i_end, j_start, j_end;
        i_start = i_end = j_start = j_end = 0;

        float direction_angle = Vec2Angle(direction_vec);

        if (direction_angle >= 45 && 135 > direction_angle)
        {
            i_start = Mathf.Max(0, x);
            i_end = Mathf.Min(width - 1, x + searchDepth);
            j_start = Mathf.Max(0, y - (searchWidth / 2));
            j_end = Mathf.Min(height - 1, y + (searchWidth / 2));
        } 
        else if (direction_angle >= 135 && 225 > direction_angle)
        {
            i_start = Mathf.Max(0, x - (searchWidth / 2));
            i_end = Mathf.Min(height - 1, x + (searchWidth / 2));
            j_start = Mathf.Min(width - 1, y - searchDepth);
            j_end = Mathf.Max(0, y);
        } 
        else if (direction_angle >= 225 && 315 > direction_angle)
        {
            i_start = Mathf.Min(width - 1, x - searchDepth);
            i_end = Mathf.Max(0, x);
            j_start = Mathf.Max(0, y - (searchWidth / 2));
            j_end = Mathf.Min(height - 1, y + (searchWidth / 2));
        } 
        else 
        {
            i_start = Mathf.Max(0, x - (searchWidth / 2));
            i_end = Mathf.Min(height - 1, x + (searchWidth / 2));
            j_start = Mathf.Max(0, y);
            j_end = Mathf.Min(width - 1, y + searchDepth);
        }

        List<Vector2> explorationArea = new List<Vector2>();
        for (int i = i_start; i <= i_end; i ++)
        {
            for (int j = j_start; j <= j_end; j++)
            {
                if (GetValue(i, j) == 0) explorationArea.Add(new Vector2(i, j));
            }
        }  
        searchArea = new Gap(pt1, pt2);
        return ExpandAndFilterExplorationArea(explorationArea, seeds);
    }

    public List<Gap> GapDetection(Vector3 pos, int searchDist, int seeds)
    {        
        int x, y;
        GetXY(pos, out x, out y);

        List<Vector2> explorationArea = new List<Vector2>();
        for (int i = Mathf.Max(0, x - searchDist); i <= Mathf.Min(width - 1, x + searchDist); i ++)
        {
            for (int j = Mathf.Max(0, y - searchDist); j <= Mathf.Min(height - 1, y + searchDist); j++)
            {
                if (GetValue(i, j) == 0) explorationArea.Add(new Vector2(i, j));
            }
        }

        return ExpandAndFilterExplorationArea(explorationArea, seeds);
    }

    private List<Gap> ExpandAndFilterExplorationArea(List<Vector2> explorationArea, int seeds) {
        List<Vector2> seedPoints = explorationArea.OrderBy(arg => System.Guid.NewGuid()).Take(seeds).ToList();
        List<Gap> detectedGaps = new List<Gap>();
        foreach (Vector2 seed in seedPoints)
        {
            Gap gap = new Gap(seed, seed);

            // Expand gap to the left - X coordinate of P1.
            while (true)
            {
                if (explorationArea.Contains(new Vector2(gap.p1.x - 1, gap.p1.y)))
                    gap.p1.x--;
                else
                    break;
            }

            // Expand gap up - Y coordinate of P1.
            while (true)
            {   
                if (CheckGapExpand(gap.p1.x, seed[0], gap.p1.y + 1, explorationArea, true))
                    gap.p1.y++;
                else
                    break;
            }

            // Expand gap right - X coordinate of P2.
            while (true)
            {
                if (CheckGapExpand(gap.p2.y, gap.p1.y, gap.p2.x + 1, explorationArea, false))
                    gap.p2.x++;
                else
                    break;
            }

            // Expand gap down - Y coordinate of P2.
            while (true)
            {
                if (CheckGapExpand(gap.p1.x, gap.p2.x, gap.p2.y - 1, explorationArea, true))
                    gap.p2.y--;
                else
                    break;
            }

            if (!IncludesGap(detectedGaps, gap))
                detectedGaps.Add(gap);

        }

        return detectedGaps;
    }

    private bool CheckGapExpand(float start, float stop, float len, List<Vector2> explorationArea, bool row)
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

    private bool IncludesGap(List<Gap> gaps, Gap gap)
    {
        foreach (Gap g in gaps)
        {
            if (g.IsEqual(gap))
            {
                return true;
            }
        }
        return false;
    }

    public List<GameObject> DetectLimiters(Gap gap)
    {

        HashSet<GameObject> limiters = new HashSet<GameObject>();

        // if top side in bounds
        if (gap.p1.y + 1 < height)
        {
            // compute start and stop index within bounds & loop
            int start = (int)Mathf.Max(gap.p1.x, 0);
            int stop = (int)Mathf.Min(gap.p2.x, width - 1);
            for (int x = start; x <= stop; x++)
            {
                // detect colliders
                Collider[] hitColliders = GetPositionColliders(x, (int)gap.p1.y + 1);
                // extract game objects and add to set
                foreach (Collider col in hitColliders)
                {
                    limiters.Add(col.gameObject);
                }
            }
        }
        // if right side in bounds
        if (gap.p2.x + 1 < width)
        {
            // compute start and stop index within bounds & loop
            int start = (int)Mathf.Max(gap.p2.y, 0);
            int stop = (int)Mathf.Min(gap.p1.y, height - 1);
            for (int y = start; y <= stop; y++)
            {
                // detect colliders
                Collider[] hitColliders = GetPositionColliders((int)gap.p2.x + 1, y);
                // extract game objects and add to set
                foreach (Collider col in hitColliders)
                {
                    limiters.Add(col.gameObject);
                }
            }
        }
        // if bottom side in bounds
        if (gap.p2.y - 1 >= 0)
        {
            // compute start and stop index within bounds & loop
            int start = (int)Mathf.Max(gap.p1.x, 0);
            int stop = (int)Mathf.Min(gap.p2.x, width - 1);
            for (int x = start; x <= stop; x++)
            {
                // detect colliders
                Collider[] hitColliders = GetPositionColliders(x, (int)gap.p2.y - 1);
                // extract game objects and add to set
                foreach (Collider col in hitColliders)
                {
                    limiters.Add(col.gameObject);
                }
            }
        }
        // if left side in bounds
        if (gap.p1.x - 1 >= 0)
        {
            // compute start and stop index within bounds & loop
            int start = (int)Mathf.Max(gap.p2.y, 0);
            int stop = (int)Mathf.Min(gap.p1.y, height - 1);
            for (int y = start; y <= stop; y++)
            {
                // detect colliders
                Collider[] hitColliders = GetPositionColliders((int)gap.p1.x - 1, y);
                // extract game objects and add to set
                foreach (Collider col in hitColliders)
                {
                    limiters.Add(col.gameObject);
                }
            }
        }
        List<GameObject> limiterList = limiters.Where(obj => obj.name.Contains("Capsule")).ToList();
        return limiterList;
    }

    private float Vec2Angle(Vector2 p_vector2)
     {
         if (p_vector2.x < 0)
         {
             return 360 - (Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg * -1);
         }
         else
         {
             return Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg;
         }
     }
}
