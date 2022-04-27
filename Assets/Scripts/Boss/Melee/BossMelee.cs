using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Enemy;

using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

public class BossMelee : MonoBehaviour
{
    public NavMeshAgent agent;
    bool alive=false;
    public LayerMask foodLayer;
    // public int hungryLevel=30;
    // public bool isHungry=false;
    // private int _hunger=100;
    // public int Hunger{get=>_hunger;set{_hunger=Mathf.Clamp(value,0,100);}}
    // public int _aggressivityLevel=0;
    // public int AggressivityLevel{get=>_aggressivityLevel;set{_aggressivityLevel=Mathf.Clamp(value,0,100);}}
    
    #region GOAP
    public List<Action> actions=new();
    public Dictionary<SubGoal,int> goals=new();
    Planner planner;
    Queue<Action> actionQueue;
    public Action currentAction;
    SubGoal currentSubGoal;
    public Action[] acts;
    #endregion
    
    // Start is called before the first frame update
    public void Start()
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
        
        alive=true;    
        acts=GetComponents<Action>();
        foreach(var a in acts){
            actions.Add(a);
        }
        LoopDetection().ConfigureAwait(false);
    }

    // void DefineInitialValue(){
        // AggressivityLevel=Random.Range(0,101);
        // Hunger=100;
    // }


    bool invoked=false;
    internal float radiusFoodDetection=50f;

    void CompleteAction(){
        currentAction.running=false;
        currentAction.PostPerform();
        invoked=false;
    }

    private async Task LoopDetection()
    {
        do
        {
            
        if(currentAction is not null && currentAction.running){
            if(currentAction.agent.hasPath&&currentAction.agent.remainingDistance<1f){
                await Task.Yield();
                if(!invoked){
                    await Task.Delay(Mathf.RoundToInt(currentAction.duration));
                    CompleteAction();
                    // Invoke("CompleteAction",currentAction.duration);
                    invoked=true;
                }
            }
            await Task.Yield();
            return;
        }
        await Task.Yield();
        if(planner is null || actionQueue is null){
            planner=new();

            await Task.Yield();
            var sortedGoal = from entry in goals orderby entry.Value descending select entry;
            Debug.LogFormat("goals count:{0} goal:{1} {2}",goals.Count,goals.First().Key,goals.First().Value);
            foreach(var kvp in sortedGoal){
                await Task.Yield();
                actionQueue=planner.plan(actions,kvp.Key.subGoals,null);
                if(actionQueue is not null){
                    currentSubGoal=kvp.Key;
                    await Task.Yield();
                    break;
                }
            }
        }

        if(actionQueue is not null && actionQueue.Count==0)
        {
            if(currentSubGoal.remove){
                goals.Remove(currentSubGoal);
            }
            planner=null;
        }

        if(actionQueue is not null && actionQueue.Count>0)
        {
            currentAction=actionQueue.Dequeue();
            if(currentAction.PrePerform(this))
            {
                if(currentAction.target==null && currentAction.targetTag is not ""){
                    currentAction.target=GameObject.FindGameObjectWithTag(currentAction.targetTag);
                }
                if(currentAction.target is not null){
                    currentAction.running=true;
                    currentAction.agent.SetDestination(currentAction.target.transform.position);
                }
            }else{
                actionQueue=null;
            }
        }
        await Task.Yield();
        } while (alive);
    }
}
public enum Goal{Eat,Attack,Flee,Social}



[System.Serializable]
public class SubGoal{
    public Dictionary<string,int> subGoals;
    public bool remove;

    public SubGoal(string key,int value, bool remove)
    {
        this.subGoals=new();
        this.subGoals.Add(key,value);
        this.remove = remove;
    }
}

