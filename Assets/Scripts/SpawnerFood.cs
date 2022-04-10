using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnerFood : MonoBehaviour
{
    AssetReference m_food;
    // Start is called before the first frame update
    void Start()
    {
        Addressables.LoadAssetAsync<GameObject>("Food").Completed += GetFood_Completed;
        StartCoroutine(SpawnFood());
        SpawnFoods().ConfigureAwait(false);
        Debug.Log("start ended food");
    }

    private void GetFood_Completed(AsyncOperationHandle<GameObject> obj)
    {
        //throw new NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnFood()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("cor");
    }

    async Task SpawnFoods()
    {
        await Task.Delay(2000);
        Debug.Log("task");
    }
}
