using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnEnemy : MonoBehaviour
{
    public AssetReference enemyTemp;
    public List<GameObject> enemies=new();
    [Range(1000,15000)]public int delay=1000;
    public GameObject plane;
    public Vector3[] posSpawn;
    private void Start()
    {
        Spawn().ConfigureAwait(false).GetAwaiter();
    }

    async Task Spawn(){
        var bounds=plane.GetComponent<UnityEngine.AI.NavMeshSurface>().navMeshData.sourceBounds;
        Debug.Log(bounds);
        posSpawn=new Vector3[2];
        posSpawn[0]=bounds.min;
        posSpawn[1]=bounds.max;
        do
        {
            await Task.Delay(delay);
            Vector3 target=new(Random.Range(posSpawn[0].x,posSpawn[1].x),0f,Random.Range(posSpawn[0].z,posSpawn[1].z));
            var en=enemyTemp.InstantiateAsync(target,Quaternion.identity,transform);
            en.WaitForCompletion();
            en.Completed+=Spawn_Completed;
        } while (UnityEditor.EditorApplication.isPlaying);        
    }

    private void Spawn_Completed(AsyncOperationHandle<GameObject> obj)
    {
        var a=obj.Result;
        if(a is not null){
            var en=a.GetComponent<Enemy.Enemy>();
            int rnd=Random.Range(0,en._actions.Count);
            int rndType=Random.Range(0,3);
            en.typeForOthers=(Enemy.TypeForOthers)rndType;
            en._actionState=(Enemy.actionState)rnd;
            a.name=$"{(Enemy.actionState)rnd}_{(Enemy.TypeForOthers)rndType}";
            enemies.Add(a);
            en.Init((Enemy.actionState)rnd);
        }
        // obj.Result.GetComponent<Enemy.Enemy>().CallbackSpawn(Enemy.TypeForOthers.Neutral).ConfigureAwait(false);
    }
}