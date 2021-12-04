using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gap
{
    // Coordinates of the top left point.
    public Vector2 p1;

    // Coordinates of the right bottom point.
    public Vector2 p2;

    public Vector3 agentToCenter;

    public Gap(Vector2 p1, Vector2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    public bool IsEqual(Gap gap) {
        return gap.p1 == this.p1 && gap.p2 == this.p2;
    }

    public Vector2 GetCenter() {
        return new Vector2((p1[0] + p2[0]) / 2, (p1[1] + p2[1]) / 2 );
    }
}
