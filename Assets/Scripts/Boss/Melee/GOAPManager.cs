using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;



public class GOAPManager : MonoBehaviour
{
    public GOAPAgent tester;
    public NavMeshAgent agent;
    internal bool alive = false;
    public LayerMask foodLayer;
    public LayerMask enemyLayer;
    public int hungryLevel = 30;
    public bool isHungry = false;
    public int HungerCostDuplication=30;
    [SerializeField] private int _hunger = 100;
    public int Hunger { get => _hunger; set { _hunger = Mathf.Clamp(value, 0, 100); } }
    public float _delayTickHunger = 1;
    private int tickHunger;
    public int DelayTickHunger { get => tickHunger; set { tickHunger = Mathf.RoundToInt(_delayTickHunger * 1000); } }
    public int _aggressivityLevel = 0;
    public int AggressivityLevel { get => _aggressivityLevel; set { _aggressivityLevel = Mathf.Clamp(value, 0, 100); } }
    int tickAggressivityDown=3000;
    #region GOAP
    public List<Action> actions = new();
    public Dictionary<SubGoal, int> goals = new();
    Planner planner;
    public Queue<Action> actionQueue;
    public Action currentAction;
    public SubGoal currentSubGoal;
    public Action[] acts;
    public GameObject[] objectives;
    #endregion
    public Coroutine routineLoopAction;
    public List<Action> Subs = new();
    public Collider[] enemiesClose;
    public bool isSocializing=false;
    public bool isAttacking=false;
    public bool canSocialize=true;
    // Start is called before the first frame update
    public void OnStart()
    {
        #region Get__Agent
        if (agent is null)
        {
            try
            {
                agent = GetComponent<NavMeshAgent>();
            }
            catch
            {
                agent = gameObject.AddComponent<NavMeshAgent>();
            }
        }
        #endregion
        DefineInitialValue();


        alive = true;
        LoopHunger().ConfigureAwait(false);
        LoopAggresivity().ConfigureAwait(false);
    }

    private async Task LoopAggresivity()
    {
        do
        {
            --AggressivityLevel;
            await Task.Delay(tickAggressivityDown);
        } while (alive);
    }

    internal void HardReset()
    {
        planner = null;
        actionQueue = null;
    }

    void DefineInitialValue()
    {
        tickHunger = Mathf.RoundToInt(_delayTickHunger);
        AggressivityLevel = Random.Range(0, 101);
        Hunger = 100;
    }

    public void GiveFood(int valueFood)
    {
        Hunger += valueFood;
        isHungry=Hunger>hungryLevel;
    }

    async Task LoopHunger()
    {
        int countDownHungerless=5;
        DelayTickHunger = 5;
        do
        {
            if(!GameManager.Instance.Paused){
                --Hunger;
                if(Hunger<=hungryLevel){
                    isHungry=true;
                }
                await Task.Delay(DelayTickHunger);
                if(Hunger==0){
                    --countDownHungerless;
                    if(countDownHungerless==0){
                        StartCoroutine(tester.Death());
                    }
                }
            }else{
                await Task.Yield();
            } 
        } while (alive);
    }



    //FIXME: this will need to be fix since it always return 0
    public void GetEnemiesClose(){
        
        var DefaultLayer=LayerMask.NameToLayer("Default");
        var selfLayer=this.gameObject.layer;
        this.gameObject.layer=DefaultLayer;
        enemiesClose=Physics.OverlapSphere(transform.position,radiusFoodDetection,enemyLayer);
        List<Collider> colliders=new();
        foreach (var item in enemiesClose)
        {
            if(transform.root!=item.transform.root){
                if(!tester.groupMembers.Contains(transform.root.gameObject)){
                    colliders.Add(item);
                    // Debug.LogFormat("<color=orange>Conatins:{0}</color>",tester.groupMembers.Contains(transform.root.gameObject));
                }
            }
                
        }
        this.gameObject.layer=selfLayer;
        enemiesClose=colliders.ToArray();
    }

    void CancelAction()
    {
        currentAction.Achievable = false;
        CompleteAction();
    }

    bool invoked = false;
    internal float radiusFoodDetection = 50f;

    void CompleteAction()
    {
        currentAction.running = false;
        // currentAction.PostPerform();
        // currentAction = null;
        invoked = false;
    }
    bool doingAction = false;

