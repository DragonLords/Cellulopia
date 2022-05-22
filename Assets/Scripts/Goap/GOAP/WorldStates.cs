using System.Collections.Generic;
#if UNITY_EDITOR
#endif

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
