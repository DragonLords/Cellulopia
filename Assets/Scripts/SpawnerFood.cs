using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnerFood : MonoBehaviour
{
    string keyFood = "Food";
    string keyMalus = "Malus";
    AssetReference foodPrefabs;
    Transform holderFood;
    Vector2 rangePlayer;
    // Start is called before the first frame update
    public void StartSpawn(Transform playerPos)
    {

        // AsyncOperationHandle<GameObject> loadOP=Addressables.LoadAssetAsync<GameObject>("Food");
        GameObject container = new("food_container");
        holderFood = container.transform;
        SpawnFoods(playerPos).ConfigureAwait(false);
        // Debug.Log("start ended food");
    }

    private void GetFood_Completed(AsyncOperationHandle<GameObject> obj)
    {
        //throw new NotImplementedException();
    }

    async Task SpawnFoods(Transform playerPos)
    {
        List<Vector2Int> positionsSpawns = GameManager.Instance.emptyTiles;
        bool spawn = true;
        do
        {
            for (int i = 0; i < 5; i++)
            {
                bool good = UnityEngine.Random.Range(0, 2) == 0;
                int rnd = UnityEngine.Random.Range(0, positionsSpawns.Count);
                Vector3 pos = new(positionsSpawns[rnd].x, positionsSpawns[rnd].y);
                if (good)
                {
                    var obj = Addressables.InstantiateAsync(keyFood, pos, Quaternion.identity, holderFood, true).WaitForCompletion();
                }
                else
                {
                    var obj = Addressables.InstantiateAsync(keyMalus, pos, Quaternion.identity, holderFood, true).WaitForCompletion();
                }
                // Color color=UnityEngine.Random.ColorHSV();
                // obj.GetComponent<SpriteRenderer>().color=color;
                // Debug.Log(playerPos);
            }
            await Task.Delay(2000 / Mathf.RoundToInt(Time.timeScale));
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == false)
                break;
            else
                continue;
#endif
        } while (spawn);
    }
}
