using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MonsterAniamtion
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class CreateMonster : MonoBehaviour
    {
        GameObject monster;
        public List<GameObject> childsMonster = new();
        public int LengthMonster = 5;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Create()
        {
            if (monster is not null)
                DeleteMonster();
            List<Transform> transfroms=new();
            //create a new monster here
            //create the empty holding the monster
            monster = new(){name="Monster"};
            Vector3 offset=new(0f,1.5f,0f);
            for (int i = 0; i < LengthMonster; i++)
            {
                GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
                child.name=$"Part {i}";
                if(childsMonster.Count==0)
                    child.transform.SetParent(monster.transform);
                else {
                    Debug.Log("here");
                    child.transform.SetParent(childsMonster[i-1].transform);
                    child.transform.position=new(0,1.5f,0);
                }
                childsMonster.Add(child);
            }
            BoneRenderer boneRenderer=monster.AddComponent<BoneRenderer>();
            
        }

        public void DeleteMonster()
        {
            if (monster is not null)
                DestroyImmediate(monster);
            childsMonster.Clear();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CreateMonster))]
    public class CreateMonsterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CreateMonster creator = (CreateMonster)target;
            if (GUILayout.Button("Create new monster"))
            {
                creator.Create();
            }
            else if (GUILayout.Button("Delete this monster"))
            {
                creator.DeleteMonster();
            }
            base.OnInspectorGUI();

        }
    }
#endif
}