using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Reprod : Action
{
    BossMelee caller;

    public override bool PostPerform()
    {
        caller.isSocializing=false;
        StartCoroutine(caller.ActionFinished());
        return true;
    }

    public override bool PrePerform(BossMelee caller, GameObject target = null)
    {
        this.caller=caller;
        var potentials=caller.enemiesClose;
        bool found=potentials.Length>0;
        if(found){
            base.target=potentials.First().gameObject;
            target=base.target;
        }
        caller.isSocializing=true;
        return found;
    }

    public override bool TargetExistance()
    {
        return target!=null;
    }
}