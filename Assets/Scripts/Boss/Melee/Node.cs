using System.Collections.Generic;
#if UNITY_EDITOR
#endif

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
