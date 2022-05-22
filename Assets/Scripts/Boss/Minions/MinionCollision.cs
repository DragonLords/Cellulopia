using UnityEngine;

namespace Boss.Minion
{
    /// <summary>
    /// classe qui sert a detecter des collision fait au sbire
    /// </summary>
    public class MinionCollision : MonoBehaviour
    {
        BossMinion minion;
        // Use this for initialization
        void Start()
        {
            minion = GetComponentInParent<BossMinion>();
        }

        public void TakeDamage(int value)
        {
            minion.TakeDamage(value);
        }
        private void OnCollisionEnter(Collision collision)
        {

        }
    }
}