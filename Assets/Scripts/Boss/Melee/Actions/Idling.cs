using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Idling : Action
{
    BossMelee caller;
    public override bool PostPerform()
    {
        return true;
    }
    Vector3 point;
    Vector3 direction;
    public override bool PrePerform(BossMelee caller, GameObject target = null)
    {
        this.caller=caller;
        int rnd=Random.Range(0,GameManager.Instance.emptyTiles.Count);
        point=new(GameManager.Instance.emptyTiles[rnd].x,1f,GameManager.Instance.emptyTiles[rnd].y);
        direction=(Vector3.up*15)-point;
        RaycastHit hit;
        bool hitted=Physics.Raycast(Vector3.up*15,direction,out hit);
        if(hitted){
            Debug.Log(hit.collider.gameObject.name);
            var empty=Addressables.InstantiateAsync("Empty",point,Quaternion.identity);
            caller.currentAction.target=empty.WaitForCompletion().gameObject;
        }
        return hitted;
    }

    public override bool TargetExistance()
    {
        return true;
    }

    private void OnDrawGizmos()
    {
        if(point!=Vector3.zero&&direction!=Vector3.zero){
            Gizmos.color=Color.green;
            Gizmos.DrawRay(point,direction);
        }
    }
}
