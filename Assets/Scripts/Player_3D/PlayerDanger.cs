using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
        [SerializeField,TagSelector] string minionTag;
        [SerializeField,TagSelector] string bossTag;
        [SerializeField] Vector3 detectionSize=new(6,1,2);
        string PSEatKey="PS_Eat";
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
            
        }

        /// <summary>
        /// OnCollisionStay is called once per frame for every collider/rigidbody
        /// that is touching rigidbody/collider.
        /// </summary>
        /// <param name="other">The Collision data associated with this collision.</param>
        void OnCollisionStay(Collision other)
        {
            Debug.LogFormat("<color=red>Collsion with:{0}</color>",other.gameObject.name);
            if(other.gameObject.CompareTag(foodTag)){
                //do srtuff about food here
                Addressables.InstantiateAsync(PSEatKey,transform.position,Quaternion.identity).Completed+=DestroyAfter;
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
            }else if(other.gameObject.CompareTag(minionTag)){
                if(other.gameObject.TryGetComponent(out Boss.Minion.MinionCollision coll)){
                    coll.TakeDamage(player.DamageValue);
                }else if(other.gameObject.TryGetComponent(out Boss.Minion.MinionAttackZone danger)){
                    danger.TakeDamage(player.DamageValue);
                }
                // other.gameObject.GetComponent<Boss.Minion.MinionCollision>().TakeDamage(player.DamageValue);
            }else if(other.gameObject.CompareTag(bossTag)){
                if(other.gameObject.TryGetComponent(out Boss.Boss boss)){
                    boss.TakeDamage(player.DamageValue);
                }else if(other.gameObject.TryGetComponent(out Boss.BossCollsion coll)){
                    coll.TakeDamage(player.DamageValue);
                }
            }
        }

        private async void DestroyAfter(AsyncOperationHandle<GameObject> obj)
        {
            var ps=obj.Result.GetComponent<ParticleSystem>();
            await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(ps.main.duration*1000));
            Destroy(obj.Result);
            // await System.Threading.Tasks.Task.Delay()
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