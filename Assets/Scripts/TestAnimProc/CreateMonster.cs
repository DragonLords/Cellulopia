using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

namespace MonsterAniamtion
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class CreateMonster : MonoBehaviour
    {
        GameObject monster;
        public int LengthMonster = 5;
        // Start is called before the first frame update
        void Start()
        {
            // Create();
        }

        // Update is called once per frame
        void Update()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
                OnUpdateLengthMonster();
        }

        public List<GameObject> childsMonster = new();
        List<Transform> transfroms = new();
        List<RigLayer> rigLayers = new();
        GameObject rigGO;
        Vector3 offset = new(0f, 1.2f, 0f);
#if UNITY_EDITOR
        BoneRenderer boneRenderer;
#endif
        public void Create()
        {
            if (monster is not null)
                DeleteMonster();
            //create a new monster here
            //create the empty holding the monster
            monster = new() { name = "Monster" };
            for (int i = 0; i < LengthMonster; i++)
            {
                GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
                child.name = $"Part {i}";
                if (childsMonster.Count == 0)
                    child.transform.SetParent(monster.transform);
                else
                {
                    Debug.Log("here");
                    child.transform.SetParent(childsMonster[i - 1].transform);
                    child.transform.position = offset * i;
                }
                childsMonster.Add(child);
                transfroms.Add(child.transform);
            }

            rigGO = new() { name = "Rig" };
            rigGO.transform.SetParent(monster.transform);
            Rig rig = rigGO.AddComponent<Rig>();
#if UNITY_EDITOR
            rig.runInEditMode = true;
#endif
            RigLayer rigLayer = new(rig);
            rigLayers.Add(rigLayer);
#if UNITY_EDITOR
            boneRenderer = monster.AddComponent<BoneRenderer>();
            boneRenderer.transforms = transfroms.ToArray();
#endif
            RigBuilder rigBuilder = monster.AddComponent<RigBuilder>();
            rigBuilder.layers = rigLayers;

            for (int i = 0; i < childsMonster.Count - 1; i++)
            {
                GameObject go = new() { name = "Damped" };
                go.transform.SetParent(rigGO.transform);
                DampedTransform damp = go.AddComponent<DampedTransform>();
                damp.data.constrainedObject = transfroms[i + 1];
                damp.data.sourceObject = transfroms[i];
            }
        }

        public void DeleteMonster()
        {
            if (monster is not null)
                DestroyImmediate(monster);
            childsMonster.Clear();
        }

        internal void OnUpdateLengthMonster()
        {
            //just need to add last and add comp
            //GameObject child = new() { name = "new child" };
            GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.name = "another one";
            child.transform.SetParent(transfroms.Last());
            transfroms.Add(child.transform);
            child.transform.position = offset * childsMonster.Count;
            childsMonster.Add(child);

            GameObject damp = new() { name = "damped_created_after" };
            damp.transform.SetParent(rigGO.transform);
            DampedTransform damped = damp.AddComponent<DampedTransform>();
            damped.data.constrainedObject = transfroms[childsMonster.Count - 1];
            damped.data.sourceObject= transfroms[childsMonster.Count - 2];
#if UNITY_EDITOR
            boneRenderer.transforms = transfroms.ToArray();
#endif
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
            else if (GUILayout.Button("Update monster"))
            {
                creator.OnUpdateLengthMonster();
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