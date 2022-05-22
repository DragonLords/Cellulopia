using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Boss
{
    /// <summary>
    /// classe qui sert a linteraction du boss
    /// </summary>
    public class Boss : MonoBehaviour
    {
        public int DebugLife;
        [SerializeField] Transform GreatestParent;
        [SerializeField] internal BossSetting setting;
        public Transform BossCenter;
        [SerializeField] Transform WhereToSpawn;
        [SerializeField] Transform minionHolder;
        [SerializeField] Slider slider;
        [SerializeField] NavMeshAgent _agent;
        [SerializeField] Transform player;
        [SerializeField] LayerMask playerLayer;
        [SerializeField] bool searchingForPlayer = false;
        [SerializeField] Transform holderProj;
        public bool alive=true;
        float orbitSpeed=25f;
        public Vector3 offset=new(.5f,0,.5f);
        string minionKey = "Minion";
        private int _life = 15;
        private int Life { get => _life; set { _life = Mathf.Clamp(value, 0, setting.MaxLife); } }
        ParticleSystem ps;
        Renderer[] rend;
        public List<GameObject> minions=new();
        public List<GameObject> idlingMinions=new();
        public List<GameObject> attackingMinions=new();
        [SerializeField] int xpGiven=500;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            _agent.speed=setting.speed;
            Life=setting.Life;
            DebugLife=Life;
            setting.DelaySpawnMinion=new(setting.delaySpawnMinion);
            LoadAsset();
            holderProj=new GameObject("holder_porjectile").transform;
            holderProj.SetParent(transform);
            searchingForPlayer=true;
            #if UNITY_EDITOR
            alive=UnityEditor.EditorApplication.isPlaying;
#else
            alive =true;
#endif
            rend=GetComponentsInChildren<Renderer>();
            for (int i = 0; i < setting.MinionAtStart; i++)
                SpawnMinion();
            yield return null;
            StartCoroutine(DetectPlayerClose());
            StartCoroutine(SpawnPerma());
        }

        /// <summary>
        /// sert a faire apparaitre les minions du boss
        /// </summary>
        /// <returns></returns>
        IEnumerator SpawnPerma(){
            do
            {
                yield return setting.DelaySpawnMinion;
                if(minions.Count<setting.MaxMinionCount){
                    SpawnMinion();
                }
            } while (alive);
        }

        /// <summary>
        /// sert a nettoyer les listes des minions des element detruits
        /// </summary>
        void CleanList(){
            minions.RemoveAll(item=>item==null);
            idlingMinions.RemoveAll(item=>item==null);
            attackingMinions.RemoveAll(item=>item==null);
        }

        /// <summary>
        /// sert a faire apparraitre les minions
        /// </summary>
        void SpawnMinion()
        {
            float posX = Random.Range(BossCenter.position.x - setting.rangeMinion, (BossCenter.position.x + setting.rangeMinion) + 1);
            float posZ = Random.Range(BossCenter.position.z - setting.rangeMinion, (BossCenter.position.z + setting.rangeMinion) + 1);
            Vector3 pos=new(posX,WhereToSpawn.position.y,posZ);
            var minion=Addressables.InstantiateAsync(minionKey,pos,Quaternion.identity).WaitForCompletion();
            minion.GetComponentInChildren<Minion.BossMinion>().SetBoss(this);
            minions.Add(minion);
            idlingMinions.Add(minion);
            minion.transform.SetParent(minionHolder);
        }

        /// <summary>
        /// sert a detecter si le joueur ets proches
        /// </summary>
        /// <returns></returns>
        IEnumerator DetectPlayerClose(){
            do
            {
                CleanList();
                var colliders=Physics.OverlapSphere(transform.position,setting.rangeDetectionPlayer,playerLayer);
                if(colliders.Length>0){
                    // Debug.Log("There is a player nearby");
                    SendMinionsToAttack(colliders.First().transform);
                }
                yield return null;
            } while (alive);
        }

        /// <summary>
        /// sert a envoyer les minions a lattaques
        /// </summary>
        /// <param name="target">la cible a attauqer</param>
        void SendMinionsToAttack(Transform target){
            if(idlingMinions.Count==0)
                return;
            
            idlingMinions.RemoveAll(item=>item==null);
            GameObject minionGO=idlingMinions.First();
            Minion.BossMinion minion=minionGO.GetComponentInChildren<Minion.BossMinion>();
            idlingMinions.Remove(minionGO.gameObject);
            attackingMinions.Add(minionGO.gameObject);
            // Debug.LogFormat("targte is:{0}",minionGO.name);
            minion.Attack(target);
        }

        /// <summary>
        /// sert a modifier les listes de quand les minions retourne de lattque
        /// </summary>
        /// <param name="minion"></param>
        public void MinionReturn(GameObject minion){
            idlingMinions.Add(minion);
            attackingMinions.Remove(minion);
        }

        /// <summary>
        /// sert a quand le boss prend des degats
        /// </summary>
        /// <param name="value">les degats subi</param>
        public void TakeDamage(int value)
        {
            Life -= value;
            DebugLife=Life;
            StartCoroutine(ShowSlider());
            if (Life == 0)
            {
                StartCoroutine(Death());
            }else{
                GameManager.Instance.PlaySoundClip(GameManager.Instance.soundStock[SoundType.Hit]);
            }
        }
        WaitForSeconds wsShow=new(3);
        IEnumerator ShowSlider(){
            slider.value=Life;
            slider.gameObject.SetActive(true);
            yield return wsShow;
            slider.gameObject.SetActive(!true);
        }

        /// <summary>
        /// sert a quand le boss meurt
        /// </summary>
        /// <returns></returns>
        IEnumerator Death()
        {
            Material mat = rend.First().material;
            GameManager.Instance.PlaySoundClip(GameManager.Instance.soundStock[SoundType.Killed]);
            GameManager.Instance.PlayerGiveEXP.Invoke(xpGiven);
            if(ps is not null){
                var part=Instantiate(ps,transform);
                if (!part.isPlaying)
                {
                    part.Play();
                }
            }
            do
            {
                foreach(var r in rend)
                {
                    Color color = r.material.color;
                    r.material.color = new(color.r, color.g, color.b, color.a - .1f);
                }
                yield return new WaitForSeconds(.2f);
            } while (mat.color.a>.1f);
            foreach(var minion in minions){
                Destroy(minion);
            }
            CleanList();
            LoaderScene.Instance.SetSceneToLoad(AddressablePath.End);
            Destroy(GreatestParent.gameObject);
            yield return null;
        }


        void LoadAsset(){
            Addressables.LoadAssetAsync<GameObject>(minionKey).WaitForCompletion();
        }
       
        private void OnDrawGizmos()
        {
            Gizmos.color=Color.yellow;
            Gizmos.DrawWireSphere(BossCenter.position,setting.rangeMinion);

            Gizmos.color=Color.red;
            Gizmos.DrawWireSphere(BossCenter.position,setting.rangeDetectionPlayer);

            Gizmos.color=Color.cyan;
            Gizmos.DrawWireSphere(BossCenter.position,setting.rangeMinionAttack);
        }

    }

}