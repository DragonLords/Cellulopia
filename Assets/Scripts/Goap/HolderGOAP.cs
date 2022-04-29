using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolderGOAP : MonoBehaviour
{
    public List<GOAPTester> goaps=new();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        goaps=new(GetComponentsInChildren<GOAPTester>());
        
    }
}
