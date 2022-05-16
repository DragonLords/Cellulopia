using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPCollsion : MonoBehaviour
{
    [SerializeField] GOAPAgent tester;
    internal int foodSaturation;
    internal string tagParent;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        tester=GetComponentInParent<GOAPAgent>();
        foodSaturation=tester.foodSaturation;
        tagParent=transform.root.tag;
    }

    public bool TakeDamage(int value){
        tester.TakeDamage(value);
        return tester.alive;
    }
}
