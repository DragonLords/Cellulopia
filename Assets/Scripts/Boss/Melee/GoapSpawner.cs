using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapSpawner : MonoBehaviour
{
    [SerializeField] GameObject prefabs;
    [SerializeField] GameObject plane;
    public WaitForSeconds ws=new(5);
    IEnumerator Spawn(){
        var b=plane.GetComponent<UnityEngine.AI.NavMeshSurface>().navMeshData.sourceBounds;
        Vector3[] pos={b.min,b.max};
        do
        {
            Vector3 target=new(Random.Range(pos[0].x,pos[1].x),0f,Random.Range(pos[0].z,pos[1].z));
            Instantiate(prefabs,target,Quaternion.identity);
            yield return ws;
        } while (true);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spawn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
