using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPCollsion : MonoBehaviour
{
    [SerializeField] GOAPTester tester;
    internal int foodSaturation;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        tester=GetComponentInParent<GOAPTester>();
        foodSaturation=tester.foodSaturation;
    }

    public bool TakeDamage(int value){
        tester.TakeDamage(value);
        return tester.alive;
    }
}
