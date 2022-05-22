using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KillTheBaddy : Action
{
    GOAPManager caller;
    public override bool PostPerform()
    {
        caller.isAttacking=false;
        return true;
    }
    public List<GameObject> validTargets=new();
    public override bool PrePerform(GOAPManager caller, GameObject target = null)
    {
        validTargets.Clear();
        this.caller = caller;
        //var potentials = caller.enemiesClose;
        var potentials=Physics.OverlapSphere(caller.transform.position,caller.tester.radiusDanger,caller.tester.dangerLayer);
        // Debug.Log(potentials.Length);
        // bool found = potentials.Length > 0;
        foreach(var item in potentials){
            if(item.gameObject.CompareTag(caller.tester.enemyTag)){
                if(caller.tester.container.groupType!=item.GetComponentInParent<GoapContainer>().groupType){
                    validTargets.Add(item.gameObject);
                }
            }else if(item.gameObject.CompareTag(caller.tester.playerTag)){
                //else it is the player so we add it
                validTargets.Add(item.gameObject);
            }
        }
        bool found=validTargets.Count>0;
        if(found){
            base.target=validTargets.First().gameObject;
        }
        caller.isAttacking=found;
        return found;
    }

    public override bool TargetExistance()
    {
        return base.target != null;
    }
}
