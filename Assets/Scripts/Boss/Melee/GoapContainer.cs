using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapContainer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var colliders=GetComponentsInChildren<Collider>();
        foreach(var collider in colliders){
            foreach(var c in colliders){
                Physics.IgnoreCollision(collider,c,true);
            }
        }
    }
}
