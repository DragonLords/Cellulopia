using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Animations.Rigging;
using UnityEditor;

namespace Boss.Spawner
{

    public class SpawnerBoss : MonoBehaviour
    {
        public int lengthMonster = 6;
        public GameObject boss;
        public List<GameObject> monsterParts = new();
        public List<Transform> animParts = new();
#if UNITY_EDITOR
        public BoneRenderer boneRenderer;
#endif
        public Vector3 offset = new(0f, 1.5f, 0f);
        public GameObject rigHolder;
        public List<RigLayer> rigLayers = new();
        public List<DampedTransform> DampedsList = new();
        RigBuilder rigBuilder;
        Rig rig;
        internal void Spawn()
        {
            KillBoss();
            boss = new("Boss");
            boss.AddComponent<Boss>();
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(boss.transform);
            monsterParts.Add(head);
            animParts.Add(head.transform);

            for (int i = 0; i < lengthMonster; i++)
            {
                GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
                part.transform.SetParent(animParts[^1]);
                part.transform.position = -1 * animParts.Count * offset;
                animParts.Add(part.transform);
                monsterParts.Add(part);
            }

            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tail.transform.SetParent(animParts[^1]);
            tail.transform.position = -1 * animParts.Count * offset;
            animParts.Add(tail.transform);
            monsterParts.Add(tail);

            CreateBone();
        }

        void CreateBone()
        {
            rigHolder = new("Rig");
            rigHolder.transform.SetParent(boss.transform);
            rig = rigHolder.AddComponent<Rig>();
#if UNITY_EDITOR
            rig.runInEditMode = true;
#endif
            RigLayer rigLayer = new(rig);
            rigLayers.Add(rigLayer);
#if UNITY_EDITOR
            boneRenderer = boss.AddComponent<BoneRenderer>();
            boneRenderer.transforms = animParts.ToArray();
#endif
            rigBuilder = boss.AddComponent<RigBuilder>();
            rigBuilder.layers = rigLayers;
            for (int i = 2; i < monsterParts.Count - 1; i++)
            {
                GameObject damped = new("Damped");
                damped.transform.SetParent(rigHolder.transform);
                DampedTransform damp = damped.AddComponent<DampedTransform>();
                damp.data.constrainedObject = animParts[i + 1];
                damp.data.sourceObject = animParts[i];
            }
            rigBuilder.Build();
        }

        internal void KillBoss()
        {
            if (boss != null)
                DestroyImmediate(boss);
            monsterParts.Clear();
            animParts.Clear();
            rigLayers.Clear();
        }
    }

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(SpawnerBoss))]
    public class SpawnerBossEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SpawnerBoss spawner = (SpawnerBoss)target;
            if (GUILayout.Button("Spawn Boss"))
            {
                spawner.Spawn();
            }
            else if (GUILayout.Button("Kill boss"))
            {
                spawner.KillBoss();
            }
            base.OnInspectorGUI();

        }
    }
#endif
    #endregion
}