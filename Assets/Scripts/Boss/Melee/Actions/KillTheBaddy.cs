using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KillTheBaddy : Action
{
    GOAPManager caller;
    public override bool PostPerform()
    {
        StartCoroutine(caller.ActionFinished());
        caller.isAttacking=false;
        return true;
    }

    public override bool PrePerform(GOAPManager caller, GameObject target = null)
    {
        this.caller = caller;
        var potentials = caller.enemiesClose;
        // Debug.Log(potentials.Length);
        bool found = potentials.Length > 0;
        if (found)
        {
            if(potentials.First() is not null){
                base.target = potentials.First().gameObject;
                target = base.target;
            }else{
                found=false;
            }
            // Debug.Log("blob");
        }
        // if(found)
        //     Debug.LogFormat("found:{0} target:{1}", found, target.name);
        // else 
        //     Debug.Log("<color=red>no target found</color>");
        caller.isAttacking=found;
        return found;
    }

    Collider[] GetTarget(Transform self, float range, LayerMask targetLayer)
    {
        int selfLayer = gameObject.layer;
        int layerDefault = LayerMask.NameToLayer("Default");
        gameObject.layer = layerDefault;
        var colls = Physics.OverlapSphere(self.position, range, targetLayer);
        gameObject.layer = selfLayer;
        return colls;
    }

    public override bool TargetExistance()
    {
        return base.target != null;
    }
}
