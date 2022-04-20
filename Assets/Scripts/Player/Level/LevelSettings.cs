using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "LevelSettings", menuName = "Cellulopia/LevelSettings", order = 0)]
public class LevelSettings : ScriptableObject
{
    public List<int> levelRequirement = new();
    public List<ulong> listUlong = new();
    public Task<int[]> AddExperience(int xpPlayerHas, int level, int skillPoint) => Task.Run(() =>
    {
        int[] final = { xpPlayerHas, level, skillPoint };
        int xpRequire = levelRequirement[level];
        if (xpPlayerHas >= xpRequire)
        {
            final[1] += 1;
            final[0] -= xpRequire;
            final[2] += 1;
        }
        // if(xpPlayerHas>=xpRequire){
        //     level++;
        //     skillPoint++;
        //     xpPlayerHas-=xpRequire;
        // }
        return final;
    });

    public Task InitTable() => Task.Run(() =>
    {
        levelRequirement.Add(50);
        for (int i = 0; i < 100; i++)
        {
            levelRequirement.Add(levelRequirement[i] * 2);
        }
        listUlong.Add(50);
        for (int i = 0; i < 100; i++)
        {
            listUlong.Add(listUlong[i] * 2);
        }
    });
}

#region editor
#if UNITY_EDITOR
[CustomEditor(typeof(LevelSettings))]
public class LevelSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var settings = (LevelSettings)target;
        if (settings.levelRequirement.Count == 0)
            if (GUILayout.Button("booo"))
            {
                settings.levelRequirement.Add(50);
                for (int i = 0; i < 100; i++)
                {
                    settings.levelRequirement.Add(settings.levelRequirement[i] * 2);
                }
                settings.listUlong.Add(50);
                for (int i = 0; i < 100; i++)
                {
                    settings.listUlong.Add(settings.listUlong[i] * 2);
                }
            }
        base.OnInspectorGUI();
    }
}
#endif
#endregion