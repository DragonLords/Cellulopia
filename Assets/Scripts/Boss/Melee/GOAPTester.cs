using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GOAPTester : BossMelee
{
    [TagSelector,SerializeField] string foodTag;
    int hunger;
    // Start is called before the first frame update
    void Start()
    {
        base.OnStart();
        SubGoal s1=new("isWaiting",1,true);
        goals.Add(s1,3);
        hunger=base.Hunger;
    }

    private void OnCollisionEnter(Collision other){
        if(other.gameObject.CompareTag(foodTag)){
            base.GiveFood(other.gameObject.GetComponent<Food>().FoodSaturation);
            Destroy(other.gameObject);
        }
    }
}
