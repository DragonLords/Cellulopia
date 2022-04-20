using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

namespace Enemy
{
    public class Enemy : MonoBehaviour
    {
        public bool walking=false;
        [SerializeField,TagSelector] string foodTag;
        public LayerMask foodLayer;
        public float radius=15f;
        internal bool alive=true;
        internal Vector3[] bounds={new(-5f,0,-15f),new(35,0,25)};
        Renderer rend;
        public NavMeshAgent agent;
        public Material[] matAction;
        public actionState _actionState;
        public float foodToGive = 15f;
        public int xpToGive = 15;
        private int MaxLife = 10;
        private int _life = 1;
        public int Life { get => _life; set { _life = Mathf.Clamp(value, 0, MaxLife); } }
        Player.PlayerDanger player;

        int MaxHunger=50;
        [SerializeField,Range(0,50)] int _hunger=15;
        public int Hunger{get=>_hunger;set{_hunger=Mathf.Clamp(value,0,MaxHunger);}}
        public int tresholdHungry=10;
        public bool isHungry=false;

        #region Social
        internal bool canSocialize=false;
        internal bool isSocalizing=false;
        internal float radiusDetectionSocial=15f;
        internal LayerMask othersLayer;
        internal int socialPoint=15;
        [SerializeField,Tooltip("Time between each socialisation in seconds and must be integer")] 
        private int _cooldownSocial=5;
        internal int cooldownSocial{get=>_cooldownSocial;set{_cooldownSocial=value*1000;}}
        internal Dictionary<GameObject,Enemy> allies=new();
        internal Dictionary<GameObject,Enemy> neutrals=new();
        internal Dictionary<GameObject,Enemy> enemies=new();
        #endregion



        #region State
        State.EnemyStateBase state;
        State.EnemyStateAttack attackState=new();
        State.EnemyStateHungry hungryState=new();
        State.EnemyStateFlee fleeState=new();
        State.EnemyStateSocialize socializeState=new();
        State.EnemyStateWalking walkingState=new();
        #endregion
        int stateAct=0;

        Dictionary<actionState,State.EnemyStateBase> _actions=new();
        void FillDict(){
            _actions.Add(actionState.Walking,walkingState);
            _actions.Add(actionState.Flee,fleeState);
            _actions.Add(actionState.Attack,attackState);
            _actions.Add(actionState.Hungry,hungryState);
            _actions.Add(actionState.Socialize,socializeState);
        }

        async Task InitEnemies(){
            var es=FindObjectsOfType<Enemy>().ToList();
            Debug.Log(es.Count);
            es.Remove(this);
            Debug.Log(es.Count);
            await Task.Yield();
        }

        public async Task CallbackSpawn(TypeForOthers typeOther){
            switch (typeOther)
            {
                case TypeForOthers.Baddy:break;
                default:break;
            }
            await Task.Yield();
        }

        async Task InitAllies(){
            await Task.Yield();
        }

        private void Awake() {
            FillDict();
            othersLayer=gameObject.layer;
        }

        // Start is called before the first frame update
        async void Start()
        {
            rend=GetComponent<Renderer>();
            ChangeState(socializeState);
            // Thread thread=new()
            await InitEnemies();
            await InitAllies();
            #region Fixes
            //FIXME: Unity crashed 2 time due to this simple line of code
            //Probly an infinite loop but where...
            // LookAction().ConfigureAwait(false);
            //okay now apprently event that is an infinite loop
            /// im loosing my mind over it 
            /// it was fine right before
            /// LoopHunger().ConfigureAwait(false);
            #endregion
        }



        // Update is called once per frame
        void Update()
        {
            if(Keyboard.current.f1Key.wasPressedThisFrame)
                {alive=false;UnityEditor.EditorApplication.isPlaying=alive;}
        }

        public void TakeDamage(int damageValue, Player.PlayerDanger player)
        {
            this.player = player;
            Life -= damageValue;
        }

        public void ChangeState(State.EnemyStateBase state){
            this.state=state;
            Debug.LogFormat("State changed to {0}",state);
            state.InitState(this);
            rend.material=matAction[(int)_actionState];
        }

        public void CycleState(){
            ++stateAct;
            if(stateAct>4){
                ChangeState(walkingState);
                stateAct=0;
            }else{
                switch(stateAct){
                    case 1:ChangeState(attackState); break;
                    case 2:ChangeState(hungryState); break;
                    case 3:ChangeState(fleeState); break;
                    case 4:ChangeState(socializeState); break;
                }
            }
            _actionState=(actionState)stateAct;
        }

        async Task LookAction()
        {
            do
            {
                if(isHungry){
                    await Task.Yield();
                    // Debug.Log(state); 
                    Debug.Log("need to kill");
                    // state.EndState(this);
                    // ChangeState(hungryState);
                }
                await Task.Yield();
            } while (alive);
        }

        async Task LoopHunger(){
            do
            {
                Hunger--;
                if(Hunger<=tresholdHungry)
                {
                    await Task.Yield();
                    isHungry=true;
                    HungryManager();
                }
                await Task.Delay(50);
            } while (alive);
        }

        void HungryManager(){
            isHungry=true;
            if(this.walking)
                this.walking=false;
            ChangeState(_actions[actionState.Hungry]);
        }

        async Task GiveFood(int value){
            Hunger+=value;
            if(Hunger>tresholdHungry){
                isHungry=false;
                await Task.Yield();
            }
            await Task.Yield();
        }

        internal async Task StatePending()
        {
            ChangeState(walkingState);
            await Task.Yield();
        }

        private void OnCollisionEnter(Collision other){
            if(other.gameObject.CompareTag(foodTag)){
                Destroy(other.gameObject);
                GiveFood(10).ConfigureAwait(false).GetAwaiter();
            }
        }
        internal Vector3 target=Vector3.zero;
        private void OnDrawGizmos()
        {
            Gizmos.color=hungryState.foodFound?Color.magenta:Color.green;
            Gizmos.DrawWireSphere(transform.position,radius);
            if(target!=Vector3.zero){
                Gizmos.color=Color.red;
                Gizmos.DrawRay(transform.position,target);
            }
        }
    }

    public enum actionState{Walking,Attack,Hungry,Flee,Socialize}
    public enum TypeForOthers{Baddy,Neutral,Good}
}