using System.Collections.Generic;
#if UNITY_EDITOR
#endif

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
