using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Boss
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class Boss : MonoBehaviour
    {
        [SerializeField] Transform[] posWalk;
        int act=0;
        NavMeshAgent _agent;
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
        // Start is called before the first frame update
        void Start()
        {
            LoadAsset();
            _agent = GetComponent<NavMeshAgent>();
            holderProj=new GameObject("holder_porjcetile").transform;
            holderProj.SetParent(transform);
            searchingForPlayer=true;
            // FindPlayer().ConfigureAwait(false);
            #if UNITY_EDITOR
            alive=UnityEditor.EditorApplication.isPlaying;
            #endif
            // Move().ConfigureAwait(false);
            // NewRot().ConfigureAwait(false);
            // DebugLine().ConfigureAwait(false);
            alive=true;
            ChargeThePlayer().ConfigureAwait(false).GetAwaiter();
        }

        void LoadAsset(){
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
        
        /// <summary>
        /// will be use to find the player
        /// </summary>
        /// <returns></returns>
        async Task Move()
        {
            do
            {
                _agent.SetDestination(posWalk[act].position);
                if(_agent.remainingDistance<1f)
                {
                    ++act;
                    await Task.Yield();
                }
                await Task.Yield();
            } while (alive&&UnityEditor.EditorApplication.isPlaying);
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
            // Gizmos.color=Color.green;
            // Gizmos.DrawRay(transform.position,(player.position-transform.position).normalized*500);
        }

    }

}