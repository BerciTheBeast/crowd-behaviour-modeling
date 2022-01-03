using System.Collections;
using System.Collections.Generic;

public class PossibleGap
{
    public int seekerId;
    public Gap gap;

    public PossibleGap(int seekerId, Gap gap)
    {
        this.seekerId = seekerId;
        this.gap = gap;
    }
}
