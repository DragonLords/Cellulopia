using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : Action
{
    public override bool PostPerform()
    {
        // Destroy(gameObject);
        // Debug.Log("i want to die");
        return true;
    }

    public override bool PrePerform(GOAPManager caller, GameObject target = null)
    {
        return true;
    }

    public override bool TargetExistance()
    {
        throw new System.NotImplementedException();
    }
}