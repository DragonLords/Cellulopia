using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Player.Rework.Danger
{
    public class PlayerDanger : MonoBehaviour
    {
        [SerializeField,TagSelector] string TheInfamousUntagged; 
        [SerializeField,TagSelector] string mapTag;
        [SerializeField,TagSelector] string playerTag;
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
            if(other.gameObject.CompareTag(playerTag)||!player.canAttack||other.gameObject.CompareTag(mapTag)||other.gameObject.CompareTag(TheInfamousUntagged)){
                return;
            }
            else
            {
                StartCoroutine(DelayAttack());
            }
            Debug.LogFormat("<color=red>Collsion with:{0}</color>",other.gameObject.name);
            GameManager refGameManager=GameManager.Instance;
            if(other.gameObject.CompareTag(foodTag)){
                //do srtuff about food here
                var food=other.gameObject.GetComponent<Food>();
                Addressables.InstantiateAsync(PSEatKey,transform.position,Quaternion.identity).Completed+=DestroyAfter;
                player.PlayerGiveFood.Invoke(food.FoodSaturation);
                GameManager.Instance.PlayerGiveEXP.Invoke(food.XpGiven);
                //Debug.Log("got food");
                player.QuestItem(other.gameObject,food.tagParent);
                Destroy(other.gameObject);
                //play sound of eating food
                refGameManager.PlaySoundClip(refGameManager.soundStock[SoundType.Eat]);
            }else if(other.gameObject.CompareTag(enemyTag)){
                //do stuff about enemy here
                Debug.Log("Enemy");
                var enemy=other.gameObject.GetComponent<GOAPCollsion>();
                if(!enemy.TakeDamage(player.DamageValue)){
                    player.QuestItem(other.gameObject,enemy.tagParent);
                    player.PlayerGiveFood.Invoke(enemy.foodSaturation);
                    refGameManager.PlaySoundClip(refGameManager.soundStock[SoundType.Killed]);
                }else{
                    refGameManager.PlaySoundClip(refGameManager.soundStock[SoundType.Hit]);
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
                Debug.LogFormat("<color=orange>Collision with boss at:{0}</color>",other.gameObject.name);
                if(other.gameObject.TryGetComponent(out Boss.Boss boss)){
                    boss.TakeDamage(player.DamageValue);
                }else if(other.gameObject.TryGetComponent(out Boss.BossCollsion coll)){
                    coll.TakeDamage(player.DamageValue);
                }
            }
        }

        IEnumerator DelayAttack(){
            player.PlayAnimEat();
            player.canAttack=false;
            yield return new WaitForSeconds(player.playerStat.DelayAttack);
            player.canAttack=true;
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
}