using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//Start of implementing thew GOAP using tutorial of Unity
[System.Serializable]
public abstract class Action : MonoBehaviour
{

    public string actionName;
    public Goal actionGoal;
    public float ActionCost = 1.0f;
    public GameObject target;
    [TagSelector] public string targetTag;
    public float duration = 0f;
    public WorldState[] preConditions;
    public WorldState[] afterEffects;
    [HideInInspector] public UnityEngine.AI.NavMeshAgent agent;
    public Dictionary<string, int> preconditions;
    public Dictionary<string, int> effects;
    public WorldStates agentPlaces;
    public bool running = false;
    public bool Achievable = true;

    public Action()
    {
        this.preconditions = new();
        this.effects = new();
    }

    //construct all in here
    public void Init()
    {

    }

    public void Awake()
    {
        agent = this.gameObject.GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
        if (preConditions != null)
        {
            foreach (var WorldState in preConditions)
            {
                preconditions.Add(WorldState.Key, WorldState.Value);
            }
        }
        if (afterEffects != null)
        {
            foreach (var WorldState in afterEffects)
            {
                effects.Add(WorldState.Key, WorldState.Value);
            }
        }
    }

    public bool IsAchievable()
    {
        return true;
    }

    public bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        foreach (var condition in conditions)
        {
            if (!conditions.ContainsKey(condition.Key))
                return false;
        }
        return true;
    }

    public abstract bool PrePerform(BossMelee caller, GameObject target = null);
    public abstract bool PostPerform();
    public abstract bool TargetExistance(GameObject target);
}