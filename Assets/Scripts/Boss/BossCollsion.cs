using UnityEngine;

namespace Boss
{

    public class BossCollsion : MonoBehaviour
    {
        public Boss boss;
        // Start is called before the first frame update
        void Awake()
        {
            boss = GetComponentInParent<Boss>();
        }

        public void TakeDamage(int value)
        {
            boss.TakeDamage(value);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log(collision.gameObject.name);
        }
    }

}