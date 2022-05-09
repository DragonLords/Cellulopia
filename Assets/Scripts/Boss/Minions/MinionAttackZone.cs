using UnityEngine;
using System.Collections;

namespace Boss.Minion
{

    public class MinionAttackZone : MonoBehaviour
    {
        public BossMinion minion;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {

        }

        public void TakeDamage(int value){
            minion.TakeDamage(value);
        }

        /// <summary>
        /// OnCollisionStay is called once per frame for every collider/rigidbody
        /// that is touching rigidbody/collider.
        /// </summary>
        /// <param name="other">The Collision data associated with this collision.</param>
        void OnCollisionStay(Collision other)
        {
            Debug.LogFormat("Collision with:{0}",other.gameObject.name);
            if (other.gameObject.CompareTag(minion.playerTag))
            {
                if(other.gameObject.TryGetComponent(out Player.Rework.Player player)){
                    player.TakeDamage(minion.damage);
                }else if(other.gameObject.TryGetComponent(out Player.Rework.PlayerCollision coll)){
                    coll.TakeDamage(minion.damage);
                }
                StartCoroutine(DelayBetweenAttack());
                // other.gameObject.GetComponent<Player.Rework.PlayerCollision>().TakeDamage(minion.damage);
            }
        }

        IEnumerator DelayBetweenAttack(){
            minion.canAttack=false;
            StartCoroutine(minion.TurnAroundPlayer());
            yield return new WaitForSeconds(200000f);
            minion.canAttack=true;
        }        
    }
}