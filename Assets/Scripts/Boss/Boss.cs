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

        // Start is called before the first frame update
        IEnumerator Start()
        {
            _agent.speed=setting.speed;
            Life=setting.Life;
            DebugLife=Life;
            setting.DelaySpawnMinion=new(setting.delaySpawnMinion);
            LoadAsset();
            // _agent = GetComponent<NavMeshAgent>();
            holderProj=new GameObject("holder_porjectile").transform;
            holderProj.SetParent(transform);
            searchingForPlayer=true;
            // FindPlayer().ConfigureAwait(false);
            #if UNITY_EDITOR
            alive=UnityEditor.EditorApplication.isPlaying;
#else
            alive =true;
#endif
            rend=GetComponentsInChildren<Renderer>();
            // Move().ConfigureAwait(false);
            //NewRot().ConfigureAwait(false);
            // DebugLine().ConfigureAwait(false);
            //ChargeThePlayer().ConfigureAwait(false).GetAwaiter();
            //AttackPlayer().ConfigureAwait(false);
            // StartCoroutine(ShowSlider());
            // for(int i=0;i<4;++i)
            //     waypoints.Add(GameManager.Instance.emptyTiles[Random.Range(0,GameManager.Instance.emptyTiles.Count)]);
            // yield return new WaitForSeconds(5f);
            // Debug.Log("Bob");
            // StartCoroutine(TryAttack());
            // slider.maxValue=Life;
            // slider.value=Life;
            // StartCoroutine(Move());
            for (int i = 0; i < setting.MinionAtStart; i++)
                SpawnMinion();
            yield return null;
            StartCoroutine(DetectPlayerClose());
            StartCoroutine(SpawnPerma());
        }

        IEnumerator SpawnPerma(){
            do
            {
                yield return setting.DelaySpawnMinion;
                if(minions.Count<setting.MaxMinionCount){
                    SpawnMinion();
                }
            } while (alive);
        }

        void CleanList(){
            minions.RemoveAll(item=>item==null);
            idlingMinions.RemoveAll(item=>item==null);
            attackingMinions.RemoveAll(item=>item==null);
        }

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

        public void MinionReturn(GameObject minion){
            idlingMinions.Add(minion);
            attackingMinions.Remove(minion);
        }

        public void TakeDamage(int value)
        {
            Life -= value;
            DebugLife=Life;
            StartCoroutine(ShowSlider());
            if (Life == 0)
            {
                StartCoroutine(Death());
                GameManager.Instance.PlaySoundClip(GameManager.Instance.soundStock[SoundType.Killed]);
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

        IEnumerator Death()
        {
            Material mat = rend.First().material;
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
            Destroy(GreatestParent.gameObject);
            yield return null;
        }


        void LoadAsset(){
            Addressables.LoadAssetAsync<GameObject>(minionKey).WaitForCompletion();
        }

        async Task RotateAroundPlayer(){
            await Task.Yield();
            do
            {
                transform.RotateAround(new(player.position.x+offset.x,player.position.y,player.position.z+offset.z),Vector3.up,orbitSpeed*Time.deltaTime);
                await Task.Yield();
            } while (alive);
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