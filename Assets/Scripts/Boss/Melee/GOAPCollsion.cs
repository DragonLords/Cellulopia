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
        foodSaturation=tester.foodSaturation;
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag(tester.foodTag)){
            tester.CollsionFood(other);
        }else if(other.gameObject.CompareTag(tester.enemyTag)){
            tester.CollsionEnemy(other);
        }
    }

    public bool TakeDamage(int value){
        tester.TakeDamage(value);
        return tester.alive;
    }
}
