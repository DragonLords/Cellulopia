using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTheBaddy : Action
{
    public override bool PostPerform()
    {
        throw new System.NotImplementedException();
    }

    public override bool PrePerform(BossMelee caller, GameObject target = null)
    {
        return caller.DetectRangeAction(caller.radiusFoodDetection,caller.layerEnemey);
    }

    public override bool TargetExistance(GameObject target)
    {
        throw new System.NotImplementedException();
    }
}
