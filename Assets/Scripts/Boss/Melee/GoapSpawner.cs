using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GoapSpawner : MonoBehaviour
{
    [SerializeField] GameObject prefabs;
    [SerializeField] GameObject plane;
    [SerializeField] WaitForSeconds ws=new(5);
    [SerializeField] WaitForSeconds wsFood=new(3);
    Vector3[] pos;
    HolderGOAP goapHolder;
    [SerializeField] Transform holderFood;
    string keyGOAP="GOAP";
    string keyFood="Food3D";
    public List<GameObject> enemies=new();
    public List<GameObject> foods=new();
    public int numberToSpawnOnStartEnemies=10;
    public int numberToSpawnOnStartFoods=20;
    public bool SpawnRuntime=false;

    IEnumerator Spawn(){
        do
        {
            var go=Addressables.InstantiateAsync(keyGOAP,RandomPos(),Quaternion.identity,goapHolder.transform);
            // Instantiate(prefabs,RandomPos(),Quaternion.identity,goapHolder.transform);
            enemies.Add(go.WaitForCompletion());
            yield return ws;
        } while (true);
    }

    IEnumerator SpawnFood(){
        do
        {
            var go=Addressables.InstantiateAsync(keyFood,RandomPos(),Quaternion.identity,holderFood);
            foods.Add(go.WaitForCompletion());
            yield return wsFood;
        } while (true);
    }

    // Start is called before the first frame update
    void Start()
    {
        CapperEntities.Start();
        goapHolder=FindObjectOfType<HolderGOAP>();
        InitPos();
        SpawnOnStartEn();
        SpawnOnStartFood();
        if(SpawnRuntime)
            StartCoroutine(Spawn());
        StartCoroutine(SpawnFood());
    }

    void SpawnOnStartEn(){
        for (int i = 0; i < numberToSpawnOnStartEnemies; i++)
        {
            var go=Addressables.InstantiateAsync(keyGOAP,RandomPos(),Quaternion.identity,goapHolder.transform);
            enemies.Add(go.WaitForCompletion());
        }
        enemies.RemoveAll(item=>item==null);
    }
    void SpawnOnStartFood(){
        for (int i = 0; i < numberToSpawnOnStartFoods; i++)
        {
            var go=Addressables.InstantiateAsync(keyFood,RandomPos(),Quaternion.identity,holderFood);
            foods.Add(go.WaitForCompletion());
        }
        foods.RemoveAll(item=>item==null);
    }

    void InitPos(){
        var b=plane.GetComponent<UnityEngine.AI.NavMeshSurface>().navMeshData.sourceBounds;
        pos=new Vector3[2]{b.min,b.max};
    }

    internal Vector3 RandomPos()=>new(Random.Range(pos[0].x,pos[1].x),0f,Random.Range(pos[0].z,pos[1].z));

}
