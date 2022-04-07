using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.IO;
using Player.Skill;
using System;

public class SkillTreeHandler : MonoBehaviour
{
    public Button[] buttons;
    delegate void A();
    public List<Player.Skill.SkillTemplate> skills=new();
    public List<Player.Skill.SkillTemplate> final=new();
    private void Awake()
    {
        List<string> keys=new(){"Skills"};
        //FIXME: skills appear not in order 
        AsyncOperationHandle<IList<Player.Skill.SkillTemplate>> loadHandle=Addressables.LoadAssetsAsync<Player.Skill.SkillTemplate>("Skills",obj=>skills.Add(obj));
        loadHandle.WaitForCompletion();

        buttons=GetComponentsInChildren<Button>(true);
        // Debug.Log(buttons.Length);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<ButtonHandler>().Init(i,
            buttons[i] as Button,
            skills[i]);
        }
        // for(int i=0;i<3;++i)
        //     Debug.Log(buttons[i].name);
        // Debug.Log(buttons[0].name);
        // Debug.Log(buttons[1].name);
        // Debug.Log(buttons[2].name);

        //on desactive lobject comme ca il nest pas obstruant a lecran
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
