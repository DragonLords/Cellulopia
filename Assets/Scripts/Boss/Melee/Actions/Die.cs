using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : Action
{
    public override bool PostPerform()
    {
        // Destroy(gameObject);
        Debug.Log("i want to die");
        return true;
    }

    public override bool PrePerform(BossMelee caller, GameObject target = null)
    {
        return true;
    }

    public override bool TargetExistance(GameObject target)
    {
        throw new System.NotImplementedException();
    }
}
