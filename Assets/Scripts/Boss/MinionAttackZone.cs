using UnityEngine;

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

        private void OnCollisionEnter(Collision other)
        {
            Debug.LogFormat("Collision with:{0}",other.gameObject.name);
            if (other.gameObject.CompareTag(minion.playerTag))
            {
                if(other.gameObject.TryGetComponent(out Player.Rework.Player player)){
                    player.TakeDamage(minion.damage);
                }else if(other.gameObject.TryGetComponent(out Player.Rework.PlayerCollision coll)){
                    coll.TakeDamage(minion.damage);
                }
                // other.gameObject.GetComponent<Player.Rework.PlayerCollision>().TakeDamage(minion.damage);
            }
        }
    }
}