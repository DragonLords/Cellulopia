using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR    
using UnityEditor;
#endif

namespace Generator
{
    [System.Serializable]
    public class Gen3D : MonoBehaviour
    {
        public GameObject Quad;
        public Material[] materials;
        public int[,] carte;
        public Vector2Int dimension = Vector2Int.one;
        public bool seedRandom = true;
        public int seed = 0;
        /// <summary>
        /// Sert a determine le taux de remplissage en % de la carte
        /// </summary>
        [Range(0, 100)] public float _tauxRemplissage = 5;
        [HideInInspector] public List<GameObject> cubes = new();
        [Range(0, 10)] public int polissageCarte = 1;
        public bool forceMur = true;
        public List<Vector2Int> emptyTiles = new();

        public void Start()
        {
            RemplirCarte();
            Generate3DMap();
            var player=FindObjectOfType<Player.Rework.Player>();
            GameManager.Instance.emptyTiles=new(emptyTiles);
            GameManager.Instance.map=carte;
            //place player
            // FindObjectOfType<EntitiesPlacer>().OnStart();
        }

        int[,] RemplirCarte()
        {
            carte = new int[dimension.x, dimension.y];
            if (seedRandom)
                seed = Random.Range(-10000, 10000);
            System.Random prng = new(seed);
            for (int x = 0; x < dimension.x; x++)
            {
                for (int y = 0; y < dimension.y; y++)
                {
                    //on verifie si la posisiton est une case de contour, si oui on la ferme
                    if (x == 0 || x == dimension.x - 1 || y == 0 || y == dimension.y - 1)
                    {
                        // Debug.Log($"wall at {x},{y}");
                        carte[x, y] = 1;
                    }
                    else
                        carte[x, y] = prng.Next(0, 100) < _tauxRemplissage ? 1 : 0;
                }
            }

            for (int i = 0; i < polissageCarte; i++)
                NormaliserCarte();

            if (forceMur)
                FermerMur();

            return carte;
        }

        void FermerMur()
        {
            for (int x = 0; x < dimension.x; x++)
            {
                for (int y = 0; y < dimension.y; y++)
                {
                    if (x == 0 || x == dimension.x - 1 || y == 0 || y == dimension.y - 1)
                        carte[x, y] = 1;
                }
            }
        }

        void NormaliserCarte()
        {
            for (int x = 0; x < carte.GetLength(0); x++)
            {
                for (int y = 0; y < carte.GetLength(1); y++)
                {
                    int mur = ObtenirMurAutour(x, y);
                    //si la case est entourer de plus que 4 mur la position devient un mur
                    if (mur > 4)
                        carte[x, y] = 1;
                    //si la case a moins que 4 mur alors la position devient vide
                    else if (mur < 4)
                        carte[x, y] = 0;
                }
            }
        }
        List<string> walls = new();
        public int ObtenirMurAutour(int posX, int posY)
        {
            int nbMur = 0;
            //on regarde les murs autour dansun patern de 3X3
            for (int x = posX - 1; x <= posX + 1; x++)
            {
                for (int y = posY - 1; y <= posY + 1; y++)
                {
                    //si la case est dans les cases du monde (pas exterieur)
                    if (x >= 0 && x < dimension.x && y >= 0 && y < dimension.y)
                    {
                        //on ne regarde pas la case cibler 
                        // if (x != posX && y != posY)
                        //alors on ajoute sa valeurs a celle des murs (soit 0 ou 1)
                        nbMur += carte[x, y];
                        //sinon le mur est a lexterieur et on renforce le fait de faire apparaitre des murs au niveau exterieur
                    }
                    else
                        ++nbMur;
                }
            }
            // Debug.Log($"at the position {posX},{posY} there is {nbMur} wall");
            return nbMur;
        }
        public Dictionary<Vector2, int> GenererPosTuiles(int[,] carte)
        {
            Dictionary<Vector2, int> pos = new();
            for (int x = 0; x < dimension.x; x++)
            {
                for (int y = 0; y < dimension.y; y++)
                {
                    pos.Add(new(x, y), carte[x, y]);
                }
            }
            return pos;
        }