[System.Serializable]
public class ActionResult{
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
    public Queue<Action> plan(List<Action> actions,Dictionary<string,int> goal,WorldStates states){
        List<Action> usableActions=new();
        foreach(var action in actions){
            if(action.IsAchievable()){
                usableActions.Add(action);
            }
        }
        List<Node> leaves=new();
        Node start=new(null,0,World.Instance.GetWorld().GetStates(),null);
        bool success=BuildGraph(start,leaves,usableActions,goal);
        if(!success){
            // Debug.LogFormat("node:{0} cost:{1} value:{2} action:{3}",start.parent,start.cost,start.state.First().Value,start.action);
            Debug.Log("<color=red>No Plan!</color>");
            return null;
        }
        Node cheapest=null;
        foreach(Node leaf in  leaves){
            if(cheapest is null){
                cheapest=leaf;
            }
            else{
                if(leaf.cost<cheapest.cost)
                {
                    cheapest=leaf;    
                }
            }
        }
        List<Action> result=new();
        Node n=cheapest;
        while(n!=null){
            if(n.action!=null){
                result.Insert(0,n.action);
            }
            n=n.parent;
        }
        Queue<Action> queue=new();
        foreach (var action in actions)
        {
            queue.Enqueue(action);
        }
        Debug.Log("the plan is: ");
        foreach(var item in queue){
            Debug.Log("Q "+item.actionName);
        }
        return queue;
    }

    private bool BuildGraph(Node start, List<Node> leaves, List<Action> usableActions, Dictionary<string, int> goal)
    {
        Debug.LogFormat("leaves:{0} actions:{1} goals:{2}",leaves.Count,usableActions.Count,goal.Count);
        bool foundPath=false;
        foreach(var action in usableActions){
            if(action.IsAchievableGiven(start.state)){
                Debug.LogFormat("achgiv:{0}",action.IsAchievableGiven(start.state));
                Dictionary<string,int> currentState=new(start.state);
                Debug.LogFormat("curr state:{0} ___effects:{1}___",currentState.Count,action.effects.Count);
                foreach(var kvp in action.effects){
                    if(!currentState.ContainsKey(kvp.Key))
                        currentState.Add(kvp.Key,kvp.Value);
                }
                Node node=new(start,start.cost+action.ActionCost,currentState,action);
                
                Debug.LogFormat("<color=pink>goal:{0} curr:{1} </color>",goal.First().Value,currentState.First().Value);

                if(GoalAchieved(goal,currentState)){
                    Debug.Log("here");
                    leaves.Add(node);
                    foundPath=true;
                }else{
                    List<Action> subset=ActionSubset(usableActions,action);
                    Debug.LogFormat("subset:{0}",subset.Count);
                    //create a recusive call to find the final plan
                    bool found=BuildGraph(node,leaves,subset,goal);
                    Debug.LogFormat("found:{0}",found);
                    if(found){
                        foundPath=true;
                    }
                }
            }
            Debug.LogFormat("found Path:{0}",foundPath);
        }
        return foundPath;
    }

    //will not add the action to the next subset
    private List<Action> ActionSubset(List<Action> usableActions, Action action)
    {
        List<Action> subset=new();
        foreach(var item in usableActions){
            if(!item.Equals(action)){
                subset.Add(item);
            }
        }
        return subset;
    }

    private bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> currentState)
    {
        foreach(var kvp in goal){
            if(!currentState.ContainsKey(kvp.Key)){
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
    //             Debug.Log("dispose started");
    //             _safeHandle.Dispose();
    //         }

    //         // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
    //         // TODO: affecter aux grands champs une valeur null
    //         disposedValue = true;
    //         Debug.Log("disposing");
    //     }
    // }

    // // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
    // ~Planner()
    // {
    //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
    //     Debug.Log("disposed");
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
public class WorldState{
    public string Key;
    public int Value;
}

[System.Serializable]
public class WorldStates{
    public Dictionary<string,int> states;

    public WorldStates()
    {
        this.states=new();
    }

    public bool HasState(string Key){
        return states.ContainsKey(Key);
    }

    void AddStates(string key,int value){
        states.Add(key,value);
    }

    public void ModifyState(string key,int value){
        if(HasState(key)){
            states[key]+=value;
        }else{
            AddStates(key,value);
        }
    }

    public void RemoveState(string key){
        if(HasState(key)){
            states.Remove(key);
        }
    }

    public void SetState(string key,int value){
        if(HasState(key)){
            states[key]+=value;
            if(states[key]<=0)
                RemoveState(key);
        }else{
            AddStates(key,value);
        }
    }

    public Dictionary<string,int> GetStates()=>states;
}

public sealed class World{
    private static readonly World instance=new();
    private static WorldStates world;
    static World(){
        world=new();
    }

    private World(){

    }

    public static World Instance{get=>instance;}

    public WorldStates GetWorld()=>world;
} 