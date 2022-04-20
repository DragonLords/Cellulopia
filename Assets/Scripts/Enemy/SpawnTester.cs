using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

public class SpawnTester : MonoBehaviour
{
    public GameObject food;
    public Enemy.Enemy enemy;
    // Start is called before the first frame update
    void Start()
    {
        Spawn().ConfigureAwait(false).GetAwaiter();
    }

    async Task Spawn(){
        do
        {
            Vector3 target=new(Random.Range(enemy.bounds[0].x,enemy.bounds[1].x),0,Random.Range(enemy.bounds[0].z,enemy.bounds[1].z));
            Instantiate(food,target,Quaternion.identity,transform);
            await Task.Delay(5000);
        } while (UnityEditor.EditorApplication.isPlaying);
    }
}
