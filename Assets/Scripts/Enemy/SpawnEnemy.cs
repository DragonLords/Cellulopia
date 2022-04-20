using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

public class SpawnEnemy : MonoBehaviour
{
    public AssetReference enemyTemp;
    public Enemy.Enemy enemy;
    List<GameObject> enemies=new();
    [Range(1000,15000)]public int delay=1000;
    private void Start()
    {
        Spawn().ConfigureAwait(false).GetAwaiter();
    }

    async Task Spawn(){
        do
        {
            await Task.Delay(delay);
            Vector3 target=new(Random.Range(enemy.bounds[0].x,enemy.bounds[1].x),0,Random.Range(enemy.bounds[0].z,enemy.bounds[1].z));
            enemyTemp.InstantiateAsync(target,Quaternion.identity,transform);
        } while (UnityEditor.EditorApplication.isPlaying);        
    }
}