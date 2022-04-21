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
        [SerializeField] LayerMask noneLayer;
        EnemyFOV fov;
        public bool walking = false;
        [SerializeField, TagSelector] string foodTag;
        [SerializeField, TagSelector] string enemyTag;
        public LayerMask foodLayer;
        public float radius = 15f;
        internal bool alive = true;
        internal Vector3[] bounds = { new(-5f, 0, -15f), new(35, 0, 25) };
        internal Renderer rend;
        internal Collider _collider;
        public NavMeshAgent agent;
        public Material[] matAction;
        public actionState _actionState;
        public float foodToGive = 15f;
        public int xpToGive = 15;
        private int MaxLife = 10;
        private int _life = 1;
        public int Life { get => _life; set { _life = Mathf.Clamp(value, 0, MaxLife); } }
        Player.PlayerDanger player;

        int MaxHunger = 50;
        [SerializeField, Range(0, 50)] int _hunger = 15;
        public int Hunger { get => _hunger; set { _hunger = Mathf.Clamp(value, 0, MaxHunger); } }
        public int tresholdHungry = 10;
        public bool isHungry = false;

        #region Social
        internal bool canSocialize = false;
        internal bool isSocalizing = false;
        internal float radiusDetectionSocial = 15f;
        [SerializeField] internal LayerMask othersLayer;
        internal int socialPoint = 15;
        [SerializeField, Tooltip("Time between each socialisation in seconds and must be integer")]
        private int _cooldownSocial = 5;
        internal int cooldownSocial { get => _cooldownSocial; set { _cooldownSocial = value * 1000; } }
        internal Dictionary<GameObject, Enemy> allies = new();
        internal Dictionary<GameObject, Enemy> neutrals = new();
        internal Dictionary<GameObject, Enemy> enemies = new();
        #endregion

        #region Attack
        internal bool isAttacking = false;
        internal bool readyToDefend = false;
        internal bool inRangeToAttack = false;
        internal float attackRange = 5f;
        #endregion

        #region Flee
        internal bool isInDanger = false;
        internal float radiusDetectionDanger = 10f;
        [SerializeField] internal LayerMask dangerLayers;
        public Vector3 targetFlee;
        internal int multiplierFlee = 3;
        internal float rangeFlee = 40f;
        #endregion


        #region State
        State.EnemyStateBase state;
        State.EnemyStateAttack attackState = new();
        State.EnemyStateHungry hungryState = new();
        State.EnemyStateFlee fleeState = new();
        State.EnemyStateSocialize socializeState = new();
        State.EnemyStateWalking walkingState = new();
        #endregion
        int stateAct = 0;
        [SerializeField] Material[] mats;
        public TypeForOthers typeForOthers;
        internal Dictionary<actionState, State.EnemyStateBase> _actions = new();
        void FillDict()
        {
            _actions.Add(actionState.Walking, walkingState);
            _actions.Add(actionState.Flee, fleeState);
            _actions.Add(actionState.Attack, attackState);
            _actions.Add(actionState.Hungry, hungryState);
            _actions.Add(actionState.Socialize, socializeState);
        }

        async Task InitEnemies()
        {
            var es = FindObjectsOfType<Enemy>().ToList();
            // Debug.Log(es.Count);
            es.Remove(this);
            // Debug.Log(es.Count);
            await Task.Yield();
        }

        public async Task CallbackSpawn(TypeForOthers typeOther)
        {
            if (rend is null)
                rend = GetComponent<Renderer>();
            _collider = GetComponent<Collider>();
            switch (typeOther)
            {
                case TypeForOthers.Baddy:
                    {
                        rend.material = matAction[0];
                    }
                    break;
                case TypeForOthers.Neutral:
                    {
                        rend.material = matAction[1];
                    }
                    break;
                case TypeForOthers.Good:
                    {
                        rend.material = matAction[2];
                    }
                    break;
            }
            await Task.Yield();
        }

        async Task InitAllies()
        {
            await Task.Yield();
        }

        internal void Init(actionState state)
        {
            _actionState = state;
        }

        private void Awake()
        {
            FillDict();
            fov = GetComponent<EnemyFOV>();
        }

        // Start is called before the first frame update
        async void Start()
        {
            if (rend is null)
                rend = GetComponent<Renderer>();
            await Task.Yield();
            RequestNewState();
            #region Fixes
            //FIXME: Unity crashed 2 time due to this simple line of code
            /// Probly an infinite loop but where...
            // LoopAction().ConfigureAwait(false).GetAwaiter();
            /// okay now apprently event that is an infinite loop
            /// im loosing my mind over it 
            /// it was fine right before
            /// LoopHunger().ConfigureAwait(false);
            #endregion
        }



        // Update is called once per frame
        void Update()
        {
            if (Keyboard.current.f1Key.wasPressedThisFrame)
            { alive = false; UnityEditor.EditorApplication.isPlaying = alive; }
        }

        public void TakeDamage(int damageValue, Player.PlayerDanger player)
        {
            this.player = player;
            Life -= damageValue;
        }

        public void ChangeState(actionState state)
        {
            // Debug.LogFormat("State {1} changed to {0}",state,_actionState);
            this.state = _actions[state];
            this.state.InitState(this);
            rend.material = matAction[(int)_actionState];
            _actionState = state;
            name=$"{state}";
        }

        public void CycleState()
        {
            ++stateAct;
            if (stateAct > 4)
            {
                ChangeState(_actionState);
                stateAct = 0;
            }
            else
            {
                switch (stateAct)
                {
                    case 1: ChangeState(actionState.Attack); break;
                    case 2: ChangeState(actionState.Hungry); break;
                    case 3: ChangeState(actionState.Flee); break;
                    case 4: ChangeState(actionState.Socialize); break;
                }
            }
            _actionState = (actionState)stateAct;
        }

        async Task LoopAction()
        {
            do
            {
                await Task.Yield();
                await CanDefendToDanger();
                // Debug.Log("loop action");
                await Task.Yield();
            } while (alive);
        }

        private async Task<bool> CanDefendToDanger()
        {
            var colliders = Physics.OverlapSphere(transform.position, radiusDetectionDanger, dangerLayers);
            var temp = colliders.ToList();
            temp.Remove(_collider);
            colliders = temp.ToArray();
            await Task.Yield();
            if (colliders.Length > 0)
            {
                await Task.Yield();
                foreach (var coll in colliders)
                {
                    if (coll.transform.root != transform.root)
                    {

                        if (fov.visibleTargets.Contains(coll.transform))
                        {
                            // Debug.LogFormat("im in danger and i see it and it is {0}", coll.gameObject.name);
                            await Task.Yield();
                            return true;
                        }
                        else
                        {
                            // Debug.LogFormat("im in danger and i dont see the problem {0}", coll.gameObject.name);
                            await Task.Yield();
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        async Task LoopHunger()
        {
            do
            {
                Hunger--;
                if (Hunger <= tresholdHungry)
                {
                    await Task.Yield();
                    isHungry = true;
                    HungryManager();
                }
                await Task.Delay(50);
            } while (alive);
        }

        void HungryManager()
        {
            isHungry = true;
            if (this.walking)
                this.walking = false;
            ChangeState(actionState.Hungry);
        }

        internal void RequestNewState()
        {
            //flee is the most important and can stop anything
            if (isInDanger)
            {
                if (CanDefendToDanger().Result)
                {
                    ChangeState(actionState.Flee);
                }
                else
                {
                    ChangeState(actionState.Attack);
                    Debug.Log("here");
                }
            }
            else
            {
                if (isHungry)
                {
                    ChangeState(actionState.Hungry);
                }
                else
                {
                    if (inRangeToAttack = InRangeToAttack())
                    {
                        ChangeState(actionState.Attack);
                        Debug.Log("there");
                    }
                    else
                    {
                        if (canSocialize = Physics.CheckSphere(transform.position, radiusDetectionSocial, othersLayer))
                        {
                            ChangeState(actionState.Socialize);
                        }
                        else
                        {
                            ChangeState(actionState.Walking);
                        }
                    }
                }
            }
        }

        bool InRangeToAttack(){
            //change correctly the layer or do overlap and take out the self
            gameObject.layer=noneLayer;
            bool detection=Physics.CheckSphere(transform.position,attackRange,dangerLayers);
            gameObject.layer=othersLayer;
            return inRangeToAttack=detection;
        }

        async Task GiveFood(int value)
        {
            Hunger += value;
            if (Hunger > tresholdHungry)
            {
                isHungry = false;
                await Task.Yield();
            }
            await Task.Yield();
        }

        internal async Task StatePending()
        {
            ChangeState(actionState.Walking);
            await Task.Yield();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag(foodTag))
            {
                Destroy(other.gameObject);
                GiveFood(10).ConfigureAwait(false).GetAwaiter();
            }
            else if (other.gameObject.CompareTag(tag) && isAttacking)
            {
                Destroy(other.gameObject);
            }
        }

        Task DestroyMe() => Task.Run(() => { Destroy(gameObject); });

        async Task DestroyOBJ(GameObject what) => await Task.Run(async () =>
        {
            await Task.Yield();
            Destroy(what);
        });

        internal Vector3 target = Vector3.zero;
        private void OnDrawGizmos()
        {
            Gizmos.color = hungryState.foodFound ? Color.magenta : Color.green;
            // Gizmos.DrawWireSphere(transform.position,radius);
            Gizmos.DrawWireSphere(transform.position, radiusDetectionDanger);
            if (target != Vector3.zero)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, target);
            }
        }
    }

    public enum actionState { Walking, Attack, Hungry, Flee, Socialize }
    public enum TypeForOthers { Baddy, Neutral, Good }
}