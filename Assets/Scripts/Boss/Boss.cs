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
        [SerializeField] Slider slider;
        [SerializeField] Transform[] posWalk;
        int act=0;
        [SerializeField] NavMeshAgent _agent;
        [SerializeField] Transform player;
        [SerializeField] Vector3? lastPosPlayer;
        [SerializeField] LayerMask playerLayer;
        [SerializeField] float rangeDetectionPlayer = 30f;
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
        bool reloading=false;
        public List<Vector2Int> waypoints=new();
        // Start is called before the first frame update
        IEnumerator Start()
        {
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
            StartCoroutine(ShowSlider());
            for(int i=0;i<4;++i)
                waypoints.Add(GameManager.Instance.emptyTiles[Random.Range(0,GameManager.Instance.emptyTiles.Count)]);
            yield return new WaitForSeconds(5f);
            Debug.Log("Bob");
            StartCoroutine(TryAttack());
            slider.maxValue=Life;
            slider.value=Life;
            StartCoroutine(Move());
        }

        void SpawnMinion()
        {
            canAttack=false;
            for(int i = 0; i < minionNumber; i++)
            {
                float posX = Random.Range(transform.position.z - radius, (transform.position.z + radius) + 1);
                float posZ = Random.Range(transform.position.z - radius, (transform.position.z + radius) + 1);
                var minion=Addressables.InstantiateAsync(minionKey,new(posX,transform.position.y,posZ),Quaternion.identity).WaitForCompletion();
                minions.Add(minion);
            }
            CooldownAttack();
            routineMinions=StartCoroutine(CheckIfMinionsAlive());
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

        public Transform DebugSphere;
        async Task FindPlayer()
        {
            do
            {
                await Task.Yield();
                if (lastPosPlayer == null)
                {
                    var colliders = Physics.OverlapSphere(transform.position, rangeDetectionPlayer, playerLayer);
                    if (colliders.Length != 0)
                    {
                        await Task.Yield();
                        player = colliders.First().transform;
                        lastPosPlayer=player.position;
                    }
                    await Task.Yield();
                }
                await Task.Yield();
            } while (searchingForPlayer);
        }

        async Task AttackPlayer()
        {
            if(player is not null){
                var proj=Addressables.InstantiateAsync(projectileKey,player.position,Quaternion.identity,holderProj);
                DestroyProjAfterTime(proj.Result).ConfigureAwait(false).GetAwaiter();
            }
            await Task.Yield();
        }

        async Task DestroyProjAfterTime(GameObject go){
            await Task.Delay(1500);
            #if UNITY_EDITOR
            if(UnityEditor.EditorApplication.isPlaying)
                Destroy(go);
            #else
                Destroy(go);
            #endif
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
        }
        public Transform head;
        async Task ChargeThePlayer(){
            await Task.Yield();
            RaycastHit hit;
            do
            {
                await Task.Yield();
                Ray ray=new(transform.position,(player.position-transform.position).normalized*10f);
                Vector3 dir=(player.position-transform.position).normalized*float.MaxValue;
                if(Physics.Raycast(ray,out hit,float.MaxValue,playerLayer)){
                    /// <summary>
                    /// FIXME:
                    /// there is a new problem
                    /// if its in negative the result is so innacurate that the boss will just 
                    /// miss the attack like a dump
                    /// </summary>
                    Vector3 targetAttack=player.transform.position;
                    Vector3 final=(targetAttack-transform.position).normalized*Vector3.Distance(transform.position,player.position);
                    var sqr=Vector3.SqrMagnitude(targetAttack);
                    Debug.LogFormat("init:{0} target:{1} final:{2} sqr:{3}",transform.position,targetAttack,final,sqr);
                    DebugSphere.position=final;
                    // if(canAttack)
                    //     Charge(hit.point).ConfigureAwait(false).GetAwaiter();
                }
                await Task.Delay(100);
            } while (true);
            // float regularSpeed=_agent.speed;
            // float newSpeed=regularSpeed*3;
            //this one wont work too dump 
            // Ray ray=new(transform.position,player.position);
            // Debug.DrawRay(transform.parent.position,player.position,Color.cyan,1f);
            // if(Physics.Raycast(ray,out hit,Mathf.Infinity,playerLayer)){
            //     Debug.Log(hit.point);
            // }else{
            //     // Debug.Log("none");
            // }
            // await Task.Delay(1000);
                
            // if(Physics.Raycast(transform.position,player.position,Vector3.Distance(transform.position,player.position)+5,)){}
            // Vector3 target=player.position;
            // Vector3 targetFinal=target*2;
            // _agent.speed=newSpeed;
            // _agent.SetDestination(targetFinal);
            // do
            // {
            //     await Task.Yield();
            // } while (_agent.remainingDistance>1f);
            // _agent.speed=regularSpeed;
        }
        public bool canAttack=true;
        async Task Charge(Vector3 target){
            await Task.Yield();
            _agent.SetDestination(target);
            do
            {
                await Task.Yield();       
            } while (_agent.remainingDistance>1f);
            await Task.Yield();
            canAttack=false;
            await Task.Delay(1000);
            canAttack=true;
        }

        async Task DebugLine(){
            var line=Addressables.InstantiateAsync("FoodPath",Vector3.zero,Quaternion.identity).WaitForCompletion();
            var lr=line.GetComponent<LineRenderer>();
            lr.startColor=Color.green;
            lr.endColor=Color.red;
            lr.widthMultiplier=1f;
            await Task.Yield();
            do
            {
                await Task.Yield();
                Vector3[] posLine={transform.position,player.transform.position};
                lr.SetPositions(posLine);
            } while (alive);
        }

        private void OnDrawGizmos()
        {
            if(waypoints.Count>0){
                Gizmos.color=Color.cyan;
                foreach(var waypoint in waypoints){
                    Gizmos.DrawSphere(new(waypoint.x,1,waypoint.y),3f);
                }
            }
            Gizmos.color=Color.magenta;
            Gizmos.DrawRay(transform.position,transform.position+transform.forward*5000);
            // Gizmos.color=Color.green;
            // Gizmos.DrawRay(transform.position,(player.position-transform.position).normalized*500);
        }

    }

}