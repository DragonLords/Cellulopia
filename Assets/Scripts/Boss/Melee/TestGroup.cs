using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;

public class TestGroup : MonoBehaviour
{
    public int numberGroup=3;
    public Group.SOGroup group;
    public GoapContainer[] containers;
    // Start is called before the first frame update
    void Start()
    {
        CreateGroup();
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.aKey.wasPressedThisFrame){
            AddToRandom();
        }else if(Keyboard.current.digit1Key.wasPressedThisFrame){
            AddToGroup(0);
        }else if(Keyboard.current.digit1Key.wasPressedThisFrame){
            AddToGroup(1);
        }else if(Keyboard.current.digit1Key.wasPressedThisFrame){
            AddToGroup(2);
        }
    }

    void AddToRandom(){
        GOAPGroupManager.AddToRandomGroup(Addressables.InstantiateAsync(AddressablePath.enemy).WaitForCompletion().GetComponent<GoapContainer>());
    }

    void AddToGroup(int i){
        GOAPGroupManager.AddToSpecificGroup((GroupType)i,Addressables.InstantiateAsync(AddressablePath.enemy).WaitForCompletion().GetComponent<GoapContainer>());
    }

    void CreateGroup(){
        // group.Clear();
        // group.CreateGroups(number:numberGroup);

        GOAPGroupManager.CreateGroups();
        foreach(var i in containers){
            AddToGroup((int)i.groupType);
        }
    }
}
