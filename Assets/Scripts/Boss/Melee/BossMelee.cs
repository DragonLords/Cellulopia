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
    public Action[] possibleActions;
    public int hungryLevel=30;
    public bool isHungry=false;
    private int _hunger=100;
    public int Hunger{get=>_hunger;set{_hunger=Mathf.Clamp(value,0,100);}}
    public int _aggressivityLevel=0;
    public int AggressivityLevel{get=>_aggressivityLevel;set{_aggressivityLevel=Mathf.Clamp(value,0,100);}}
    
    #region GOAP
    public List<Action> actions;
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
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
        // TesConst().ConfigureAwait(false);
    }

    void DefineInitialValue(){
        AggressivityLevel=Random.Range(0,101);
        Hunger=100;
    }



    private void Update()
    {

    }
}
public enum Goal{Eat,Attack,Flee,Social}


//Start of implementing thew GOAP using tutorial of Unity

[System.Serializable]
public abstract class Action
{
    public string actionName;
    public Goal actionGoal;
    public float ActionCost = 1.0f;
    public GameObject target;
    public GameObject targetTag;
    public float duration=0f;
    public  WorldState[] preConditions;
    public WorldState[] afterEffects;
    public NavMeshAgent agent;
    public Dictionary<string,int> preconditions;
    public Dictionary<string,int> effects;
    public WorldStates agentPlaces;
    public bool running=false;
    public bool Acheivable=true;

    public Action(NavMeshAgent agent)
    {
        this.agent=agent;
        this.preconditions =new();
        this.effects = new();
    }

    public void Awake(){
        if(preConditions!=null){
            foreach(var WorldState in preConditions){
                preconditions.Add(WorldState.Key,WorldState.Value);
            }
        }
        if(afterEffects!=null){
            foreach(var WorldState in preConditions){
                effects.Add(WorldState.Key,WorldState.Value);
            }
        }
    }

    public bool IsAchievable(){
        return true;
    }

    public bool IsAchievableGiven(Dictionary<string,int> conditions){
        foreach (var condition in conditions)
        {
            if(!conditions.ContainsKey(condition.Key))
                return false;
        }
        return true;
    }

    public abstract bool PrePerform();
    public abstract bool PostPerform();
}

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
public class Planner : System.IDisposable
{
    System.Runtime.InteropServices.SafeHandle _safeHandle = new SafeFileHandle(System.IntPtr.Zero, true);
    private bool disposedValue;
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
        bool foundPath=false;
        foreach(var action in usableActions){
            if(action.IsAchievableGiven(start.state)){
                Dictionary<string,int> currentState=new(start.state);
                foreach(var kvp in action.effects){
                    if(!currentState.ContainsKey(kvp.Key))
                        currentState.Add(kvp.Key,kvp.Value);
                }
                Node node=new(start,start.cost+action.ActionCost,currentState,action);
                if(GoalAchieved(goal,currentState)){
                    leaves.Add(node);
                    foundPath=true;
                }else{
                    List<Action> subset=ActionSubset(usableActions,action);
                    //create a recusive call to find the final plan
                    bool found=BuildGraph(node,leaves,subset,goal);
                    if(found){
                        foundPath=true;
                    }
                }
            }
        }
        return true;
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

    public Planner(){
        Debug.Log("created");
    }
    public List<Action> DoAPlanning(Goal goal,BossMelee boss){
        List<Action> actions=new();
        actions.Add(boss.possibleActions[0]);
        // actions.Add();
        return actions;
    }

    #region Dispose
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: supprimer l'état managé (objets managés)
                Debug.Log("dispose started");
                _safeHandle.Dispose();
            }

            // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
            // TODO: affecter aux grands champs une valeur null
            disposedValue = true;
            Debug.Log("disposing");
        }
    }

    // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
    ~Planner()
    {
        // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        Debug.Log("disposed");
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
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
        if(states.ContainsKey(key)){
            states[key]+=value;
        }else{
            states.Add(key,value);
        }
    }

    public void RemoveState(string key){
        if(states.ContainsKey(key)){
            states.Remove(key);
        }
    }

    public void SetState(string key,int value){
        if(states.ContainsKey(key)){
            states[key]=value;
        }else{
            states.Add(key,value);
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