        public void Generate3DMap()
        {
            
            GameObject fill = new("fill");
            for (int x = 0; x < carte.GetLength(0); x++)
            {
                for (int z = 0; z < carte.GetLength(1); z++)
                {
                    if (carte[x, z] == 1)
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = new(x, 0, z);
                        cube.transform.parent = fill.transform;
                        cube.transform.localScale=new(1,15,1);
                        cube.GetComponent<Renderer>().material=materials[1];
                    }
                    else
                    {
                        // GameObject go=new("new");
                        // go.transform.SetParent(empty.transform);
                        // var render=go.AddComponent<MeshRenderer>();
                        // var filter=go.AddComponent<MeshFilter>();
                        // var mesh=new Mesh{name="dsfhsd"};
                        // mesh.vertices=TriangleGen();
                        // mesh.triangles=new int[]{0,1,2,3,4,5};
                        // filter.mesh=mesh;
                        // var coll=go.AddComponent<MeshCollider>();
                        // coll.sharedMesh=mesh;
                        // render.material=materials[0];
                        // go.transform.position=new(x,0f,z);

                            GameObject quad=GameObject.CreatePrimitive(PrimitiveType.Cube);

                        // GameObject quad=Instantiate(Quad,new(x,0,z),Quaternion.identity);
                        quad.transform.position=new(x,0,z);
                        quad.transform.parent = this.empty.transform;
                        quad.transform.rotation=new(-90f,0f,0f,0f);
                        quad.GetComponent<Renderer>().material=materials[0];
                        emptyTiles.Add(new(x,z));
                        quad.layer=empty.layer;
                    }
                    // carte[x,z]==1?
                }
            }
            this.empty.transform.SetParent(transform);
            fill.transform.SetParent(transform);
            var arr = new GameObject[2] { this.empty, fill };
            this.empty.GetComponent<NavMeshSurface>().BuildNavMesh();
            // FuseMesh(arr);
            // StartCoroutine(SolveMeshCount());
        }

        public Vector3[] TriangleGen(){
        Vector3[] v3=new Vector3[6];
        v3[0]=new(0,0,0);
        v3[1]=new(1,0,0);
        v3[2]=new(0,0,1);
        v3[3]=new(1,0,0);
        v3[4]=new(0,0,1);
        v3[5]=new(1,0,1);
        return v3;
    }

        public void FuseMesh(GameObject[] gos)
        {
            bool empty = true;
            foreach (var go in gos)
            {
                MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combine = new CombineInstance[filters.Length];
                Debug.Log(filters.Length);
                
                
                for (int i = 0; i < filters.Length; i++)
                {
                    combine[i].mesh = filters[i].sharedMesh;
                    combine[i].transform = filters[i].transform.localToWorldMatrix;
                    // filters[i].gameObject.SetActive(false);
                }
                MeshFilter filter;
                try{
                    filter = go.AddComponent<MeshFilter>();
                }catch{
                    filter=go.GetComponent<MeshFilter>();
                }
                Renderer rend;
                try{
                    rend=go.AddComponent<MeshRenderer>();
                }
                catch{
                    rend=go.GetComponent<Renderer>();
                }
                filter.mesh = new() { name = $"Mesh final {go.name}" };
                filter.sharedMesh.CombineMeshes(combine, true);
                MeshCollider collider = go.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = filter.sharedMesh;
                if (empty){
                    rend.material = materials[0];
                    go.isStatic=true;
                    AddStuff();
                }
                else{
                    rend.material = materials[1];
                    go.transform.localScale=new(1,15,1);
                    collider.convex=true;
                    // var rb=go.AddComponent<Rigidbody>();
                    // rb.useGravity=false;
                }
                
                for (int i = 0; i < filters.Length; i++)
                {
                    DestroyImmediate(filters[i].gameObject);
                }
                empty = false;
            }
        }

        IEnumerator SolveMeshCount()
        {   
            const int maxAmount=65000;
            const int numberVert=24;
            int act=0;
            int i=0;
            var filters=empty.GetComponentsInChildren<MeshFilter>();
            do
            {
                GameObject count=new();
                while (act<maxAmount)
                {
                    Destroy(filters[i].gameObject);
                    act+=numberVert;
                    i++;
                    yield return null;
                }
                count.name=$"{act}";
                act=0;
            yield return null;
            } while (i<filters.Length);
        }

        void AddStuff()
        {
            for (int i = 0; i < carte.GetLength(0); i++)
            {
                for (int z = 0; z < carte.GetLength(1); z++)
                {
                    if(carte[i,z]==0){
                        emptyTiles.Add(new(i,z));
                    }
                }
            }
            surface=empty.GetComponent<NavMeshSurface>();
            surface.BuildNavMesh();
        }
        public GameObject empty;
        NavMeshSurface surface;
        internal void DebugSurface(){
            Debug.Log(surface.layerMask.value);

            Debug.Log(empty.layer);
        }


    }


#if UNITY_EDITOR
    [CustomEditor(typeof(Gen3D))]
    class Gen3DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Gen3D gen3D = (Gen3D)target;
            if(GUILayout.Button("Click")){
                gen3D.Start();
            }else if(GUILayout.Button("boom")){
                int child=gen3D.transform.childCount;
                for (int i = 0; i <= child; i++)
                {
                    DestroyImmediate(gen3D.transform.GetChild(i).gameObject);
                }
            }else if(GUILayout.Button("deug")){
                gen3D.DebugSurface();
            }
            base.OnInspectorGUI();
        }
    }
#endif
}