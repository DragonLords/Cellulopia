using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPTester : BossMelee
{
    [TagSelector,SerializeField] string foodTag;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        SubGoal s1=new("isWaiting",1,true);
        goals.Add(s1,3);
    }

    private void OnCollisionEnter(Collision other){
        
    }
}
