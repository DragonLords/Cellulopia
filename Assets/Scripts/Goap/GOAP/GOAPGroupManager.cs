using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public static class GOAPGroupManager 
{
    private static Dictionary<GroupType,GroupData> _groupsData=new();
    public readonly static Dictionary<GroupType,GroupData> GroupsData=_groupsData;
    public static void Clear(){
        _groupsData.Clear();
    }
    public static void CreateGroups(){
        _groupsData.Clear();
        for(int i=0;i<Enum.GetNames(typeof(GroupType)).Length;++i){
            _groupsData.Add((GroupType)i,new GroupData((GroupType)i));
        }
    }
    public static void AddToSpecificGroup(GroupType groupType,GoapContainer member){
        _groupsData[groupType].AddNewMember(member);
    }
    public static GroupType AddToRandomGroup(GoapContainer member){
        GroupType gt=(GroupType)UnityEngine.Random.Range(0,Enum.GetNames(typeof(GroupType)).Length);
        _groupsData[gt].AddNewMember(member);
        return gt;
    }

    public static bool SameGroup(GoapContainer self,GoapContainer other)=>
        _groupsData[self.groupType].members.Contains(other);

    public static bool Same(GoapContainer c1,GoapContainer c2,out GameObject target){
        bool c=_groupsData[c1.groupType].members.Contains(c2);
        Debug.Log($"<color=cyan>{c}</color>");
        if(c){
            target=c2.gameObject;
        }
        else target=null;
        return c;
    }
}

[System.Serializable]
public struct GroupData{
    public int Count=>members.Count;
    public List<GoapContainer> members;
    public GroupType groupType;
    public GroupData(GroupType groupType){
        members=new();
        this.groupType=groupType;
    }

    public void AddNewMember(GoapContainer newMember){
        members.Add(newMember);
        members.RemoveAll(item=>item==null);
    }
}
