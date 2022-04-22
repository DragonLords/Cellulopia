using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

public class SpawnTester : MonoBehaviour
{
    public string keyFood="Food3D";
    public GameObject food;
    public GameObject plane;
    public Vector3[] posSpawn;
    // Start is called before the first frame update
    void Start()
    {
        food=Addressables.LoadAssetAsync<GameObject>(keyFood).WaitForCompletion();
        var bounds=plane.GetComponent<UnityEngine.AI.NavMeshSurface>().navMeshData.sourceBounds;
        Debug.Log(bounds);
        posSpawn=new Vector3[2];
        posSpawn[0]=bounds.min;
        posSpawn[1]=bounds.max;
        Spawn().ConfigureAwait(false).GetAwaiter();
    }

    async Task Spawn(){
        do
        {
            Vector3 target=new(Random.Range(posSpawn[0].x,posSpawn[1].x),0f,Random.Range(posSpawn[0].z,posSpawn[1].z));
            Instantiate(food,target,Quaternion.identity,transform);
            await Task.Delay(500);
        } while (UnityEditor.EditorApplication.isPlaying);
    }
}
