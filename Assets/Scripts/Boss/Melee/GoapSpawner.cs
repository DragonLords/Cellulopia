using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class GoapSpawner : MonoBehaviour
{
    [SerializeField] int _maxEnemyInLevel=3;
    [SerializeField] int _maxFoodsInLevel=20;
    [SerializeField] GameObject prefabs;
    [SerializeField] GameObject plane;
    [SerializeField] WaitForSeconds ws=new(5);
    [SerializeField] WaitForSeconds wsFood=new(3);
    Vector3[] pos;
    HolderGOAP goapHolder;
    [SerializeField] Transform holderFood;
    string keyGOAP="EnemyHolder_3D";
    string keyFood="Food3D";
    public List<GameObject> enemies=new();
    public List<GameObject> foods=new();
    public int numberToSpawnOnStartEnemies=10;
    public int numberToSpawnOnStartFoods=20;
    public bool SpawnRuntime=false;
    public bool spawnFood=true;
    public bool SpawnEnemy = false;
    public Generator.Gen3D gen;

    IEnumerator Spawn(){
        do
        {
            enemies.RemoveAll(item=>item==null);
            if(enemies.Count<_maxEnemyInLevel){
                var go=Addressables.InstantiateAsync(keyGOAP,RandomPosFinal(),Quaternion.identity,goapHolder.transform);
                // Instantiate(prefabs,RandomPos(),Quaternion.identity,goapHolder.transform);
                enemies.Add(go.WaitForCompletion());
            }
            yield return ws;
        } while (true);
    }

    IEnumerator SpawnFood(){
        do
        {
            foods.RemoveAll(item=>item==null);
            if(foods.Count<_maxFoodsInLevel){
                var go=Addressables.InstantiateAsync(keyFood,RandomPosFinal(),Quaternion.identity,holderFood);
                foods.Add(go.WaitForCompletion());
            }
            yield return wsFood;
        } while (true);
    }

    // Start is called before the first frame update
    void Start()
    {
        if(gen == null)
            gen=FindObjectOfType<Generator.Gen3D>();
        CapperEntities.Start();
        goapHolder=FindObjectOfType<HolderGOAP>();
        if (SpawnEnemy)
        {
            SpawnOnStartEn();
            if(SpawnRuntime){
                StartCoroutine(Spawn());
                StartCoroutine(CheckEnemy());
            }
        }
        if (spawnFood)
        {
            SpawnOnStartFood();
            StartCoroutine(SpawnFood());
        }
        GameManager.Instance.StartCheckEnemy();
        GameManager.Instance.maxEnemiesInLevel=_maxEnemyInLevel;
    }

    IEnumerator CheckEnemy(){
        do
        {
            if(CapperEntities.CanSpawn()){
                SpawnNewEnemy();
            }        
            yield return null;
        } while (true);
    }

    void SpawnOnStartEn(){
        for (int i = 0; i < numberToSpawnOnStartEnemies; i++)
        {
            var go=Addressables.InstantiateAsync(keyGOAP,RandomPosFinal(),Quaternion.identity,goapHolder.transform);
            enemies.Add(go.WaitForCompletion());
        }
        GameManager.Instance.enemies=new(enemies);
        enemies.RemoveAll(item=>item==null);
    }
    void SpawnOnStartFood(){
        for (int i = 0; i < numberToSpawnOnStartFoods; i++)
        {
            var go=Addressables.InstantiateAsync(keyFood,RandomPosFinal(),Quaternion.identity,holderFood);
            foods.Add(go.WaitForCompletion());
            go.WaitForCompletion();
            go.Result.transform.rotation = new(-90f, 0, 0,0);
        }
        foods.RemoveAll(item=>item==null);
    }

    public void SpawnNewEnemy(){
        var go=Addressables.InstantiateAsync(keyGOAP,RandomPosFinal(),Quaternion.identity,goapHolder.transform);
        enemies.Add(go.WaitForCompletion());
        GameManager.Instance.enemies.Add(go.WaitForCompletion());
    }

    void InitPos(){
        var b=plane.GetComponent<UnityEngine.AI.NavMeshSurface>().navMeshData.sourceBounds;
        pos=new Vector3[2]{b.min,b.max};
    }

    internal Vector3 RandomPos()=>new(Random.Range(pos[0].x,pos[1].x),1f,Random.Range(pos[0].z,pos[1].z));
    internal Vector3 RandomPosFinal(){
        int rnd=Random.Range(0,gen.emptyTiles.Count);
        return new(gen.emptyTiles[rnd].x,1f,gen.emptyTiles[rnd].y);
    }

}
