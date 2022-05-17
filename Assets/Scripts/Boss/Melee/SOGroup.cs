using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Group
{
    [CreateAssetMenu(fileName = "SOGroup", menuName = "")]
    public class SOGroup : ScriptableObject
    {
        public GroupData[] groups;
        public void Clear()
        {
            groups=null;
        }
        public void CreateGroups(int number){
            groups=new GroupData[number];
            for(int i=0;i<number;++i){
                groups[i]=new((GroupType)i);
            }
        }
    }

    [System.Serializable]
    public struct GroupData{
        public List<GoapContainer> members;
        public int membersCount=>members.Count;
        public GroupType groupType;
        public GroupData(GroupType groupType){
            members=new();
            this.groupType=groupType;
        }

        public void AddToGroup(GoapContainer newMember){
            members.Add(newMember);
            members.RemoveAll(item=>item==null);
        }
    }
}