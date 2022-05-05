using System.Collections;
using UnityEngine;

namespace Boss.Minion
{
    public class MinionCollision : MonoBehaviour
    {
        BossMinion minion;
        // Use this for initialization
        void Start()
        {
            minion=GetComponentInParent<BossMinion>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            
        }
    }
}