using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Linq;

public class Reprod : Action
{
    GOAPManager caller;

    public override bool PostPerform()
    {
        caller.isSocializing=false;
        return true;
    }

    public override bool PrePerform(GOAPManager caller, GameObject target = null)
    {
        this.caller=caller;
        return caller.tester.Hunger>caller.HungerCostDuplication;
    }

    

    public override bool TargetExistance()
    {
        return target!=null;
    }
}
