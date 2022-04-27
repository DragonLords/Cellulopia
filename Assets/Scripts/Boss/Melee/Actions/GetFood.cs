using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetFood : Action
{
    public override bool PostPerform()
    {
        return true;
    }

    public override bool PrePerform(BossMelee caller,GameObject target=null)
    {
        return Physics.CheckSphere(caller.transform.position,caller.radiusFoodDetection,caller.foodLayer);
        // return true;
    }

    public override bool TargetExistance(GameObject target){
        return target!=null;
    }


}
