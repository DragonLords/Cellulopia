using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapDanger : MonoBehaviour
{
    GOAPAgent tester;
    // Start is called before the first frame update
    void Start()
    {
        tester=GetComponentInParent<GOAPAgent>();
    }

    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionStay(Collision other)
    {
        if(other.gameObject==null)
            return;
        if(other.gameObject.CompareTag(tester.enemyTag)){
            if(other.gameObject.TryGetComponent(out GOAPCollsion coll)){
                coll.TakeDamage(tester.damage);
                // other.gameObject.GetComponent<GOAPCollsion>().TakeDamage(tester.damage);
            }
        }
    }
}
