using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gap
{
    // Coordinates of the top left point.
    public Vector2 p1;

    // Coordinates of the right bottom point.
    public Vector2 p2;

    public Gap(Vector2 p1, Vector2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }
}
