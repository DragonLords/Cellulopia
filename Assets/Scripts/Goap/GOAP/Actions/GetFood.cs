using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetFood : Action
{
    GOAPManager caller;
    public override bool PostPerform()
    {
        // Destroy(this.gameObject);
        return true;
    }

    public override bool PrePerform(GOAPManager caller,GameObject target=null)
    {
        caller.isAttacking=false;
        caller.isAttacking=false;
        caller.canSocialize=false;
        this.caller=caller;
        var potentials=Physics.OverlapSphere(caller.transform.position,caller.radiusFoodDetection,caller.foodLayer);
        bool found=potentials.Length>0;
        if(found){
            float dst=float.MaxValue;
            int selected=0;
            for(int i=0;i<potentials.Length;++i){
                float distance=Vector3.Distance(caller.transform.position,potentials[i].transform.position);
                if(distance<dst){
                    selected=i;
                    dst=distance;
                }
            }
            base.target=potentials[selected].gameObject;
            target=base.target;
        }
        //FIXME: make the target beeing the closest one
        Debug.Log(potentials.Length);
        // Debug.LogFormat("found:{0} target:{1}",found,base.target);
        // return true;
        return found;
    }

    public override bool TargetExistance(){
        return base.target!=null;
    }


}
