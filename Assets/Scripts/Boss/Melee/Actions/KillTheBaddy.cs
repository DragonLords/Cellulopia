using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTheBaddy : Action
{
    BossMelee caller;
    public override bool PostPerform()
    {
        StartCoroutine(caller.ActionFinished());
        return true;
    }

    public override bool PrePerform(BossMelee caller, GameObject target = null)
    {
        this.caller=caller;
        var potentials=GetTarget(caller.transform,caller.radiusFoodDetection,caller.enemyLayer);
        Debug.Log(potentials.Length);
        bool found=potentials.Length>0;
        if(found){
            base.target=target;
            target=base.target;
            Debug.Log("blob");
        }
        Debug.LogFormat("found:{0} target:{1}",found,base.target);
        return found; 
    }

        Collider[] GetTarget(Transform self,float range,LayerMask targetLayer){
        int selfLayer=gameObject.layer;
        int layerDefault=LayerMask.NameToLayer("Default");
        gameObject.layer=layerDefault;
        var colls=Physics.OverlapSphere(self.position,range,targetLayer);
        gameObject.layer=selfLayer;
        return colls;
    }

    public override bool TargetExistance()
    {
        return base.target!=null;
    }
}
