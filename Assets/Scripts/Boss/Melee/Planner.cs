using System.Collections.Generic;
#if UNITY_EDITOR
#endif

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
}
