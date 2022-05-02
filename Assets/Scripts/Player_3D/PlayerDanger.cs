using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Player.Rework.Danger
{
    public class PlayerDanger : MonoBehaviour
    {
        Player player;
        [SerializeField, TagSelector] string foodTag;
        [SerializeField, TagSelector] string enemyTag;
        [SerializeField] Vector3 detectionSize=new(6,1,2);
        Collider Collider;
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            Collider = GetComponent<Collider>();
            player=GetComponentInParent<Player>();
            SetIgnore();
        }

        internal void SetCollider()
        {
            GetComponent<BoxCollider>().size=detectionSize*2;
        }

        private void OnCollisionEnter(Collision other)
        {
            if(other.gameObject.CompareTag(foodTag)){
                //do srtuff about food here
                player.PlayerGiveFood.Invoke(other.gameObject.GetComponent<Food>().FoodSaturation);
                //Debug.Log("got food");
                player.QuestItem(other.gameObject);
                Destroy(other.gameObject);
            }else if(other.gameObject.CompareTag(enemyTag)){
                //do stuff about enemy here
                Debug.Log("Enemy");
                var enemy=other.gameObject.GetComponent<GOAPCollsion>();
                if(!enemy.TakeDamage(player.DamageValue)){
                    player.QuestItem(other.gameObject);
                    player.PlayerGiveFood.Invoke(enemy.foodSaturation);
                }
            }else if(other.gameObject.CompareTag(player.portalTag)){
                other.gameObject.GetComponent<Portal>().TriggerBossFight();
            }

        }

        void SetIgnore()
        {
            var objs = GameObject.FindGameObjectsWithTag("Player");
            foreach(var obj in objs)
            {
                if (obj.TryGetComponent(out Collider collider)) {
                    Physics.IgnoreCollision(collider,Collider);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color=Color.magenta;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(PlayerDanger))]
    class PlayerDangerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PlayerDanger danger= (PlayerDanger)target;

            base.OnInspectorGUI();
        }
    }
#endif
}