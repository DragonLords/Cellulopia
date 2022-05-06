using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Enemy;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

public class BossMelee : MonoBehaviour
{
    public GOAPTester tester;
    public NavMeshAgent agent;
    internal bool alive = false;
    public LayerMask foodLayer;
    public LayerMask enemyLayer;
    public int hungryLevel = 30;
    public bool isHungry = false;
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
    public GoapSpawner spawner;
    // Start is called before the first frame update
    public void OnStart()
    {
        spawner=FindObjectOfType<GoapSpawner>();
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
        } while (alive);
    }



    public void GetEnemiesClose(){
        var DefaultLayer=LayerMask.NameToLayer("Default");
        var selfLayer=this.gameObject.layer;
        this.gameObject.layer=DefaultLayer;
        enemiesClose=Physics.OverlapSphere(transform.position,radiusFoodDetection,enemyLayer);
        this.gameObject.layer=selfLayer;
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
    IEnumerator DoingTheAction()
    {
        doingAction = true;
        do
        {
            // Debug.Log("<color=olive>AAA</color>");
            if (currentAction.target == null)
            {
                // Debug.Log("wsiugdsif");
            }
            yield return null;
        } while (doingAction && agent.remainingDistance > 1f && currentAction.target is not null);
        currentAction.PostPerform();
        doingAction = false;
        // Debug.Log(goals.Count);
        if (actionQueue.Count == 0)
        {
            actions.Clear();
            // Debug.Log("<color=red>STOP</color>");
            //reroll action
            StopCoroutine(routineLoopAction);
            doingAction = false;
            acts = GetComponentsInChildren<Action>();
            foreach (var a in acts)
            {
                actions.Add(a);
            }
            routineLoopAction = StartCoroutine(LoopDetection());
        }
    }


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
            if(currentAction.target != null){
                agent.SetDestination(currentAction.target.transform.position);
                yield return new WaitUntil(() => agent.remainingDistance < 1f||currentAction.Achieved);
            }
        }
        // Debug.Log("finished");
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
                // Debug.Log("Queue is null");
            }
            else
            {
                // Debug.Log(actionQueue.Count);
            }
        }
        
        yield return null;
        // Highlight();
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


    private void LateUpdate()
    {
        #region Test
        // if(currentAction is null){
        //     Debug.Log("achieve");
        // }
        // if(currentAction is not null){
        //     if(!currentAction.Achievable){
        //         Debug.Log("i need to stop");
        //     }
        // }
        // if(currentAction is not null && currentAction.running){
        //     // CheckTarget().ConfigureAwait(false).GetAwaiter();
        //     if(currentAction.agent.hasPath&&currentAction.agent.remainingDistance<1f){
        //         if(!invoked){
        //             // await Task.Delay(Mathf.RoundToInt(currentAction.duration));
        //             // CompleteAction();
        //             Invoke("CompleteAction",currentAction.duration);
        //             invoked=true;
        //         }
        //     }
        //     return;
        // }
        // if(planner is null || actionQueue is null){
        //     planner=new();
        //     Debug.Log("whyyyyy");
        //     var sortedGoal = from entry in goals orderby entry.Value descending select entry;
        //     Debug.LogFormat("coutn:{0}",sortedGoal.ToList().Count);
        //     if(sortedGoal.ToList().Count==0){
        //         //heres the problem and the crash so
        //         //FIXME: make unity crash and aslo due to 1 time completition
        //         //the end is near for the agent
        //         //need to recreate new action and then loop over them
        //         DeleteAllActions().ConfigureAwait(false).GetAwaiter().GetResult();
        //         // Debug.LogFormat("goals count:{0} goal:{1} {2}",goals.Count,goals.First().Key,goals.First().Value);
        //     }
        //     foreach(var kvp in sortedGoal){
        //         actionQueue=planner.plan(actions,kvp.Key.subGoals,null);
        //         if(actionQueue is not null){
        //             currentSubGoal=kvp.Key;
        //             break;
        //         }
        //     }
        // }

        // if(actionQueue is not null && actionQueue.Count==0)
        // {
        //     if(currentSubGoal.remove){
        //         goals.Remove(currentSubGoal);
        //     }
        //     Debug.Log("here");
        //     planner=null;
        // }

        // if(actionQueue is not null && actionQueue.Count>0)
        // {
        //     currentAction=actionQueue.Dequeue();
        //     if(currentAction.PrePerform(this))
        //     {
        //         if(currentAction.target==null && currentAction.targetTag is not ""){
        //             currentAction.target=GameObject.FindGameObjectWithTag(currentAction.targetTag);
        //         }
        //         if(currentAction.target is not null){
        //             currentAction.running=true;
        //             currentAction.agent.SetDestination(currentAction.target.transform.position);
        //         }
        //     }else{
        //         actionQueue=null;
        //     }
        //     Debug.LogFormat("Queue:{0}",actionQueue.Count);
        // }
        // Debug.Log("test test this is a test WRIIIIIII");
        #endregion
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



[System.Serializable]
public class SubGoal
{
    public Dictionary<string, int> subGoals;
    public bool remove;

    public SubGoal(string key, int value, bool remove)
    {
        this.subGoals = new();
        this.subGoals.Add(key, value);
        this.remove = remove;
    }
}

[System.Serializable]
public class ActionResult
{
    public bool result;
}

[System.Serializable]
public class Node
{
    public Node parent;
    public float cost;
    public Dictionary<string, int> state;
    public Action action;

    public Node(Node parent, float cost, Dictionary<string, int> state, Action action)
    {
        this.parent = parent;
        this.cost = cost;
        this.state = new(state);
        this.action = action;
    }
}

[System.Serializable]
public class Planner
// : System.IDisposable
{
    // System.Runtime.InteropServices.SafeHandle _safeHandle = new SafeFileHandle(System.IntPtr.Zero, true);
    // private bool disposedValue;
    public Queue<Action> plan(List<Action> actions, Dictionary<string, int> goal, WorldStates states)
    {
        List<Action> usableActions = new();
        foreach (var action in actions)
        {
            if (action.IsAchievable())
            {
                usableActions.Add(action);
            }
        }
        List<Node> leaves = new();
        Node start = new(null, 0, World.Instance.GetWorld().GetStates(), null);
        bool success = BuildGraph(start, leaves, usableActions, goal);
        if (!success)
        {
            // Debug.LogFormat("node:{0} cost:{1} value:{2} action:{3}", start.parent, start.cost, start.state.First().Value, start.action);
            // Debug.Log("<color=red>No Plan!</color>");
            return null;
        }
        Node cheapest = null;
        foreach (Node leaf in leaves)
        {
            if (cheapest is null)
            {
                cheapest = leaf;
            }
            else
            {
                if (leaf.cost < cheapest.cost)
                {
                    cheapest = leaf;
                }
            }
        }
        List<Action> result = new();
        Node n = cheapest;
        while (n != null)
        {
            if (n.action != null)
            {
                result.Insert(0, n.action);
            }
            n = n.parent;
        }
        Queue<Action> queue = new();
        foreach (var action in actions)
        {
            queue.Enqueue(action);
        }
        // Debug.Log("the plan is: ");
        foreach (var item in queue)
        {
            // Debug.Log("Q "+item.actionName);
        }
        return queue;
    }

    private bool BuildGraph(Node start, List<Node> leaves, List<Action> usableActions, Dictionary<string, int> goal)
    {
        // Debug.LogFormat("leaves:{0} actions:{1} goals:{2}",leaves.Count,usableActions.Count,goal.Count);
        bool foundPath = false;
        if(!foundPath){
            foreach (var action in usableActions)
            {
                if (action.IsAchievableGiven(start.state))
                {
                    // Debug.LogFormat("achgiv:{0}",action.IsAchievableGiven(start.state));
                    Dictionary<string, int> currentState = new(start.state);
                    // Debug.LogFormat("curr state:{0} ___effects:{1}___",currentState.Count,action.effects.Count);
                    foreach (var kvp in action.effects)
                    {
                        if (!currentState.ContainsKey(kvp.Key))
                            currentState.Add(kvp.Key, kvp.Value);
                    }
                    Node node = new(start, start.cost + action.ActionCost, currentState, action);

                    // Debug.LogFormat("<color=pink>goal:{0} curr:{1} </color>",goal.First().Value,currentState.First().Value);

                    if (GoalAchieved(goal, currentState))
                    {
                        // Debug.Log("here");
                        leaves.Add(node);
                        foundPath = true;
                    }
                    else
                    {
                        List<Action> subset = ActionSubset(usableActions, action);
                        // Debug.LogFormat("subset:{0}",subset.Count);
                        //create a recusive call to find the final plan
                        bool found = BuildGraph(node, leaves, subset, goal);
                        // Debug.LogFormat("found:{0}",found);
                        if (found)
                        {
                            foundPath = true;
                            // Debug.Log("it will be true");
                        }
                    }
                }
                // Debug.LogFormat("found Path:{0}",foundPath);
            }
        }
        return foundPath;
        // return true;
    }

    //will not add the action to the next subset
    private List<Action> ActionSubset(List<Action> usableActions, Action action)
    {
        List<Action> subset = new();
        foreach (var item in usableActions)
        {
            if (!item.Equals(action))
            {
                subset.Add(item);
            }
        }
        return subset;
    }

    private bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> currentState)
    {
        foreach (var kvp in goal)
        {
            if (currentState.ContainsKey(kvp.Key))
            {
                return false;
            }
        }
        return true;
    }

    #region Dispose
    // protected virtual void Dispose(bool disposing)
    // {
    //     if (!disposedValue)
    //     {
    //         if (disposing)
    //         {
    //             // TODO: supprimer l'état managé (objets managés)
    // Debug.Log("dispose started");
    //             _safeHandle.Dispose();
    //         }

    //         // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
    //         // TODO: affecter aux grands champs une valeur null
    //         disposedValue = true;
    // Debug.Log("disposing");
    //     }
    // }

    // // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
    // ~Planner()
    // {
    //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
    // Debug.Log("disposed");
    //     Dispose(disposing: false);
    // }

    // public void Dispose()
    // {
    //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
    //     Dispose(disposing: true);
    //     System.GC.SuppressFinalize(this);
    // }
    #endregion
}

[System.Serializable]
public class WorldState
{
    public string Key;
    public int Value;
}

[System.Serializable]
public class WorldStates
{
    public Dictionary<string, int> states;

    public WorldStates()
    {
        this.states = new();
    }

    public bool HasState(string Key)
    {
        return states.ContainsKey(Key);
    }

    void AddStates(string key, int value)
    {
        states.Add(key, value);
    }

    public void ModifyState(string key, int value)
    {
        if (HasState(key))
        {
            states[key] += value;
        }
        else
        {
            AddStates(key, value);
        }
    }

    public void RemoveState(string key)
    {
        if (HasState(key))
        {
            states.Remove(key);
        }
    }

    public void SetState(string key, int value)
    {
        if (HasState(key))
        {
            states[key] += value;
            if (states[key] <= 0)
                RemoveState(key);
        }
        else
        {
            AddStates(key, value);
        }
    }

    public Dictionary<string, int> GetStates() => states;
}

public sealed class World
{
    private static readonly World instance = new();
    private static WorldStates world;
    static World()
    {
        world = new();
    }

    private World()
    {

    }

    public static World Instance { get => instance; }

    public WorldStates GetWorld() => world;
}