    IEnumerator DoAction()
    {
        agent.isStopped=true;
        doingAction = true;
        currentAction = actionQueue.Dequeue();
        agent.isStopped=false;
        // Debug.LogFormat("Doing: {0} target:{1}", currentAction,currentAction.target.name);
        // Debug.LogFormat("target found:{0}",currentAction.PrePerform(this));
        if (currentAction.PrePerform(this))
        {
            Debug.Log("aaaaaaaa");
            if(currentAction.target != null){
                do
                {
                    agent.SetDestination(currentAction.target.transform.position);
                    // yield return null;
                    yield return new WaitUntil(() => agent.remainingDistance < 1f||currentAction.Achieved||currentAction.target==null);
                } while (agent.remainingDistance>1f&&!currentAction.Achieved);
                Debug.Log("exit");
            }
        }
        // Debug.LogFormat("finished the action {0}",currentAction.actionName);
        // UnityEditor.EditorApplication.isPaused=true;
        doingAction = false;
        yield return StartCoroutine(ActionFinished());
        // currentAction.PostPerform();
    }

    internal IEnumerator ActionFinished()
    {
        yield return null;
        if (currentAction is not null)
        {
            actions.Remove(currentAction);
            // currentAction.PostPerform();
            goals.Remove(currentSubGoal);
            // Debug.Log("removed");
        }
        if (actionQueue.Count == 0)
        {
            // Debug.Log("need new queue");
            // StartCoroutine(tester.Redo());
            StartCoroutine(RedoIntern());
        }
    }

    internal IEnumerator InitQueue()
    {
        if (planner is null)
        {
            planner = new();
            tester.OnSetGoal();
            var sortedGoal = from entry in goals orderby entry.Value descending select entry;
            // Debug.Log(sortedGoal.Count());
            foreach (var kvp in sortedGoal)
            {
                actionQueue = planner.plan(actions, kvp.Key.subGoals, null);
                if (actionQueue is not null)
                {
                    currentSubGoal = kvp.Key;
                    break;
                }
            }
            if (actionQueue is null)
            {
                Debug.Log("Queue is null");
                
            }
            else
            {
                Debug.Log(actionQueue.Count);
            }
        }
        
        yield return null;
    }

    internal IEnumerator StartAction(){
        do
        {
            if(actionQueue.Count>0){
                yield return StartCoroutine(DoActionRework());
                yield return new WaitWhile(() => doingAction == true);
            }
            yield return null;
        } while (alive);
    }

    private IEnumerator DoActionRework()
    {
        agent.isStopped=true;
        doingAction=true;
        currentAction=actionQueue.Dequeue();
        agent.isStopped=false;
        if(tester.actionEnum==ActionEnum.Reprod){
            //TODO: uncomment here when finished
            tester.Duplicate();
        }else{
            if(currentAction.PrePerform(this)){
                do
                {
                    agent.SetDestination(currentAction.target.transform.position);
                    yield return new WaitUntil(()=>agent.remainingDistance<1f||currentAction.Achieved||currentAction.target==null||!tester.alive);
                } while (currentAction.target!=null&&agent.remainingDistance>1f&&!currentAction.Achieved&&tester.alive);
            }
        }
        // Debug.LogFormat("Can do action:{0}",currentAction.PrePerform(this));
        yield return null;
        doingAction=false;
        currentAction.PostPerform();
        yield return StartCoroutine(ActionFinishedRework());
    }

    IEnumerator ActionFinishedRework(){
        yield return null;
        if(currentAction != null){
            actions.Remove(currentAction);
            goals.Remove(currentSubGoal);
            tester.acts=null;
            currentAction=null;
        }
        if(actionQueue.Count==0){
            Debug.Log("All action in queue completed will need to restart the loop");
            planner=null;
            yield return StartCoroutine(tester.StartDoingAction());
        }
    }



    internal IEnumerator FinalDetection()
    {
        do
        {

            if (planner is null)
            {
                planner = new();
                var sortedGoal = from entry in goals orderby entry.Value descending select entry;
                // Debug.Log(sortedGoal.Count());
                foreach (var kvp in sortedGoal)
                {
                    actionQueue = planner.plan(actions, kvp.Key.subGoals, null);
                    if (actionQueue is not null)
                    {
                        currentSubGoal = kvp.Key;
                        break;
                    }
                }
                if (actionQueue is null)
                {
                    // Debug.Log("Queue is null");
                }
                else
                {
                    // Debug.Log(actionQueue.Count);
                }
            }

            if (actionQueue is not null)
            {
                if (actionQueue.Count > 0)
                {
                    yield return StartCoroutine(DoAction());
                    yield return new WaitWhile(() => doingAction == true);

                }
            }

            if (currentAction is not null)
            {
                if (!currentAction.TargetExistance())
                {
                    currentAction.Achieved = true;
                }
                if (currentAction.Achieved)
                {
                    CompleteAction();
                }
            }



            yield return null;
        } while (alive);
    }

