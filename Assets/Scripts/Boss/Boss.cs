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
        [SerializeField] BossSetting setting;
        [SerializeField] int numberMinionAtStart=50;
        [SerializeField] int maxMinionNumber=150;
        public Transform BossCenter;
        [SerializeField] Transform WhereToSpawn;
        [SerializeField] internal float _maxDistanceMinions=5f;
        [SerializeField] float _rangeDetectionPlayer = 15f;
        [SerializeField] internal float _maxDistanceProtection=15f;
        [SerializeField] Transform minionHolder;
        [SerializeField] Slider slider;
        [SerializeField] Transform[] posWalk;
        int act=0;
        [SerializeField] NavMeshAgent _agent;
        [SerializeField] Transform player;
        [SerializeField] Vector3? lastPosPlayer;
        [SerializeField] LayerMask playerLayer;
        [SerializeField] bool searchingForPlayer = false;
        [SerializeField] string projectileKey="Proj";
        [SerializeField] Transform holderProj;
        public bool alive=true;
        float orbitSpeed=25f;
        public Vector3 offset=new(.5f,0,.5f);
        public Vector3 pos;
        public float radius=5f;
        public int minionNumber = 3;
        WaitForSeconds wsDelayAttack;
        WaitForSeconds wsDelayMinion;
        public float delayMinion = 3f;
        string minionKey = "Minion";
        private int _life = 15;
        private int Life { get => _life; set { _life = Mathf.Clamp(value, 0, 3); } }
        ParticleSystem ps;
        Renderer[] rend;
        public List<GameObject> minions=new();
        public List<GameObject> idlingMinions=new();
        public List<GameObject> attackingMinions=new();
        bool reloading=false;
        public List<Vector2Int> waypoints=new();
        // Start is called before the first frame update
        IEnumerator Start()
        {
            _agent.speed=setting.speed;
            Life=setting.Life;
            wsDelayMinion = new(delayMinion);
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
            for (int i = 0; i < numberMinionAtStart; i++)
                SpawnMinion();
            yield return null;
            StartCoroutine(DetectPlayerClose());
            StartCoroutine(SpawnPerma());
        }

        IEnumerator SpawnPerma(){
            do
            {
                yield return new WaitForSeconds(3f);
                if(minions.Count<maxMinionNumber){
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
            canAttack=false;
            float posX = Random.Range(BossCenter.position.x - _maxDistanceMinions, (BossCenter.position.x + _maxDistanceMinions) + 1);
            float posZ = Random.Range(BossCenter.position.z - _maxDistanceMinions, (BossCenter.position.z + _maxDistanceMinions) + 1);
            Vector3 pos=new(posX,WhereToSpawn.position.y,posZ);
            // Debug.Log(pos);
            var minion=Addressables.InstantiateAsync(minionKey,pos,Quaternion.identity).WaitForCompletion();
            minion.GetComponentInChildren<Minion.BossMinion>().SetBoss(this);
            minions.Add(minion);
            idlingMinions.Add(minion);
            minion.transform.SetParent(minionHolder);
            CooldownAttack();
            routineMinions=StartCoroutine(CheckIfMinionsAlive());
        }

        IEnumerator DetectPlayerClose(){
            do
            {
                CleanList();
                var colliders=Physics.OverlapSphere(transform.position,_rangeDetectionPlayer,playerLayer);
                if(colliders.Length>0){
                    Debug.Log("There is a player nearby");
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
            Debug.LogFormat("targte is:{0}",minionGO.name);
            minion.Attack(target);
        }

        public void MinionReturn(GameObject minion){
            idlingMinions.Add(minion);
            attackingMinions.Remove(minion);
        }



        Coroutine routineMinions;
        IEnumerator CheckIfMinionsAlive(){
            do
            {
                minions.RemoveAll(items=>items==null);
                if(minions.Count==0){
                    reloading=true;
                }
                
                yield return null;
            } while (alive&&!reloading);
            StartCoroutine(ReloadingMinions());
        }

        WaitForSeconds wsReload=new(2f);
        IEnumerator ReloadingMinions(){
            //while boss is relaoding it doesnt move and is vunerable to attack 
            _agent.isStopped=true;
            yield return wsReload;
            _agent.isStopped=false;
        }

        int actWaypoint=0;
        int CycleWaypoint(){
            if(actWaypoint==waypoints.Count-1){
                return actWaypoint=0;
            }else{
                return actWaypoint++;
            }
        }

        IEnumerator Move(){
            _agent.isStopped=false;
            CycleWaypoint();
            Vector3 destination=new(waypoints[actWaypoint][0],transform.position[1],waypoints[actWaypoint][1]);
            Debug.LogFormat("Move to:{0},{1},{2}",destination.x,destination.y,destination.z);
            _agent.SetDestination(destination);
            do
            {
                
                yield return null;
            } while (!reloading||_agent.remainingDistance>1f);
        }


        public void TakeDamage(int value)
        {
            Life -= value;
            StartCoroutine(ShowSlider());
            if (Life == 0)
            {
                StartCoroutine(Death());
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
            yield return null;
        }

        IEnumerator CooldownAttack()
        {
            canAttack = false;
            yield return wsDelayMinion;
            canAttack = true;
        }


        IEnumerator TryAttack()
        {
            do
            {
                if (canAttack && Random.Range(0, 2) == 1)
                {
                    SpawnMinion();
                }
                yield return wsDelayAttack;
            } while (alive);
        }

        void LoadAsset(){
            Addressables.LoadAssetAsync<GameObject>(minionKey).WaitForCompletion();
            Addressables.LoadAssetAsync<GameObject>(projectileKey);
        }

        async Task RotateAroundPlayer(){
            await Task.Yield();
            do
            {
                transform.RotateAround(new(player.position.x+offset.x,player.position.y,player.position.z+offset.z),Vector3.up,orbitSpeed*Time.deltaTime);
                await Task.Yield();
            } while (alive);
        }

        async Task NewRot(){
            do
            {
            pos=radius*Vector3.Normalize(transform.position-player.position)+player.position;
            transform.position=pos;
            transform.RotateAround(player.position,Vector3.up,orbitSpeed*Time.deltaTime);
            await Task.Yield();
            } while (alive);
            await Task.Yield();
        }





        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            // if(Keyboard.current.aKey.wasPressedThisFrame)
            //     AttackPlayer().ConfigureAwait(false).GetAwaiter();
            // if(Keyboard.current.mKey.wasPressedThisFrame)
                // ChargeThePlayer().ConfigureAwait(false);
            if(Keyboard.current.f1Key.wasPressedThisFrame)
                alive=!alive;

            if(Keyboard.current.anyKey.isPressed)
            {
                SpawnMinion();
            }
        }
       
        public bool canAttack=true;

        private void OnDrawGizmos()
        {
            Gizmos.color=Color.yellow;
            Gizmos.DrawWireSphere(BossCenter.position,_maxDistanceMinions);

            Gizmos.color=Color.red;
            Gizmos.DrawWireSphere(BossCenter.position,_rangeDetectionPlayer);

            Gizmos.color=Color.cyan;
            Gizmos.DrawWireSphere(BossCenter.position,_maxDistanceProtection);
        }

    }

}