using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using Newtonsoft;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Player.AnimationProc
{

    public class AnimProc : MonoBehaviour
    {
        public List<GameObject> monsterParts = new();
        public List<Transform> animParts = new();
        public int initialLength=3;
#if UNITY_EDITOR
        public BoneRenderer boneRenderer;
#endif
        public Vector3 offset=new(0f,1.5f,0f);
        public GameObject rigHolder;
        public GameObject monsterHolder;
        public List<RigLayer> rigLayers=new();
        public List<DampedTransform> DampedsList=new();
        public Sprite[] partsSprite;
        RigBuilder rigBuilder;
        Rig rig;
#if UNITY_EDITOR
        public SaveDummy.Data data;
#endif
        // Use this for initialization
        void Start()
        {
            //Create();
            // StartCoroutine(Animate());
        }

        // Update is called once per frame
        void Update()
        {
            if(Keyboard.current.qKey.wasPressedThisFrame)
                IncreaseLength();
        }

        internal void Create()
        {
            DestroyAll();
            monsterHolder = new("Player");
            monsterHolder.AddComponent<Player>();
            #region Create Head
            GameObject head = new("Head");
            SpriteRenderer sr = head.AddComponent<SpriteRenderer>();
            sr.sprite = partsSprite[(int)PartSelection.Head];
            head.transform.SetParent(monsterHolder.transform);
            monsterParts.Add(head);
            animParts.Add(head.transform);
            #endregion
            CreateBone();
            #region Body
            for (int i = 0; i < initialLength; i++)
                AddNewBody();
            #endregion
            #region Tail
            //then we create the tail
            CreateTail();
            #endregion
            UpdateBone();
            CreateCollider();
        }

        private void CreateTail()
        {
            GameObject tail = new($"part_{monsterParts.Count}");
            SpriteRenderer srTail = tail.AddComponent<SpriteRenderer>();
            srTail.sprite = partsSprite[(int)PartSelection.Tail];
            tail.transform.SetParent(animParts[animParts.Count - 1]);
            tail.transform.position = offset * animParts.Count * -1;
            animParts.Add(tail.transform);
            monsterParts.Add(tail);
        }

        void CreateBone(){
            rigHolder=new("Rig");
            rigHolder.transform.SetParent(monsterHolder.transform);
            rig=rigHolder.AddComponent<Rig>();
#if UNITY_EDITOR
            rig.runInEditMode=true;
#endif
            RigLayer rigLayer=new(rig);
            rigLayers.Add(rigLayer);
#if UNITY_EDITOR
            boneRenderer=monsterHolder.AddComponent<BoneRenderer>();
            boneRenderer.transforms=animParts.ToArray();
#endif
            rigBuilder=monsterHolder.AddComponent<RigBuilder>();
            rigBuilder.layers=rigLayers;
            //why -1 well we dont want to create one empty and useless bone at the end
            for(int i=2;i<monsterParts.Count-1;i++){
                GameObject damped=new("Damped");
                damped.transform.SetParent(rigHolder.transform);
                DampedTransform damp=damped.AddComponent<DampedTransform>();
                damp.data.constrainedObject=animParts[i+1];
                damp.data.sourceObject=animParts[i];
            }
            rigBuilder.Build();
        }
        
        Vector3 target=new(0,0,-20);
        IEnumerator Animate(){
            yield return new WaitForSeconds(1);
            // print(monsterParts[0].name);
            float i=0;
            do
            {

                var q=Quaternion.Euler(0,0,i);
                monsterParts[0].transform.rotation=Quaternion.Slerp(monsterParts[0].transform.rotation,q,5f*Time.deltaTime);
                // monsterParts[0].transform.right=target-monsterParts[0].transform.position;
                yield return null;
                ++i;
            } while (true);
        }

        internal void AddNewBody()
        {
            GameObject part=new($"Body_{monsterParts.Count-1}");
            SpriteRenderer sr=part.AddComponent<SpriteRenderer>();
            sr.sprite=partsSprite[(int)PartSelection.Body];
            part.transform.SetParent(animParts[^1]);
            part.transform.position = -1 * animParts.Count * offset;
            animParts.Add(part.transform);
            monsterParts.Add(part);
            UpdateBone();
        }

        void UpdateBone(){
            //if (animParts.Count == 2)
            //    return;
            GameObject damp=new("New_Damped");
            damp.transform.SetParent(rigHolder.transform);
            DampedTransform damped=damp.AddComponent<DampedTransform>();
            //Debug.LogFormat("source:{0}  con:{1}  count:{2}",animParts[^2].name,animParts[^1].name,animParts.Count);
            damped.data.constrainedObject=animParts[^1];
            damped.data.sourceObject=animParts[^2];
#if UNITY_EDITOR
            boneRenderer.transforms=animParts.ToArray();
#endif
            damped.data.dampPosition=.5f;
            damped.data.dampRotation=.5f;
            damped.data.maintainAim=true;
            rigBuilder.Build();
            DampedsList.Add(damped);
        }

        internal void IncreaseLength(){
            if(monsterHolder==null)
                Create();
            monsterParts.Last().GetComponent<SpriteRenderer>().sprite=partsSprite[(int)PartSelection.Body];

            GameObject go=new($"part_{monsterParts.Count}");
            go.transform.SetParent(monsterHolder.transform,true);
            go.transform.SetParent(animParts.Last(),false);
            animParts.Add(go.transform);
            //go.transform.position = new(animParts[^1].position.x,-10f,animParts[^1].position.z);
            go.transform.position = Vector3.zero;
            //go.transform.position = new(animParts[^2].position.x, (animParts[^2].position.y) -offset.y, animParts[^2].position.z);
            //go.transform.position = (animParts.Count - 1) * -1 * offset;
            monsterParts.Add(go);
            go.AddComponent<SpriteRenderer>().sprite=partsSprite[(int)PartSelection.Tail];
#if UNITY_EDITOR
            boneRenderer.transforms=animParts.ToArray();
#endif
            GameObject damped=new("damped");
            damped.transform.SetParent(rigHolder.transform);
            DampedTransform damp=damped.AddComponent<DampedTransform>();
            damp.data.sourceObject=animParts[^2];
            damp.data.constrainedObject=animParts[^1];
            DampedsList.Add(damp);

            // GameObject lastPart=monsterParts.Last();
            // lastPart.GetComponent<SpriteRenderer>().sprite=partsSprite[(int)PartSelection.Body];
            // lastPart.transform.SetParent()
            // lastPart.name=$"part_{monsterParts.Count-1}";
            // GameObject tail=new("tail");
            // tail.transform.SetParent(lastPart.transform);
            // GameObject tail=monsterParts.Last();
            // tail.name=$"Swiper_{monsterParts.Count}";

            // SpriteRenderer sr=tail.GetComponent<SpriteRenderer>();
            // sr.sprite=partsSprite[(int)PartSelection.Body];
            // GameObject newEnd=new($"Tail");
            // newEnd.transform.SetParent(animParts.Last());
            // newEnd.transform.position=offset*monsterParts.Count*-1;
            // sr=newEnd.AddComponent<SpriteRenderer>();
            // sr.sprite=partsSprite[(int)PartSelection.Tail];
            // GameObject damped=new("new_damped");
            // damped.transform.SetParent(rigHolder.transform);
            // DampedTransform damp=damped.AddComponent<DampedTransform>();
            // animParts.Add(newEnd.transform);
            // monsterParts.Add(newEnd);
            // DampedsList.Add(damp);
            // damp.data.sourceObject=animParts[animParts.Count-2];
            // damp.data.constrainedObject=animParts.Last();
            // Debug.Log(animParts.Last());
            // boneRenderer.transforms=animParts.ToArray();
            CreateCollider();
            rigBuilder.Build();
        }

        internal void CreateCollider()
        {
            foreach(var part in monsterParts)
            {
                var col=part.AddComponent<BoxCollider2D>();
            }
        }

        internal void RemoveOne()
        {

        }

        internal void DestroyAll()
        {
            try{
                if(monsterHolder is not null){
                    GameObject go=FindObjectOfType<DummyClass>().gameObject;
                    // Debug.Log(go.name);
                    if(go is not null)
                        DestroyImmediate(FindObjectOfType<DummyClass>().gameObject);
                }
            }catch{}
            animParts.Clear();
            monsterParts.Clear();
            rigLayers.Clear();    
            DampedsList.Clear();        
        }
#if UNITY_EDITOR
        SaveDummy s;
        internal void Save(){

            MapGenerator mapGenerator=new();
            mapGenerator.dimension=new(60,60);
            mapGenerator.seed=5363;
            mapGenerator._tauxRemplissage=36;
            mapGenerator.forceMur=true;
            mapGenerator.polissageCarte=3;
            SaveDummy save=new(mapGenerator.GenererCarte());
            data=save.data;
            s=save;
        }

        internal void Load(){
            data=s.Load();
        }

        internal void Empty(){
            data=new(null,DateTime.Today,null);
        }
#endif
    }

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(AnimProc))]
    internal class AnimProcEditor : Editor {
        public override void OnInspectorGUI()
        {
            AnimProc animProc = (AnimProc)target;
            if (GUILayout.Button("Create"))
            {
                animProc.Create();
            }else if(GUILayout.Button("Add new body"))
            {
                animProc.IncreaseLength();
            }else if(GUILayout.Button("Remove one body"))
            {
                animProc.RemoveOne();
            }else if(GUILayout.Button("Kill it"))
            {
                animProc.DestroyAll();
            }else if(GUILayout.Button("Save")){
                animProc.Save();
            }else if(GUILayout.Button("Load")){
                animProc.Load();
            }else if(GUILayout.Button("Empty data")){
                animProc.Empty();
            }
            base.OnInspectorGUI();
        }
    }