    internal void SetGoal()
    {
        actions.Clear();
        goals.Clear();
        // Debug.Log(goals.Count);
        acts = GetComponentsInChildren<Action>();
        foreach (var a in acts)
        {
            actions.Add(a);
        }
        for (int i = 0; i < actions.Count; i++)
        {
            SubGoal s1 = new($"bob{i}", 1, true);
            goals.Add(s1, 5);
        }
    }

    internal IEnumerator RedoIntern(){
        if(routineLoopAction is not null)
            StopCoroutine(routineLoopAction);
        yield return StartCoroutine(tester.SelectNewObjective());
        SetGoal();
        HardReset();
        yield return StartCoroutine(InitQueue());
        if(actionQueue is null){
            do
            {
                // Debug.Log("retry");
                StartCoroutine(InitQueue());
                yield return null;
            } while (actionQueue is null);
        }
        routineLoopAction=StartCoroutine(FinalDetection());
    }

    private IEnumerator LoopDetection()
    {
        do
        {
            yield return null;
            if (currentAction is not null && currentAction.running)
            {
                // CheckTarget().ConfigureAwait(false).GetAwaiter();
                if (currentAction.agent.hasPath && currentAction.agent.remainingDistance < 1f)
                {
                    if (!invoked)
                    {
                        // await Task.Delay(Mathf.RoundToInt(currentAction.duration));
                        yield return new WaitUntil(() => doingAction == false);
                        yield return new WaitForSeconds(currentAction.duration);
                        CompleteAction();
                        // Invoke("CompleteAction", currentAction.duration);
                        invoked = true;
                    }
                }
                yield return null;
            }
            if (planner is null || actionQueue is null)
            {
                planner = new();
                var sortedGoal = from entry in goals orderby entry.Value descending select entry;
                // Debug.LogFormat("coutn:{0}", sortedGoal.ToList().Count);
                // if(sortedGoal.ToList().Count==0){
                //     //heres the problem and the crash so
                //     //FIXME: make unity crash and aslo due to 1 time completition
                //     //the end is near for the agent
                //     //need to recreate new action and then loop over them
                //     DeleteAllActionsSync();
                //     // Debug.LogFormat("goals count:{0} goal:{1} {2}",goals.Count,goals.First().Key,goals.First().Value);
                // }
                foreach (var kvp in sortedGoal)
                {
                    actionQueue = planner.plan(actions, kvp.Key.subGoals, null);
                    if (actionQueue is not null)
                    {
                        currentSubGoal = kvp.Key;
                        break;
                    }
                }
            }

            if (actionQueue is not null && actionQueue.Count == 0)
            {
                if (currentSubGoal.remove)
                {
                    // Debug.Log("aaa");
                    goals.Remove(currentSubGoal);
                }
                // Debug.Log("here");
                planner = null;
                yield return null;
            }

            //do the action here
            #region Action Doing

            if (actionQueue is not null && actionQueue.Count > 0)
            {
                currentAction = actionQueue.Dequeue();
                if (currentAction.PrePerform(this))
                {
                    if (currentAction.target == null && currentAction.targetTag is not "")
                    {
                        currentAction.target = GameObject.FindGameObjectWithTag(currentAction.targetTag);
                    }
                    if (currentAction.target is not null)
                    {
                        currentAction.running = true;
                        currentAction.agent.SetDestination(currentAction.target.transform.position);
                        // doingAction=true;
                        // StartCoroutine(DoingTheAction());
                    }
                }
                else
                {
                    actionQueue = null;
                }
                yield return null;
                // Debug.LogFormat("Queue:{0}", actionQueue.Count);
            }

            #endregion

            if (currentAction is not null)
            {
                if (currentAction.Achieved)
                {
                    // Debug.Log("action achieved");
                }
            }
            if (goals.Count == 0)
            {
                // Debug.Log("break");
                break;
            }

        } while (alive && !doingAction);
    }
}
public enum Goal { Eat, Attack, Flee, Social }
