using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetFood : Action
{
    BossMelee caller;
    public override bool PostPerform()
    {
        StartCoroutine(caller.tester.CoolDownSocializing());
        StartCoroutine(caller.ActionFinished());
        // Destroy(this.gameObject);
        return true;
    }

    public override bool PrePerform(BossMelee caller,GameObject target=null)
    {
        caller.isAttacking=false;
        caller.isAttacking=false;
        caller.canSocialize=false;
        this.caller=caller;
        var potentials=Physics.OverlapSphere(caller.transform.position,caller.radiusFoodDetection,caller.foodLayer);
        bool found=potentials.Length>0;
        if(found){
            base.target=potentials.First().gameObject;
            target=base.target;
        }
        // Debug.LogFormat("found:{0} target:{1}",found,base.target);
        // return true;
        return found;
    }

    public override bool TargetExistance(){
        return base.target!=null;
    }


}