#endif
    #endregion

    public enum PartSelection{Head,Body,Tail}

#if UNITY_EDITOR
    public class SaveDummy{
        int[,] map;
        string path;
        public Data data;
        public SaveDummy(int[,] map){
            System.Text.StringBuilder sb=new();
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    sb.Append(map[x,y]);
                    sb.Append(' ');
                }
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
            this.map=map;
            this.path=$"{Application.dataPath}/SaveData/dummy.json";
            Init();
        }

        void Init(){
            CheckFile();
            CreateDummy();
            string output=Newtonsoft.Json.JsonConvert.SerializeObject(data,Newtonsoft.Json.Formatting.Indented);
            Debug.Log(output);
            FileStream fs=new(path,FileMode.OpenOrCreate);
            fs.Close();
            // fs.Write(output);
            // File.OpenWrite(path);
            File.WriteAllText(path,output);
        }   

        void CreateDummy(){
            data=new("dummy boi",DateTime.Today,map);
        }

        private void CheckFile()
        {
            try{
                FileStream fs=new(path,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
                fs.Close();
            }catch{}
        }

        internal Data Load()
        {
            Newtonsoft.Json.JsonConvert.DeserializeObject<Data>(File.ReadAllText(path));
            return data;
        }

        [Serializable]
        public class Data : IDisposable {
            public string name;
            public DateTime date;
            public int[,] map;
            public Data(string name, DateTime date,int[,] map)
            {
                this.name = name;
                this.date = date;
                this.map=map;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                // Debug.Log(this.GetType().Name);
            }
            bool _disposed=true;
            protected virtual void Dispose(bool disposed){
                if(_disposed)
                    return;
                if(disposed){
                    
                }
                _disposed=true;
            }
        }
    }
#endif
}