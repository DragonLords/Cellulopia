using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsTemplate : Action
{
    public override bool PostPerform()
    {
        return true;
    }

    public override bool PrePerform(BossMelee caller,GameObject target=null)
    {
        return true;
    }

    public override bool TargetExistance()
    {
        throw new System.NotImplementedException();
    }
}
